// File: Services/Gw2RunAfterLauncher.cs
using System.Diagnostics;
using GWxLauncher.Domain;

namespace GWxLauncher.Services
{
    internal sealed class Gw2RunAfterLauncher
    {
        public LaunchStep Start(GameProfile profile)
        {
            var step = new LaunchStep { Label = "Run After Programs" };

            if (profile.GameType != GameType.GuildWars2)
            {
                step.Outcome = StepOutcome.Skipped;
                step.Detail = "Not a GW2 profile.";
                return step;
            }

            if (!profile.Gw2RunAfterEnabled)
            {
                step.Outcome = StepOutcome.Skipped;
                step.Detail = "Run After Programs disabled.";
                return step;
            }

            if (profile.Gw2RunAfterPrograms == null || profile.Gw2RunAfterPrograms.Count == 0)
            {
                step.Outcome = StepOutcome.Skipped;
                step.Detail = "No programs configured.";
                return step;
            }

            var launched = new List<string>();
            var skipped = new List<string>();
            var failed = new List<string>();

            foreach (var p in profile.Gw2RunAfterPrograms)
            {
                if (!p.Enabled)
                {
                    skipped.Add(p.Name);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(p.ExePath))
                {
                    failed.Add($"{p.Name} (path not configured)");
                    continue;
                }

                if (!File.Exists(p.ExePath))
                {
                    failed.Add($"{p.Name} (not found at: {p.ExePath})");
                    continue;
                }

                try
                {
                    string mumbleName = Gw2MumbleLinkService.GetMumbleLinkName(profile);

                    if (p.PassMumbleLinkName && !string.IsNullOrWhiteSpace(mumbleName))
                    {
                        var psi = new ProcessStartInfo
                        {
                            FileName = p.ExePath,
                            WorkingDirectory = Path.GetDirectoryName(p.ExePath) ?? "",
                            Arguments = $"--mumble \"{mumbleName}\""
                        };
                        Process.Start(psi);
                        launched.Add($"{p.Name} (MumbleLink)");
                    }
                    else
                    {
                        Process.Start(p.ExePath);
                        launched.Add(p.Name);
                    }
                }
                catch (Exception ex)
                {
                    failed.Add($"{p.Name} ({ex.Message})");
                }
            }

            if (launched.Count > 0)
            {
                step.Outcome = StepOutcome.Success;
                var details = new List<string> { $"Launched {launched.Count} program(s): {string.Join(", ", launched)}" };
                if (skipped.Count > 0)
                    details.Add($"Skipped {skipped.Count}: {string.Join(", ", skipped)}");
                if (failed.Count > 0)
                    details.Add($"Failed {failed.Count}: {string.Join(", ", failed)}");
                step.Detail = string.Join(" | ", details);
            }
            else if (failed.Count > 0)
            {
                step.Outcome = StepOutcome.Failed;
                step.Detail = $"Failed to launch: {string.Join(", ", failed)}";
            }
            else
            {
                step.Outcome = StepOutcome.Skipped;
                step.Detail = "All programs disabled.";
            }

            return step;
        }
    }
}
