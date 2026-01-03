using System;
using System.Windows.Forms;

namespace GWxLauncher.UI.Controllers
{
    /// <summary>
    /// Owns status label text + marquee behavior.
    /// Keeps MainForm free of timer/marquee details.
    /// </summary>
    internal sealed class StatusBarController : IDisposable
    {
        private readonly Label _label;
        private readonly System.Windows.Forms.Timer _timer = new System.Windows.Forms.Timer();

        private string _fullText = "";
        private int _index = 0;
        private bool _updateQueued = false;

        private const int IntervalMs = 120;
        private const string Gap = "   •   ";

        public StatusBarController(Label label)
        {
            _label = label ?? throw new ArgumentNullException(nameof(label));
            _timer.Interval = IntervalMs;
            _timer.Tick += (_, __) => Tick();
            _label.SizeChanged += (_, __) =>
            {
                _index = 0;
                QueueUpdate();
            };

            _label.HandleCreated += (_, __) =>
            {
                _index = 0;
                QueueUpdate();
            };
        }
        public void SetText(string text)
        {
            string newText = text ?? "";

            // If text hasn't changed, don't reset marquee position.
            // (MainForm may call SetStatus repeatedly with the same value.)
            bool changed = !string.Equals(_fullText, newText, StringComparison.Ordinal);

            _fullText = newText;

            if (changed)
            {
                _index = 0;

                // Show immediately; label will clip if needed.
                _label.Text = _fullText;

                // Re-evaluate marquee after layout settles.
                QueueUpdate();
                return;
            }

            // Same text: keep scrolling if already scrolling; still ensure we start if we weren't.
            if (!_timer.Enabled)
                QueueUpdate();
        }

        private void QueueUpdate()
        {
            if (_updateQueued)
                return;

            if (!_label.IsHandleCreated)
                return;

            _updateQueued = true;

            _label.BeginInvoke((Action)(() =>
            {
                _updateQueued = false;
                Update();
            }));
        }


        private void Update()
        {
            var full = _fullText;

            if (_label.ClientSize.Width <= 0)
            {
                Stop();
                QueueUpdate(); // try again after layout settles
                return;
            }

            if (string.IsNullOrWhiteSpace(full))
            {
                Stop();
                _label.Text = "";
                return;
            }

            if (TextFits(full))
            {
                Stop();
                _label.Text = full;
                return;
            }

            // Need marquee
            if (!_timer.Enabled)
            {
                // Only initialize the marquee display when starting.
                Start();
                _label.Text = BuildSlice(full, _index);
            }
            // If already running, Tick() owns updating the label text.
        }

        private bool TextFits(string text)
        {
            if (string.IsNullOrEmpty(text))
                return true;

            // Match the old MainForm behavior: single-line, no padding.
            var size = TextRenderer.MeasureText(
                text,
                _label.Font,
                new System.Drawing.Size(int.MaxValue, _label.Height),
                TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);

            int padding = _label.Padding.Left + _label.Padding.Right;
            int available = Math.Max(0, _label.ClientSize.Width - padding);

            return size.Width <= available;
        }

        private void Start()
        {
            if (!_timer.Enabled)
            {
                _index = 0;
                _timer.Start();
            }
        }

        private void Stop()
        {
            if (_timer.Enabled)
                _timer.Stop();

            _index = 0;
        }
        private void Tick()
        {
            if (!_timer.Enabled)
                return;

            var full = _fullText ?? "";

            if (string.IsNullOrWhiteSpace(full))
            {
                Stop();
                _label.Text = "";
                return;
            }

            if (TextFits(full))
            {
                Stop();
                _label.Text = full;
                return;
            }

            _index++;
            _label.Text = BuildSlice(full, _index);
        }


        private static string BuildSlice(string full, int index)
        {
            if (string.IsNullOrEmpty(full))
                return "";

            // Create a looping marquee buffer
            var buffer = full + Gap + full + Gap;

            // Keep index in range
            index %= buffer.Length;
            if (index < 0) index += buffer.Length;

            // Slice to a reasonable length; label will clip anyway.
            const int sliceLen = 200;
            if (buffer.Length <= sliceLen)
                return buffer;

            var start = index;
            if (start + sliceLen <= buffer.Length)
                return buffer.Substring(start, sliceLen);

            // Wraparound
            var part1 = buffer.Substring(start);
            var part2 = buffer.Substring(0, sliceLen - part1.Length);
            return part1 + part2;
        }

        public void Dispose()
        {
            try
            {
                Stop();
                _timer.Dispose();
            }
            catch { /* best-effort */ }
        }
    }
}
