namespace GWxLauncher.UI.Helpers
{
    internal static class FilePickerHelper
    {
        public static bool TryPickDll(IWin32Window owner, TextBox target, string title)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            if (target == null) throw new ArgumentNullException(nameof(target));

            return TryPickFileIntoTextBox(
                owner: owner,
                target: target,
                title: title,
                filter: "DLL files (*.dll)|*.dll|All files (*.*)|*.*",
                fallbackInitialDirectory: null);
        }

        public static bool TryPickDll(IWin32Window owner, TextBox target, string title, string? fallbackInitialDirectory)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            if (target == null) throw new ArgumentNullException(nameof(target));

            return TryPickFileIntoTextBox(
                owner: owner,
                target: target,
                title: title,
                filter: "DLL files (*.dll)|*.dll|All files (*.*)|*.*",
                fallbackInitialDirectory: fallbackInitialDirectory);
        }

        public static bool TryPickExe(IWin32Window owner, TextBox target, string title, string? fallbackInitialDirectory)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            if (target == null) throw new ArgumentNullException(nameof(target));

            return TryPickFileIntoTextBox(
                owner: owner,
                target: target,
                title: title,
                filter: "Executable files (*.exe)|*.exe|All files (*.*)|*.*",
                fallbackInitialDirectory: fallbackInitialDirectory);
        }

        private static bool TryPickFileIntoTextBox(
            IWin32Window owner,
            TextBox target,
            string title,
            string filter,
            string? fallbackInitialDirectory)
        {
            using var dlg = new OpenFileDialog
            {
                Filter = filter,
                Title = title
            };

            var current = (target.Text ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(current) && File.Exists(current))
            {
                dlg.FileName = current;

                try
                {
                    var dir = Path.GetDirectoryName(current);
                    if (!string.IsNullOrWhiteSpace(dir) && Directory.Exists(dir))
                        dlg.InitialDirectory = dir;
                }
                catch
                {
                    // best-effort
                }
            }
            else
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(fallbackInitialDirectory) && Directory.Exists(fallbackInitialDirectory))
                        dlg.InitialDirectory = fallbackInitialDirectory;
                }
                catch
                {
                    // best-effort
                }
            }

            if (dlg.ShowDialog(owner) != DialogResult.OK)
                return false;

            target.Text = dlg.FileName;
            return true;
        }
    }
}
