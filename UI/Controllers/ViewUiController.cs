using GWxLauncher.Services;

namespace GWxLauncher.UI.Controllers
{
    internal sealed class ViewUiController
    {
        private readonly ViewStateStore _views;
        private readonly TextBox _txtView;
        private readonly CheckBox _chkShowCheckedOnly;

        private readonly Action<bool> _setShowCheckedOnly;
        private Action<RefreshReason>? _requestRefresh;
        private readonly Action<string> _setStatus;

        private bool _suppressViewTextEvents;
        private bool _suppressCheckboxEvents;

        private bool _viewNameDirty;
        private string _viewNameBeforeEdit = "";

        public ViewUiController(
            ViewStateStore views,
            TextBox txtView,
            CheckBox chkShowCheckedOnly,
            Action<bool> setShowCheckedOnly,
            Action<RefreshReason>? requestRefresh,
            Action<string> setStatus)
        {
            _views = views ?? throw new ArgumentNullException(nameof(views));
            _txtView = txtView ?? throw new ArgumentNullException(nameof(txtView));
            _chkShowCheckedOnly = chkShowCheckedOnly ?? throw new ArgumentNullException(nameof(chkShowCheckedOnly));

            _setShowCheckedOnly = setShowCheckedOnly ?? throw new ArgumentNullException(nameof(setShowCheckedOnly));
            _requestRefresh = requestRefresh;
            _setStatus = setStatus ?? throw new ArgumentNullException(nameof(setStatus));
        }

        public void SetRequestRefresh(Action<RefreshReason> requestRefresh)
        {
            _requestRefresh = requestRefresh ?? throw new ArgumentNullException(nameof(requestRefresh));
        }

        public void InitializeFromStore()
        {
            _suppressViewTextEvents = true;
            _txtView.Text = _views.ActiveViewName;
            ApplyViewScopedUiState();
            _suppressViewTextEvents = false;
        }

        public void ApplyViewScopedUiState()
        {
            bool showCheckedOnly = _views.GetShowCheckedOnly(_views.ActiveViewName);

            _setShowCheckedOnly(showCheckedOnly);

            _suppressCheckboxEvents = true;
            _chkShowCheckedOnly.Checked = showCheckedOnly;
            _suppressCheckboxEvents = false;
        }

        public void OnShowCheckedOnlyChanged()
        {
            if (_suppressCheckboxEvents)
                return;

            bool show = _chkShowCheckedOnly.Checked;

            _setShowCheckedOnly(show);

            _views.SetShowCheckedOnly(_views.ActiveViewName, show);
            _views.Save();

            _requestRefresh?.Invoke(RefreshReason.ShowCheckedOnlyChanged);
        }

        public void StepView(int delta)
        {
            var newName = _views.StepActiveView(delta);
            _views.Save();

            _suppressViewTextEvents = true;
            _txtView.Text = newName;
            ApplyViewScopedUiState();
            _suppressViewTextEvents = false;

            _requestRefresh?.Invoke(RefreshReason.ViewChanged);
        }

        public void OnViewTextChanged()
        {
            if (_suppressViewTextEvents)
                return;

            _viewNameDirty = true;
        }

        public void OnViewEnter()
        {
            _viewNameBeforeEdit = _views.ActiveViewName;
            _viewNameDirty = false;
        }

        public void OnViewLeave()
        {
            CommitViewRenameIfDirty();
        }

        public void OnViewKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            e.Handled = true;
            e.SuppressKeyPress = true;
            CommitViewRenameIfDirty();
        }

        public void CreateNewView()
        {
            var newName = _views.CreateNewView("New View");
            _views.Save();

            _suppressViewTextEvents = true;
            _txtView.Text = newName;
            ApplyViewScopedUiState();
            _suppressViewTextEvents = false;

            _requestRefresh?.Invoke(RefreshReason.ViewChanged);
        }

        private void CommitViewRenameIfDirty()
        {
            if (!_viewNameDirty)
                return;

            _viewNameDirty = false;

            string newName = (_txtView.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(newName))
            {
                // Revert
                _suppressViewTextEvents = true;
                _txtView.Text = _views.ActiveViewName;
                _suppressViewTextEvents = false;
                return;
            }

            if (!_views.RenameActiveView(newName))
            {
                _suppressViewTextEvents = true;
                _txtView.Text = _views.ActiveViewName;
                _suppressViewTextEvents = false;

                _setStatus("View rename failed (name already exists).");
                return;
            }

            _views.Save();
            _requestRefresh?.Invoke(RefreshReason.ViewChanged);
        }
    }
}
