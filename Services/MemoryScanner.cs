using System.Runtime.InteropServices;
using static GWxLauncher.Services.NativeMethods;

namespace GWxLauncher.Services
{
    internal static class MemoryScanner
    {
        public static int IndexOf(byte[] haystack, byte[] needle)
        {
            if (needle == null || needle.Length == 0 || haystack == null || haystack.Length < needle.Length)
                return -1;

            for (int i = 0; i <= haystack.Length - needle.Length; i++)
            {
                bool match = true;

                for (int j = 0; j < needle.Length; j++)
                {
                    if (haystack[i + j] != needle[j])
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                    return i;
            }

            return -1;
        }

        public static bool TryGetImageBaseFromPeb(IntPtr processHandle, out IntPtr imageBase, out string error)
        {
            imageBase = IntPtr.Zero;
            error = string.Empty;

            var pbi = new PROCESS_BASIC_INFORMATION();
            int retLen;

            int nt = NtQueryInformationProcess(
                processHandle,
                0, // ProcessBasicInformation
                ref pbi,
                Marshal.SizeOf<PROCESS_BASIC_INFORMATION>(),
                out retLen);

            if (nt != 0 || pbi.PebBaseAddress == IntPtr.Zero)
            {
                error = $"NtQueryInformationProcess failed (status={nt}).";
                return false;
            }

            byte[] pebBuf = new byte[Marshal.SizeOf<PEB_MIN>()];
            if (!ReadProcessMemory(processHandle, pbi.PebBaseAddress, pebBuf, pebBuf.Length, out _))
            {
                error = $"ReadProcessMemory(PEB) failed. Win32 error: {Marshal.GetLastWin32Error()}";
                return false;
            }

            var handle = GCHandle.Alloc(pebBuf, GCHandleType.Pinned);
            try
            {
                var peb = Marshal.PtrToStructure<PEB_MIN>(handle.AddrOfPinnedObject());
                if (peb.ImageBaseAddress == IntPtr.Zero)
                {
                    error = "Failed to resolve ImageBaseAddress from PEB.";
                    return false;
                }

                imageBase = peb.ImageBaseAddress;
                return true;
            }
            finally
            {
                handle.Free();
            }
        }
    }
}
