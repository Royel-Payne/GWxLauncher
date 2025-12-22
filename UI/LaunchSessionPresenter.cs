using GWxLauncher.Services;

namespace GWxLauncher.UI
{
    internal sealed class LaunchSessionPresenter
    {
        private LaunchReport? _lastLaunchReport;
        private readonly List<LaunchReport> _lastLaunchReports = new();

        public bool HasAnyReports => _lastLaunchReports.Count > 0;

        public void BeginSession(bool bulkMode)
        {
            _lastLaunchReports.Clear();
            _lastLaunchReport = null;
        }

        public void Record(LaunchReport report)
        {
            _lastLaunchReport = report;
            _lastLaunchReports.Add(report);
        }

        public string BuildStatusText()
        {
            if (_lastLaunchReports.Count == 0)
                return "";

            if (_lastLaunchReports.Count == 1)
                return _lastLaunchReports[0].BuildSummary();

            // Bulk session: show attempt count + last attempt summary (no dependency on a Success property).
            return $"Bulk launch: {_lastLaunchReports.Count} attempts • Last: {_lastLaunchReport?.BuildSummary() ?? ""}";
        }

        public LaunchReport? LastReport => _lastLaunchReport;

        public IReadOnlyList<LaunchReport> AllReports => _lastLaunchReports;
    }
}
