using GWxLauncher.Domain;

namespace GWxLauncher.Services
{
    /// <summary>
    /// Result of GW2 isolation launch operation.
    /// </summary>
    internal class Gw2IsolationLaunchResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = "";
        public uint ProcessId { get; set; }
    }

    /// <summary>
    /// Service for launching GW2 with per-profile AppData isolation via DLL injection.
    /// Uses a x64 helper process (GWxInjector.exe) for injection since the launcher is x86.
    /// </summary>
    internal class Gw2IsolationService
    {
        private const string HOOK_DLL_NAME = "Gw2FolderHook.dll";
        private const string INJECTOR_EXE_NAME = "GWxInjector.exe";

        /// <summary>
        /// Launch GW2 with isolation enabled for the given profile.
        /// </summary>
        public Gw2IsolationLaunchResult LaunchWithIsolation(
            GameProfile profile,
            string? arguments = null)
        {
            try
            {
                // Determine exe path
                // If IsolationGameFolderPath is set, use it. Otherwise, fall back to ExecutablePath (original folder)
                string exePath;
                
                if (!string.IsNullOrWhiteSpace(profile.IsolationGameFolderPath))
                {
                    // Profile has a dedicated isolation folder
                    exePath = Path.Combine(profile.IsolationGameFolderPath, "Gw2-64.exe");
                }
                else
                {
                    // Profile uses original game folder (no copy needed)
                    exePath = profile.ExecutablePath ?? "";
                }

                if (!File.Exists(exePath))
                {
                    return new Gw2IsolationLaunchResult
                    {
                        Success = false,
                        ErrorMessage = $"Gw2-64.exe not found at: {exePath}"
                    };
                }

                // Get profile root (use configured or default)
                string profileRoot = !string.IsNullOrWhiteSpace(profile.IsolationProfileRoot)
                    ? profile.IsolationProfileRoot
                    : profile.GetDefaultIsolationProfileRoot();

                // Prepare profile directories
                string roamingPath = Path.Combine(profileRoot, "Roaming");
                string localPath = Path.Combine(profileRoot, "Local");

                Directory.CreateDirectory(roamingPath);
                Directory.CreateDirectory(localPath);

                // Find hook DLL (in launcher directory)
                string launcherDir = AppDomain.CurrentDomain.BaseDirectory;
                string hookDllPath = Path.Combine(launcherDir, HOOK_DLL_NAME);

                if (!File.Exists(hookDllPath))
                {
                    return new Gw2IsolationLaunchResult
                    {
                        Success = false,
                        ErrorMessage = $"Hook DLL not found: {hookDllPath}"
                    };
                }

                // Find injector helper (x64)
                string injectorPath = Path.Combine(launcherDir, INJECTOR_EXE_NAME);

                if (!File.Exists(injectorPath))
                {
                    return new Gw2IsolationLaunchResult
                    {
                        Success = false,
                        ErrorMessage = $"Injector helper not found: {injectorPath}. " +
                                      "The x64 helper executable is required for GW2 isolation."
                    };
                }

                // Launch via helper process
                uint processId = LaunchViaHelper(injectorPath, exePath, hookDllPath, roamingPath, localPath, arguments);

                return new Gw2IsolationLaunchResult
                {
                    Success = true,
                    ProcessId = processId
                };
            }
            catch (Exception ex)
            {
                return new Gw2IsolationLaunchResult
                {
                    Success = false,
                    ErrorMessage = $"Launch failed: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Launch GW2 via the x64 helper process which performs the injection.
        /// </summary>
        private uint LaunchViaHelper(
            string injectorPath,
            string exePath,
            string hookDllPath,
            string roamingPath,
            string localPath,
            string? arguments)
        {
            // Build command line for helper
            // Format: GWxInjector.exe <exePath> <dllPath> <roamingPath> <localPath> [gameArguments]
            var helperArgs = new List<string>
            {
                $"\"{exePath}\"",
                $"\"{hookDllPath}\"",
                $"\"{roamingPath}\"",
                $"\"{localPath}\""
            };

            if (!string.IsNullOrWhiteSpace(arguments))
            {
                helperArgs.Add(arguments);
            }

            string helperArguments = string.Join(" ", helperArgs);

            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = injectorPath,
                Arguments = helperArguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var helperProcess = System.Diagnostics.Process.Start(startInfo);

            if (helperProcess == null)
            {
                throw new InvalidOperationException("Failed to start injector helper process");
            }

            // Read output asynchronously to prevent deadlock
            var outputBuilder = new System.Text.StringBuilder();
            var errorBuilder = new System.Text.StringBuilder();

            helperProcess.OutputDataReceived += (s, e) =>
            {
                if (e.Data != null)
                    outputBuilder.AppendLine(e.Data);
            };

            helperProcess.ErrorDataReceived += (s, e) =>
            {
                if (e.Data != null)
                    errorBuilder.AppendLine(e.Data);
            };

            helperProcess.BeginOutputReadLine();
            helperProcess.BeginErrorReadLine();

            // Wait for helper to complete (should be quick - just does injection and exits)
            if (!helperProcess.WaitForExit(15000)) // 15 second timeout
            {
                helperProcess.Kill();
                throw new InvalidOperationException("Injector helper timed out");
            }

            // Ensure async reads complete
            helperProcess.WaitForExit();

            string output = outputBuilder.ToString().Trim();
            string errorOutput = errorBuilder.ToString().Trim();

            // Check exit code
            if (helperProcess.ExitCode != 0)
            {
                string errorMsg = errorOutput.StartsWith("ERROR:")
                    ? errorOutput.Substring(6).Trim()
                    : $"Injector failed with exit code {helperProcess.ExitCode}\nStderr: {errorOutput}\nStdout: {output}";

                throw new InvalidOperationException(errorMsg);
            }

            if (!output.StartsWith("SUCCESS:"))
            {
                throw new InvalidOperationException($"Unexpected helper output: {output}");
            }

            string pidString = output.Substring(8); // "SUCCESS:".Length
            if (!uint.TryParse(pidString, out uint processId))
            {
                throw new InvalidOperationException($"Failed to parse process ID from helper output: {pidString}");
            }

            return processId;
        }
    }
}
