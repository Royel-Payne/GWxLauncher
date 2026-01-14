using System.Diagnostics;

namespace Gw2AppDataRedirectPoC;

class Program
{
    static int Main(string[] args)
    {
        Console.WriteLine("???????????????????????????????????????????????????????");
        Console.WriteLine("  GW2 AppData Redirection - Proof of Concept");
        Console.WriteLine("???????????????????????????????????????????????????????");
        Console.WriteLine();

        // Parse arguments
        if (args.Length < 2)
        {
            ShowUsage();
            return 1;
        }

        string gw2ExePath = args[0];
        string profileRootPath = args[1];
        string? gw2Arguments = args.Length > 2 ? string.Join(" ", args.Skip(2)) : null;

        // Validate inputs
        if (!File.Exists(gw2ExePath))
        {
            Console.WriteLine($"? ERROR: GW2 executable not found: {gw2ExePath}");
            Console.WriteLine();
            return 2;
        }

        if (!gw2ExePath.EndsWith("Gw2-64.exe", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"??  WARNING: Expected Gw2-64.exe but got: {Path.GetFileName(gw2ExePath)}");
            Console.WriteLine("   This PoC is designed for 64-bit GW2 only.");
            Console.WriteLine();
        }

        // Create profile directories
        Console.WriteLine($"Profile root: {profileRootPath}");
        string roamingPath = Path.Combine(profileRootPath, "Roaming");
        string localPath = Path.Combine(profileRootPath, "Local");

        try
        {
            Directory.CreateDirectory(roamingPath);
            Directory.CreateDirectory(localPath);
            Console.WriteLine($"? Created: {roamingPath}");
            Console.WriteLine($"? Created: {localPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? ERROR: Failed to create profile directories: {ex.Message}");
            return 3;
        }

        Console.WriteLine();

        // Find the DLL (should be in same directory as this EXE)
        string exeDirectory = AppContext.BaseDirectory;
        string dllPath = Path.Combine(exeDirectory, "Gw2FolderHook.dll");

        if (!File.Exists(dllPath))
        {
            Console.WriteLine($"? ERROR: Hook DLL not found: {dllPath}");
            Console.WriteLine();
            Console.WriteLine("Make sure Gw2FolderHook.dll is in the same directory as this executable.");
            Console.WriteLine("You may need to build the C++ project first.");
            return 4;
        }

        Console.WriteLine($"Hook DLL: {dllPath}");
        Console.WriteLine();

        // Set up environment variables for the DLL to read
        var environment = new Dictionary<string, string>
        {
            // For the hook DLL to know where to redirect
            ["GW2_REDIRECT_ROAMING"] = roamingPath,
            ["GW2_REDIRECT_LOCAL"] = localPath,
            ["GW2_HOOK_LOG"] = @"C:\Temp\Gw2FolderHook.log",
            
            // CRITICAL: Override the actual APPDATA environment variables
            // GW2 reads these directly instead of calling Shell APIs
            ["APPDATA"] = roamingPath,
            ["LOCALAPPDATA"] = localPath
        };

        Console.WriteLine("Environment variables for hook:");
        foreach (var kvp in environment)
        {
            Console.WriteLine($"  {kvp.Key} = {kvp.Value}");
        }
        Console.WriteLine();

        // Create temp directory for logs
        Directory.CreateDirectory(@"C:\Temp");

        // Clear old log if it exists
        string logPath = @"C:\Temp\Gw2FolderHook.log";
        if (File.Exists(logPath))
        {
            try
            {
                File.Delete(logPath);
                Console.WriteLine($"? Cleared old log: {logPath}");
            }
            catch
            {
                Console.WriteLine($"??  Could not delete old log (may be in use)");
            }
        }

        Console.WriteLine();
        Console.WriteLine("???????????????????????????????????????????????????????");
        Console.WriteLine("  Starting Injection Process");
        Console.WriteLine("???????????????????????????????????????????????????????");
        Console.WriteLine();

        try
        {
            // Get GW2 working directory
            string gw2WorkingDir = Path.GetDirectoryName(gw2ExePath) ?? Environment.CurrentDirectory;

            // Perform injection
            var injector = new ProcessInjector(gw2ExePath, dllPath, gw2WorkingDir);
            uint processId = injector.LaunchAndInject(gw2Arguments, environment);

            Console.WriteLine();
            Console.WriteLine("???????????????????????????????????????????????????????");
            Console.WriteLine("  ? SUCCESS!");
            Console.WriteLine("???????????????????????????????????????????????????????");
            Console.WriteLine();
            Console.WriteLine($"GW2 Process ID: {processId}");
            Console.WriteLine();
            Console.WriteLine("Next steps:");
            Console.WriteLine("  1. Wait for GW2 to fully load");
            Console.WriteLine("  2. Log in and change a setting (e.g., graphics preset)");
            Console.WriteLine("  3. Exit GW2");
            Console.WriteLine("  4. Check the hook log:");
            Console.WriteLine($"     type {logPath}");
            Console.WriteLine("  5. Verify files were created in the redirected location:");
            Console.WriteLine($"     dir \"{roamingPath}\\Guild Wars 2\"");
            Console.WriteLine($"     dir \"{localPath}\\ArenaNet\\Guild Wars 2\"");
            Console.WriteLine("  6. Verify files were NOT created in your real AppData:");
            Console.WriteLine($"     dir \"%APPDATA%\\Guild Wars 2\"");
            Console.WriteLine();

            // Wait a bit and check if log was created
            Console.WriteLine("Waiting 3 seconds to check for hook activation...");
            Thread.Sleep(3000);

            if (File.Exists(logPath))
            {
                Console.WriteLine();
                Console.WriteLine("???????????????????????????????????????????????????????");
                Console.WriteLine("  Hook Log (first 20 lines):");
                Console.WriteLine("???????????????????????????????????????????????????????");
                try
                {
                    var logLines = File.ReadAllLines(logPath).Take(20);
                    foreach (var line in logLines)
                    {
                        Console.WriteLine(line);
                    }
                }
                catch (IOException)
                {
                    Console.WriteLine("  (Log file is locked - process is using it, which is a good sign!)");
                }
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("??  WARNING: Hook log not found yet.");
                Console.WriteLine("   This could mean:");
                Console.WriteLine("   - The hook hasn't been called yet (GW2 still loading)");
                Console.WriteLine("   - The DLL failed to initialize");
                Console.WriteLine("   - Check for an error log at C:\\Temp\\Gw2FolderHook_error.log");
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine("???????????????????????????????????????????????????????");
            Console.WriteLine("  ? FAILURE");
            Console.WriteLine("???????????????????????????????????????????????????????");
            Console.WriteLine();
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine();
            Console.WriteLine($"Stack trace:");
            Console.WriteLine(ex.StackTrace);
            Console.WriteLine();

            return 99;
        }
    }

    static void ShowUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  Gw2AppDataRedirectPoC.exe <gw2ExePath> <profileRootPath> [gw2Arguments]");
        Console.WriteLine();
        Console.WriteLine("Arguments:");
        Console.WriteLine("  gw2ExePath       - Full path to Gw2-64.exe");
        Console.WriteLine("  profileRootPath  - Directory where profile-specific folders will be created");
        Console.WriteLine("  gw2Arguments     - (Optional) Additional arguments to pass to GW2");
        Console.WriteLine();
        Console.WriteLine("Example:");
        Console.WriteLine("  Gw2AppDataRedirectPoC.exe \"C:\\Program Files\\Guild Wars 2\\Gw2-64.exe\" \"C:\\Temp\\GW2Test\\Profile1\"");
        Console.WriteLine();
        Console.WriteLine("This will:");
        Console.WriteLine("  - Create C:\\Temp\\GW2Test\\Profile1\\Roaming");
        Console.WriteLine("  - Create C:\\Temp\\GW2Test\\Profile1\\Local");
        Console.WriteLine("  - Launch GW2 with injected hook DLL");
        Console.WriteLine("  - GW2 will use those folders instead of your real AppData");
        Console.WriteLine();
    }
}
