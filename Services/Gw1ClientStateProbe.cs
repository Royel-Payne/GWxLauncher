using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GWxLauncher.Services
{
    /// <summary>
    /// Read-only GW1 readiness probe for bulk throttling.
    /// "Ready" == character select reached, defined as:
    ///   Read&lt;ushort&gt;(CharnamePtr) != 0
    ///
    /// Probe is optional: if initialization fails, it becomes unavailable and should be treated as non-fatal.
    /// </summary>
    internal sealed class Gw1ClientStateProbe : IDisposable
    {
        // Signature: CharnamePtr (primary readiness signal)
        private static readonly byte[] CharnameSignature =
        {
            0x8B, 0xF8, 0x6A, 0x03, 0x68,
            0x0F, 0x00, 0x00, 0xC0,
            0x8B, 0xCF, 0xE8
        };

        private const int CharnameOffset = -0x42;
        private const bool CharnameRelative = true;

        // Matches existing GW1 multiclient patch scan region used in Gw1InjectionService.
        private const int DefaultImageReadSize = 0x48D000;

        private IntPtr _processHandle = IntPtr.Zero;
        private IntPtr _moduleBase = IntPtr.Zero;

        public bool IsAvailable { get; private set; }
        public IntPtr CharnamePtr { get; private set; } = IntPtr.Zero;
        public string UnavailableReason { get; private set; } = string.Empty;

        public bool TryInitialize(Process gwProcess)
        {
            ResetState();

            if (gwProcess == null)
            {
                MarkUnavailable("Process is null");
                return false;
            }

            if (gwProcess.HasExited)
            {
                MarkUnavailable("Process has exited");
                return false;
            }

            _processHandle = OpenProcess(
                ProcessAccessFlags.PROCESS_QUERY_INFORMATION | ProcessAccessFlags.PROCESS_VM_READ,
                false,
                gwProcess.Id);

            if (_processHandle == IntPtr.Zero)
            {
                MarkUnavailable($"OpenProcess failed. Win32 error: {Marshal.GetLastWin32Error()}");
                return false;
            }

            if (!TryGetImageBaseFromPeb(_processHandle, out _moduleBase, out var pebError))
            {
                MarkUnavailable(pebError);
                return false;
            }

            // Read a fixed region of the image and scan for the signature.
            byte[] image = new byte[DefaultImageReadSize];
            if (!ReadProcessMemory(_processHandle, _moduleBase, image, image.Length, out _))
            {
                MarkUnavailable($"ReadProcessMemory(image) failed. Win32 error: {Marshal.GetLastWin32Error()}");
                return false;
            }

            int sigIndex = IndexOf(image, CharnameSignature);
            if (sigIndex < 0)
            {
                MarkUnavailable("GW1 Charname signature not found in process image");
                return false;
            }

            // Apply offset relative to start of signature.
            int ptrLocInImage = sigIndex + CharnameOffset;
            if (ptrLocInImage < 0 || ptrLocInImage + 4 > image.Length)
            {
                MarkUnavailable("Charname pointer location is out of bounds after applying offset");
                return false;
            }

            // Resolve relative pointer:
            // absolute = (moduleBase + ptrLoc) + 4 + rel32
            int rel32 = BitConverter.ToInt32(image, ptrLocInImage);
            long absolute = _moduleBase.ToInt64() + ptrLocInImage + 4L + rel32;

            CharnamePtr = new IntPtr(absolute);
            if (CharnamePtr == IntPtr.Zero)
            {
                MarkUnavailable("Resolved CharnamePtr is null");
                return false;
            }

            IsAvailable = true;
            return true;
        }

        /// <summary>
        /// Returns true only when probe is available and readiness definition evaluates to true.
        /// </summary>
        public bool IsReady()
        {
            if (!IsAvailable || _processHandle == IntPtr.Zero || CharnamePtr == IntPtr.Zero)
                return false;

            if (!TryReadUInt16(CharnamePtr, out ushort value))
                return false;

            return value != 0;
        }

        public void Dispose()
        {
            ResetState();
        }

        private void ResetState()
        {
            IsAvailable = false;
            CharnamePtr = IntPtr.Zero;
            UnavailableReason = string.Empty;
            _moduleBase = IntPtr.Zero;

            if (_processHandle != IntPtr.Zero)
            {
                CloseHandle(_processHandle);
                _processHandle = IntPtr.Zero;
            }
        }

        private void MarkUnavailable(string reason)
        {
            IsAvailable = false;
            UnavailableReason = reason ?? string.Empty;

            // Keep handles tidy even on failure.
            if (_processHandle != IntPtr.Zero)
            {
                CloseHandle(_processHandle);
                _processHandle = IntPtr.Zero;
            }
        }

        private bool TryReadUInt16(IntPtr address, out ushort value)
        {
            value = 0;

            byte[] buf = new byte[2];
            if (!ReadProcessMemory(_processHandle, address, buf, buf.Length, out _))
                return false;

            value = BitConverter.ToUInt16(buf, 0);
            return true;
        }

        private static int IndexOf(byte[] haystack, byte[] needle)
        {
            if (needle.Length == 0 || haystack.Length < needle.Length)
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

        private static bool TryGetImageBaseFromPeb(IntPtr processHandle, out IntPtr imageBase, out string error)
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
                    error = "Failed to resolve GW1 ImageBaseAddress from PEB.";
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

        #region Win32 interop (read-only)

        [Flags]
        private enum ProcessAccessFlags : uint
        {
            PROCESS_QUERY_INFORMATION = 0x0400,
            PROCESS_VM_READ = 0x0010
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_BASIC_INFORMATION
        {
            public IntPtr Reserved1;
            public IntPtr PebBaseAddress;
            public IntPtr Reserved2_0;
            public IntPtr Reserved2_1;
            public IntPtr UniqueProcessId;
            public IntPtr Reserved3;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PEB_MIN
        {
            public IntPtr Reserved0;
            public IntPtr Reserved1;
            public IntPtr ImageBaseAddress;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle, int processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out] byte[] lpBuffer,
            int dwSize,
            out IntPtr lpNumberOfBytesRead);

        [DllImport("ntdll.dll")]
        private static extern int NtQueryInformationProcess(
            IntPtr processHandle,
            int processInformationClass,
            ref PROCESS_BASIC_INFORMATION processInformation,
            int processInformationLength,
            out int returnLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        #endregion
    }
}
