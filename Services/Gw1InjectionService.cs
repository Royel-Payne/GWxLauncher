using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using GWxLauncher.Config;
using GWxLauncher.Domain;
using static GWxLauncher.Services.NativeMethods;
using PEB_MIN = GWxLauncher.Services.NativeMethods.PEB_MIN;
using PROCESS_BASIC_INFORMATION = GWxLauncher.Services.NativeMethods.PROCESS_BASIC_INFORMATION;
using PROCESS_INFORMATION = GWxLauncher.Services.NativeMethods.PROCESS_INFORMATION;
using ProcessAccessFlags = GWxLauncher.Services.NativeMethods.ProcessAccessFlags;
using STARTUPINFO = GWxLauncher.Services.NativeMethods.STARTUPINFO;

namespace GWxLauncher.Services
{
    /// <summary>
    /// Launch + injection pipeline for Guild Wars 1:
    ///  - gMod  : early injection via CreateProcessW in suspended mode
    ///  - Toolbox: normal injection after launch
    ///  - Py4GW : waits for window, then injects on a background thread
    ///
    /// NOTE: GW1 is 32-bit. The launcher should be built as x86 to avoid cross-arch injection issues.
    /// </summary>
    internal class Gw1InjectionService
    {
        private static void KillProcessIfCreatedButFailed(PROCESS_INFORMATION procInfo)
        {
            try
            {
                if (procInfo.hProcess != IntPtr.Zero)
                {
                    // Deterministic: if we created it and we’re bailing out, kill it.
                    TerminateProcess(procInfo.hProcess, 1);
                }
            }
            catch
            {
                // Best-effort only. We still close handles in the caller.
            }
        }

        #region GW1 Auto-Login Args (GW.exe flags)

        private static string BuildGw1AutoLoginArgs(GameProfile profile, LaunchReport report)
        {
            var step = new LaunchStep { Label = "Auto-Login" };
            report.Steps.Add(step);

            if (!profile.Gw1AutoLoginEnabled)
            {
                step.Outcome = StepOutcome.Skipped;
                step.Detail = "Disabled";
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(profile.Gw1Email) ||
                string.IsNullOrWhiteSpace(profile.Gw1PasswordProtected))
            {
                step.Outcome = StepOutcome.Failed;
                step.Detail = "Email or password not configured";
                return string.Empty;
            }

            string password;
            try
            {
                password = DpapiProtector.UnprotectFromBase64(profile.Gw1PasswordProtected);
            }
            catch
            {
                step.Outcome = StepOutcome.Failed;
                step.Detail = "Stored password could not be decrypted";
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                step.Outcome = StepOutcome.Failed;
                step.Detail = "Decrypted password was empty";
                return string.Empty;
            }

            var sb = new StringBuilder();
            sb.Append($"-email \"{profile.Gw1Email}\" ");
            sb.Append($"-password \"{password}\"");

            // Always include -character for reliability:
            // - If auto-select is enabled and name is present, pass it.
            // - Otherwise pass placeholder: -character " "
            if (profile.Gw1AutoSelectCharacterEnabled && !string.IsNullOrWhiteSpace(profile.Gw1CharacterName))
            {
                sb.Append($" -character \"{profile.Gw1CharacterName}\"");
                step.Detail = "Email + password + character";
            }
            else
            {
                sb.Append(" -character \" \"");
                step.Detail = profile.Gw1AutoSelectCharacterEnabled
                    ? "Email + password (character placeholder)"
                    : "Email + password (character placeholder; auto-select disabled)";
            }

            step.Outcome = StepOutcome.Success;
            return sb.ToString();
        }

        private static string BuildCreateProcessCommandLine(string exePath, string? args)
        {
            // CreateProcessW expects a full command line string.
            string quotedExe = $"\"{exePath}\"";

            string a = (args ?? "").Trim();
            if (string.IsNullOrWhiteSpace(a))
                return quotedExe;

            return quotedExe + " " + a;
        }

        #endregion

        #region Per-profile gMod folder

        internal static string GetGw1AccountFolder(string profileId)
        {
            // %AppData%\GWxLauncher\accounts\<ProfileId>\
            string baseDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(baseDir, "GWxLauncher", "accounts", profileId);
        }

        internal static string PreparePerProfileGModFolder(GameProfile profile)
        {
            string folder = GetGw1AccountFolder(profile.Id);
            Directory.CreateDirectory(folder);

            string linkPath = Path.Combine(folder, "gMod.dll");
            string canonical = profile.Gw1GModDllPath;

            if (string.IsNullOrWhiteSpace(canonical) || !File.Exists(canonical))
                throw new FileNotFoundException("Canonical gMod.dll path is missing or invalid.", canonical);

            // Always recreate to guarantee correctness.
            if (File.Exists(linkPath))
                File.Delete(linkPath);

            // Try hardlink first (per decisions.md), fallback to copy if it fails (e.g. cross-volume)
            if (!CreateHardLinkW(linkPath, canonical, IntPtr.Zero))
            {
                File.Copy(canonical, linkPath, overwrite: true);
            }

            // Generate modlist.txt deterministically.
            string modlist = Path.Combine(folder, "modlist.txt");

            var paths = (profile.Gw1GModPluginPaths ?? new List<string>())
                .Select(p => (p ?? "").Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                .ToList();

            // Auto-remove missing plugin files from the profile list.
            var existing = paths.Where(File.Exists).ToList();
            if (existing.Count != paths.Count)
                profile.Gw1GModPluginPaths = existing;

            File.WriteAllLines(modlist, existing);

            return linkPath; // DLL injected when gMod is enabled
        }

        #endregion

        #region Public entry points

        /// <summary>
        /// Generic utility: normal Process.Start then inject one or more DLLs.
        /// </summary>
        public bool TryLaunchWithDlls(
            GameProfile profile,
            string gwExePath,
            IEnumerable<string> dllPaths,
            out string errorMessage)
        {
            errorMessage = string.Empty;

            if (profile.GameType != GameType.GuildWars1)
            {
                errorMessage = "DLL injection is only supported for Guild Wars 1 profiles.";
                return false;
            }

            if (!File.Exists(gwExePath))
            {
                errorMessage = $"Guild Wars 1 executable not found:\n{gwExePath}";
                return false;
            }

            var dllList = dllPaths?
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList() ?? new();

            if (dllList.Count == 0)
            {
                errorMessage = "No DLLs were specified for injection.";
                return false;
            }

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = gwExePath,
                    UseShellExecute = false
                };

                var process = Process.Start(startInfo);
                if (process == null)
                {
                    errorMessage = "Failed to start Guild Wars 1 process.";
                    return false;
                }

                try { process.WaitForInputIdle(5000); }
                catch { /* ignore */ }

                if (process.HasExited)
                {
                    errorMessage = "Guild Wars 1 exited before injection could be performed.";
                    return false;
                }

                foreach (var dllPath in dllList)
                {
                    if (!File.Exists(dllPath))
                    {
                        errorMessage = $"DLL not found for injection:\n{dllPath}";
                        return false;
                    }

                    if (!InjectDllIntoProcess(process, dllPath, out var injectError))
                    {
                        errorMessage = $"Failed to inject DLL:\n{dllPath}\n\n{injectError}";
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Failed to launch Guild Wars 1 with DLL injection:\n\n{ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Toolbox-only convenience wrapper (legacy).
        /// </summary>
        public bool TryLaunchWithToolbox(GameProfile profile, string gwExePath, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (profile.GameType != GameType.GuildWars1)
            {
                errorMessage = "Toolbox injection is only supported for Guild Wars 1 profiles.";
                return false;
            }

            if (!File.Exists(gwExePath))
            {
                errorMessage = $"Guild Wars 1 executable not found:\n{gwExePath}";
                return false;
            }

            if (!profile.Gw1ToolboxEnabled)
            {
                errorMessage = "Toolbox injection is disabled for this profile.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(profile.Gw1ToolboxDllPath) ||
                !File.Exists(profile.Gw1ToolboxDllPath))
            {
                errorMessage =
                    "GW1 Toolbox DLL path is not configured, or the file does not exist.\n\n" +
                    "Configure this in Profile Settings.";
                return false;
            }

            // Optional: seed the list
            if (profile.Gw1InjectedDlls != null && profile.Gw1InjectedDlls.Count == 0)
            {
                profile.Gw1InjectedDlls.Add(new Gw1InjectedDll
                {
                    Name = "Toolbox",
                    Path = profile.Gw1ToolboxDllPath,
                    Enabled = true
                });
            }

            return TryLaunchWithDlls(profile, gwExePath, new[] { profile.Gw1ToolboxDllPath }, out errorMessage);
        }

        /// <summary>
        /// Primary GW1 profile launch flow:
        ///  - gMod early injection (optional)
        ///  - Toolbox immediate injection (optional)
        ///  - Py4GW background injection after window (optional)
        ///  - Multiclient patch if requested (suspended CreateProcessW path)
        /// </summary>
        public bool TryLaunchGw1(
            GameProfile profile,
            LauncherConfig config,
            string exePath,
            bool gw1MulticlientEnabled,
            IWin32Window owner,
            out Process? launchedProcess,
            out string errorMessage,
            out LaunchReport report,
            Action<string, string, bool>? showMessage = null)
        {
            errorMessage = string.Empty;
            launchedProcess = null;

            report = new LaunchReport
            {
                GameName = "Guild Wars 1",
                ExecutablePath = exePath
            };

            // Non-fatal diagnostic note for protected install paths (warning-only).
            ProtectedInstallPathPolicy.TryAppendLaunchReportNote(report, exePath);

            // Steps in real-world order (Multiclient -> gMod -> Toolbox -> Py4GW -> Auto-Login)
            var stepMulticlient = new LaunchStep { Label = "Multiclient" };
            var stepLaunch = new LaunchStep { Label = "GW1 Launch" };
            var stepGmod = new LaunchStep { Label = "gMod" };
            var stepToolbox = new LaunchStep { Label = "Toolbox" };
            var stepPy4Gw = new LaunchStep { Label = "Py4GW" };

            report.Steps.Add(stepMulticlient);
            report.Steps.Add(stepLaunch);
            report.Steps.Add(stepGmod);
            report.Steps.Add(stepToolbox);
            report.Steps.Add(stepPy4Gw);

            // Note: This step is intentionally about the *flag state* (enabled/disabled),
            // not a definitive validation that the patch succeeded.
            stepMulticlient.Outcome = gw1MulticlientEnabled ? StepOutcome.Success : StepOutcome.Skipped;
            stepMulticlient.Detail = gw1MulticlientEnabled ? "Enabled" : "Disabled";

            stepLaunch.Outcome = StepOutcome.Pending;
            stepLaunch.Detail = "Launching Guild Wars 1";
            string gwArgs = BuildGw1AutoLoginArgs(profile, report);

            if (profile.GameType != GameType.GuildWars1)
            {
                errorMessage = "This profile is not a Guild Wars 1 profile.";
                report.Succeeded = false;
                report.FailureMessage = errorMessage;

                stepGmod.Outcome = StepOutcome.Skipped;
                stepToolbox.Outcome = StepOutcome.Skipped;
                stepPy4Gw.Outcome = StepOutcome.Skipped;

                return false;
            }

            if (string.IsNullOrWhiteSpace(exePath) || !File.Exists(exePath))
            {
                errorMessage = $"Guild Wars 1 executable not found:\n{exePath}";
                report.Succeeded = false;
                report.FailureMessage = errorMessage;

                stepGmod.Outcome = StepOutcome.Skipped;
                stepToolbox.Outcome = StepOutcome.Skipped;
                stepPy4Gw.Outcome = StepOutcome.Skipped;

                return false;
            }

            Process? process = null;

            // 1) gMod early injection path (suspended CreateProcessW)
            bool gmodEnabled =
                profile.Gw1GModEnabled &&
                config.GlobalGModEnabled;

            bool gmodConfigured =
                gmodEnabled &&
                !string.IsNullOrWhiteSpace(profile.Gw1GModDllPath) &&
                File.Exists(profile.Gw1GModDllPath);

            if (gmodConfigured)
            {
                string gmodToInject;

                try
                {
                    // Repair per-profile folder + gMod.dll + modlist.txt
                    gmodToInject = PreparePerProfileGModFolder(profile);
                }
                catch (Exception ex)
                {
                    errorMessage = $"gMod is enabled but setup failed:\n{ex.Message}";

                    stepGmod.Outcome = StepOutcome.Failed;
                    stepGmod.Detail = ex.Message;

                    stepToolbox.Outcome = StepOutcome.Skipped;
                    stepPy4Gw.Outcome = StepOutcome.Skipped;

                    report.UsedSuspendedLaunch = true;
                    report.Succeeded = false;
                    report.FailureMessage = errorMessage;
                    return false;
                }

                report.UsedSuspendedLaunch = true;
                stepLaunch.Outcome = StepOutcome.Pending;
                stepLaunch.Detail = gw1MulticlientEnabled
                    ? "Starting (suspended CreateProcessW + multiclient patch + gMod injection)"
                    : "Starting (suspended CreateProcessW + gMod injection)";

                if (!TryLaunchGw1WithGMod(
                        exePath,
                        gmodToInject,
                        gwArgs,
                        gw1MulticlientEnabled,
                        out process,
                        out var gmodError))
                {
                    errorMessage = gmodError;

                    stepLaunch.Outcome = StepOutcome.Failed;
                    stepLaunch.Detail = errorMessage;
                    stepGmod.Outcome = StepOutcome.Failed;
                    stepGmod.Detail = gmodError;

                    stepToolbox.Outcome = StepOutcome.Skipped;
                    stepPy4Gw.Outcome = StepOutcome.Skipped;

                    report.UsedSuspendedLaunch = true;
                    report.Succeeded = false;
                    report.FailureMessage = errorMessage;

                    return false;
                }

                stepGmod.Outcome = StepOutcome.Success;
                stepGmod.Detail = "Injected";
                stepLaunch.Outcome = StepOutcome.Success;
                stepLaunch.Detail = gw1MulticlientEnabled
                    ? "Created suspended, patched for multiclient, injected gMod, resumed"
                    : "Created suspended, injected gMod, resumed";

                report.UsedSuspendedLaunch = true;
            }
            else
            {
                stepGmod.Outcome = StepOutcome.Skipped;
                stepGmod.Detail = !config.GlobalGModEnabled
                    ? "Disabled globally"
                    : profile.Gw1GModEnabled
                        ? "Enabled, but DLL path missing or file not found"
                        : "Disabled";

                // 2) Normal launch (with or without multiclient patch)
                if (gw1MulticlientEnabled)
                {
                    report.UsedSuspendedLaunch = true;
                    stepLaunch.Outcome = StepOutcome.Pending;
                    stepLaunch.Detail = "Starting (suspended CreateProcessW + multiclient patch)";

                    var startupInfo = new STARTUPINFO { cb = (uint)Marshal.SizeOf(typeof(STARTUPINFO)) };
                    PROCESS_INFORMATION procInfo = default;

                    string workingDir = Path.GetDirectoryName(exePath) ?? string.Empty;
                    string cmdLine = BuildCreateProcessCommandLine(exePath, gwArgs);

                    bool created = CreateProcessW(
                        exePath,
                        cmdLine,
                        IntPtr.Zero,
                        IntPtr.Zero,
                        false,
                        CREATE_SUSPENDED,
                        IntPtr.Zero,
                        workingDir,
                        ref startupInfo,
                        out procInfo);

                    if (!created)
                    {
                        errorMessage =
                            "Failed to create Guild Wars 1 process in suspended mode.\n" +
                            $"Win32 error: {Marshal.GetLastWin32Error()}";


                        stepLaunch.Outcome = StepOutcome.Failed;
                        stepLaunch.Detail = errorMessage;
                        report.Succeeded = false;
                        report.FailureMessage = errorMessage;

                        stepToolbox.Outcome = StepOutcome.Skipped;
                        stepPy4Gw.Outcome = StepOutcome.Skipped;
                        return false;
                    }

                    bool launchedOk = false;

                    try
                    {
                        if (!TryApplyGw1MulticlientPatch(procInfo.hProcess, out var patchError))
                        {
                            errorMessage = $"GW1 multiclient patch failed:\n{patchError}";

                            stepLaunch.Outcome = StepOutcome.Failed;
                            stepLaunch.Detail = errorMessage;
                            report.UsedSuspendedLaunch = true;
                            report.Succeeded = false;
                            report.FailureMessage = errorMessage;

                            stepToolbox.Outcome = StepOutcome.Skipped;
                            stepPy4Gw.Outcome = StepOutcome.Skipped;
                            return false;
                        }

                        process = Process.GetProcessById(procInfo.dwProcessId);

                        ResumeThread(procInfo.hThread);

                        // Give GW a moment to fail fast if it's going to
                        Thread.Sleep(500);

                        try
                        {
                            if (process != null && process.HasExited)
                            {
                                errorMessage = $"GW exited immediately after resume. ExitCode={process.ExitCode}";
                                stepLaunch.Outcome = StepOutcome.Failed;
                                stepLaunch.Detail = errorMessage;
                                report.Succeeded = false;
                                report.FailureMessage = errorMessage;
                                return false;
                            }
                        }
                        catch
                        {
                            errorMessage = "GW exited immediately after resume (process vanished).";
                            stepLaunch.Outcome = StepOutcome.Failed;
                            stepLaunch.Detail = errorMessage;
                            report.Succeeded = false;
                            report.FailureMessage = errorMessage;
                            return false;
                        }

                        stepLaunch.Outcome = StepOutcome.Success;
                        stepLaunch.Detail = "Created suspended, patched for multiclient, resumed";
                        report.UsedSuspendedLaunch = true;

                        launchedOk = true;
                    }
                    finally
                    {
                        // kill orphaned suspended GW if we failed after CreateProcessW
                        if (!launchedOk)
                            KillProcessIfCreatedButFailed(procInfo);

                        if (procInfo.hThread != IntPtr.Zero) CloseHandle(procInfo.hThread);
                        if (procInfo.hProcess != IntPtr.Zero) CloseHandle(procInfo.hProcess);
                    }
                }
                else
                {
                    stepLaunch.Outcome = StepOutcome.Pending;
                    stepLaunch.Detail = "Starting (normal CreateProcessW)";

                    var startupInfo = new STARTUPINFO { cb = (uint)Marshal.SizeOf(typeof(STARTUPINFO)) };
                    PROCESS_INFORMATION procInfo = default;

                    string workingDir = Path.GetDirectoryName(exePath) ?? string.Empty;
                    string cmdLine = BuildCreateProcessCommandLine(exePath, gwArgs);

                    try
                    {
                        bool created = CreateProcessW(
                            exePath,
                            cmdLine,
                            IntPtr.Zero,
                            IntPtr.Zero,
                            false,
                            0,
                            IntPtr.Zero,
                            workingDir,
                            ref startupInfo,
                            out procInfo);

                        if (!created)
                        {
                            errorMessage =
                                "Failed to create Guild Wars 1 process.\n" +
                                $"Win32 error: {Marshal.GetLastWin32Error()}";

                            stepLaunch.Outcome = StepOutcome.Failed;
                            stepLaunch.Detail = errorMessage;

                            report.Succeeded = false;
                            report.FailureMessage = errorMessage;
                            return false;
                        }

                        process = Process.GetProcessById(procInfo.dwProcessId);


                        stepLaunch.Outcome = StepOutcome.Success;
                        stepLaunch.Detail = "CreateProcessW (normal)";

                        report.UsedSuspendedLaunch = false;
                    }
                    catch (Exception ex)
                    {
                        errorMessage = $"Failed to start Guild Wars 1:\n{ex.Message}";
                        stepLaunch.Outcome = StepOutcome.Failed;
                        stepLaunch.Detail = errorMessage;
                        report.Succeeded = false;
                        report.FailureMessage = errorMessage;
                        return false;
                    }
                    finally
                    {
                        if (procInfo.hThread != IntPtr.Zero) CloseHandle(procInfo.hThread);
                        if (procInfo.hProcess != IntPtr.Zero) CloseHandle(procInfo.hProcess);
                    }
                }
            }

            if (process == null || process.HasExited)
            {
                errorMessage = "Guild Wars 1 process exited unexpectedly.";

                report.Succeeded = false;
                report.FailureMessage = errorMessage;

                stepToolbox.Outcome = StepOutcome.Skipped;
                stepPy4Gw.Outcome = StepOutcome.Skipped;

                return false;
            }

            // 3) Toolbox: immediate injection (after gMod, if present)
            if (profile.Gw1ToolboxEnabled && config.GlobalToolboxEnabled)
            {
                var toolboxPath = profile.Gw1ToolboxDllPath;

                if (string.IsNullOrWhiteSpace(toolboxPath) || !File.Exists(toolboxPath))
                {
                    stepToolbox.Outcome = StepOutcome.Failed;
                    stepToolbox.Detail = "Enabled, but DLL path missing or file not found";

                    showMessage?.Invoke(
                        "GW1 Toolbox is enabled for this profile, but the DLL path is not configured " +
                        "or the file does not exist.\n\nThe game will launch without Toolbox.",
                        "GW1 Toolbox injection",
                        false); // Warning
                }
                else
                {
                    if (!InjectDllIntoProcess(process, toolboxPath, out var injectError))
                    {
                        stepToolbox.Outcome = StepOutcome.Failed;
                        stepToolbox.Detail = injectError;

                        showMessage?.Invoke(
                            $"Failed to inject GW1 Toolbox DLL:\n\n{injectError}\n\nThe game will continue without Toolbox.",
                            "GW1 Toolbox injection",
                            false); // Warning
                    }
                    else
                    {
                        stepToolbox.Outcome = StepOutcome.Success;
                    }
                }
            }
            else
            {
                stepToolbox.Outcome = StepOutcome.Skipped;
                stepToolbox.Detail = !config.GlobalToolboxEnabled
                    ? "Disabled globally"
                    : "Disabled";
            }

            // 4) Py4GW: background injection after window is ready
            bool py4GwEnabled =
                profile.Gw1Py4GwEnabled &&
                config.GlobalPy4GwEnabled;

            bool py4GwConfigured =
                py4GwEnabled &&
                !string.IsNullOrWhiteSpace(profile.Gw1Py4GwDllPath) &&
                File.Exists(profile.Gw1Py4GwDllPath);

            if (py4GwConfigured)
            {
                stepPy4Gw.Outcome = StepOutcome.Pending;
                stepPy4Gw.Detail = "Queued for background injection after window is ready";

                _ = Task.Run(() => InjectPy4GwAfterWindowReady(process, profile, owner, stepPy4Gw));
            }
            else
            {
                if (py4GwEnabled)
                {
                    stepPy4Gw.Outcome = StepOutcome.Failed;
                    stepPy4Gw.Detail = "Enabled, but DLL path missing or file not found";
                }
                if (!config.GlobalPy4GwEnabled)
                {
                    stepPy4Gw.Outcome = StepOutcome.Skipped;
                    stepPy4Gw.Detail = "Disabled globally";
                }
                else if (profile.Gw1Py4GwEnabled)
                {
                    stepPy4Gw.Outcome = StepOutcome.Failed;
                    stepPy4Gw.Detail = "Enabled, but DLL path missing or file not found";
                }
                else
                {
                    stepPy4Gw.Outcome = StepOutcome.Skipped;
                    stepPy4Gw.Detail = "Disabled";
                }
            }

            launchedProcess = process;

            report.Succeeded = true;
            return true;
        }

        #endregion

        #region Py4GW background injection

        /// <summary>
        /// Waits until the Guild Wars process has a main window handle, or times out.
        /// Returns false if the process exits first.
        /// </summary>
        private bool WaitForGuildWarsWindow(Process process, TimeSpan timeout)
        {
            var sw = Stopwatch.StartNew();

            while (sw.Elapsed < timeout)
            {
                if (process.HasExited)
                    return false;

                process.Refresh();

                if (process.MainWindowHandle != IntPtr.Zero)
                    return true;

                Thread.Sleep(250);
            }

            return false;
        }

        /// <summary>
        /// Waits for the GW1 window, then injects Py4GW.
        /// Runs on a background thread.
        /// </summary>
        private void InjectPy4GwAfterWindowReady(
            Process process,
            GameProfile profile,
            IWin32Window? owner,
            LaunchStep stepPy4Gw)
        {
            try
            {
                if (!profile.Gw1Py4GwEnabled)
                {
                    stepPy4Gw.Outcome = StepOutcome.Skipped;
                    stepPy4Gw.Detail = "Disabled";
                    return;
                }

                var dllPath = profile.Gw1Py4GwDllPath;
                if (string.IsNullOrWhiteSpace(dllPath) || !File.Exists(dllPath))
                {
                    stepPy4Gw.Outcome = StepOutcome.Failed;
                    stepPy4Gw.Detail = "Enabled, but DLL path missing or file not found";
                    return;
                }

                if (!WaitForGuildWarsWindow(process, TimeSpan.FromSeconds(30)))
                {
                    if (process.HasExited)
                    {
                        stepPy4Gw.Outcome = StepOutcome.Failed;
                        stepPy4Gw.Detail = "Process exited before window was ready";
                    }
                    else
                    {
                        stepPy4Gw.Outcome = StepOutcome.Failed;
                        stepPy4Gw.Detail = "Timed out waiting for window to be ready";
                    }

                    return;
                }

                Thread.Sleep(TimeSpan.FromSeconds(5));

                if (process.HasExited)
                {
                    stepPy4Gw.Outcome = StepOutcome.Failed;
                    stepPy4Gw.Detail = "Process exited before injection";
                    return;
                }

                var ok = InjectDllIntoProcess(process, dllPath, out var error);

                if (ok)
                {
                    stepPy4Gw.Outcome = StepOutcome.Success;
                    stepPy4Gw.Detail = "Injected";
                }
                else
                {
                    stepPy4Gw.Outcome = StepOutcome.Failed;
                    stepPy4Gw.Detail = error;
                }

                _ = owner; // placeholder: owner may be used later
            }
            catch (Exception ex)
            {
                stepPy4Gw.Outcome = StepOutcome.Failed;
                stepPy4Gw.Detail = $"Exception during background injection: {ex.Message}";
            }
        }

        #endregion

        #region gMod early injection

        /// <summary>
        /// Launch Gw.exe in suspended mode, inject gMod, then resume.
        /// </summary>
        private bool TryLaunchGw1WithGMod(
            string exePath,
            string gmodDllPath,
            string gwArgs,
            bool gw1MulticlientEnabled,
            out Process? process,
            out string errorMessage)
        {
            process = null;
            errorMessage = string.Empty;

            var startupInfo = new STARTUPINFO { cb = (uint)Marshal.SizeOf(typeof(STARTUPINFO)) };
            PROCESS_INFORMATION procInfo = default;

            string workingDir = Path.GetDirectoryName(exePath) ?? string.Empty;
            string cmdLine = BuildCreateProcessCommandLine(exePath, gwArgs);

            bool launchedOk = false;

            try
            {
                bool created = CreateProcessW(
                    exePath,
                    cmdLine,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    false,
                    CREATE_SUSPENDED,
                    IntPtr.Zero,
                    workingDir,
                    ref startupInfo,
                    out procInfo);

                if (!created)
                {
                    errorMessage =
                        "Failed to create Guild Wars 1 process in suspended mode.\n" +
                        $"Win32 error: {Marshal.GetLastWin32Error()}";
                    return false;
                }

                process = Process.GetProcessById(procInfo.dwProcessId);

                if (gw1MulticlientEnabled)
                {
                    if (!TryApplyGw1MulticlientPatch(procInfo.hProcess, out var patchError))
                    {
                        errorMessage = $"GW1 multiclient patch failed:\n{patchError}";
                        return false;
                    }
                }

                if (!InjectDllIntoProcess(process, gmodDllPath, out var injectError))
                {
                    errorMessage = $"Failed to inject gMod DLL:\n{injectError}";
                    return false;
                }

                ResumeThread(procInfo.hThread);

                launchedOk = true;
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Failed during gMod early injection:\n{ex.Message}";
                return false;
            }
            finally
            {
                // kill orphaned suspended GW if we failed after CreateProcessW
                if (!launchedOk)
                    KillProcessIfCreatedButFailed(procInfo);

                if (procInfo.hThread != IntPtr.Zero) CloseHandle(procInfo.hThread);
                if (procInfo.hProcess != IntPtr.Zero) CloseHandle(procInfo.hProcess);
            }
        }

        #endregion

        #region Core DLL injection

        /// <summary>
        /// Core injection logic: OpenProcess, allocate memory, write DLL path,
        /// then create a remote thread calling LoadLibraryW(path).
        /// </summary>
        private bool InjectDllIntoProcess(Process process, string dllPath, out string errorMessage)
        {
            errorMessage = string.Empty;

            IntPtr hProcess = IntPtr.Zero;
            IntPtr remoteMemory = IntPtr.Zero;
            IntPtr hThread = IntPtr.Zero;

            try
            {
                hProcess = OpenProcess(
                    ProcessAccessFlags.PROCESS_CREATE_THREAD |
                    ProcessAccessFlags.PROCESS_QUERY_INFORMATION |
                    ProcessAccessFlags.PROCESS_VM_OPERATION |
                    ProcessAccessFlags.PROCESS_VM_WRITE |
                    ProcessAccessFlags.PROCESS_VM_READ,
                    false,
                    process.Id);

                if (hProcess == IntPtr.Zero)
                {
                    errorMessage = $"OpenProcess failed. Win32 error: {Marshal.GetLastWin32Error()}";
                    return false;
                }

                byte[] dllBytes = Encoding.Unicode.GetBytes(dllPath + "\0");
                uint size = (uint)dllBytes.Length;

                remoteMemory = VirtualAllocEx(
                    hProcess,
                    IntPtr.Zero,
                    size,
                    MEM_COMMIT | MEM_RESERVE,
                    PAGE_READWRITE);

                if (remoteMemory == IntPtr.Zero)
                {
                    errorMessage = $"VirtualAllocEx failed. Win32 error: {Marshal.GetLastWin32Error()}";
                    return false;
                }

                if (!WriteProcessMemory(hProcess, remoteMemory, dllBytes, size, out var bytesWritten) ||
                    bytesWritten == IntPtr.Zero ||
                    (uint)bytesWritten != size)
                {
                    errorMessage = $"WriteProcessMemory failed. Win32 error: {Marshal.GetLastWin32Error()}";
                    return false;
                }

                IntPtr hKernel32 = GetModuleHandle("kernel32.dll");
                if (hKernel32 == IntPtr.Zero)
                {
                    errorMessage = $"GetModuleHandle(\"kernel32.dll\") failed. Win32 error: {Marshal.GetLastWin32Error()}";
                    return false;
                }

                IntPtr loadLibraryWPtr = GetProcAddress(hKernel32, "LoadLibraryW");
                if (loadLibraryWPtr == IntPtr.Zero)
                {
                    errorMessage = $"GetProcAddress(\"LoadLibraryW\") failed. Win32 error: {Marshal.GetLastWin32Error()}";
                    return false;
                }

                hThread = CreateRemoteThread(
                    hProcess,
                    IntPtr.Zero,
                    0,
                    loadLibraryWPtr,
                    remoteMemory,
                    0,
                    out _);

                if (hThread == IntPtr.Zero)
                {
                    errorMessage = $"CreateRemoteThread failed. Win32 error: {Marshal.GetLastWin32Error()}";
                    return false;
                }

                WaitForSingleObject(hThread, 5000);
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"DLL injection failed: {ex.Message}";
                return false;
            }
            finally
            {
                if (hThread != IntPtr.Zero) CloseHandle(hThread);

                if (remoteMemory != IntPtr.Zero)
                {
                    // Correctly free the memory allocated in the target process.
                    VirtualFreeEx(hProcess, remoteMemory, 0, MEM_RELEASE);
                }

                if (hProcess != IntPtr.Zero) CloseHandle(hProcess);
            }
        }

        #endregion

        #region GW1 Multiclient Patch

        private static readonly byte[] Gw1MulticlientSignature =
        {
            0x56, 0x57, 0x68, 0x00, 0x01, 0x00, 0x00, 0x89, 0x85, 0xF4, 0xFE, 0xFF, 0xFF,
            0xC7, 0x00, 0x00, 0x00, 0x00, 0x00
        };

        private bool TryApplyGw1MulticlientPatch(IntPtr processHandle, out string error)
        {
            error = string.Empty;

            if (!MemoryScanner.TryGetImageBaseFromPeb(processHandle, out IntPtr moduleBase, out string pebError))
            {
                error = pebError;
                return false;
            }

            // 3) Read fixed region (0x48D000)
            const int readSize = 0x48D000;
            byte[] image = new byte[readSize];

            if (!ReadProcessMemory(processHandle, moduleBase, image, image.Length, out _))
            {
                error = $"ReadProcessMemory(image) failed. Win32 error: {Marshal.GetLastWin32Error()}";
                return false;
            }

            // 4) Signature scan
            int idx = MemoryScanner.IndexOf(image, Gw1MulticlientSignature);
            if (idx < 0)
            {
                error = "GW1 multiclient signature not found in process image. Aborting (no blind patch).";
                return false;
            }

            // 5) patchAddress = moduleBase + idx - 0x1A
            IntPtr patchAddress = IntPtr.Add(moduleBase, idx - 0x1A);

            // Payload: 31 C0 90 C3
            byte[] patch = { 0x31, 0xC0, 0x90, 0xC3 };

            if (!WriteProcessMemory(processHandle, patchAddress, patch, (uint)patch.Length, out var written) ||
                written.ToInt64() != patch.Length)
            {
                error = $"WriteProcessMemory(patch) failed. Win32 error: {Marshal.GetLastWin32Error()}";
                return false;
            }

            // Read-back verify
            byte[] verify = new byte[patch.Length];
            if (!ReadProcessMemory(processHandle, patchAddress, verify, verify.Length, out _))
            {
                error = $"ReadProcessMemory(verify) failed. Win32 error: {Marshal.GetLastWin32Error()}";
                return false;
            }

            for (int i = 0; i < patch.Length; i++)
            {
                if (verify[i] != patch[i])
                {
                    error = "GW1 multiclient patch verification failed (read-back bytes do not match payload).";
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}
