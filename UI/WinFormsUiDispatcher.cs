using System;
using System.Windows.Forms;

namespace GWxLauncher.UI
{
    /// <summary>
    /// Central UI-thread marshaling helper for WinForms.
    /// This wraps Control.BeginInvoke while handling disposal safely.
    /// </summary>
    internal sealed class WinFormsUiDispatcher
    {
        private readonly Control _control;

        public WinFormsUiDispatcher(Control control)
        {
            _control = control ?? throw new ArgumentNullException(nameof(control));
        }

        public void Post(Action action)
        {
            if (action == null)
                return;

            if (_control.IsDisposed)
                return;

            try
            {
                if (_control.InvokeRequired)
                    _control.BeginInvoke(action);
                else
                    action();
            }
            catch (ObjectDisposedException)
            {
                // UI is closing; ignore.
            }
            catch (InvalidOperationException)
            {
                // Handle not created / shutting down; ignore.
            }
        }
    }
}
