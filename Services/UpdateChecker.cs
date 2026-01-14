using System.Reflection;
using System.Text.Json;

namespace GWxLauncher.Services
{
    /// <summary>
    /// Information about available updates.
    /// </summary>
    internal class UpdateInfo
    {
        public bool UpdateAvailable { get; set; }
        public string CurrentVersion { get; set; } = "";
        public string LatestVersion { get; set; } = "";
        public string ReleaseUrl { get; set; } = "";
        public string ReleaseNotes { get; set; } = "";
        public DateTime? PublishedAt { get; set; }
    }

    /// <summary>
    /// Service for checking GitHub releases for updates.
    /// Compares current version against latest GitHub release.
    /// </summary>
    internal class UpdateChecker
    {
        private const string GITHUB_API_URL = "https://api.github.com/repos/Royel-Payne/GWxLauncher/releases/latest";
        private static readonly HttpClient _httpClient = new HttpClient();

        static UpdateChecker()
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "GWxLauncher-UpdateChecker");
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
        }

        /// <summary>
        /// Check for updates by querying GitHub releases API.
        /// Returns update information including current and latest versions.
        /// </summary>
        public async Task<UpdateInfo> CheckForUpdatesAsync()
        {
            try
            {
                string currentVersion = GetCurrentVersion();
                
                var response = await _httpClient.GetStringAsync(GITHUB_API_URL);
                var release = JsonSerializer.Deserialize<GitHubRelease>(response);

                if (release == null || string.IsNullOrEmpty(release.tag_name))
                {
                    return new UpdateInfo { CurrentVersion = currentVersion };
                }

                string latestVersion = release.tag_name.TrimStart('v');
                bool updateAvailable = IsNewerVersion(currentVersion, latestVersion);

                return new UpdateInfo
                {
                    UpdateAvailable = updateAvailable,
                    CurrentVersion = currentVersion,
                    LatestVersion = latestVersion,
                    ReleaseUrl = release.html_url ?? "",
                    ReleaseNotes = release.body ?? "",
                    PublishedAt = release.published_at
                };
            }
            catch
            {
                return new UpdateInfo { CurrentVersion = GetCurrentVersion() };
            }
        }

        /// <summary>
        /// Get the current application version from assembly attributes.
        /// </summary>
        private string GetCurrentVersion()
        {
            var version = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion ?? "1.5.0";
            
            return version.Split('+')[0]; // Remove git hash if present
        }

        /// <summary>
        /// Compare two version strings to determine if latest is newer.
        /// </summary>
        private bool IsNewerVersion(string current, string latest)
        {
            try
            {
                var currentVer = new Version(current);
                var latestVer = new Version(latest);
                return latestVer > currentVer;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// GitHub API release response.
        /// </summary>
        private class GitHubRelease
        {
            public string? tag_name { get; set; }
            public string? html_url { get; set; }
            public string? body { get; set; }
            public DateTime published_at { get; set; }
        }
    }
}
