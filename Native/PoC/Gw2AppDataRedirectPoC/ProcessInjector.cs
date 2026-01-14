using System.Runtime.InteropServices;
using System.Text;

namespace Gw2AppDataRedirectPoC;

/// <summary>
/// Handles DLL injection into a suspended process using the classic CreateRemoteThread + LoadLibrary technique.
/// </summary>
public class ProcessInjector
{
    private readonly string _targetExecutable;
    private readonly string _dllPath;
    private readonly string _workingDirectory;

    public ProcessInjector(string targetExecutable, string dllPath, string workingDirectory)
    {
        _targetExecutable = targetExecutable ?? throw new ArgumentNullException(nameof(targetExecutable));
        _dllPath = dllPath ?? throw new ArgumentNullException(nameof(dllPath));
        _workingDirectory = workingDirectory ?? throw new ArgumentNullException(nameof(workingDirectory));

        if (!File.Exists(_targetExecutable))
            throw new FileNotFoundException($"Target executable not found: {_targetExecutable}");

        if (!File.Exists(_dllPath))
            throw new FileNotFoundException($"DLL not found: {_dllPath}");
    }

    /// <summary>
    /// Launches the target process in suspended mode, injects the DLL, and resumes execution.
    /// </summary>
    /// <param name="arguments">Command line arguments to pass to the target executable.</param>
    /// <param name="environment">Environment variables to pass to injected DLL (optional).</param>
    /// <returns>Process ID of the launched process.</returns>
    public uint LaunchAndInject(string? arguments = null, Dictionary<string, string>? environment = null)
    {
        Console.WriteLine($"[Injector] Target: {_targetExecutable}");
        Console.WriteLine($"[Injector] DLL: {_dllPath}");
        Console.WriteLine($"[Injector] Working Directory: {_workingDirectory}");
        Console.WriteLine();

        // Step 1: Create the process in suspended mode
        Console.WriteLine("[1/5] Creating suspended process...");
        var processInfo = CreateSuspendedProcess(arguments, environment);
        Console.WriteLine($"      ? Process created (PID: {processInfo.dwProcessId})");
        Console.WriteLine();

        try
        {
            // Step 2: Inject the DLL
            Console.WriteLine("[2/5] Injecting DLL into process...");
            InjectDll(processInfo.hProcess);
            Console.WriteLine("      ? DLL injected successfully");
            Console.WriteLine();

            // Step 3: Resume the main thread
            Console.WriteLine("[3/5] Resuming main thread...");
            uint suspendCount = NativeMethods.ResumeThread(processInfo.hThread);
            if (suspendCount == unchecked((uint)-1))
            {
                throw new InvalidOperationException($"Failed to resume thread. Error: {Marshal.GetLastWin32Error()}");
            }
            Console.WriteLine($"      ? Thread resumed (previous suspend count: {suspendCount})");
            Console.WriteLine();

            // Step 4: Verify process is running
            Console.WriteLine("[4/5] Verifying process state...");
            Thread.Sleep(500); // Give it a moment to initialize
            
            // Check if process is still running (didn't crash immediately)
            uint exitCode;
            if (NativeMethods.GetExitCodeThread(processInfo.hThread, out exitCode))
            {
                if (exitCode == 259) // STILL_ACTIVE
                {
                    Console.WriteLine("      ? Process is running");
                }
                else
                {
                    Console.WriteLine($"      ? Process exited with code: {exitCode}");
                }
            }
            Console.WriteLine();

            Console.WriteLine("[5/5] Cleanup...");
            return processInfo.dwProcessId;
        }
        finally
        {
            // Cleanup handles
            NativeMethods.CloseHandle(processInfo.hThread);
            NativeMethods.CloseHandle(processInfo.hProcess);
            Console.WriteLine("      ? Handles closed");
        }
    }

    private NativeMethods.PROCESS_INFORMATION CreateSuspendedProcess(string? arguments, Dictionary<string, string>? environment)
    {
        var startupInfo = new NativeMethods.STARTUPINFO
        {
            cb = (uint)Marshal.SizeOf<NativeMethods.STARTUPINFO>()
        };

        // When using lpApplicationName, lpCommandLine can be null or just contain arguments
        // This is more reliable for paths with spaces
        string? commandLine = null;
        if (!string.IsNullOrWhiteSpace(arguments))
        {
            commandLine = arguments;
        }

        // Build environment block if provided
        IntPtr environmentPtr = IntPtr.Zero;
        if (environment != null && environment.Count > 0)
        {
            environmentPtr = BuildEnvironmentBlock(environment);
        }

        try
        {
            // If we have environment variables, we need to add CREATE_UNICODE_ENVIRONMENT flag
            uint creationFlags = NativeMethods.CREATE_SUSPENDED;
            if (environmentPtr != IntPtr.Zero)
            {
                creationFlags |= 0x00000400; // CREATE_UNICODE_ENVIRONMENT
            }

            bool success = NativeMethods.CreateProcess(
                lpApplicationName: _targetExecutable,  // Pass executable path separately
                lpCommandLine: commandLine,            // Just arguments (or null)
                lpProcessAttributes: IntPtr.Zero,
                lpThreadAttributes: IntPtr.Zero,
                bInheritHandles: false,
                dwCreationFlags: creationFlags,
                lpEnvironment: environmentPtr,
                lpCurrentDirectory: _workingDirectory,
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

    private void InjectDll(IntPtr processHandle)
    {
        // Get the address of LoadLibraryW in kernel32.dll
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

        Console.WriteLine($"      LoadLibraryW address: 0x{loadLibraryAddr.ToInt64():X}");

        // Allocate memory in the target process for the DLL path
        byte[] dllPathBytes = Encoding.Unicode.GetBytes(_dllPath + '\0');
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

        Console.WriteLine($"      Allocated {dllPathSize} bytes at 0x{remoteMemory.ToInt64():X}");

        try
        {
            // Write the DLL path to the allocated memory
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

            Console.WriteLine($"      Wrote DLL path ({bytesWritten} bytes)");

            // Create a remote thread that calls LoadLibraryW with the DLL path
            IntPtr remoteThread = NativeMethods.CreateRemoteThread(
                processHandle,
                IntPtr.Zero,
                0,
                loadLibraryAddr,
                remoteMemory,
                0,
                out IntPtr threadId);

            if (remoteThread == IntPtr.Zero)
            {
                int error = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"Failed to create remote thread. Error: {error}");
            }

            Console.WriteLine($"      Created remote thread (TID: {threadId})");

            try
            {
                // Wait for LoadLibrary to complete
                uint waitResult = NativeMethods.WaitForSingleObject(remoteThread, 5000); // 5 second timeout
                
                if (waitResult == 0) // WAIT_OBJECT_0
                {
                    // Check the return value of LoadLibrary
                    if (NativeMethods.GetExitCodeThread(remoteThread, out uint exitCode))
                    {
                        if (exitCode == 0)
                        {
                            throw new InvalidOperationException("LoadLibrary returned NULL - DLL failed to load. Check if DLL is 64-bit and dependencies are available.");
                        }
                        else
                        {
                            Console.WriteLine($"      LoadLibrary returned: 0x{exitCode:X} (DLL module handle)");
                        }
                    }
                }
                else if (waitResult == 0x102) // WAIT_TIMEOUT
                {
                    throw new TimeoutException("LoadLibrary call timed out after 5 seconds");
                }
                else
                {
                    throw new InvalidOperationException($"Wait failed with result: {waitResult}");
                }
            }
            finally
            {
                NativeMethods.CloseHandle(remoteThread);
            }
        }
        finally
        {
            // Free the allocated memory
            NativeMethods.VirtualFreeEx(processHandle, remoteMemory, 0, NativeMethods.MEM_RELEASE);
        }
    }

    private IntPtr BuildEnvironmentBlock(Dictionary<string, string> environment)
    {
        // Environment block format: "KEY1=VALUE1\0KEY2=VALUE2\0\0"
        var sb = new StringBuilder();
        
        // Copy current environment
        foreach (System.Collections.DictionaryEntry entry in Environment.GetEnvironmentVariables())
        {
            string key = entry.Key?.ToString() ?? "";
            string value = entry.Value?.ToString() ?? "";
            
            // Override with our custom values if specified
            if (environment.ContainsKey(key))
            {
                value = environment[key];
            }
            
            sb.Append($"{key}={value}\0");
        }
        
        // Add any new variables
        foreach (var kvp in environment)
        {
            if (!Environment.GetEnvironmentVariables().Contains(kvp.Key))
            {
                sb.Append($"{kvp.Key}={kvp.Value}\0");
            }
        }
        
        // Double null terminator
        sb.Append('\0');
        
        // Convert to Unicode and copy to unmanaged memory
        byte[] envBytes = Encoding.Unicode.GetBytes(sb.ToString());
        IntPtr envPtr = Marshal.AllocHGlobal(envBytes.Length);
        Marshal.Copy(envBytes, 0, envPtr, envBytes.Length);
        
        return envPtr;
    }
}
