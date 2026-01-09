using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using GWxLauncher.Domain;

namespace GWxLauncher.Services
{
    internal sealed class Gw1InstanceTracker
    {
        private readonly object _gate = new();
        private readonly Dictionary<string, TrackedInstance> _instancesByProfileId = new(StringComparer.OrdinalIgnoreCase);

        public event EventHandler? RunStateChanged;

        public bool IsRunning(string profileId)
        {
            if (string.IsNullOrWhiteSpace(profileId))
                return false;

            lock (_gate)
            {
                return _instancesByProfileId.TryGetValue(profileId, out var inst) && inst.IsAlive();
            }
        }

        public bool TryGetProcessId(string profileId, out int processId)
        {
            processId = 0;
            if (string.IsNullOrWhiteSpace(profileId))
                return false;

            lock (_gate)
            {
                if (!_instancesByProfileId.TryGetValue(profileId, out var inst) || !inst.IsAlive())
                    return false;

                processId = inst.ProcessId;
                return true;
            }
        }
        public bool TryGetProfileIdByProcessId(int processId, out string profileId)
        {
            profileId = "";

            if (processId <= 0)
                return false;

            lock (_gate)
            {
                foreach (var kvp in _instancesByProfileId)
                {
                    var inst = kvp.Value;
                    if (inst.ProcessId == processId && inst.IsAlive())
                    {
                        profileId = kvp.Key;
                        return true;
                    }
                }
            }

            return false;
        }
        public bool TryGetProcessIdByProfileId(string profileId, out int processId)
        {
            processId = 0;

            if (string.IsNullOrWhiteSpace(profileId))
                return false;

            lock (_gate)
            {
                if (_instancesByProfileId.TryGetValue(profileId, out var inst) && inst.IsAlive())
                {
                    processId = inst.ProcessId;
                    return true;
                }
            }

            return false;
        }

        public void TrackLaunched(string profileId, Process process)
        {
            if (string.IsNullOrWhiteSpace(profileId))
                return;
            if (process == null)
                return;

            lock (_gate)
            {
                _instancesByProfileId[profileId] = new TrackedInstance(profileId, process);
            }

            HookExit(process);
            RaiseRunStateChanged();
        }

        public void Clear(string profileId)
        {
            if (string.IsNullOrWhiteSpace(profileId))
                return;

            lock (_gate)
            {
                _instancesByProfileId.Remove(profileId);
            }

            RaiseRunStateChanged();
        }

        /// <summary>
        /// Best-effort rehydrate on launcher startup: attempts to map currently-running GW1 processes
        /// back to profiles. Conservative but will "best-guess" if multiple profiles share the same exe path.
        /// </summary>
        public void RehydrateFromRunningProcesses(IEnumerable<GameProfile> profiles)
        {
            if (profiles == null)
                return;

            var gw1Profiles = profiles
                .Where(p => p.GameType == GameType.GuildWars1)
                .Where(p => !string.IsNullOrWhiteSpace(p.ExecutablePath))
                .ToList();

            if (gw1Profiles.Count == 0)
                return;

            var profilesByExe = gw1Profiles
                .GroupBy(p => NormalizePath(p.ExecutablePath))
                .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

            lock (_gate) _instancesByProfileId.Clear();

            foreach (var kvp in profilesByExe)
            {
                var exePath = kvp.Key;
                var groupProfiles = kvp.Value;

                if (string.IsNullOrWhiteSpace(exePath) || !File.Exists(exePath))
                    continue;

                var procs = FindProcessesByExactPath(exePath);
                if (procs.Count == 0)
                    continue;

                if (groupProfiles.Count == 1)
                {
                    var p = procs.FirstOrDefault(pr => !pr.HasExited);
                    if (p != null)
                        TrackLaunched(groupProfiles[0].Id, p);

                    continue;
                }

                var unusedProfiles = new List<GameProfile>(groupProfiles);
                foreach (var proc in procs.Where(pr => !pr.HasExited))
                {
                    GameProfile? match = null;

                    foreach (var candidate in unusedProfiles)
                    {
                        var title = WindowTitleService.TryGetMainWindowTitle(proc);
                        if (!string.IsNullOrWhiteSpace(title) &&
                            title.IndexOf(candidate.Name ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            match = candidate;
                            break;
                        }
                    }

                    match ??= unusedProfiles.FirstOrDefault();
                    if (match == null)
                        break;

                    TrackLaunched(match.Id, proc);
                    unusedProfiles.Remove(match);
                    if (unusedProfiles.Count == 0)
                        break;
                }
            }
        }

        internal static string NormalizePath(string path)
        {
            try { return Path.GetFullPath(path.Trim()); }
            catch { return (path ?? "").Trim(); }
        }

        internal static List<Process> FindProcessesByExactPath(string exePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(exePath);
            var matches = new List<Process>();

            foreach (var p in Process.GetProcessesByName(fileName))
            {
                try
                {
                    if (p.HasExited) continue;

                    var modulePath = p.MainModule?.FileName;
                    if (!string.IsNullOrWhiteSpace(modulePath) &&
                        string.Equals(NormalizePath(modulePath), exePath, StringComparison.OrdinalIgnoreCase))
                    {
                        matches.Add(p);
                    }
                }
                catch
                {
                    // ignore
                }
            }

            return matches;
        }

        private void HookExit(Process process)
        {
            try
            {
                process.EnableRaisingEvents = true;
                process.Exited -= Process_Exited;
                process.Exited += Process_Exited;
            }
            catch
            {
                // best-effort
            }
        }

        private void Process_Exited(object? sender, EventArgs e)
        {
            bool changed = false;

            lock (_gate)
            {
                var dead = _instancesByProfileId
                    .Where(kvp => !kvp.Value.IsAlive())
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var id in dead)
                {
                    _instancesByProfileId.Remove(id);
                    changed = true;
                }
            }

            if (changed)
                RaiseRunStateChanged();
        }

        private void RaiseRunStateChanged()
            => RunStateChanged?.Invoke(this, EventArgs.Empty);

        private sealed class TrackedInstance
        {
            public string ProfileId { get; }
            public int ProcessId { get; }
            private readonly Process _process;

            public TrackedInstance(string profileId, Process process)
            {
                ProfileId = profileId;
                _process = process;
                ProcessId = process.Id;
            }

            public bool IsAlive()
            {
                try { return !_process.HasExited; }
                catch { return false; }
            }
        }
    }
}
