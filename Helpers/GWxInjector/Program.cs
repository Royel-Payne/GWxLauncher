using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace GWxInjector;

/// <summary>
/// x64 helper process for injecting 64-bit DLLs into 64-bit game processes.
/// Required because a 32-bit launcher cannot inject into 64-bit processes.
/// </summary>
class Program
{
    static int Main(string[] args)
    {
        if (args.Length < 3)
        {
            Console.Error.WriteLine("Usage: GWxInjector.exe <exePath> <dllPath> <roamingPath> <localPath> [gameArguments]");
            Console.Error.WriteLine();
            Console.Error.WriteLine("Example:");
            Console.Error.WriteLine("  GWxInjector.exe \"C:\\Games\\GW2\\Gw2-64.exe\" \"C:\\Launcher\\Hook.dll\" \"C:\\Profiles\\P1\\Roaming\" \"C:\\Profiles\\P1\\Local\"");
            return 1;
        }

        string exePath = args[0];
        string dllPath = args[1];
        string roamingPath = args[2];
        string localPath = args[3];
        string? gameArgs = args.Length > 4 ? string.Join(" ", args.Skip(4)) : null;

        try
        {
            // Validate inputs
            if (!File.Exists(exePath))
            {
                Console.Error.WriteLine($"ERROR:Executable not found: {exePath}");
                return 2;
            }

            if (!File.Exists(dllPath))
            {
                Console.Error.WriteLine($"ERROR:DLL not found: {dllPath}");
                return 3;
            }

            // Prepare environment variables for the hook DLL
            var environment = new Dictionary<string, string>
            {
                ["GW2_REDIRECT_ROAMING"] = roamingPath,
                ["GW2_REDIRECT_LOCAL"] = localPath,
                ["GW2_HOOK_LOG"] = Path.Combine(Path.GetTempPath(), "Gw2FolderHook.log")
            };

            string workingDir = Path.GetDirectoryName(exePath) ?? Environment.CurrentDirectory;

            // Perform injection
            uint processId = LaunchAndInject(exePath, dllPath, workingDir, gameArgs, environment);

            // Output success with PID
            Console.WriteLine($"SUCCESS:{processId}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"ERROR:{ex.Message}");
            return 99;
        }
    }

    private static uint LaunchAndInject(
        string exePath,
        string dllPath,
        string workingDir,
        string? arguments,
        Dictionary<string, string> environment)
    {
        var processInfo = CreateSuspendedProcess(exePath, workingDir, arguments, environment);

        try
        {
            InjectDll(processInfo.hProcess, dllPath);
            NativeMethods.ResumeThread(processInfo.hThread);
            NativeMethods.CloseHandle(processInfo.hThread);
            NativeMethods.CloseHandle(processInfo.hProcess);

            return (uint)processInfo.dwProcessId;
        }
        catch
        {
            NativeMethods.TerminateProcess(processInfo.hProcess, 1);
            NativeMethods.CloseHandle(processInfo.hThread);
            NativeMethods.CloseHandle(processInfo.hProcess);
            throw;
        }
    }

    private static NativeMethods.PROCESS_INFORMATION CreateSuspendedProcess(
        string exePath,
        string workingDir,
        string? arguments,
        Dictionary<string, string> environment)
    {
        var startupInfo = new NativeMethods.STARTUPINFO
        {
            cb = (uint)Marshal.SizeOf<NativeMethods.STARTUPINFO>()
        };

        IntPtr environmentPtr = BuildEnvironmentBlock(environment);

        try
        {
            uint creationFlags = NativeMethods.CREATE_SUSPENDED | 0x00000400; // CREATE_UNICODE_ENVIRONMENT

            bool success = NativeMethods.CreateProcess(
                lpApplicationName: exePath,
                lpCommandLine: arguments,
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
                Marshal.FreeHGlobal(environmentPtr);
        }
    }

    private static void InjectDll(IntPtr processHandle, string dllPath)
    {
        IntPtr kernel32 = NativeMethods.GetModuleHandle("kernel32.dll");
        if (kernel32 == IntPtr.Zero)
            throw new InvalidOperationException("Failed to get kernel32.dll handle");

        IntPtr loadLibraryAddr = NativeMethods.GetProcAddress(kernel32, "LoadLibraryW");
        if (loadLibraryAddr == IntPtr.Zero)
            throw new InvalidOperationException("Failed to get LoadLibraryW address");

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
            throw new InvalidOperationException($"Failed to allocate memory. Error: {error}");
        }

        try
        {
            bool writeSuccess = NativeMethods.WriteProcessMemory(
                processHandle,
                remoteMemory,
                dllPathBytes,
                dllPathSize,
                out IntPtr bytesWritten);

            if (!writeSuccess || bytesWritten.ToInt64() != dllPathSize)
            {
                int error = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"Failed to write DLL path. Error: {error}");
            }

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
                uint waitResult = NativeMethods.WaitForSingleObject(remoteThread, 10000);
                if (waitResult != 0)
                    throw new InvalidOperationException("LoadLibrary call timed out or failed");

                if (!NativeMethods.GetExitCodeThread(remoteThread, out uint exitCode))
                    throw new InvalidOperationException("Failed to get LoadLibrary result");

                if (exitCode == 0)
                    throw new InvalidOperationException("LoadLibrary returned NULL - DLL failed to load");
            }
            finally
            {
                NativeMethods.CloseHandle(remoteThread);
            }
        }
        finally
        {
            NativeMethods.VirtualFreeEx(processHandle, remoteMemory, 0, NativeMethods.MEM_RELEASE);
        }
    }

    private static IntPtr BuildEnvironmentBlock(Dictionary<string, string> environment)
    {
        var sb = new StringBuilder();

        foreach (System.Collections.DictionaryEntry entry in Environment.GetEnvironmentVariables())
        {
            string key = entry.Key?.ToString() ?? "";
            string value = entry.Value?.ToString() ?? "";

            if (environment.ContainsKey(key))
                value = environment[key];

            sb.Append($"{key}={value}\0");
        }

        foreach (var kvp in environment)
        {
            if (!Environment.GetEnvironmentVariables().Contains(kvp.Key))
                sb.Append($"{kvp.Key}={kvp.Value}\0");
        }

        sb.Append('\0');

        byte[] envBytes = Encoding.Unicode.GetBytes(sb.ToString());
        IntPtr ptr = Marshal.AllocHGlobal(envBytes.Length);
        Marshal.Copy(envBytes, 0, ptr, envBytes.Length);

        return ptr;
    }

    private static class NativeMethods
    {
        internal const uint MEM_COMMIT = 0x1000;
        internal const uint MEM_RESERVE = 0x2000;
        internal const uint MEM_RELEASE = 0x8000;
        internal const uint PAGE_READWRITE = 0x04;
        internal const uint CREATE_SUSPENDED = 0x00000004;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct STARTUPINFO
        {
            public uint cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr VirtualAllocEx(
            IntPtr hProcess,
            IntPtr lpAddress,
            uint dwSize,
            uint flAllocationType,
            uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool VirtualFreeEx(
            IntPtr hProcess,
            IntPtr lpAddress,
            int dwSize,
            uint dwFreeType);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            uint nSize,
            out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr CreateRemoteThread(
            IntPtr hProcess,
            IntPtr lpThreadAttributes,
            uint dwStackSize,
            IntPtr lpStartAddress,
            IntPtr lpParameter,
            uint dwCreationFlags,
            out uint lpThreadId);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        internal static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool CreateProcess(
            string? lpApplicationName,
            string? lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string? lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern uint ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool GetExitCodeThread(IntPtr hThread, out uint lpExitCode);
    }
}
