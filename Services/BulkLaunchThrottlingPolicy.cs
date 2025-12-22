using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using GWxLauncher.Domain;

namespace GWxLauncher.Services
{
    internal static class BulkLaunchThrottlingPolicy
    {
        // Locked clamp range from BulkLaunchThrottling.md
        private const int MinDelaySeconds = 5;
        private const int MaxDelaySeconds = 90;

        // Hard-coded internal timeout (not user-configurable).
        // This prevents deadlock when readiness never arrives (e.g., wrong credentials).
        private const int InternalTimeoutMs = 15_000;

        // Grace period before showing readiness UI (allows game window + login to appear)
        private const int ReadinessStatusGraceMs = 7000;

        public static async Task<BulkLaunchThrottlingResult> ApplyAsync(
            GameType gameType,
            int requestedDelaySeconds,
            Func<bool>? readinessCheck,
            Action<string>? statusCallback,
            LaunchReport? report,
            CancellationToken cancellationToken = default)
        {
            int effectiveDelaySeconds = ClampDelaySeconds(requestedDelaySeconds);

            int readinessWaitMs = 0;
            int delayMs = 0;

            bool readyDetected = false;
            bool timedOut = false;

            // 1) Optional readiness wait (GW1 probe)
            if (readinessCheck != null)
            {
                var waitSw = Stopwatch.StartNew();
                int lastReportedSecond = -1;

                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (readinessCheck())
                    {
                        readyDetected = true;
                        break;
                    }

                    int elapsedMs = (int)waitSw.ElapsedMilliseconds;

                    // Don’t show readiness UI immediately — give the game time to create its window.
                    if (elapsedMs >= ReadinessStatusGraceMs)
                    {
                        int remainingMs = Math.Max(0, InternalTimeoutMs - elapsedMs);
                        int remainingSeconds = remainingMs / 1000;

                        if (remainingSeconds != lastReportedSecond)
                        {
                            lastReportedSecond = remainingSeconds;
                            statusCallback?.Invoke(
                                $"You may enter the game world, or wait — {remainingSeconds}s remaining…");
                        }
                    }
                    if (elapsedMs >= InternalTimeoutMs)
                    {
                        timedOut = true;
                        break;
                    }

                    await Task.Delay(100, cancellationToken);
                }

                waitSw.Stop();
                readinessWaitMs = (int)waitSw.ElapsedMilliseconds;

            }

            // 2) Post-ready pacing delay (always applies per game delay)
            if (effectiveDelaySeconds > 0)
            {
                var delaySw = Stopwatch.StartNew();

                for (int remaining = effectiveDelaySeconds; remaining > 0; remaining--)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    statusCallback?.Invoke($"Throttling: launching next account in {remaining}s…");
                    await Task.Delay(1000, cancellationToken);
                }

                delaySw.Stop();
                delayMs = (int)delaySw.ElapsedMilliseconds;
            }

            int totalMs = readinessWaitMs + delayMs;

            var result = BuildResult(
                gameType,
                requestedDelaySeconds,
                effectiveDelaySeconds,
                readinessCheck,
                readyDetected,
                timedOut,
                readinessWaitMs,
                delayMs,
                totalMs);

            // 3) LaunchReport step (locked label)
            if (report != null)
            {
                report.Steps.Add(new LaunchStep
                {
                    Label = "Throttling",
                    Outcome = StepOutcome.Success,
                    Detail = result.ReasonText
                });
            }

            statusCallback?.Invoke(string.Empty);
            return result;
        }

        private static int ClampDelaySeconds(int requestedDelaySeconds)
        {
            if (requestedDelaySeconds < MinDelaySeconds) return MinDelaySeconds;
            if (requestedDelaySeconds > MaxDelaySeconds) return MaxDelaySeconds;
            return requestedDelaySeconds;
        }

        private static BulkLaunchThrottlingResult BuildResult(
           GameType gameType,
           int requestedDelaySeconds,
           int effectiveDelaySeconds,
           Func<bool>? readinessCheck,
           bool readyDetected,
           bool timedOut,
           int readinessWaitMs,
           int delayMs,
           int totalMs)
        {
            string reason;

            if (readinessCheck == null)
            {
                // No readiness check (GW2 path) or probe unavailable (GW1 fallback).
                reason = effectiveDelaySeconds > 0
                    ? $"Probe unavailable; using delay only. Delaying {effectiveDelaySeconds} {(effectiveDelaySeconds == 1 ? "second" : "seconds")}"
                    : "Probe unavailable; using delay only";
            }
            else
            {
                if (readyDetected)
                {
                    reason = $"Ready detected after {FormatSeconds(readinessWaitMs)}";
                }
                else if (timedOut)
                {
                    reason = $"Timed out waiting for readiness after {FormatSeconds(readinessWaitMs)}; continuing";
                }
                else
                {
                    reason = $"Continuing (waited {FormatSeconds(readinessWaitMs)})";
                }

                if (effectiveDelaySeconds > 0)
                {
                    string unit = effectiveDelaySeconds == 1 ? "second" : "seconds";
                    reason += Environment.NewLine + $"Delaying {effectiveDelaySeconds} {unit}";
                }
            }

            if (requestedDelaySeconds != effectiveDelaySeconds)
            {
                reason += Environment.NewLine + $"(Requested {requestedDelaySeconds}s; clamped to {effectiveDelaySeconds}s)";
            }

            return new BulkLaunchThrottlingResult
            {
                GameType = gameType,
                ReadyDetected = readyDetected,
                TimedOut = timedOut,
                WaitedMs = totalMs,
                EffectiveDelaySeconds = effectiveDelaySeconds,
                ReasonText = reason
            };
        }

        private static string FormatSeconds(int ms)
        {
            // 1 decimal place (e.g., 7.4s)
            double s = ms / 1000.0;
            return $"{s:0.0}s";
        }
    }

    internal sealed class BulkLaunchThrottlingResult
    {
        public GameType GameType { get; init; }
        public bool ReadyDetected { get; init; }
        public bool TimedOut { get; init; }
        public int WaitedMs { get; init; }
        public int EffectiveDelaySeconds { get; init; }
        public string ReasonText { get; init; } = "";
    }
}
