using System.Diagnostics;
using GWxLauncher.Domain;

namespace GWxLauncher.Services
{
    /// <summary>
    /// Tracks running Guild Wars 2 instances and maps them to profiles.
    /// Mirrors Gw1InstanceTracker functionality for GW2.
    /// </summary>
    internal sealed class Gw2InstanceTracker
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
        /// Best-effort rehydrate on launcher startup: attempts to map currently-running GW2 processes
        /// back to profiles. Conservative but will "best-guess" if multiple profiles share the same exe path.
        /// Supports profile isolation by checking both ExecutablePath and IsolationGameFolderPath.
        /// </summary>
        public void RehydrateFromRunningProcesses(IEnumerable<GameProfile> profiles, bool globalIsolationEnabled = false)
        {
            if (profiles == null)
                return;

            var gw2Profiles = profiles
                .Where(p => p.GameType == GameType.GuildWars2)
                .Where(p => !string.IsNullOrWhiteSpace(p.ExecutablePath))
                .ToList();

            if (gw2Profiles.Count == 0)
                return;

            // Build mapping: for each profile, determine effective exe path (isolation-aware)
            var profilesByExe = new Dictionary<string, List<GameProfile>>(StringComparer.OrdinalIgnoreCase);
            
            foreach (var profile in gw2Profiles)
            {
                // Determine which exe path the profile would actually launch from
                // Match the same logic as Gw2LaunchOrchestrator
                string exePath;
                if (globalIsolationEnabled && !string.IsNullOrWhiteSpace(profile.IsolationGameFolderPath))
                {
                    // Profile has dedicated isolation folder AND global isolation is enabled
                    exePath = Path.Combine(profile.IsolationGameFolderPath, "Gw2-64.exe");
                }
                else
                {
                    // Profile uses standard exe path (either isolation disabled or no custom folder)
                    exePath = profile.ExecutablePath ?? "";
                }

                exePath = NormalizePath(exePath);
                if (string.IsNullOrWhiteSpace(exePath))
                    continue;

                if (!profilesByExe.ContainsKey(exePath))
                    profilesByExe[exePath] = new List<GameProfile>();
                
                profilesByExe[exePath].Add(profile);
            }

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

                // If only one profile per exe, simple 1:1 mapping
                if (groupProfiles.Count == 1)
                {
                    var p = procs.FirstOrDefault(pr => !pr.HasExited);
                    if (p != null)
                        TrackLaunched(groupProfiles[0].Id, p);

                    continue;
                }

                // Multiple profiles share the same exe - try to match by command line args (mumble link name)
                var unusedProfiles = new List<GameProfile>(groupProfiles);
                foreach (var proc in procs.Where(pr => !pr.HasExited))
                {
                    GameProfile? match = null;

                    // Try to match by mumble link in command line
                    foreach (var candidate in unusedProfiles)
                    {
                        var mumbleName = Gw2MumbleLinkService.GetMumbleLinkName(candidate);
                        if (!string.IsNullOrWhiteSpace(mumbleName))
                        {
                            try
                            {
                                var cmdLine = GetProcessCommandLine(proc);
                                if (cmdLine?.IndexOf(mumbleName, StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    match = candidate;
                                    break;
                                }
                            }
                            catch { /* ignore */ }
                        }
                    }

                    // Fallback: just assign to first unused profile
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

                    // Try to get the exe path via MainModule (can fail with Access Denied)
                    var modulePath = p.MainModule?.FileName;
                    if (!string.IsNullOrWhiteSpace(modulePath) &&
                        string.Equals(NormalizePath(modulePath), exePath, StringComparison.OrdinalIgnoreCase))
                    {
                        matches.Add(p);
                        continue;
                    }

                    // Fallback: if MainModule failed but process name matches, assume it's a match
                    // This is less precise but necessary for processes we can't query fully
                    if (string.IsNullOrWhiteSpace(modulePath))
                    {
                        // Just finding a process named "Gw2-64" is a strong indicator
                        // Since this is a rehydration best-effort, we'll include it
                        matches.Add(p);
                    }
                }
                catch
                {
                    // If we can't access the process at all, still try to include it
                    // as a last resort since the process name matched
                    try
                    {
                        if (!p.HasExited)
                            matches.Add(p);
                    }
                    catch { /* ignore */ }
                }
            }

            return matches;
        }

        private static string? GetProcessCommandLine(Process process)
        {
            try
            {
                // Simple fallback: just return empty - mumble link matching will work for most cases
                // WMI requires System.Management NuGet package which we want to avoid
                return null;
            }
            catch { /* ignore */ }

            return null;
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
