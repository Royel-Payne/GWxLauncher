namespace GWxLauncher.UI.Controllers
{
    internal enum RefreshReason
    {
        Unknown = 0,
        Startup,
        ViewChanged,
        ShowCheckedOnlyChanged,
        EligibilityChanged,
        ProfilesChanged,
        ImportCompleted,
        ThemeChanged,
        BulkLaunchStateChanged
    }

    /// <summary>
    /// Coalesces and orders UI refresh operations so MainForm can call one thing.
    /// Keeps refresh sequencing deterministic and avoids "fixes itself on resize" behavior.
    /// </summary>
    internal sealed class MainFormRefresher
    {
        private readonly WinFormsUiDispatcher _ui;
        private readonly Action _refreshProfileList;
        private readonly Action _updateBulkArmingUi;
        private readonly Action _applyResponsiveProfileCardLayout;
        private readonly Action? _refreshTheme;

        private int _pending; // 0/1 guard to coalesce refresh requests

        public MainFormRefresher(
            WinFormsUiDispatcher ui,
            Action refreshProfileList,
            Action updateBulkArmingUi,
            Action applyResponsiveProfileCardLayout,
            Action? refreshTheme = null)
        {
            _ui = ui ?? throw new ArgumentNullException(nameof(ui));
            _refreshProfileList = refreshProfileList ?? throw new ArgumentNullException(nameof(refreshProfileList));
            _updateBulkArmingUi = updateBulkArmingUi ?? throw new ArgumentNullException(nameof(updateBulkArmingUi));
            _applyResponsiveProfileCardLayout = applyResponsiveProfileCardLayout ?? throw new ArgumentNullException(nameof(applyResponsiveProfileCardLayout));
            _refreshTheme = refreshTheme;
        }

        public void RequestRefresh(RefreshReason reason)
        {
            // Coalesce: many actions can trigger refresh bursts (toggle + save + filter).
            if (System.Threading.Interlocked.Exchange(ref _pending, 1) == 1)
                return;

            _ui.Post(() =>
            {
                try
                {
                    if (reason == RefreshReason.ThemeChanged)
                        _refreshTheme?.Invoke();

                    // 1) Rebuild cards based on current view/filter state.
                    _refreshProfileList();

                    // 2) Re-evaluate bulk arming (depends on eligibility + showCheckedOnly).
                    _updateBulkArmingUi();

                    // 3) Force layout reflow after rebuild.
                    _applyResponsiveProfileCardLayout();
                }
                finally
                {
                    System.Threading.Interlocked.Exchange(ref _pending, 0);
                }
            });
        }
    }
}
