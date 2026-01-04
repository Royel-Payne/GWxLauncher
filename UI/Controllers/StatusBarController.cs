namespace GWxLauncher.UI.Controllers
{
    internal sealed class StatusBarController : IDisposable
    {
        private readonly Label _label;
        private readonly System.Windows.Forms.Timer _timer;

        private string _fullText = "";
        private int _scrollPx = 0;

        private int _textWidthPx = 0;
        private int _gapWidthPx = 0;
        private bool _needsScroll = false;

        // Tweak if you want faster/slower.
        private const int IntervalMs = 40;
        private const int StepPx = 2;

        private const TextFormatFlags DrawFlags =
            TextFormatFlags.SingleLine |
            TextFormatFlags.NoPrefix |
            TextFormatFlags.EndEllipsis; // only used when not scrolling

        public StatusBarController(Label label)
        {
            _label = label ?? throw new ArgumentNullException(nameof(label));

            // We do our own drawing.
            _label.Text = "";
            _label.AutoEllipsis = false;

            _timer = new System.Windows.Forms.Timer { Interval = IntervalMs };
            _timer.Tick += (_, __) => OnTick();

            _label.Paint += Label_Paint;
            _label.SizeChanged += (_, __) => RecomputeAndInvalidate(resetScroll: false);
            _label.FontChanged += (_, __) => RecomputeAndInvalidate(resetScroll: true);
            _label.PaddingChanged += (_, __) => RecomputeAndInvalidate(resetScroll: false);
            _label.HandleCreated += (_, __) => RecomputeAndInvalidate(resetScroll: true);

            RecomputeAndInvalidate(resetScroll: true);
        }

        public void SetText(string text)
        {
            var newText = text ?? "";

            // If text didn't change, do not reset scroll (prevents “stuck at start”).
            if (string.Equals(_fullText, newText, StringComparison.Ordinal))
            {
                // Still ensure scrolling state is correct if we weren't running.
                EnsureTimerState();
                return;
            }

            _fullText = newText;
            _scrollPx = 0;

            RecomputeAndInvalidate(resetScroll: true);
        }

        private void OnTick()
        {
            if (!_needsScroll)
            {
                StopTimer();
                return;
            }

            int cycle = _textWidthPx + _gapWidthPx;
            if (cycle <= 0)
            {
                StopTimer();
                return;
            }

            _scrollPx += StepPx;
            if (_scrollPx >= cycle)
                _scrollPx -= cycle;

            _label.Invalidate();
        }

        private void Label_Paint(object? sender, PaintEventArgs e)
        {
            // Let the label paint its background normally.
            // We only draw text.
            var text = _fullText;
            if (string.IsNullOrEmpty(text))
                return;

            var rc = _label.ClientRectangle;

            // Apply padding
            rc = new Rectangle(
                rc.Left + _label.Padding.Left,
                rc.Top + _label.Padding.Top,
                Math.Max(0, rc.Width - (_label.Padding.Left + _label.Padding.Right)),
                Math.Max(0, rc.Height - (_label.Padding.Top + _label.Padding.Bottom)));

            if (rc.Width <= 0 || rc.Height <= 0)
                return;

            if (!_needsScroll)
            {
                // Just draw once (no scroll).
                TextRenderer.DrawText(
                    e.Graphics,
                    text,
                    _label.Font,
                    rc,
                    _label.ForeColor,
                    DrawFlags);
                return;
            }

            // Scrolling: draw two copies offset by the cycle length.
            // Draw at x = -_scrollPx, then again at x = -_scrollPx + cycle.
            int cycle = _textWidthPx + _gapWidthPx;
            int x1 = rc.Left - _scrollPx;
            int x2 = x1 + cycle;

            // Draw in a “tall” rect so text stays vertically aligned.
            var r1 = new Rectangle(x1, rc.Top, rc.Width + cycle, rc.Height);
            var r2 = new Rectangle(x2, rc.Top, rc.Width + cycle, rc.Height);

            TextRenderer.DrawText(e.Graphics, text, _label.Font, r1, _label.ForeColor, TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
            TextRenderer.DrawText(e.Graphics, text, _label.Font, r2, _label.ForeColor, TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
        }

        private void RecomputeAndInvalidate(bool resetScroll)
        {
            if (!_label.IsHandleCreated)
                return;

            if (resetScroll)
                _scrollPx = 0;

            using var g = _label.CreateGraphics();

            // Available width is the label width minus padding.
            int available = Math.Max(0, _label.ClientSize.Width - (_label.Padding.Left + _label.Padding.Right));

            if (string.IsNullOrEmpty(_fullText) || available <= 0)
            {
                _needsScroll = false;
                _textWidthPx = 0;
                _gapWidthPx = 0;
                StopTimer();
                _label.Invalidate();
                return;
            }

            // Measure the full text width.
            var textSize = TextRenderer.MeasureText(
                g,
                _fullText,
                _label.Font,
                new Size(int.MaxValue, _label.Height),
                TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);

            _textWidthPx = textSize.Width;

            // Gap between repeats (in pixels). Use a bullet-ish gap for aesthetics.
            // You can change this string if you want a wider/narrower gap.
            var gapSize = TextRenderer.MeasureText(
                g,
                "   •   ",
                _label.Font,
                new Size(int.MaxValue, _label.Height),
                TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);

            _gapWidthPx = Math.Max(24, gapSize.Width);

            _needsScroll = _textWidthPx > available;

            EnsureTimerState();
            _label.Invalidate();
        }

        private void EnsureTimerState()
        {
            if (_needsScroll)
            {
                if (!_timer.Enabled)
                    _timer.Start();
            }
            else
            {
                StopTimer();
            }
        }

        private void StopTimer()
        {
            if (_timer.Enabled)
                _timer.Stop();
        }

        public void Dispose()
        {
            try
            {
                StopTimer();
                _timer.Dispose();

                _label.Paint -= Label_Paint;
                // SizeChanged / FontChanged / PaddingChanged were attached via lambdas,
                // so there’s nothing to detach (safe; label lifetime == controller lifetime).
            }
            catch
            {
                // best-effort
            }
        }
    }
}
