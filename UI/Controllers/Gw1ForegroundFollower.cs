using GWxLauncher.Services;
using static GWxLauncher.Services.NativeMethods;

namespace GWxLauncher.UI.Controllers
{
    /// <summary>
    /// GW1-only: polls the active (foreground) window and selects the matching profile
    /// if the process ID is tracked by Gw1InstanceTracker.
    /// Does not steal focus or bring the launcher to front.
    /// </summary>
    internal sealed class Gw1ForegroundFollower : IDisposable
    {
        private readonly Gw1InstanceTracker _tracker;
        private readonly Func<string?> _getSelectedProfileId;
        private readonly Action<string> _selectProfile;

        private readonly System.Windows.Forms.Timer _timer = new System.Windows.Forms.Timer();
        private int _lastForegroundPid;

        public Gw1ForegroundFollower(
            Gw1InstanceTracker tracker,
            Func<string?> getSelectedProfileId,
            Action<string> selectProfile,
            int intervalMs = 300)
        {
            _tracker = tracker ?? throw new ArgumentNullException(nameof(tracker));
            _getSelectedProfileId = getSelectedProfileId ?? throw new ArgumentNullException(nameof(getSelectedProfileId));
            _selectProfile = selectProfile ?? throw new ArgumentNullException(nameof(selectProfile));

            _timer.Interval = Math.Max(100, intervalMs);
            _timer.Tick += (_, __) => Tick();
        }

        public void Start() => _timer.Start();

        public void Stop() => _timer.Stop();

        private void Tick()
        {
            int pid = TryGetForegroundProcessId();
            if (pid <= 0)
                return;

            // Avoid repeated work if foreground pid didn't change.
            if (pid == _lastForegroundPid)
                return;

            _lastForegroundPid = pid;

            if (_tracker.TryGetProfileIdByProcessId(pid, out var profileId))
            {
                var current = _getSelectedProfileId();
                if (!string.Equals(current, profileId, StringComparison.OrdinalIgnoreCase))
                    _selectProfile(profileId);
            }
        }

        private static int TryGetForegroundProcessId()
        {
            IntPtr hwnd = GetForegroundWindow();
            if (hwnd == IntPtr.Zero)
                return 0;

            _ = GetWindowThreadProcessId(hwnd, out uint pid);
            return (int)pid;
        }

        public void Dispose()
        {
            try
            {
                Stop();
                _timer.Dispose();
            }
            catch
            {
                // best-effort
            }
        }
    }
}
