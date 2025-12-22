using GWxLauncher.Config;
using GWxLauncher.Domain;

namespace GWxLauncher.Services
{
    /// <summary>
    /// Pass 4: encapsulates "what’s allowed" policy (bulk eligibility + multiclient gating).
    /// This class does NOT touch UI.
    /// </summary>
    internal sealed class LaunchEligibilityPolicy
    {
        internal sealed class BulkArmingEvaluation
        {
            public bool AnyEligible { get; init; }
            public bool Armed { get; init; }
            public string StatusText { get; init; } = "";
            public bool MulticlientSatisfied { get; init; }
            public string MissingMulticlientDetail { get; init; } = "";
            public int EligibleGw1Count { get; init; }
            public int EligibleGw2Count { get; init; }
        }

        private readonly ViewStateStore _views;

        public LaunchEligibilityPolicy(ViewStateStore views)
        {
            _views = views;
        }

        public BulkArmingEvaluation EvaluateBulkArming(
            IEnumerable<GameProfile> allProfiles,
            string activeViewName,
            bool showCheckedOnly,
            LauncherConfig config)
        {
            bool anyEligible = _views.AnyEligibleInActiveView(allProfiles);
            bool armed = anyEligible && showCheckedOnly;

            GetEligibleGameTypeCounts(allProfiles, activeViewName, out int gw1Count, out int gw2Count);

            bool mcOk = IsMulticlientEnabledForEligible(gw1Count, gw2Count, config, out string missing);

            string statusText;

            if (!showCheckedOnly)
            {
                statusText = $"Launch All not armed · Enable \"Show Checked Accounts Only\" · View: {_views.ActiveViewName}";
            }
            else if (!anyEligible)
            {
                statusText = $"No checked profiles in view · View: {_views.ActiveViewName}";
            }
            else if (!mcOk)
            {
                statusText = $"Launch All requires multiclient: {missing} · View: {_views.ActiveViewName}";
            }
            else
            {
                statusText = $"Launch All ready · View: {_views.ActiveViewName}";
            }

            return new BulkArmingEvaluation
            {
                AnyEligible = anyEligible,
                Armed = armed,
                StatusText = statusText,
                MulticlientSatisfied = mcOk,
                MissingMulticlientDetail = missing,
                EligibleGw1Count = gw1Count,
                EligibleGw2Count = gw2Count
            };
        }

        public List<GameProfile> BuildBulkTargets(IEnumerable<GameProfile> allProfiles, string activeViewName)
        {
            return allProfiles
                .Where(p => _views.IsEligible(activeViewName, p.Id))
                .OrderBy(p => p.GameType)
                .ThenBy(p => p.Name, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }

        public bool IsMulticlientEnabledForEligible(
            IEnumerable<GameProfile> allProfiles,
            string activeViewName,
            LauncherConfig config,
            out string missingDetail)
        {
            GetEligibleGameTypeCounts(allProfiles, activeViewName, out int gw1Count, out int gw2Count);
            return IsMulticlientEnabledForEligible(gw1Count, gw2Count, config, out missingDetail);
        }

        public void EnableRequiredMulticlientFlagsForEligible(
            IEnumerable<GameProfile> allProfiles,
            string activeViewName,
            LauncherConfig config)
        {
            GetEligibleGameTypeCounts(allProfiles, activeViewName, out int gw1Count, out int gw2Count);

            if (gw1Count > 1) config.Gw1MulticlientEnabled = true;
            if (gw2Count > 1) config.Gw2MulticlientEnabled = true;
        }

        private void GetEligibleGameTypeCounts(
            IEnumerable<GameProfile> allProfiles,
            string activeViewName,
            out int gw1Count,
            out int gw2Count)
        {
            var eligible = allProfiles.Where(p => _views.IsEligible(activeViewName, p.Id));

            gw1Count = eligible.Count(p => p.GameType == GameType.GuildWars1);
            gw2Count = eligible.Count(p => p.GameType == GameType.GuildWars2);
        }

        private static bool IsMulticlientEnabledForEligible(
            int gw1Count,
            int gw2Count,
            LauncherConfig config,
            out string missingDetail)
        {
            var missing = new List<string>();

            // Multiclient is only REQUIRED when launching 2+ instances of the same game type.
            if (gw1Count > 1 && !config.Gw1MulticlientEnabled)
                missing.Add("Guild Wars 1");

            if (gw2Count > 1 && !config.Gw2MulticlientEnabled)
                missing.Add("Guild Wars 2");

            if (missing.Count == 0)
            {
                missingDetail = "";
                return true;
            }

            missingDetail = string.Join(", ", missing);
            return false;
        }
    }
}
