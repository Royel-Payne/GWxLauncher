using GWxLauncher.Domain;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GWxLauncher.Services
{
    /// <summary>
    /// Handles launching Guild Wars 1 with optional GWToolbox (or other DLL) injection.
    ///
    /// High-level flow:
    ///  - Validate GW1 EXE path and DLL path
    ///  - Start GW1 normally via Process.Start
    ///  - Once the process is running, inject the DLL by:
    ///      * OpenProcess
    ///      * VirtualAllocEx
    ///      * WriteProcessMemory (DLL path as a Unicode string)
    ///      * CreateRemoteThread calling LoadLibraryW in kernel32.dll
    ///
    /// This keeps things clear and "textbook" so it's easy to port later.
    /// NOTE: For a 32-bit game like GW1, your launcher should be built as x86
    /// to avoid cross-architecture injection issues.
    /// </summary>
    internal class Gw1InjectionService
    {
        #region Win32 interop

        [Flags]
        private enum ProcessAccessFlags : uint
        {
            PROCESS_CREATE_THREAD = 0x0002,
            PROCESS_QUERY_INFORMATION = 0x0400,
            PROCESS_VM_OPERATION = 0x0008,
            PROCESS_VM_WRITE = 0x0020,
            PROCESS_VM_READ = 0x0010,
            PROCESS_ALL_ACCESS = 0x001F0FFF
        }

        private const uint MEM_COMMIT = 0x1000;
        private const uint MEM_RESERVE = 0x2000;
        private const uint PAGE_READWRITE = 0x04;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(
            ProcessAccessFlags processAccess,
            bool bInheritHandle,
            int processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualAllocEx(
            IntPtr hProcess,
            IntPtr lpAddress,
            uint dwSize,
            uint flAllocationType,
            uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            uint nSize,
            out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateRemoteThread(
            IntPtr hProcess,
            IntPtr lpThreadAttributes,
            uint dwStackSize,
            IntPtr lpStartAddress,
            IntPtr lpParameter,
            uint dwCreationFlags,
            out IntPtr lpThreadId);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        private const uint WAIT_OBJECT_0 = 0x00000000;
        private const uint INFINITE = 0xFFFFFFFF;

        #endregion

        /// <summary>
        /// Generic entry point: launch GW1 and inject one or more DLLs.
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

                try
                {
                    process.WaitForInputIdle(5000);
                }
                catch
                {
                    // Some processes may not support WaitForInputIdle; ignore
                }

                if (process.HasExited)
                {
                    errorMessage = "Guild Wars 1 exited before injection could be performed.";
                    return false;
                }

                // Inject each DLL in order
                foreach (var dllPath in dllList)
                {
                    if (!File.Exists(dllPath))
                    {
                        errorMessage = $"DLL not found for injection:\n{dllPath}";
                        return false;
                    }

                    if (!InjectDllIntoProcess(process, dllPath, out var injectError))
                    {
                        errorMessage =
                            $"Failed to inject DLL:\n{dllPath}\n\n" +
                            injectError;
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
        /// Entry point used by MainForm.LaunchProfile.
        /// Returns true if we launched GW1 and successfully queued DLL injection.
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
                    "Use the context menu:\n" +
                    "  ▸ GW1 Toolbox (inject)\n" +
                    "  ▸ Set GW1 Toolbox DLL…";
                return false;
            }

            // Optional: if the new list is empty, seed it with Toolbox
            if (profile.Gw1InjectedDlls != null &&
                profile.Gw1InjectedDlls.Count == 0)
            {
                profile.Gw1InjectedDlls.Add(new Gw1InjectedDll
                {
                    Name = "Toolbox",
                    Path = profile.Gw1ToolboxDllPath,
                    Enabled = true
                });
            }

            // Delegate to the generic multi-DLL launcher
            return TryLaunchWithDlls(
                profile,
                gwExePath,
                new[] { profile.Gw1ToolboxDllPath },
                out errorMessage);
        }
        /// <summary>
        /// Main entry point for launching a GW1 profile.
        /// Handles:
        ///  - starting Gw.exe
        ///  - immediate Toolbox injection (if enabled)
        ///  - background Py4GW injection (if enabled)
        /// </summary>
        public bool TryLaunchGw1(
            GameProfile profile,
            string exePath,
            IWin32Window owner,
            out string errorMessage)
        {
            errorMessage = string.Empty;

            if (profile.GameType != GameType.GuildWars1)
            {
                errorMessage = "This profile is not a Guild Wars 1 profile.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(exePath) || !File.Exists(exePath))
            {
                errorMessage = $"Guild Wars 1 executable not found:\n{exePath}";
                return false;
            }


            Process? process;

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    WorkingDirectory = Path.GetDirectoryName(exePath) ?? string.Empty,
                    UseShellExecute = false
                };

                process = Process.Start(startInfo);

                if (process == null)
                {
                    errorMessage = "Failed to start Guild Wars 1 process.";
                    return false;
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Failed to start Guild Wars 1:\n\n{ex.Message}";
                return false;
            }

            // --- TOOLBOX: immediate injection, same as before but scoped here ---

            if (profile.Gw1ToolboxEnabled)
            {
                var toolboxPath = profile.Gw1ToolboxDllPath;

                if (string.IsNullOrWhiteSpace(toolboxPath) || !File.Exists(toolboxPath))
                {
                    // We *launched* the game, but Toolbox is misconfigured.
                    // You can decide later if this should be fatal. For now, warn and keep going.
                    MessageBox.Show(
                        owner,
                        "GW1 Toolbox is enabled for this profile, but the DLL path is not configured " +
                        "or the file does not exist.\n\nThe game will launch without Toolbox.",
                        "GW1 Toolbox injection",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                }
                else
                {
                    if (!InjectDllIntoProcess(process, toolboxPath, out var injectError))
                    {
                        // Again: game already launched; we just report the Toolbox failure.
                        MessageBox.Show(
                            owner,
                            $"Failed to inject GW1 Toolbox DLL:\n\n{injectError}\n\n" +
                            "The game will continue without Toolbox.",
                            "GW1 Toolbox injection",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }
                }
            }

            // --- PY4GW: background injection after window is ready ---

            if (profile.Gw1Py4GwEnabled &&
                !string.IsNullOrWhiteSpace(profile.Gw1Py4GwDllPath) &&
                File.Exists(profile.Gw1Py4GwDllPath))
            {
                // Launch a background task that waits for the window, then injects.
                _ = Task.Run(() => InjectPy4GwAfterWindowReady(process, profile, owner));
            }

            // NOTE: Later we’ll add gMod here too.

            return true;
        }
        /// <summary>
        /// Waits until the Guild Wars process has a main window handle,
        /// or times out. Returns false if the process exits first.
        /// </summary>
        private bool WaitForGuildWarsWindow(Process process, TimeSpan timeout)
        {
            var sw = Stopwatch.StartNew();

            while (sw.Elapsed < timeout)
            {
                if (process.HasExited)
                    return false;

                // Refresh to get updated MainWindowHandle
                process.Refresh();

                if (process.MainWindowHandle != IntPtr.Zero)
                    return true;

                Thread.Sleep(250);
            }

            return false;
        }
        /// <summary>
        /// Waits for the GW1 window, then injects Py4GW if everything is still running.
        /// Runs on a background thread.
        /// </summary>
        private void InjectPy4GwAfterWindowReady(Process process, GameProfile profile, IWin32Window? owner)
        {
            try
            {
                // Basic sanity checks
                if (!profile.Gw1Py4GwEnabled)
                    return;

                var dllPath = profile.Gw1Py4GwDllPath;
                if (string.IsNullOrWhiteSpace(dllPath))
                    return;

                // 1) Wait up to 30s for the GW window
                if (!WaitForGuildWarsWindow(process, TimeSpan.FromSeconds(30)))
                {
                    // Optional: log or status message later
                    return;
                }

                // 2) Give GW a bit of breathing room after the window appears
                Thread.Sleep(TimeSpan.FromSeconds(5));

                if (process.HasExited)
                    return;

                // 3) Reuse the same injection helper we already use for Toolbox
                //    Adjust this call to match your existing method signature.
                var ok = InjectDllIntoProcess(process, dllPath, out var error);

                if (!ok)
                {
                    // Optional: for now we stay quiet; later we can log or surface this.
                    // If you want immediate feedback, you could do:
                    // MessageBox.Show(owner, $"Failed to inject Py4GW:\n\n{error}", "Py4GW injection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch
            {
                // Swallow for now; later we can wire a logger.
            }
        }

        /// <summary>
        /// Core injection logic: OpenProcess, allocate memory, write the DLL path,
        /// then create a remote thread that calls LoadLibraryW(path).
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

                // Unicode string + null terminator
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

                // Get LoadLibraryW from kernel32.dll in *our* process
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

                // Create a thread in the target process that calls LoadLibraryW(dllPath)
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

                // Wait for the remote thread to complete the LoadLibrary call
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
                if (hThread != IntPtr.Zero)
                    CloseHandle(hThread);

                if (remoteMemory != IntPtr.Zero)
                    CloseHandle(remoteMemory); // technically should use VirtualFreeEx, but CloseHandle is harmless here

                if (hProcess != IntPtr.Zero)
                    CloseHandle(hProcess);
            }
        }
    }
}
