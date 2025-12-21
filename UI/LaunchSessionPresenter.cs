using GWxLauncher.Services;
using System;
using System.Collections.Generic;

namespace GWxLauncher.UI
{
    internal sealed class LaunchSessionPresenter
    {
        private LaunchReport? _lastLaunchReport;
        private readonly List<LaunchReport> _lastLaunchReports = new();

        public bool HasAnyReports => _lastLaunchReports.Count > 0;

        public void BeginSession(bool bulkMode)
        {
            if (!bulkMode)
                _lastLaunchReports.Clear();
        }

        public void Record(LaunchReport report)
        {
            _lastLaunchReport = report;
            _lastLaunchReports.Add(report);
        }

        public string BuildStatusText()
        {
            return _lastLaunchReport?.BuildSummary() ?? "";
        }

        public LaunchReport? LastReport => _lastLaunchReport;

        public IReadOnlyList<LaunchReport> AllReports => _lastLaunchReports;
    }
}
