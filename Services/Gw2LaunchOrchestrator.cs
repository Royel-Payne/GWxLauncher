using GWxLauncher.Domain;
using System.Diagnostics;

namespace GWxLauncher.Services
{
    /// <summary>
    /// Central GW2 launch orchestration used by both:
    /// - Single launch (LaunchProfile)
    /// - Bulk launch worker (LaunchProfileGw2BulkWorker)
    ///
    /// This class does NOT touch UI. It returns a result with:
    /// - LaunchReport (when created)
    /// - Optional user-facing message data (caller decides how/when to show it)
    /// </summary>
    internal sealed class Gw2LaunchOrchestrator
    {
        internal sealed class Gw2LaunchResult
        {
            public LaunchReport? Report { get; init; }

            // When non-empty, caller should show a MessageBox with this text/title/icon.
            public string MessageBoxText { get; init; } = "";
            public string MessageBoxTitle { get; init; } = "";
            public bool MessageBoxIsError { get; init; } = false;

            public bool HasMessageBox => !string.IsNullOrWhiteSpace(MessageBoxText);
        }

        public Gw2LaunchResult Launch(
            GameProfile profile,
            string exePath,
            bool mcEnabled,
            bool bulkMode,
            Gw2AutomationCoordinator automationCoordinator,
            Action<GameProfile> runAfterInvoker)
        {
            if (profile == null)
                return new Gw2LaunchResult();

            // Keep missing/invalid exe handling in MainForm (call sites already do this).
            // This orchestrator assumes exePath is non-empty and exists.

            var report = new LaunchReport
            {
                GameName = "Guild Wars 2",
                ExecutablePath = exePath
            };

            var mcStep = new LaunchStep { Label = "Multiclient" };
            report.Steps.Add(mcStep);

            const string Gw2MutexName = Gw2MutexKiller.Gw2MutexLeafName;

            bool mutexOpen;
            try
            {
                using var _ = Mutex.OpenExisting(Gw2MutexName);
                mutexOpen = true;
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                mutexOpen = false;
            }
            catch (Exception ex)
            {
                report.Succeeded = false;
                report.FailureMessage = $"Failed to check GW2 mutex: {ex.Message}";
                mcStep.Outcome = StepOutcome.Failed;
                mcStep.Detail = "Mutex check failed.";

                return new Gw2LaunchResult
                {
                    Report = report,
                    MessageBoxText = report.FailureMessage,
                    MessageBoxTitle = "Guild Wars 2 launch",
                    MessageBoxIsError = false
                };
            }

            if (!mcEnabled)
            {
                mcStep.Outcome = StepOutcome.Skipped;
                mcStep.Detail = "Multiclient disabled.";
            }
            else
            {
                // If GW2 is already running (mutex open), we must clear the mutex or abort.
                if (mutexOpen)
                {
                    int attempts = bulkMode ? 4 : 1;  // bulk: retry a few times
                    int delayMs = bulkMode ? 350 : 0; // bulk: tiny backoff

                    bool cleared = false;
                    int clearedPid = 0;
                    string killDetail = "";
                    bool usedElevated = false;

                    for (int i = 0; i < attempts; i++)
                    {
                        if (Gw2MutexKiller.TryKillGw2Mutex(
                                out clearedPid,
                                out killDetail,
                                allowElevatedFallback: true,
                                out usedElevated))
                        {
                            cleared = true;
                            break;
                        }

                        if (i < attempts - 1 && delayMs > 0)
                            Thread.Sleep(delayMs);
                    }

                    if (!cleared)
                    {
                        string msg =
                            "Guild Wars 2 is already running.\n\n" +
                            "Close it or launch all instances via GWxLauncher.";

                        report.Succeeded = false;
                        report.FailureMessage = msg;
                        mcStep.Outcome = StepOutcome.Failed;
                        mcStep.Detail = $"GW2 mutex exists and could not be cleared. {killDetail}";

                        return new Gw2LaunchResult
                        {
                            Report = report,
                            MessageBoxText = msg,
                            MessageBoxTitle = "Guild Wars 2 launch",
                            MessageBoxIsError = false
                        };
                    }

                    mcStep.Outcome = StepOutcome.Success;
                    mcStep.Detail = usedElevated
                        ? $"Cleared GW2 mutex in PID {clearedPid} (elevated retry)."
                        : $"Cleared GW2 mutex in PID {clearedPid}.";
                }
            }

            // Launch GW2 (add -shareArchive only when multiclient enabled)
            try
            {
                string mumbleName = Gw2MumbleLinkService.GetMumbleLinkName(profile);

                var args = new List<string>();

                if (mcEnabled)
                    args.Add("-shareArchive");

                if (!string.IsNullOrWhiteSpace(mumbleName))
                    args.Add($"-mumble \"{mumbleName}\"");

                var startInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    WorkingDirectory = Path.GetDirectoryName(exePath) ?? "",
                    Arguments = string.Join(" ", args)
                };

                // Safety: if we got this far and mc is enabled, the step must not remain "NotAttempted".
                if (mcEnabled && mcStep.Outcome == StepOutcome.NotAttempted)
                {
                    mcStep.Outcome = StepOutcome.Success;
                    if (string.IsNullOrWhiteSpace(mcStep.Detail))
                        mcStep.Detail = "Multiclient enabled.";
                }

                var process = Process.Start(startInfo);

                if (bulkMode && mcEnabled)
                {
                    // Wait until GW2 recreates its mutex, so the next bulk launch can reliably clear it again.
                    if (!WaitForGw2MutexToExist(timeoutMs: 8000, out int waited))
                        mcStep.Detail += $" (Warning: GW2 mutex did not appear within {waited}ms)";
                    else
                        mcStep.Detail += $" (GW2 mutex observed after {waited}ms)";
                }

                // If multiclient enabled, keep the step success but add the arg note.
                if (mcEnabled)
                {
                    mcStep.Detail = string.IsNullOrWhiteSpace(mcStep.Detail)
                        ? "Launched with -shareArchive."
                        : mcStep.Detail + " Launched with -shareArchive.";
                }

                if (profile.Gw2AutoLoginEnabled)
                {
                    // Preserve existing difference:
                    // - Single path (bulkMode == false) calls TryAutomateLogin even if process is null
                    // - Bulk worker (bulkMode == true) guarded against null
                    bool shouldCallAutomation = bulkMode ? (process != null) : true;

                    if (shouldCallAutomation)
                    {
                        if (!automationCoordinator.TryAutomateLogin(process, profile, report, bulkMode: bulkMode, out var autoLoginError))
                        {
                            if (!string.IsNullOrWhiteSpace(autoLoginError))
                                report.FailureMessage = $"Auto-login failed: {autoLoginError}";
                        }
                    }
                }

                runAfterInvoker(profile);

                report.Succeeded = true;

                return new Gw2LaunchResult { Report = report };
            }
            catch (Exception ex)
            {
                report.Succeeded = false;
                report.FailureMessage = ex.Message;
                mcStep.Outcome = StepOutcome.Failed;
                mcStep.Detail = "Process.Start failed.";

                return new Gw2LaunchResult
                {
                    Report = report,
                    MessageBoxText = $"Failed to launch Guild Wars 2:\n\n{ex.Message}",
                    MessageBoxTitle = "Launch failed",
                    MessageBoxIsError = true
                };
            }
        }

        private static bool WaitForGw2MutexToExist(int timeoutMs, out int waitedMs)
        {
            waitedMs = 0;
            const int stepMs = 50;

            while (waitedMs < timeoutMs)
            {
                try
                {
                    // If this succeeds, GW2 has created its mutex again.
                    using var _ = Mutex.OpenExisting(Gw2MutexKiller.Gw2MutexLeafName);
                    return true;
                }
                catch (WaitHandleCannotBeOpenedException)
                {
                    // Not created yet, keep waiting.
                }
                catch
                {
                    // Any other failure: keep waiting a bit (don’t hard-fail over transient issues).
                }

                Thread.Sleep(stepMs);
                waitedMs += stepMs;
            }

            return false;
        }
    }
}
