using System.ComponentModel;

namespace GWxLauncher.Services
{
    /// <summary>
    /// Progress update from folder copy operation.
    /// </summary>
    internal class Gw2FolderCopyProgress
    {
        public long TotalBytes { get; set; }
        public long CopiedBytes { get; set; }
        public int PercentComplete { get; set; }
        public string CurrentFileName { get; set; } = "";
        public long CurrentFileSize { get; set; }
        public long CurrentFileCopiedBytes { get; set; }
        public string StatusMessage { get; set; } = "";
    }

    /// <summary>
    /// Result of a folder copy operation.
    /// </summary>
    internal class Gw2FolderCopyResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = "";
        public string DestinationPath { get; set; } = "";
    }

    /// <summary>
    /// Service for copying GW2 game folders with progress tracking.
    /// Uses BackgroundWorker pattern for UI integration.
    /// </summary>
    internal class Gw2GameFolderCopyService
    {
        private const long LARGE_FILE_THRESHOLD = 100 * 1024 * 1024; // 100MB
        private const int COPY_BUFFER_SIZE = 4 * 1024 * 1024; // 4MB chunks for large files

        /// <summary>
        /// Copy a GW2 game folder to a destination with progress reporting.
        /// Must be called from a BackgroundWorker's DoWork event.
        /// </summary>
        /// <param name="worker">BackgroundWorker for progress reporting</param>
        /// <param name="sourceFolder">Source game folder (containing Gw2-64.exe)</param>
        /// <param name="destinationFolder">Destination folder path</param>
        /// <returns>Copy result with success status</returns>
        public Gw2FolderCopyResult CopyGameFolder(
            BackgroundWorker worker,
            string sourceFolder,
            string destinationFolder)
        {
            try
            {
                // Validate source
                if (!Directory.Exists(sourceFolder))
                {
                    return new Gw2FolderCopyResult
                    {
                        Success = false,
                        ErrorMessage = $"Source folder does not exist: {sourceFolder}"
                    };
                }

                // Create destination
                Directory.CreateDirectory(destinationFolder);

                // Get all files and calculate total size
                var filesToCopy = Directory.GetFiles(sourceFolder, "*", SearchOption.AllDirectories)
                    .Select(path => new FileInfo(path))
                    .ToList();

                long totalBytes = filesToCopy.Sum(f => f.Length);

                // Report initial progress
                var progress = new Gw2FolderCopyProgress
                {
                    TotalBytes = totalBytes,
                    CopiedBytes = 0,
                    PercentComplete = 0,
                    StatusMessage = $"Preparing to copy {filesToCopy.Count} files..."
                };
                worker.ReportProgress(0, progress);

                long copiedBytes = 0;

                // Copy files
                foreach (var sourceFile in filesToCopy)
                {
                    if (worker.CancellationPending)
                    {
                        return new Gw2FolderCopyResult
                        {
                            Success = false,
                            ErrorMessage = "Copy cancelled by user"
                        };
                    }

                    // Calculate relative path
                    string relativePath = Path.GetRelativePath(sourceFolder, sourceFile.FullName);
                    string destFilePath = Path.Combine(destinationFolder, relativePath);

                    // Create destination directory
                    string? destDir = Path.GetDirectoryName(destFilePath);
                    if (!string.IsNullOrEmpty(destDir))
                    {
                        Directory.CreateDirectory(destDir);
                    }

                    // Update progress for this file
                    progress = new Gw2FolderCopyProgress
                    {
                        TotalBytes = totalBytes,
                        CopiedBytes = copiedBytes,
                        PercentComplete = (int)((copiedBytes * 100) / totalBytes),
                        CurrentFileName = sourceFile.Name,
                        CurrentFileSize = sourceFile.Length,
                        CurrentFileCopiedBytes = 0,
                        StatusMessage = $"Copying: {sourceFile.Name}"
                    };
                    worker.ReportProgress(progress.PercentComplete, progress);

                    // Copy file (chunked for large files, direct for small)
                    if (sourceFile.Length > LARGE_FILE_THRESHOLD)
                    {
                        CopyLargeFile(sourceFile.FullName, destFilePath, sourceFile.Length, 
                            copiedBytes, totalBytes, worker);
                    }
                    else
                    {
                        File.Copy(sourceFile.FullName, destFilePath, overwrite: true);
                    }

                    copiedBytes += sourceFile.Length;

                    // Update progress after file complete
                    progress = new Gw2FolderCopyProgress
                    {
                        TotalBytes = totalBytes,
                        CopiedBytes = copiedBytes,
                        PercentComplete = (int)((copiedBytes * 100) / totalBytes),
                        CurrentFileName = sourceFile.Name,
                        CurrentFileSize = sourceFile.Length,
                        CurrentFileCopiedBytes = sourceFile.Length,
                        StatusMessage = $"Copied: {sourceFile.Name}"
                    };
                    worker.ReportProgress(progress.PercentComplete, progress);
                }

                // Complete
                progress = new Gw2FolderCopyProgress
                {
                    TotalBytes = totalBytes,
                    CopiedBytes = totalBytes,
                    PercentComplete = 100,
                    StatusMessage = "Copy complete!"
                };
                worker.ReportProgress(100, progress);

                return new Gw2FolderCopyResult
                {
                    Success = true,
                    DestinationPath = destinationFolder
                };
            }
            catch (Exception ex)
            {
                return new Gw2FolderCopyResult
                {
                    Success = false,
                    ErrorMessage = $"Copy failed: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Copy a large file in chunks with progress updates.
        /// </summary>
        private void CopyLargeFile(
            string sourceFile,
            string destFile,
            long fileSize,
            long alreadyCopiedBytes,
            long totalBytes,
            BackgroundWorker worker)
        {
            using var sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var destStream = new FileStream(destFile, FileMode.Create, FileAccess.Write, FileShare.None);

            byte[] buffer = new byte[COPY_BUFFER_SIZE];
            long fileCopiedBytes = 0;
            int bytesRead;

            while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                if (worker.CancellationPending)
                    break;

                destStream.Write(buffer, 0, bytesRead);
                fileCopiedBytes += bytesRead;

                // Report progress every chunk
                var progress = new Gw2FolderCopyProgress
                {
                    TotalBytes = totalBytes,
                    CopiedBytes = alreadyCopiedBytes + fileCopiedBytes,
                    PercentComplete = (int)(((alreadyCopiedBytes + fileCopiedBytes) * 100) / totalBytes),
                    CurrentFileName = Path.GetFileName(sourceFile),
                    CurrentFileSize = fileSize,
                    CurrentFileCopiedBytes = fileCopiedBytes,
                    StatusMessage = $"Copying: {Path.GetFileName(sourceFile)} " +
                                  $"({FormatBytes(fileCopiedBytes)} / {FormatBytes(fileSize)})"
                };
                worker.ReportProgress(progress.PercentComplete, progress);
            }
        }

        /// <summary>
        /// Format bytes into human-readable string (KB, MB, GB).
        /// </summary>
        private string FormatBytes(long bytes)
        {
            const long KB = 1024;
            const long MB = KB * 1024;
            const long GB = MB * 1024;

            if (bytes >= GB)
                return $"{bytes / (double)GB:F1} GB";
            if (bytes >= MB)
                return $"{bytes / (double)MB:F1} MB";
            if (bytes >= KB)
                return $"{bytes / (double)KB:F1} KB";
            
            return $"{bytes} bytes";
        }
    }
}
