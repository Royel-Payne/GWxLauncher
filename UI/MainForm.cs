using GWxLauncher.Config;
using GWxLauncher.Domain;
using GWxLauncher.Services;
using GWxLauncher.UI;
using System.Windows.Forms.VisualStyles;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Diagnostics;

namespace GWxLauncher
{
    public partial class MainForm : Form
    {

        private LauncherConfig _config;
        private readonly ProfileManager _profileManager = new();

        private readonly Image _gw1Image = Properties.Resources.Gw1;
        private readonly Image _gw2Image = Properties.Resources.Gw2;

        private LaunchReport? _lastLaunchReport;
        private readonly List<LaunchReport> _lastLaunchReports = new();

        private readonly ViewStateStore _views = new();
        private bool _showCheckedOnly = false;
        private string _viewNameBeforeEdit = "";
        private bool _viewNameDirty = false;
        private bool _suppressViewTextEvents = false;
        private bool _suppressArmBulkEvents = false;
        private readonly Font _nameFont;
        private readonly Font _subFont;

        private int _hotIndex = -1;

        private const bool ShowSelectionBorder = false;

        private void RefreshProfileList()
        {
            int topIndex = lstProfiles.TopIndex;
            object? selectedItem = lstProfiles.SelectedItem;

            lstProfiles.BeginUpdate();
            try
            {
                lstProfiles.Items.Clear();

                var profiles = _profileManager.Profiles
                    .OrderBy(p => p.GameType) // GW1 before GW2
                    .ThenBy(p => p.Name, StringComparer.CurrentCultureIgnoreCase)
                    .AsEnumerable();

                // Visibility is controlled ONLY by Show Checked Accounts Only (view-scoped)
                if (_showCheckedOnly)
                {
                    profiles = profiles.Where(p => _views.IsEligible(_views.ActiveViewName, p.Id));
                }

                foreach (var profile in profiles)
                    lstProfiles.Items.Add(profile);

                // Restore selection by object identity (works because you re-add the same profile instances)
                if (selectedItem != null)
                {
                    int idx = lstProfiles.Items.IndexOf(selectedItem);
                    if (idx >= 0)
                        lstProfiles.SelectedIndex = idx;
                }

                // Restore scroll position (clamp to list size)
                if (lstProfiles.Items.Count > 0)
                {
                    if (topIndex < 0) topIndex = 0;
                    if (topIndex >= lstProfiles.Items.Count) topIndex = lstProfiles.Items.Count - 1;
                    lstProfiles.TopIndex = topIndex;
                }

                UpdateBulkArmingUi();
            }
            finally
            {
                lstProfiles.EndUpdate();
            }
        }



        private void UpdateBulkArmingUi()
        {
            // Always allow toggling this checkbox. It is a visibility control (UI-only).
            chkArmBulk.Enabled = true;

            bool anyEligible = _views.AnyEligibleInActiveView(_profileManager.Profiles);

            // Bulk armed (future) is a derived state: (any eligible) + (show checked only)
            bool armed = anyEligible && _showCheckedOnly;

            btnLaunchAll.Enabled = armed;

            if (armed)
                lblStatus.Text = $"Launch All ready · View: {_views.ActiveViewName}";
            if (!armed)
                lblStatus.Text = $"Launch All not ready · View: {_views.ActiveViewName}";
            else if (_showCheckedOnly && !anyEligible)
                lblStatus.Text = $"No checked profiles in view · View: {_views.ActiveViewName}";
        }
        private void chkArmBulk_CheckedChanged(object sender, EventArgs e)
        {
            if (_suppressArmBulkEvents)
                return;

            _showCheckedOnly = chkArmBulk.Checked;

            _views.SetShowCheckedOnly(_views.ActiveViewName, _showCheckedOnly);
            _views.Save();

            RefreshProfileList();
        }

        private void ReenableListScrollbarAndFillPanel()
        {
            // Stop using the "hide scrollbar by widening listbox" trick while iterating.
            // Keep listbox exactly sized to the panel.
            lstProfiles.Dock = DockStyle.Fill;
            lstProfiles.IntegralHeight = false;

            // Remove any previous "widened listbox" sizing effects
            lstProfiles.Width = panelProfiles.ClientSize.Width;
            lstProfiles.Height = panelProfiles.ClientSize.Height;

            // Keep it correct on resize
            panelProfiles.Resize -= PanelProfiles_Resize_FillList;
            panelProfiles.Resize += PanelProfiles_Resize_FillList;
        }

        private void PanelProfiles_Resize_FillList(object? sender, EventArgs e)
        {
            // Ensure fill stays accurate even if Dock is changed later
            if (lstProfiles.Dock == DockStyle.None)
            {
                lstProfiles.SetBounds(0, 0, panelProfiles.ClientSize.Width, panelProfiles.ClientSize.Height);
            }
        }


        public MainForm()
        {
            InitializeComponent();
            var baseFont = Font; // form font
            try
            {
                _nameFont = new Font("Segoe UI Variable", baseFont.Size + 1, FontStyle.Bold);
                _subFont = new Font("Segoe UI Variable", baseFont.Size - 1, FontStyle.Regular);
            }
            catch
            {
                _nameFont = new Font(baseFont.FontFamily, baseFont.Size + 1, FontStyle.Bold);
                _subFont = new Font(baseFont.FontFamily, baseFont.Size - 1, FontStyle.Regular);
            }

            lstProfiles.MouseMove += lstProfiles_MouseMove;
            lstProfiles.MouseLeave += lstProfiles_MouseLeave;

            chkArmBulk.BringToFront(); // ensure it's not obscured

            ctxProfiles.Opening += ctxProfiles_Opening;

            _config = LauncherConfig.Load();

            ThemeService.ApplyToForm(this);

            ReenableListScrollbarAndFillPanel();

            panelView.Paint += (s, e) =>
            {
                var r = panelView.ClientRectangle;
                using var pen = new Pen(ThemeService.Palette.Separator);
                e.Graphics.DrawLine(pen, r.Left, r.Bottom - 1, r.Right, r.Bottom - 1);
            };
            lblStatus.Paint += (s, e) =>
            {
                using var pen = new Pen(ThemeService.Palette.Separator);
                e.Graphics.DrawLine(pen, 0, 0, lblStatus.Width - 1, 0);
            };
            lblStatus.Resize += (s, e) => lblStatus.Invalidate();

            panelProfiles.Resize += (s, e) => panelProfiles.Invalidate();

            lblView.Visible = true;
            lblView.Text = "Show Checked \nAccounts Only";
            lblView.AutoSize = true;
            lblView.ForeColor = ThemeService.Palette.SubtleFore;
            var tip = new ToolTip();
            tip.SetToolTip(chkArmBulk, "Show Checked Accounts Only (enables launch all)");
            tip.SetToolTip(lblView, "Show Checked Accounts Only (enables launch all)");

            // (your window-position restore logic)
            if (_config.WindowX >= 0 && _config.WindowY >= 0)
            {
                StartPosition = FormStartPosition.Manual;
                Location = new Point(_config.WindowX, _config.WindowY);
            }

            if (_config.WindowWidth > 0 && _config.WindowHeight > 0)
            {
                Size = new Size(_config.WindowWidth, _config.WindowHeight);
            }

            if (_config.WindowMaximized)
            {
                WindowState = FormWindowState.Maximized;
            }

            _profileManager.Load();
            _views.Load();

            _suppressViewTextEvents = true;
            txtView.Text = _views.ActiveViewName;
            ApplyViewScopedUiState();
            _suppressViewTextEvents = false;

            RefreshProfileList();
        }

        private void lstProfiles_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();

            if (e.Index < 0 || e.Index >= lstProfiles.Items.Count)
                return;

            if (lstProfiles.Items[e.Index] is not GameProfile profile)
                return;

            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            bool hot = (e.Index == _hotIndex);

            var g = e.Graphics;
            using (var rowBg = new SolidBrush(ThemeService.Palette.WindowBack))
            {
                g.FillRectangle(rowBg, e.Bounds);
            }

            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Card bounds
            Rectangle card = e.Bounds;
            card.Inflate(-ThemeService.CardMetrics.OuterPadding, -ThemeService.CardMetrics.OuterPadding);
            card.Width -= ThemeService.CardMetrics.RightGutter;

            Color backColor =
                selected ? ThemeService.CardPalette.SelectedBack :
                hot ? ThemeService.CardPalette.HoverBack :
                           ThemeService.CardPalette.Back;

            Color borderColor =
                selected ? ThemeService.CardPalette.Border :   // subtle border even when selected
                hot ? ThemeService.CardPalette.HoverBorder :
                           ThemeService.CardPalette.Border;

            using (var bgBrush = new SolidBrush(backColor))
            {
                g.FillRectangle(bgBrush, card);

                if (ShowSelectionBorder)
                {
                    using var borderPen = new Pen(borderColor);
                    g.DrawRectangle(borderPen, card);
                }
            }


            // Ensure listbox background matches overall theme
            lstProfiles.BackColor = ThemeService.Palette.WindowBack;
            lstProfiles.ForeColor = ThemeService.Palette.WindowFore;
            lstProfiles.BorderStyle = BorderStyle.None; // optional, but helps reduce contrast noise

            // Accent bar (left) for selected (and optional hover)
            if (selected || hot)
            {
                var accentRect = new Rectangle(
                    card.Left,
                    card.Top,
                    ThemeService.CardMetrics.AccentWidth,
                    card.Height);

                using var accentBrush = new SolidBrush(ThemeService.CardPalette.Accent);
                g.FillRectangle(accentBrush, accentRect);
            }

            // Eligibility checkbox area (bulk launch eligibility; not selection)
            Rectangle cb = new Rectangle(
                card.Left + ThemeService.CardMetrics.CheckboxOffsetX,
                card.Top + ThemeService.CardMetrics.CheckboxOffsetY,
                ThemeService.CardMetrics.CheckboxSize,
                ThemeService.CardMetrics.CheckboxSize);

            bool eligible = _views.IsEligible(_views.ActiveViewName, profile.Id);
            CheckBoxRenderer.DrawCheckBox(
                g,
                cb.Location,
                eligible ? CheckBoxState.CheckedNormal : CheckBoxState.UncheckedNormal);

            // Icon area
            Rectangle iconRect = new Rectangle(
                card.Left + ThemeService.CardMetrics.IconOffsetX,
                card.Top + ThemeService.CardMetrics.IconOffsetY,
                ThemeService.CardMetrics.IconSize,
                ThemeService.CardMetrics.IconSize);

            Image? icon = profile.GameType == GameType.GuildWars1 ? _gw1Image : _gw2Image;
            if (icon != null)
                g.DrawImage(icon, iconRect);

            // Text area
            float textLeft = iconRect.Right + ThemeService.CardMetrics.TextOffsetX;
            float textTop = card.Top + ThemeService.CardMetrics.TextOffsetY;

            var nameFont = _nameFont;
            var subFont = _subFont;

            using (var nameBrush = new SolidBrush(ThemeService.CardPalette.NameFore))
            using (var subBrush = new SolidBrush(ThemeService.CardPalette.SubFore))
            {
                g.DrawString(profile.Name, nameFont, nameBrush, textLeft, textTop);

                string gameLabel = profile.GameType == GameType.GuildWars1 ? "Guild Wars 1" : "Guild Wars 2";
                g.DrawString(
                    gameLabel,
                    subFont,
                    subBrush,
                    textLeft,
                    textTop + nameFont.Height + ThemeService.CardMetrics.SubtitleGapY);
            }

            // Local helper: draw badge pills (right aligned)
            void DrawBadges(IReadOnlyList<string> badges)
            {
                if (badges.Count == 0)
                    return;

                int badgeRight = card.Right - ThemeService.CardMetrics.BadgeRightPadding;
                int badgeTop = card.Top + ThemeService.CardMetrics.BadgeTopPadding;

                using var badgeBg = new SolidBrush(ThemeService.CardPalette.BadgeBack);
                using var badgePen = new Pen(ThemeService.CardPalette.BadgeBorder);

                // Draw right-to-left so the right edge stays aligned
                for (int i = badges.Count - 1; i >= 0; i--)
                {
                    string badge = badges[i];

                    var sz = g.MeasureString(badge, ThemeService.Typography.BadgeFont);
                    int w = (int)sz.Width + ThemeService.CardMetrics.BadgeHorizontalPad;
                    int h = (int)sz.Height + ThemeService.CardMetrics.BadgeVerticalPad;

                    var rect = new Rectangle(badgeRight - w, badgeTop, w, h);

                    g.FillRectangle(badgeBg, rect);
                    g.DrawRectangle(badgePen, rect);

                    TextRenderer.DrawText(
                        g,
                        badge,
                        ThemeService.Typography.BadgeFont,
                        rect,
                        ThemeService.CardPalette.BadgeFore,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                    badgeRight -= w + ThemeService.CardMetrics.BadgeSpacing;
                }
            }

            // --- Badges (GW1): TB, gMod, Py4GW---
            if (profile.GameType == GameType.GuildWars1)
            {
                var badges = new List<string>(2);
                if (profile.Gw1ToolboxEnabled) badges.Add("TB");
                if (profile.Gw1GModEnabled) badges.Add("gMod");
                if (profile.Gw1Py4GwEnabled) badges.Add("Py4");

                DrawBadges(badges);
            }

            // --- Badges (GW2): Blish (when RunAfter is enabled and contains a Blish entry) ---
            if (profile.GameType == GameType.GuildWars2)
            {
                var progs = profile.Gw2RunAfterPrograms ?? new List<RunAfterProgram>();

                bool anyEnabled = profile.Gw2RunAfterEnabled && progs.Any(x => x.Enabled);

                if (anyEnabled)
                {
                    bool blish = progs.Any(x =>
                        x.Enabled &&
                        (
                            (x.Name?.IndexOf("Blish", StringComparison.OrdinalIgnoreCase) ?? -1) >= 0 ||
                            (x.ExePath?.IndexOf("Blish", StringComparison.OrdinalIgnoreCase) ?? -1) >= 0
                        ));

                    if (blish)
                    {
                        DrawBadges(new List<string> { "Blish" });
                    }
                }
            }

            //if (selected)
            //    e.DrawFocusRectangle();
        }


        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                _config.WindowX = Left;
                _config.WindowY = Top;
                _config.WindowWidth = Width;
                _config.WindowHeight = Height;
                _config.WindowMaximized = false;
            }
            else if (WindowState == FormWindowState.Maximized)
            {
                _config.WindowMaximized = true;

                // Remember where the window would be in normal state
                var bounds = RestoreBounds;
                _config.WindowX = bounds.Left;
                _config.WindowY = bounds.Top;
                _config.WindowWidth = bounds.Width;
                _config.WindowHeight = bounds.Height;
            }
            _nameFont?.Dispose();
            _subFont?.Dispose();
            _config.Save();
        }
        private void ctxProfiles_Opening(object sender, CancelEventArgs e)
        {
            var profile = GetSelectedProfile();
            bool hasProfile = profile != null;

            menuLaunchProfile.Enabled = hasProfile;
            menuEditProfile.Enabled = hasProfile;

            // This is the real "Delete" menu item name in your Designer
            deleteToolStripMenuItem.Enabled = hasProfile;

            // Enable if we have any attempts recorded
            menuShowLastLaunchDetails.Enabled = _lastLaunchReports.Count > 0;
        }
        private void btnLaunchGw1_Click(object sender, EventArgs e)
        {
            LaunchGame(_config.Gw1Path, "Guild Wars 1");
        }
        private void btnLaunchGw2_Click(object sender, EventArgs e)
        {
            LaunchGame(_config.Gw2Path, "Guild Wars 2");
        }
        private GameProfile? GetSelectedProfile()
        {
            return lstProfiles.SelectedItem as GameProfile;
        }
        // Lets the user pick an executable for a profile, then saves & refreshes the list.
        // Returns true if a path was chosen, false if the user cancelled.
        private bool TrySelectProfileExecutable(GameProfile profile)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = $"Select executable for {profile.Name}";
                dialog.Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*";

                // Start from the profile's own path if we have one…
                string initialPath = profile.ExecutablePath;

                // …otherwise fall back to the legacy global config just for convenience
                if (string.IsNullOrWhiteSpace(initialPath))
                {
                    initialPath = profile.GameType == GameType.GuildWars1
                        ? _config.Gw1Path
                        : _config.Gw2Path;
                }

                if (!string.IsNullOrWhiteSpace(initialPath))
                {
                    try
                    {
                        dialog.InitialDirectory = Path.GetDirectoryName(initialPath);
                    }
                    catch
                    {
                        // ignore bad paths
                    }
                }

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    profile.ExecutablePath = dialog.FileName;
                    _profileManager.Save();
                    RefreshProfileList();
                    lblStatus.Text = $"Updated path for {profile.Name}.";

                    return true;
                }

                lblStatus.Text = $"No path selected for {profile.Name}.";
                return false;
            }
        }
        private bool TrySelectGw1ToolboxDll(GameProfile profile)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = $"Select GW1 Toolbox DLL for {profile.Name}";
                dialog.Filter = "DLL files (*.dll)|*.dll|All files (*.*)|*.*";

                if (!string.IsNullOrWhiteSpace(profile.Gw1ToolboxDllPath))
                {
                    try
                    {
                        dialog.InitialDirectory = Path.GetDirectoryName(profile.Gw1ToolboxDllPath);
                    }
                    catch
                    {
                        // ignore bad paths
                    }
                }

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    profile.Gw1ToolboxDllPath = dialog.FileName;
                    _profileManager.Save();
                    lblStatus.Text = $"Set GW1 Toolbox DLL for {profile.Name}.";
                    return true;
                }

                lblStatus.Text = $"No Toolbox DLL selected for {profile.Name}.";
                return false;
            }
        }

        private void LaunchProfile(GameProfile profile, bool bulkMode)
        {
            if (profile == null)
                return;

            // Single launch = new "session". Bulk launch = append attempts to the same session.
            if (!bulkMode)
                _lastLaunchReports.Clear();

            _config = LauncherConfig.Load();

            string exePath = profile.ExecutablePath;

            if (string.IsNullOrWhiteSpace(exePath))
            {
                exePath = profile.GameType == GameType.GuildWars1
                    ? _config.Gw1Path
                    : _config.Gw2Path;
            }

            if (string.IsNullOrWhiteSpace(exePath) || !File.Exists(exePath))
            {
                MessageBox.Show(
                    "No valid executable path is configured for this profile.\n\n" +
                    "Edit the profile or configure the game path in settings.",
                    "Missing executable",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // GW1: delegate launch + injection to Gw1InjectionService
            if (profile.GameType == GameType.GuildWars1)
            {
                var gw1Service = new Gw1InjectionService();
                bool mcEnabled = _config.Gw1MulticlientEnabled;

                if (gw1Service.TryLaunchGw1(profile, exePath, mcEnabled, this, out var gw1Error, out var report))
                {
                    _lastLaunchReport = report;
                    _lastLaunchReports.Add(report);
                    lblStatus.Text = report.BuildSummary();
                }
                else
                {
                    _lastLaunchReport = report;
                    _lastLaunchReports.Add(report);

                    MessageBox.Show(
                        this,
                        gw1Error,
                        "Guild Wars 1 launch",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    lblStatus.Text = report.BuildSummary();
                }

                return;
            }

            // GW2: multiclient is mutex + args (no patching)
            if (profile.GameType == GameType.GuildWars2)
            {
                var report = new LaunchReport
                {
                    GameName = "Guild Wars 2",
                    ExecutablePath = exePath
                };

                bool mcEnabled = _config.Gw2MulticlientEnabled;

                var mcStep = new LaunchStep { Label = "Multiclient" };
                report.Steps.Add(mcStep);

                const string Gw2MutexName = GWxLauncher.Services.Gw2MutexKiller.Gw2MutexLeafName;

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

                    _lastLaunchReport = report;
                    _lastLaunchReports.Add(report);
                    lblStatus.Text = report.BuildSummary();

                    MessageBox.Show(this, report.FailureMessage, "Guild Wars 2 launch", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
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
                        int attempts = bulkMode ? 4 : 1;        // bulk: retry a few times
                        int delayMs = bulkMode ? 350 : 0;       // bulk: tiny backoff

                        bool cleared = false;
                        int clearedPid = 0;
                        string killDetail = "";
                        bool usedElevated = false;

                        for (int i = 0; i < attempts; i++)
                        {
                            if (GWxLauncher.Services.Gw2MutexKiller.TryKillGw2Mutex(
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

                            _lastLaunchReport = report;
                            _lastLaunchReports.Add(report);
                            lblStatus.Text = report.BuildSummary();

                            MessageBox.Show(this, msg, "Guild Wars 2 launch", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
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
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = exePath,
                        WorkingDirectory = Path.GetDirectoryName(exePath) ?? "",
                        Arguments = mcEnabled ? "-shareArchive" : ""
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
                        var loginSvc = new Gw2AutoLoginService();

                        // Best-effort: do not fail the entire launch if automation fails.
                        if (!loginSvc.TryAutomateLogin(process, profile, report, out var autoLoginError))
                        {
                            if (!string.IsNullOrWhiteSpace(autoLoginError))
                                report.FailureMessage = $"Auto-login failed: {autoLoginError}";
                        }
                    }

                    StartGw2RunAfterPrograms(profile);

                    report.Succeeded = true;

                    _lastLaunchReport = report;
                    _lastLaunchReports.Add(report);
                    lblStatus.Text = report.BuildSummary();
                }
                catch (Exception ex)
                {
                    report.Succeeded = false;
                    report.FailureMessage = ex.Message;
                    mcStep.Outcome = StepOutcome.Failed;
                    mcStep.Detail = "Process.Start failed.";

                    _lastLaunchReport = report;
                    _lastLaunchReports.Add(report);
                    lblStatus.Text = report.BuildSummary();

                    MessageBox.Show(
                        this,
                        $"Failed to launch Guild Wars 2:\n\n{ex.Message}",
                        "Launch failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }

                return;
            }
        }


        private void StartGw2RunAfterPrograms(GameProfile profile)
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
                    Process.Start(p.ExePath);
                }
                catch
                {
                    // swallow for now; later we can surface this in LaunchReport-like UI
                }
            }
        }

        private void lstProfiles_DoubleClick(object sender, EventArgs e)
        {
            var profile = GetSelectedProfile();
            if (profile != null)
            {
                LaunchProfile(profile, bulkMode: false);
            }
        }
        private void lstProfiles_MouseDown(object sender, MouseEventArgs e)
        {
            int index = lstProfiles.IndexFromPoint(e.Location);
            if (index < 0) return;

            // Map click to item bounds
            Rectangle itemRect = lstProfiles.GetItemRectangle(index);
            var profile = lstProfiles.Items[index] as GameProfile;
            if (profile == null) return;

            // Checkbox rectangle must match drawing logic
            Rectangle card = itemRect;
            card.Inflate(-ThemeService.CardMetrics.OuterPadding, -ThemeService.CardMetrics.OuterPadding);

            Rectangle cb = new Rectangle(
                card.Left + ThemeService.CardMetrics.CheckboxOffsetX,
                card.Top + ThemeService.CardMetrics.CheckboxOffsetY,
                ThemeService.CardMetrics.CheckboxSize,
                ThemeService.CardMetrics.CheckboxSize);

            if (e.Button == MouseButtons.Left && cb.Contains(e.Location))
            {
                _views.ToggleEligible(_views.ActiveViewName, profile.Id);
                _views.Save();

                if (_showCheckedOnly)
                {
                    // In "show checked only" mode, eligibility changes affect visibility,
                    // so we must rebuild the list.
                    RefreshProfileList();
                }
                else
                {
                    // Otherwise, a lightweight repaint is enough.
                    lstProfiles.Invalidate(lstProfiles.GetItemRectangle(index));
                }

                // Keep bulk launch UI state accurate either way.
                UpdateBulkArmingUi();
            }

            // Existing right-click selection behavior
            if (e.Button == MouseButtons.Right)
            {
                lstProfiles.SelectedIndex = index;
            }
        }
        private void StepView(int delta)
        {
            // Step through saved View names (Team1/Team2/Team3), not profiles
            var newName = _views.StepActiveView(delta);

            _suppressViewTextEvents = true;
            txtView.Text = newName;
            ApplyViewScopedUiState();
            _suppressViewTextEvents = false;

            // Switching view changes which profiles are checked (per-view),
            // but does not hide profiles by name.
            RefreshProfileList();
        }

        private void btnViewPrev_Click(object sender, EventArgs e)
        {
            StepView(-1);
        }

        private void btnViewNext_Click(object sender, EventArgs e)
        {
            StepView(+1);
        }
        private void ApplyViewScopedUiState()
        {
            _showCheckedOnly = _views.GetShowCheckedOnly(_views.ActiveViewName);

            _suppressArmBulkEvents = true;
            chkArmBulk.Checked = _showCheckedOnly;
            _suppressArmBulkEvents = false;
        }


        private void txtView_TextChanged(object sender, EventArgs e)
        {
            if (_suppressViewTextEvents)
                return;

            // Don’t mutate view store during typing.
            _viewNameDirty = true;
        }
        private void CommitViewRenameIfDirty()
        {
            if (!_viewNameDirty)
                return;

            _viewNameDirty = false;

            string newName = (txtView.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(newName))
            {
                // Revert to old name if user blanked it
                _suppressViewTextEvents = true;
                txtView.Text = _views.ActiveViewName;
                _suppressViewTextEvents = false;
                return;
            }

            // Try rename. If it fails (duplicate name), revert & show a status hint.
            if (!_views.RenameActiveView(newName))
            {
                _suppressViewTextEvents = true;
                txtView.Text = _views.ActiveViewName;
                _suppressViewTextEvents = false;

                lblStatus.Text = "View rename failed (name already exists).";
                return;
            }

            _views.Save();
            RefreshProfileList();
        }
        private void txtView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                CommitViewRenameIfDirty();
            }
        }

        private void txtView_Leave(object sender, EventArgs e)
        {
            CommitViewRenameIfDirty();
        }
        private void txtView_Enter(object sender, EventArgs e)
        {
            _viewNameBeforeEdit = _views.ActiveViewName;
            _viewNameDirty = false;
        }
        private void btnNewView_Click(object sender, EventArgs e)
        {
            var newName = _views.CreateNewView("New View");
            _views.Save();

            _suppressViewTextEvents = true;
            txtView.Text = newName;
            ApplyViewScopedUiState();
            _suppressViewTextEvents = false;

            RefreshProfileList();
        }
        private void menuShowLastLaunchDetails_Click(object sender, EventArgs e)
        {
            if (_lastLaunchReports.Count == 0)
                return;

            using var dlg = new LastLaunchDetailsForm(_lastLaunchReports);
            dlg.ShowDialog(this);
        }
        private void menuLaunchProfile_Click(object sender, EventArgs e)
        {
            var profile = GetSelectedProfile();
            if (profile != null)
            {
                LaunchProfile(profile, bulkMode: false);
            }
        }
        private void menuSetProfilePath_Click(object sender, EventArgs e)
        {
            var profile = GetSelectedProfile();
            if (profile == null)
                return;

            TrySelectProfileExecutable(profile);
        }
        private void menuEditProfile_Click(object sender, EventArgs e)
        {
            var profile = lstProfiles.SelectedItem as GameProfile;
            if (profile == null)
                return;

            using var dlg = new ProfileSettingsForm(profile);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                _profileManager.Save();
                RefreshProfileList();
            }
        }
        private void menuDeleteProfile_Click(object sender, EventArgs e)
        {
            var profile = GetSelectedProfile();
            if (profile == null)
                return;

            var result = MessageBox.Show(
                $"Delete account \"{profile.Name}\"?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _profileManager.RemoveProfile(profile);
                _profileManager.Save();
                RefreshProfileList();
                lblStatus.Text = $"Deleted account: {profile.Name}.";
            }
        }
        private void menuGw1ToolboxToggle_Click(object sender, EventArgs e)
        {
            var profile = GetSelectedProfile();
            if (profile == null || profile.GameType != GameType.GuildWars1)
            {
                // Safety: if somehow clicked with no GW1 profile, just uncheck.
                // menuGw1ToolboxToggle.Checked = false;
                return;
            }

            //profile.Gw1ToolboxEnabled = menuGw1ToolboxToggle.Checked;
            _profileManager.Save();

            lblStatus.Text = $"GW1 Toolbox injection " +
                             (profile.Gw1ToolboxEnabled ? "enabled" : "disabled") +
                             $" for {profile.Name}.";
        }
        private void menuGw1ToolboxPath_Click(object sender, EventArgs e)
        {
            var profile = GetSelectedProfile();
            if (profile == null || profile.GameType != GameType.GuildWars1)
                return;

            TrySelectGw1ToolboxDll(profile);
        }
        private void LaunchGame(string exePath, string gameName)
        {
            try
            {
                // 🔹 New: handle missing/blank path from config
                if (string.IsNullOrWhiteSpace(exePath))
                {
                    MessageBox.Show(
                        $"{gameName} path is not set.\n\n" +
                        "Please set the correct EXE path.",
                        "Path Not Set",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                // Existing check: path is set, but file not found on disk
                if (!File.Exists(exePath))
                {
                    MessageBox.Show(
                        $"Could not find {gameName} executable.\n\nPath:\n{exePath}",
                        "Executable Not Found",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    UseShellExecute = false
                };

                var process = Process.Start(startInfo);

                if (lblStatus != null)
                    lblStatus.Text = $"{gameName} launched.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to launch {gameName}.\n\n{ex.Message}",
                    "Launch Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                if (lblStatus != null)
                    lblStatus.Text = $"Error launching {gameName}.";
            }
        }
        private void btnLaunchAll_Click(object sender, EventArgs e)
        {
            // Guard: bulk launch must be explicitly armed.
            bool anyEligible = _views.AnyEligibleInActiveView(_profileManager.Profiles);
            bool armed = anyEligible && _showCheckedOnly;

            if (!armed)
            {
                lblStatus.Text = $"Bulk launch not armed · View: {_views.ActiveViewName}";
                return;
            }

            var targets = _profileManager.Profiles
                .Where(p => _views.IsEligible(_views.ActiveViewName, p.Id))
                .OrderBy(p => p.GameType)
                .ThenBy(p => p.Name, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            if (targets.Count == 0)
            {
                lblStatus.Text = $"No checked profiles in view · View: {_views.ActiveViewName}";
                return;
            }

            _lastLaunchReports.Clear();

            foreach (var profile in targets)
                LaunchProfile(profile, bulkMode: true);

        }
        private void btnSetGw1Path_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "Select Guild Wars 1 executable";
                dialog.Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*";

                // If we already have a path, use its folder as the starting point
                if (!string.IsNullOrWhiteSpace(_config.Gw1Path))
                {
                    try
                    {
                        dialog.InitialDirectory = Path.GetDirectoryName(_config.Gw1Path);
                    }
                    catch
                    {
                        // ignore invalid paths
                    }
                }

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    _config.Gw1Path = dialog.FileName;
                    _config.Save();
                    lblStatus.Text = "GW1 path updated.";
                }
            }
        }
        private void btnSetGw2Path_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "Select Guild Wars 2 executable";
                dialog.Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*";

                if (!string.IsNullOrWhiteSpace(_config.Gw2Path))
                {
                    try
                    {
                        dialog.InitialDirectory = Path.GetDirectoryName(_config.Gw2Path);
                    }
                    catch
                    {
                        // ignore invalid paths
                    }
                }

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    _config.Gw2Path = dialog.FileName;
                    _config.Save();
                    lblStatus.Text = "GW2 path updated.";
                }
            }
        }
        private void btnAddAcount_Click(object sender, EventArgs e)
        {
            using (var dialog = new AddAccountDialog())
            {
                var result = dialog.ShowDialog(this);
                if (result == DialogResult.OK && dialog.CreatedProfile != null)
                {
                    _profileManager.AddProfile(dialog.CreatedProfile);
                    _profileManager.Save();     // 🔹 persist to profiles.json

                    RefreshProfileList();
                    UpdateBulkArmingUi();
                    lblStatus.Text = $"Added account: {dialog.CreatedProfile.Name}";
                }
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
        private void MainForm_Load(object sender, EventArgs e)
        {

        }
        private void lstProfiles_MouseMove(object? sender, MouseEventArgs e)
        {
            int idx = lstProfiles.IndexFromPoint(e.Location);

            if (idx == _hotIndex)
                return;

            int old = _hotIndex;
            _hotIndex = idx;

            // Old hot index might be invalid if the list was refreshed/filtered.
            if (old >= 0 && old < lstProfiles.Items.Count)
                lstProfiles.Invalidate(lstProfiles.GetItemRectangle(old));

            // New hot index might be -1 (no item) or invalid if list changed mid-event.
            if (_hotIndex >= 0 && _hotIndex < lstProfiles.Items.Count)
                lstProfiles.Invalidate(lstProfiles.GetItemRectangle(_hotIndex));
        }
    
        private void lstProfiles_MouseLeave(object? sender, EventArgs e)
        {
            if (_hotIndex < 0)
                return;

            int old = _hotIndex;
            _hotIndex = -1;

            if (old >= 0 && old < lstProfiles.Items.Count)
                lstProfiles.Invalidate(lstProfiles.GetItemRectangle(old));
        }
    }
}

