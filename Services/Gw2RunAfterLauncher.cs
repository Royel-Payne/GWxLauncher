// File: Services/Gw2RunAfterLauncher.cs
using System.Diagnostics;
using GWxLauncher.Domain;

namespace GWxLauncher.Services
{
    internal sealed class Gw2RunAfterLauncher
    {
        public void Start(GameProfile profile)
        {
            if (profile.GameType != GameType.GuildWars2)
                return;

            if (!profile.Gw2RunAfterEnabled)
                return;

            if (profile.Gw2RunAfterPrograms == null || profile.Gw2RunAfterPrograms.Count == 0)
                return;

            foreach (var p in profile.Gw2RunAfterPrograms)
            {
                if (!p.Enabled)
                    continue;

                if (string.IsNullOrWhiteSpace(p.ExePath) || !File.Exists(p.ExePath))
                    continue;

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
                    }
                    else
                    {
                        Process.Start(p.ExePath);
                    }
                }
                catch
                {
                    // swallow for now; later we can surface this in LaunchReport-like UI
                }
            }
        }
    }
}
