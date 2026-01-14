using System.ComponentModel;
using GWxLauncher.Domain;
using GWxLauncher.Services;

namespace GWxLauncher.UI.Dialogs
{
    /// <summary>
    /// Dialog showing progress during GW2 game folder copy operation.
    /// Uses BackgroundWorker for thread-safe UI updates.
    /// </summary>
    internal partial class Gw2FolderCopyProgressDialog : Form
    {
        private readonly BackgroundWorker _worker;
        private readonly Gw2GameFolderCopyService _copyService;
        private readonly GameProfile _profile;
        private readonly string _sourceFolder;
        private readonly string _destinationFolder;

        public Gw2FolderCopyResult? CopyResult { get; private set; }

        public Gw2FolderCopyProgressDialog(
            GameProfile profile,
            string sourceFolder,
            string destinationFolder)
        {
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
            _sourceFolder = sourceFolder ?? throw new ArgumentNullException(nameof(sourceFolder));
            _destinationFolder = destinationFolder ?? throw new ArgumentNullException(nameof(destinationFolder));

            InitializeComponent();

            _copyService = new Gw2GameFolderCopyService();

            _worker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            _worker.DoWork += Worker_DoWork;
            _worker.ProgressChanged += Worker_ProgressChanged;
            _worker.RunWorkerCompleted += Worker_RunWorkerCompleted;

            lblProfileName.Text = $"Copying Profile: {_profile.Name}";
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Start copy operation when dialog loads
            _worker.RunWorkerAsync();
        }

        private void Worker_DoWork(object? sender, DoWorkEventArgs e)
        {
            var worker = (BackgroundWorker)sender!;
            var result = _copyService.CopyGameFolder(worker, _sourceFolder, _destinationFolder);
            e.Result = result;
        }

        private void Worker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            // UI thread - safe to update controls
            if (e.UserState is Gw2FolderCopyProgress progress)
            {
                lblStatus.Text = progress.StatusMessage;
                progressBar.Value = Math.Min(progress.PercentComplete, 100);
            }
        }

        private void Worker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            // UI thread - safe to close dialog
            if (e.Error != null)
            {
                CopyResult = new Gw2FolderCopyResult
                {
                    Success = false,
                    ErrorMessage = $"Copy failed with exception: {e.Error.Message}"
                };

                MessageBox.Show(
                    CopyResult.ErrorMessage,
                    "Copy Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                DialogResult = DialogResult.Cancel;
            }
            else if (e.Result is Gw2FolderCopyResult result)
            {
                CopyResult = result;

                if (result.Success)
                {
                    DialogResult = DialogResult.OK;
                }
                else
                {
                    MessageBox.Show(
                        result.ErrorMessage,
                        "Copy Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    DialogResult = DialogResult.Cancel;
                }
            }

            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (_worker.IsBusy)
            {
                var result = MessageBox.Show(
                    "Are you sure you want to cancel the copy operation?\n\n" +
                    "The destination folder may be incomplete.",
                    "Cancel Copy?",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    _worker.CancelAsync();
                    lblStatus.Text = "Cancelling...";
                    btnCancel.Enabled = false;
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Prevent closing while work is in progress (unless cancelled)
            if (_worker.IsBusy && !_worker.CancellationPending && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
            }

            base.OnFormClosing(e);
        }
    }
}
