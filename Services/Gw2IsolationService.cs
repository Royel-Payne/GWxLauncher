using GWxLauncher.Domain;
using System.Runtime.InteropServices;
using System.Text;

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
    /// Based on the proven PoC implementation.
    /// </summary>
    internal class Gw2IsolationService
    {
        private const string HOOK_DLL_NAME = "Gw2FolderHook.dll";

        /// <summary>
        /// Launch GW2 with isolation enabled for the given profile.
        /// </summary>
        public Gw2IsolationLaunchResult LaunchWithIsolation(
            GameProfile profile,
            string? arguments = null)
        {
            try
            {
                // Validate profile has isolation configured
                if (string.IsNullOrWhiteSpace(profile.IsolationGameFolderPath))
                {
                    return new Gw2IsolationLaunchResult
                    {
                        Success = false,
                        ErrorMessage = "Profile does not have IsolationGameFolderPath configured"
                    };
                }

                // Determine exe path (use isolation folder if set, otherwise ExecutablePath)
                string exePath = Path.Combine(profile.IsolationGameFolderPath, "Gw2-64.exe");
                if (!File.Exists(exePath))
                {
                    return new Gw2IsolationLaunchResult
                    {
                        Success = false,
                        ErrorMessage = $"Gw2-64.exe not found in isolation folder: {exePath}"
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

                // Set environment variables for the hook DLL
                var environment = new Dictionary<string, string>
                {
                    ["GW2_REDIRECT_ROAMING"] = roamingPath,
                    ["GW2_REDIRECT_LOCAL"] = localPath,
                    ["GW2_HOOK_LOG"] = Path.Combine(Path.GetTempPath(), "Gw2FolderHook.log")
                };

                // Get working directory from exe path
                string? workingDir = Path.GetDirectoryName(exePath);
                if (string.IsNullOrEmpty(workingDir))
                {
                    workingDir = Environment.CurrentDirectory;
                }

                // Launch with injection
                uint processId = LaunchAndInject(exePath, hookDllPath, workingDir, arguments, environment);

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
        /// Core injection logic: Launch suspended, inject DLL, resume.
        /// Adapted from PoC ProcessInjector.
        /// </summary>
        private uint LaunchAndInject(
            string exePath,
            string dllPath,
            string workingDir,
            string? arguments,
            Dictionary<string, string> environment)
        {
            // Create suspended process
            var processInfo = CreateSuspendedProcess(exePath, workingDir, arguments, environment);

            try
            {
                // Inject DLL
                InjectDll(processInfo.hProcess, dllPath);

                // Resume main thread
                NativeMethods.ResumeThread(processInfo.hThread);

                // Close handles
                NativeMethods.CloseHandle(processInfo.hThread);
                NativeMethods.CloseHandle(processInfo.hProcess);

                return (uint)processInfo.dwProcessId;
            }
            catch
            {
                // If injection fails, terminate the process
                NativeMethods.TerminateProcess(processInfo.hProcess, 1);
                NativeMethods.CloseHandle(processInfo.hThread);
                NativeMethods.CloseHandle(processInfo.hProcess);
                throw;
            }
        }

        private NativeMethods.PROCESS_INFORMATION CreateSuspendedProcess(
            string exePath,
            string workingDir,
            string? arguments,
            Dictionary<string, string> environment)
        {
            var startupInfo = new NativeMethods.STARTUPINFO
            {
                cb = (uint)Marshal.SizeOf<NativeMethods.STARTUPINFO>()
            };

            // Build command line
            string? commandLine = null;
            if (!string.IsNullOrWhiteSpace(arguments))
            {
                commandLine = arguments;
            }

            // Build environment block
            IntPtr environmentPtr = IntPtr.Zero;
            if (environment != null && environment.Count > 0)
            {
                environmentPtr = BuildEnvironmentBlock(environment);
            }

            try
            {
                uint creationFlags = NativeMethods.CREATE_SUSPENDED;
                if (environmentPtr != IntPtr.Zero)
                {
                    creationFlags |= 0x00000400; // CREATE_UNICODE_ENVIRONMENT
                }

                bool success = NativeMethods.CreateProcess(
                    lpApplicationName: exePath,
                    lpCommandLine: commandLine,
                    lpProcessAttributes: IntPtr.Zero,
                    lpThreadAttributes: IntPtr.Zero,
                    bInheritHandles: false,
                    dwCreationFlags: creationFlags,
                    lpEnvironment: environmentPtr,
                    lpCurrentDirectory: workingDir,
                    ref startupInfo,
                    out NativeMethods.PROCESS_INFORMATION processInfo);

                if (!success)
                {
                    int error = Marshal.GetLastWin32Error();
                    throw new InvalidOperationException($"Failed to create process. Error code: {error}");
                }

                return processInfo;
            }
            finally
            {
                if (environmentPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(environmentPtr);
                }
            }
        }

        private void InjectDll(IntPtr processHandle, string dllPath)
        {
            // Get LoadLibraryW address
            IntPtr kernel32 = NativeMethods.GetModuleHandle("kernel32.dll");
            if (kernel32 == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to get kernel32.dll module handle");
            }

            IntPtr loadLibraryAddr = NativeMethods.GetProcAddress(kernel32, "LoadLibraryW");
            if (loadLibraryAddr == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to get LoadLibraryW address");
            }

            // Allocate memory in target process for DLL path
            byte[] dllPathBytes = Encoding.Unicode.GetBytes(dllPath + '\0');
            uint dllPathSize = (uint)dllPathBytes.Length;

            IntPtr remoteMemory = NativeMethods.VirtualAllocEx(
                processHandle,
                IntPtr.Zero,
                dllPathSize,
                NativeMethods.MEM_COMMIT | NativeMethods.MEM_RESERVE,
                NativeMethods.PAGE_READWRITE);

            if (remoteMemory == IntPtr.Zero)
            {
                int error = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"Failed to allocate memory in target process. Error: {error}");
            }

            try
            {
                // Write DLL path to target process
                bool writeSuccess = NativeMethods.WriteProcessMemory(
                    processHandle,
                    remoteMemory,
                    dllPathBytes,
                    dllPathSize,
                    out IntPtr bytesWritten);

                if (!writeSuccess || bytesWritten.ToInt64() != dllPathSize)
                {
                    int error = Marshal.GetLastWin32Error();
                    throw new InvalidOperationException($"Failed to write DLL path to target process. Error: {error}");
                }

                // Create remote thread to call LoadLibraryW
                IntPtr remoteThread = NativeMethods.CreateRemoteThread(
                    processHandle,
                    IntPtr.Zero,
                    0,
                    loadLibraryAddr,
                    remoteMemory,
                    0,
                    out uint threadId);

                if (remoteThread == IntPtr.Zero)
                {
                    int error = Marshal.GetLastWin32Error();
                    throw new InvalidOperationException($"Failed to create remote thread. Error: {error}");
                }

                try
                {
                    // Wait for LoadLibrary to complete
                    uint waitResult = NativeMethods.WaitForSingleObject(remoteThread, 10000); // 10 second timeout
                    if (waitResult != 0) // WAIT_OBJECT_0
                    {
                        throw new InvalidOperationException("LoadLibrary call timed out or failed");
                    }

                    // Get exit code (HMODULE of loaded DLL)
                    if (!NativeMethods.GetExitCodeThread(remoteThread, out uint exitCode))
                    {
                        throw new InvalidOperationException("Failed to get LoadLibrary result");
                    }

                    if (exitCode == 0)
                    {
                        throw new InvalidOperationException("LoadLibrary returned NULL - DLL failed to load");
                    }
                }
                finally
                {
                    NativeMethods.CloseHandle(remoteThread);
                }
            }
            finally
            {
                // Free allocated memory
                NativeMethods.VirtualFreeEx(processHandle, remoteMemory, 0, NativeMethods.MEM_RELEASE);
            }
        }

        private IntPtr BuildEnvironmentBlock(Dictionary<string, string> environment)
        {
            // Build Unicode environment block: "KEY1=VALUE1\0KEY2=VALUE2\0\0"
            var sb = new StringBuilder();

            // Copy current environment
            foreach (System.Collections.DictionaryEntry entry in Environment.GetEnvironmentVariables())
            {
                string key = entry.Key?.ToString() ?? "";
                string value = entry.Value?.ToString() ?? "";

                // Override with custom values if specified
                if (environment.ContainsKey(key))
                {
                    value = environment[key];
                }

                sb.Append($"{key}={value}\0");
            }

            // Add new variables not in current environment
            foreach (var kvp in environment)
            {
                if (!Environment.GetEnvironmentVariables().Contains(kvp.Key))
                {
                    sb.Append($"{kvp.Key}={kvp.Value}\0");
                }
            }

            // Double null terminator
            sb.Append('\0');

            // Convert to Unicode byte array
            byte[] envBytes = Encoding.Unicode.GetBytes(sb.ToString());

            // Allocate unmanaged memory
            IntPtr ptr = Marshal.AllocHGlobal(envBytes.Length);
            Marshal.Copy(envBytes, 0, ptr, envBytes.Length);

            return ptr;
        }
    }
}
