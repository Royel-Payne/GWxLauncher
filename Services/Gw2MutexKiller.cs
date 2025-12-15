using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace GWxLauncher.Services
{
    internal static class Gw2MutexKiller
    {
        public const string Gw2MutexLeafName = "AN-Mutex-Window-Guild Wars 2";

        public static bool TryKillGw2Mutex(
    out int clearedPid,
    out string detail,
    bool allowElevatedFallback,
    out bool usedElevated)
        {
            clearedPid = -1;
            usedElevated = false;
            detail = "";

            // First attempt: normal (non-elevated) clear
            if (TryKillGw2MutexInternal(out clearedPid, out detail))
                return true;

            if (!allowElevatedFallback)
                return false;

            // If we already ARE admin, no point retrying with elevation.
            if (IsRunningAsAdmin())
                return false;

            // Escalation: run a one-shot elevated helper (same exe) to clear the mutex.
            if (!TryRunElevatedSelf("--gw2-kill-mutex", out string elevateError))
            {
                detail = string.IsNullOrWhiteSpace(detail)
                    ? elevateError
                    : $"{detail} | Elevated retry failed: {elevateError}";
                return false;
            }

            // After elevated helper returns, try one more time (still non-elevated here).
            // If it worked, the mutex handle should be gone now.
            if (TryKillGw2MutexInternal(out clearedPid, out string postDetail))
            {
                usedElevated = true;
                detail = postDetail;
                return true;
            }

            usedElevated = true;
            detail = string.IsNullOrWhiteSpace(detail)
                ? "Elevated retry ran, but mutex still could not be cleared."
                : $"{detail} | Elevated retry ran, but mutex still could not be cleared.";
            return false;
        }

        private static bool TryKillGw2MutexInternal(out int clearedPid, out string detail)
        {
            clearedPid = -1;
            detail = "";

            // GW2 process name is usually Gw2-64; fall back to Gw2
            var procs = Process.GetProcessesByName("Gw2-64");
            if (procs.Length == 0)
                procs = Process.GetProcessesByName("Gw2");

            if (procs.Length == 0)
            {
                detail = "No GW2 process found to clear mutex from.";
                return false;
            }

            foreach (var p in procs)
            {
                try
                {
                    if (TryCloseNamedMutexHandleInProcess(p.Id, Gw2MutexLeafName, out detail))
                    {
                        clearedPid = p.Id;
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    detail = $"Exception while clearing mutex: {ex.Message}";
                }
            }

            if (string.IsNullOrWhiteSpace(detail))
                detail = "Failed to locate/close GW2 mutex handle in GW2 process.";

            return false;
        }

        private static bool IsRunningAsAdmin()
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private static bool TryRunElevatedSelf(string arguments, out string error)
        {
            error = "";

            try
            {
                string exePath =
                    Environment.ProcessPath
                    ?? Process.GetCurrentProcess().MainModule?.FileName
                    ?? "";

                if (string.IsNullOrWhiteSpace(exePath))
                {
                    error = "Could not determine current executable path for elevation.";
                    return false;
                }

                var psi = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = arguments,
                    UseShellExecute = true,
                    Verb = "runas",
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                using var p = Process.Start(psi);
                if (p == null)
                {
                    error = "Failed to start elevated helper process.";
                    return false;
                }

                p.WaitForExit();

                if (p.ExitCode == 0)
                    return true;

                error = $"Elevated helper returned ExitCode={p.ExitCode}.";
                return false;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private static bool TryCloseNamedMutexHandleInProcess(int pid, string mutexLeafName, out string detail)
        {
            detail = "";

            IntPtr hProcess = OpenProcess(ProcessAccessFlags.DupHandle | ProcessAccessFlags.QueryLimitedInformation, false, pid);
            if (hProcess == IntPtr.Zero)
            {
                detail = $"OpenProcess(DUP_HANDLE) failed for PID {pid}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}";
                return false;
            }

            try
            {
                // Query system handles
                int size = 0x10000;
                IntPtr buffer = IntPtr.Zero;

                try
                {
                    int status;
                    do
                    {
                        buffer = Marshal.AllocHGlobal(size);
                        status = NtQuerySystemInformation(SYSTEM_INFORMATION_CLASS.SystemExtendedHandleInformation, buffer, size, out int needed);
                        if (status == STATUS_INFO_LENGTH_MISMATCH)
                        {
                            Marshal.FreeHGlobal(buffer);
                            buffer = IntPtr.Zero;
                            size = Math.Max(size * 2, needed);
                        }
                    }
                    while (status == STATUS_INFO_LENGTH_MISMATCH);

                    if (status != 0)
                    {
                        detail = $"NtQuerySystemInformation failed: 0x{status:X}";
                        return false;
                    }

                    // Parse handle table
                    var info = Marshal.PtrToStructure<SYSTEM_HANDLE_INFORMATION_EX>(buffer);
                    long handleCount = info.NumberOfHandles.ToInt64();
                    IntPtr entryPtr = IntPtr.Add(buffer, Marshal.SizeOf<SYSTEM_HANDLE_INFORMATION_EX>());

                    for (long i = 0; i < handleCount; i++)
                    {
                        var entry = Marshal.PtrToStructure<SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX>(entryPtr);
                        entryPtr = IntPtr.Add(entryPtr, Marshal.SizeOf<SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX>());

                        if (entry.UniqueProcessId.ToInt64() != pid)
                            continue;

                        // Duplicate handle into our process so we can query its name/type
                        if (!DuplicateHandle(hProcess, entry.HandleValue, GetCurrentProcess(), out IntPtr hDup, 0, false, DUPLICATE_SAME_ACCESS))
                            continue;

                        try
                        {
                            // We only care about Mutant objects (mutex)
                            if (!TryGetObjectTypeName(hDup, out var typeName))
                                continue;

                            if (!string.Equals(typeName, "Mutant", StringComparison.OrdinalIgnoreCase))
                                continue;

                            if (!TryGetObjectName(hDup, out var objName))
                                continue;

                            // Object names may be reported as:
                            // \Sessions\X\BaseNamedObjects\NAME
                            // \BaseNamedObjects\NAME
                            // NAME
                            bool isMatch =
                                objName.EndsWith("\\" + mutexLeafName, StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(objName, mutexLeafName, StringComparison.OrdinalIgnoreCase);

                            if (isMatch)
                            {
                                if (!DuplicateHandle(hProcess, entry.HandleValue, IntPtr.Zero, out _, 0, false, DUPLICATE_CLOSE_SOURCE))
                                {
                                    detail = $"Found GW2 mutex but failed to close it: {new Win32Exception(Marshal.GetLastWin32Error()).Message}";
                                    return false;
                                }

                                detail = $"Closed GW2 mutex handle in PID {pid}.";
                                return true;
                            }
                        }
                        finally
                        {
                            CloseHandle(hDup);
                        }
                    }

                    detail = $"Did not find GW2 mutex handle in PID {pid}.";
                    return false;
                }
                finally
                {
                    if (buffer != IntPtr.Zero)
                        Marshal.FreeHGlobal(buffer);
                }
            }
            finally
            {
                CloseHandle(hProcess);
            }
        }

        private static bool TryGetObjectName(IntPtr hObject, out string name)
        {
            name = "";
            return QueryObjectString(hObject, OBJECT_INFORMATION_CLASS.ObjectNameInformation, out name);
        }

        private static bool TryGetObjectTypeName(IntPtr hObject, out string typeName)
        {
            typeName = "";
            return QueryObjectString(hObject, OBJECT_INFORMATION_CLASS.ObjectTypeInformation, out typeName);
        }

        private static bool QueryObjectString(IntPtr hObject, OBJECT_INFORMATION_CLASS infoClass, out string result)
        {
            result = "";
            int size = 0x1000;

            for (int attempt = 0; attempt < 6; attempt++)
            {
                IntPtr buffer = Marshal.AllocHGlobal(size);
                try
                {
                    int status = NtQueryObject(hObject, infoClass, buffer, size, out int needed);
                    if (status == STATUS_INFO_LENGTH_MISMATCH)
                    {
                        size = Math.Max(size * 2, needed);
                        continue;
                    }

                    if (status != 0)
                        return false;

                    // For both name + type, the first field is a UNICODE_STRING.
                    var uni = Marshal.PtrToStructure<UNICODE_STRING>(buffer);
                    if (uni.Buffer == IntPtr.Zero || uni.Length == 0)
                        return false;

                    result = Marshal.PtrToStringUni(uni.Buffer, uni.Length / 2) ?? "";
                    return !string.IsNullOrWhiteSpace(result);
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                }
            }

            return false;
        }

        private const int STATUS_INFO_LENGTH_MISMATCH = unchecked((int)0xC0000004);
        private const uint DUPLICATE_SAME_ACCESS = 0x00000002;
        private const uint DUPLICATE_CLOSE_SOURCE = 0x00000001;

        private enum SYSTEM_INFORMATION_CLASS
        {
            SystemExtendedHandleInformation = 64
        }

        private enum OBJECT_INFORMATION_CLASS
        {
            ObjectNameInformation = 1,
            ObjectTypeInformation = 2
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SYSTEM_HANDLE_INFORMATION_EX
        {
            public IntPtr NumberOfHandles;
            public IntPtr Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX
        {
            public IntPtr Object;
            public IntPtr UniqueProcessId;
            public IntPtr HandleValue;
            public uint GrantedAccess;
            public ushort CreatorBackTraceIndex;
            public ushort ObjectTypeIndex;
            public uint HandleAttributes;
            public uint Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct UNICODE_STRING
        {
            public ushort Length;
            public ushort MaximumLength;
            public IntPtr Buffer;
        }

        [Flags]
        private enum ProcessAccessFlags : uint
        {
            QueryLimitedInformation = 0x1000,
            DupHandle = 0x0040
        }

        [DllImport("ntdll.dll")]
        private static extern int NtQuerySystemInformation(
            SYSTEM_INFORMATION_CLASS systemInformationClass,
            IntPtr systemInformation,
            int systemInformationLength,
            out int returnLength);

        [DllImport("ntdll.dll")]
        private static extern int NtQueryObject(
            IntPtr handle,
            OBJECT_INFORMATION_CLASS objectInformationClass,
            IntPtr objectInformation,
            int objectInformationLength,
            out int returnLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(ProcessAccessFlags access, bool inherit, int processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool DuplicateHandle(
            IntPtr hSourceProcessHandle,
            IntPtr hSourceHandle,
            IntPtr hTargetProcessHandle,
            out IntPtr lpTargetHandle,
            uint dwDesiredAccess,
            bool bInheritHandle,
            uint dwOptions);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);
    }
}
