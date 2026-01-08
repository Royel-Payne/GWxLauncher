using System.Diagnostics;
using GWxLauncher.Domain;

namespace GWxLauncher.Services
{
    /// <summary>
    /// Serializes GW2 automation across accounts (Launch All),
    /// so only one instance is in the "typing/clicking / post-login settle" phase at a time.
    /// </summary>
    internal sealed class Gw2AutomationCoordinator
    {
        private readonly SemaphoreSlim _gate = new SemaphoreSlim(1, 1);
        private readonly Gw2AutoLoginService _service = new Gw2AutoLoginService();

        public bool TryAutomateLogin(Process? gw2Process, GameProfile profile, LaunchReport report, bool bulkMode, out string error)
        {
            _gate.Wait();
            try
            {
                return _service.TryAutomateLogin(gw2Process, profile, report, bulkMode, out error);
            }
            finally
            {
                _gate.Release();
            }
        }
    }
}
