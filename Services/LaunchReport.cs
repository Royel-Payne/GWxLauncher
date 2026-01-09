namespace GWxLauncher.Services
{
    internal sealed class LaunchReport
    {
        public DateTime Timestamp { get; init; } = DateTime.Now;

        public string GameName { get; init; } = "Guild Wars 1";
        public string ExecutablePath { get; init; } = "";

        public string LaunchArguments { get; set; } = "";
        public string FullCommandLine { get; set; } = "";

        public bool UsedSuspendedLaunch { get; set; } = false;

        // Per-step outcomes (in the real order we attempted)
        public List<LaunchStep> Steps { get; } = new();

        public bool Succeeded { get; set; } = false;
        public string FailureMessage { get; set; } = "";

        public string BuildSummary()
        {
            if (!Succeeded)
            {
                // Keep it short for status label
                var reason = string.IsNullOrWhiteSpace(FailureMessage) ? "unknown error" : FailureMessage.Split('\n').FirstOrDefault();
                return $"{GameName} launch failed · {reason}";
            }

            // Success summary: show key steps
            // Example: "GW1 launched · gMod ✓ · Toolbox ✓ · Py4GW pending"
            var parts = new List<string> { $"{GameName} launched" };

            foreach (var s in Steps)
            {
                // Only include meaningful steps (attempted)
                parts.Add($"{s.Label} {s.OutcomeGlyph}");
            }

            return string.Join(" · ", parts);
        }

        public override string ToString()
        {
            var lines = new List<string>
            {
                $"Timestamp: {Timestamp}",
                $"Game: {GameName}",
                $"Executable: {ExecutablePath}",
                $"Arguments: {LaunchArguments}",
                $"Command line: {FullCommandLine}",
                $"Launch mode: {(UsedSuspendedLaunch ? "Suspended (gMod early injection)" : "Normal (Process.Start)")}",
                $"Result: {(Succeeded ? "SUCCESS" : "FAILURE")}"
            };

            if (!Succeeded && !string.IsNullOrWhiteSpace(FailureMessage))
            {
                lines.Add("");
                lines.Add("Failure:");
                lines.Add(FailureMessage);
            }

            if (Steps.Count > 0)
            {
                lines.Add("");
                lines.Add("Mods:");
                foreach (var s in Steps)
                {
                    lines.Add($"- {s.Label}: {s.OutcomeText}");
                    if (!string.IsNullOrWhiteSpace(s.Detail))
                        lines.Add($"  {s.Detail}");
                }
            }

            return string.Join(Environment.NewLine, lines);
        }
    }

    internal sealed class LaunchStep
    {
        public string Label { get; init; } = ""; // "gMod", "Toolbox", "Py4GW"
        public StepOutcome Outcome { get; set; } = StepOutcome.NotAttempted;
        public string Detail { get; set; } = "";

        public string OutcomeText => Outcome switch
        {
            StepOutcome.Success => "Success",
            StepOutcome.Failed => "Failed",
            StepOutcome.Pending => "Pending",
            StepOutcome.Skipped => "Skipped",
            _ => "Not attempted"
        };

        public string OutcomeGlyph => Outcome switch
        {
            StepOutcome.Success => "✓",
            StepOutcome.Failed => "✗",
            StepOutcome.Pending => "pending",
            StepOutcome.Skipped => "skipped",
            _ => "n/a"
        };
    }

    internal enum StepOutcome
    {
        NotAttempted = 0,
        Success,
        Failed,
        Pending,
        Skipped
    }
}
