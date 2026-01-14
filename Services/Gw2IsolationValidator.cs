using GWxLauncher.Config;
using GWxLauncher.Domain;

namespace GWxLauncher.Services
{
    /// <summary>
    /// Result of validating whether GW2 isolation can be enabled.
    /// </summary>
    internal class Gw2IsolationValidationResult
    {
        public bool CanEnable { get; set; }
        public string Message { get; set; } = "";
        
        // Profiles that share the same exe path (need copying)
        public List<GameProfile> ProfilesWithDuplicateExePath { get; set; } = new();
        
        // Map: exe path -> profiles using it
        public Dictionary<string, List<GameProfile>> ExePathToProfiles { get; set; } = new();
    }

    /// <summary>
    /// Validates prerequisites for enabling GW2 per-profile isolation.
    /// Checks:
    /// - GW2 profiles have unique exe paths
    /// - Disk space availability for copying
    /// </summary>
    internal class Gw2IsolationValidator
    {
        /// <summary>
        /// Validate whether isolation can be enabled for GW2 profiles.
        /// Returns validation result with details about any issues.
        /// </summary>
        public Gw2IsolationValidationResult Validate(List<GameProfile> allProfiles)
        {
            var result = new Gw2IsolationValidationResult { CanEnable = true };

            // Get all GW2 profiles
            var gw2Profiles = allProfiles
                .Where(p => p.GameType == GameType.GuildWars2)
                .ToList();

            if (gw2Profiles.Count == 0)
            {
                result.CanEnable = true;
                result.Message = "No GW2 profiles found. Isolation can be enabled.";
                return result;
            }

            // Group profiles by exe path (case-insensitive)
            var exePathGroups = gw2Profiles
                .Where(p => !string.IsNullOrWhiteSpace(p.ExecutablePath))
                .GroupBy(p => p.ExecutablePath, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1) // Only groups with duplicates
                .ToList();

            if (exePathGroups.Any())
            {
                result.CanEnable = false;
                result.ProfilesWithDuplicateExePath = exePathGroups
                    .SelectMany(g => g)
                    .ToList();

                result.ExePathToProfiles = exePathGroups
                    .ToDictionary(
                        g => g.Key,
                        g => g.ToList(),
                        StringComparer.OrdinalIgnoreCase);

                result.Message = $"Found {result.ProfilesWithDuplicateExePath.Count} profiles sharing game folders. " +
                                "Each profile must have its own unique game folder for isolation.";
            }
            else
            {
                result.CanEnable = true;
                result.Message = "All GW2 profiles have unique game folders. Isolation can be enabled.";
            }

            return result;
        }

        /// <summary>
        /// Check if there's enough disk space to copy a game folder to a destination drive.
        /// </summary>
        /// <param name="sourceFolderPath">Source game folder (containing Gw2-64.exe)</param>
        /// <param name="destinationPath">Destination path for the copy</param>
        /// <param name="safetyMarginGB">Extra GB to require beyond folder size (default 5GB)</param>
        /// <returns>True if enough space, false otherwise</returns>
        public bool CheckDiskSpace(string sourceFolderPath, string destinationPath, double safetyMarginGB = 5.0)
        {
            try
            {
                // Calculate source folder size
                long sourceSizeBytes = GetDirectorySize(sourceFolderPath);
                double sourceSizeGB = sourceSizeBytes / (1024.0 * 1024.0 * 1024.0);

                // Get destination drive info
                string destinationRoot = Path.GetPathRoot(Path.GetFullPath(destinationPath)) ?? "";
                if (string.IsNullOrEmpty(destinationRoot))
                    return false;

                var drive = new DriveInfo(destinationRoot);
                double freeSpaceGB = drive.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0);

                double requiredSpaceGB = sourceSizeGB + safetyMarginGB;

                return freeSpaceGB >= requiredSpaceGB;
            }
            catch
            {
                // If we can't check, be conservative and return false
                return false;
            }
        }

        /// <summary>
        /// Get detailed disk space info for user display.
        /// </summary>
        public (double sourceSizeGB, double freeSpaceGB, double requiredSpaceGB) GetDiskSpaceInfo(
            string sourceFolderPath, 
            string destinationPath, 
            double safetyMarginGB = 5.0)
        {
            long sourceSizeBytes = GetDirectorySize(sourceFolderPath);
            double sourceSizeGB = sourceSizeBytes / (1024.0 * 1024.0 * 1024.0);

            string destinationRoot = Path.GetPathRoot(Path.GetFullPath(destinationPath)) ?? "";
            var drive = new DriveInfo(destinationRoot);
            double freeSpaceGB = drive.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0);

            double requiredSpaceGB = sourceSizeGB + safetyMarginGB;

            return (sourceSizeGB, freeSpaceGB, requiredSpaceGB);
        }

        /// <summary>
        /// Recursively calculate total size of a directory.
        /// </summary>
        private long GetDirectorySize(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                return 0;

            long size = 0;

            // Get all files
            try
            {
                var files = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    try
                    {
                        var fileInfo = new FileInfo(file);
                        size += fileInfo.Length;
                    }
                    catch
                    {
                        // Skip files we can't access
                    }
                }
            }
            catch
            {
                // If we can't enumerate, return 0
            }

            return size;
        }
    }
}
