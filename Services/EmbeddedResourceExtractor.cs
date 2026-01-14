using System.Reflection;
using System.Security.Cryptography;

namespace GWxLauncher.Services
{
    /// <summary>
    /// Extracts embedded native dependencies (DLLs and helper executables) from the application assembly
    /// to a local AppData folder. This enables single-file distribution while still supporting 
    /// external native dependencies required for GW2 isolation.
    /// </summary>
    internal static class EmbeddedResourceExtractor
    {
        private static readonly string BinFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "GWxLauncher",
            "Bin"
        );

        /// <summary>
        /// Ensures all required native dependencies are extracted and up-to-date.
        /// Call this on application startup before any features that require native DLLs.
        /// </summary>
        public static void EnsureNativeDependencies()
        {
            try
            {
                Directory.CreateDirectory(BinFolder);

                // Extract native C++ DLL (for GW2 isolation hook)
                ExtractIfNeeded("Gw2FolderHook.dll");

                // Extract x64 injector helper (required because main app is x86)
                // Built as single-file executable, so no separate .dll needed
                ExtractIfNeeded("GWxInjector.exe");
            }
            catch (Exception ex)
            {
                // Log but don't crash - app can still run without isolation feature
                System.Diagnostics.Debug.WriteLine($"Failed to extract native dependencies: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the full path to the extracted Gw2FolderHook.dll.
        /// </summary>
        public static string GetNativeDllPath() => Path.Combine(BinFolder, "Gw2FolderHook.dll");

        /// <summary>
        /// Gets the full path to the extracted GWxInjector.exe.
        /// </summary>
        public static string GetInjectorPath() => Path.Combine(BinFolder, "GWxInjector.exe");

        /// <summary>
        /// Extracts an embedded resource to disk if it doesn't exist or has changed.
        /// Uses SHA256 hash comparison to detect if the file needs updating.
        /// </summary>
        private static void ExtractIfNeeded(string resourceName)
        {
            var targetPath = Path.Combine(BinFolder, resourceName);

            // Check if file exists and matches current version
            if (File.Exists(targetPath))
            {
                try
                {
                    var embeddedHash = GetEmbeddedResourceHash(resourceName);
                    var diskHash = GetFileHash(targetPath);

                    if (embeddedHash == diskHash)
                    {
                        // File is up-to-date
                        return;
                    }
                }
                catch
                {
                    // If hash comparison fails, re-extract to be safe
                }
            }

            // Extract from embedded resource
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(resourceName);

            if (stream == null)
            {
                // Resource not found - this is expected if native DLLs weren't built
                System.Diagnostics.Debug.WriteLine($"Embedded resource not found: {resourceName}");
                return;
            }

            // Write to disk
            using var fileStream = File.Create(targetPath);
            stream.CopyTo(fileStream);

            System.Diagnostics.Debug.WriteLine($"Extracted {resourceName} to {targetPath}");
        }

        /// <summary>
        /// Computes SHA256 hash of an embedded resource.
        /// </summary>
        private static string GetEmbeddedResourceHash(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(resourceName);

            if (stream == null)
                return string.Empty;

            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(stream);
            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Computes SHA256 hash of a file on disk.
        /// </summary>
        private static string GetFileHash(string filePath)
        {
            using var stream = File.OpenRead(filePath);
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(stream);
            return Convert.ToBase64String(hash);
        }
    }
}
