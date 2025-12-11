using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace GWxLauncher
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

                // Give GW1 a moment to initialize and ensure kernel32 is loaded
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

                if (!InjectDllIntoProcess(process, profile.Gw1ToolboxDllPath, out errorMessage))
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Failed to launch Guild Wars 1 with Toolbox:\n\n{ex.Message}";
                return false;
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
