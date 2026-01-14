# GW2 Per-Profile AppData Redirection (Injection + Filesystem Hook)

**Version:** 1.0  
**Date:** January 2026  
**Status:** Implementation-Ready Technical Design

---

## Goal

Enable GWxLauncher to run multiple Guild Wars 2 instances with **complete AppData isolation** by:
1. Launching GW2 in a suspended state
2. Injecting a hook DLL (`Gw2FolderHook.dll`) that redirects filesystem operations
3. Rewriting all file paths from system AppData directories to per-profile directories
4. Achieving this **without requiring administrator privileges**

**Success Criteria:**
- Each GW2 instance writes `Local.dat`, `GFXSettings.Gw2-64.exe.xml`, and cache files to its own profile directory
- Multiple instances run simultaneously without file conflicts
- Old AppData locations remain untouched
- Solution works on Windows 10/11 x64 without UAC elevation

---

## Non-Goals

- **Not** redirecting game installation files (`Gw2-64.exe`, `.dat` archives)
- **Not** hooking network APIs or preventing account enforcement
- **Not** modifying GW2 executable or `.dat` files (no patching)
- **Not** supporting 32-bit GW2 (`Gw2.exe`)
- **Not** providing UI for profile management (launcher integration only)
- **Not** hooking registry write operations (GW2 reads registry for discovery; we intercept file operations)

---

## Background: What GW2 Touches

### Registry Access
Guild Wars 2 queries these registry keys (observed via ProcMon):
- `HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders`
- `HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders`

It reads values:
- `AppData` → Typically `%USERPROFILE%\AppData\Roaming`
- `Local AppData` → Typically `%USERPROFILE%\AppData\Local`

**Note:** GW2 reads the registry using internal/undocumented methods. We do **not** hook registry APIs; instead, we let GW2 read the normal paths, then intercept filesystem operations.

### Filesystem Access Patterns

#### Roaming AppData (`%APPDATA%\Guild Wars 2\`)
- `Local.dat` - Account credentials and login state (binary, ~40MB)
- `GFXSettings.Gw2-64.exe.xml` - Graphics settings (XML)
- `Settings.json` - Client preferences (JSON, may not exist initially)

#### Local AppData (`%LOCALAPPDATA%\Temp\gw2cache-{GUID}\`)
- `user\Cache\*` - Chromium cache (CEF browser for launcher UI)
- `user\Cache\Code Cache\*` - JavaScript/WASM cache
- `user\Cache\GPUCache\*` - GPU shader cache
- `user\Cache\Local Storage\*` - Browser local storage
- `user\Cache\Session Storage\*` - Session data
- `user\Cache\Network\*` - HTTP cache

**Critical Observation from PoC:**
- GW2 creates temp cache directories with random GUIDs
- Cache directories use mixed forward-slash and backslash paths (e.g., `Cache/LOG`, `Cache\blob_storage`)
- Hundreds of small files created during initialization
- All paths contain either `\AppData\Roaming` or `\AppData\Local` as substrings

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────┐
│ GWxLauncher (Launcher Process)                          │
│                                                          │
│ 1. CreateProcess(Gw2-64.exe, CREATE_SUSPENDED)         │
│ 2. Set environment: GW2_REDIRECT_ROAMING=<path>        │
│                     GW2_REDIRECT_LOCAL=<path>           │
│ 3. VirtualAllocEx + WriteProcessMemory (DLL path)      │
│ 4. CreateRemoteThread(LoadLibraryW, DLL path)          │
│ 5. WaitForSingleObject(thread, INFINITE)               │
│ 6. GetExitCodeThread → verify LoadLibrary succeeded    │
│ 7. ResumeThread(main thread)                           │
└─────────────────────────────────────────────────────────┘
                         │
                         │ Injection
                         ↓
┌─────────────────────────────────────────────────────────┐
│ Gw2-64.exe (Target Process - SUSPENDED)                │
│                                                          │
│ ┌─────────────────────────────────────────────────┐   │
│ │ Gw2FolderHook.dll (Injected)                    │   │
│ │                                                   │   │
│ │ DllMain(DLL_PROCESS_ATTACH):                    │   │
│ │  - Read env vars (GW2_REDIRECT_*)                │   │
│ │  - Get original paths (SHGetFolderPathW)         │   │
│ │  - Initialize MinHook                            │   │
│ │  - Hook 6 filesystem APIs                        │   │
│ │  - Enable hooks (MH_EnableHook ALL)              │   │
│ │                                                   │   │
│ │ Hooks:                                           │   │
│ │  CreateFileW        → Rewrite path               │   │
│ │  CreateDirectoryW   → Rewrite path               │   │
│ │  GetFileAttributesW → Rewrite path               │   │
│ │  DeleteFileW        → Rewrite path               │   │
│ │  RemoveDirectoryW   → Rewrite path               │   │
│ │  SetFileAttributesW → Rewrite path               │   │
│ │                                                   │   │
│ │ Rewrite Logic:                                   │   │
│ │  IF path.contains("AppData\Roaming"):            │   │
│ │    replace with profile\Roaming                  │   │
│ │  IF path.contains("AppData\Local"):              │   │
│ │    replace with profile\Local                    │   │
│ │                                                   │   │
│ │ Logging: C:\Temp\Gw2FolderHook.log               │   │
│ └─────────────────────────────────────────────────┘   │
│                                                          │
│ After hooks active:                                     │
│  GW2 calls CreateFileW("C:\...\AppData\Roaming\...") │
│    → Hook intercepts                                   │
│    → Returns CreateFileW("C:\GW2Profiles\P1\Roaming")│
│                                                          │
└─────────────────────────────────────────────────────────┘
```

**Key Principle:** Let GW2 discover AppData paths via registry (any method it wants), then **intercept at the filesystem boundary** where paths are actually used.

---

## Components and Responsibilities

### 1. **GWxLauncher (C# / Launcher Executable)**

**Responsibilities:**
- Manage user profiles (create profile directories before launch)
- Prepare environment variables for redirection
- Execute suspended process launch + injection sequence
- Monitor injection success/failure
- Resume GW2 main thread
- Handle errors and report to user

**Key APIs:**
- `CreateProcess` with `CREATE_SUSPENDED` flag
- `SetEnvironmentVariable` (for child process inheritance)
- `VirtualAllocEx`, `WriteProcessMemory` (inject DLL path string)
- `GetProcAddress(kernel32, "LoadLibraryW")` (in target process)
- `CreateRemoteThread` (call LoadLibraryW with DLL path)
- `WaitForSingleObject` (wait for LoadLibraryW to complete)
- `GetExitCodeThread` (verify DLL loaded, non-zero HMODULE)
- `ResumeThread` (resume GW2 main thread)

### 2. **Gw2FolderHook.dll (C++ / Native Hook DLL)**

**Responsibilities:**
- Read configuration from environment variables (`GW2_REDIRECT_ROAMING`, `GW2_REDIRECT_LOCAL`)
- Detect original AppData paths using `SHGetFolderPathW` (before hooking)
- Initialize MinHook library
- Install hooks on 6 critical filesystem APIs
- Implement path rewriting logic (case-insensitive substring replacement)
- Auto-create parent directories when redirecting file creation
- Log all redirections to `C:\Temp\Gw2FolderHook.log`
- Properly cleanup on `DLL_PROCESS_DETACH`

**Hooked APIs (kernel32.dll):**
1. `CreateFileW` - Main file open/create API
2. `CreateDirectoryW` - Directory creation
3. `GetFileAttributesW` - Attribute queries (file/dir existence checks)
4. `SetFileAttributesW` - Attribute modification
5. `DeleteFileW` - File deletion
6. `RemoveDirectoryW` - Directory deletion

**Why These APIs:**
- **CreateFileW**: Primary file I/O entry point; handles files and directories
- **CreateDirectoryW**: Must redirect to create profile-specific directories
- **GetFileAttributesW**: GW2 checks file existence before opening
- **Delete/Remove**: Ensures cleanup operations also redirect
- **SetFileAttributesW**: Handles attribute changes (rare but possible)

**Why NOT NtCreateFile:**
- `CreateFileW` in kernel32.dll is sufficient; GW2 uses user-mode APIs
- MinHook targets user-mode DLLs; ntdll hooking is complex and fragile
- If needed later, add `NtCreateFile` hook in ntdll.dll

### 3. **MinHook Library (Third-Party)**

**Responsibilities:**
- Provide inline hooking infrastructure (x64 trampoline generation)
- Thread-safe hook installation/removal
- Preserve original function pointers for calling unhooked versions

**Integration:**
- Static lib (`libMinHook.x64.lib`) linked into `Gw2FolderHook.dll`
- Header: `MinHook.h`
- Download via `download_minhook.ps1` script (version 1.3.3)

---

## Launch Sequence (Step-by-Step)

### Launcher-Side (C# in GWxLauncher)

```
STEP 1: Prepare Profile Directories
  ├─ Validate profile path exists (e.g., C:\GW2Profiles\Account1)
  ├─ Create <ProfileRoot>\Roaming if not exists
  ├─ Create <ProfileRoot>\Local if not exists
  ├─ Create <ProfileRoot>\Roaming\Guild Wars 2 if not exists (optional pre-creation)
  └─ Create <ProfileRoot>\Local\Temp if not exists (optional pre-creation)

STEP 2: Set Environment Variables (before CreateProcess)
  ├─ SetEnvironmentVariable("GW2_REDIRECT_ROAMING", "<ProfileRoot>\Roaming")
  ├─ SetEnvironmentVariable("GW2_REDIRECT_LOCAL", "<ProfileRoot>\Local")
  └─ Note: These vars inherit to child process

STEP 3: Launch GW2 Suspended
  ├─ STARTUPINFO si = { sizeof(si) }
  ├─ PROCESS_INFORMATION pi
  ├─ CreateProcess(
  │    lpApplicationName: "C:\...\Gw2-64.exe",
  │    lpCommandLine: command-line args (optional),
  │    bInheritHandles: FALSE,
  │    dwCreationFlags: CREATE_SUSPENDED,
  │    lpEnvironment: NULL (use parent env with our vars),
  │    ... 
  │    lpProcessInformation: &pi
  │  )
  ├─ Check: If FALSE, handle error (log GetLastError)
  └─ Store pi.hProcess, pi.hThread, pi.dwProcessId

STEP 4: Inject Hook DLL
  ├─ Get DLL path: Path.Combine(launcher dir, "Gw2FolderHook.dll")
  ├─ Validate DLL exists, log path
  │
  ├─ 4a. Allocate memory in target process:
  │      remoteMem = VirtualAllocEx(
  │        pi.hProcess,
  │        NULL,
  │        (dllPath.Length + 1) * sizeof(wchar_t),
  │        MEM_COMMIT | MEM_RESERVE,
  │        PAGE_READWRITE
  │      )
  │      Check: remoteMem != NULL
  │
  ├─ 4b. Write DLL path to target memory:
  │      WriteProcessMemory(
  │        pi.hProcess,
  │        remoteMem,
  │        dllPathBytes (UTF-16 LE),
  │        bytesNeeded,
  │        &bytesWritten
  │      )
  │      Check: bytesWritten == bytesNeeded
  │
  ├─ 4c. Get LoadLibraryW address in target (same as our process on x64):
  │      hKernel32 = GetModuleHandle("kernel32.dll")
  │      loadLibAddr = GetProcAddress(hKernel32, "LoadLibraryW")
  │      Note: Address is identical in target due to ASLR consistency
  │
  ├─ 4d. Create remote thread to call LoadLibraryW:
  │      hThread = CreateRemoteThread(
  │        pi.hProcess,
  │        NULL,
  │        0,
  │        loadLibAddr,  // Thread start = LoadLibraryW
  │        remoteMem,    // Parameter = DLL path
  │        0,
  │        &threadId
  │      )
  │      Check: hThread != NULL
  │
  ├─ 4e. Wait for LoadLibraryW to complete:
  │      WaitForSingleObject(hThread, INFINITE)
  │      GetExitCodeThread(hThread, &exitCode)
  │      Check: exitCode != 0 (non-zero HMODULE means success)
  │      Log: "DLL loaded, HMODULE = 0x{exitCode:X}"
  │
  └─ 4f. Cleanup:
       CloseHandle(hThread)
       VirtualFreeEx(pi.hProcess, remoteMem, ...) [optional, not critical]

STEP 5: Resume GW2 Main Thread
  ├─ ResumeThread(pi.hThread)
  ├─ Check: previous suspend count == 1 (only suspended once by us)
  └─ CloseHandle(pi.hThread), CloseHandle(pi.hProcess) [optional, keep open for monitoring]

STEP 6: Monitor (Optional)
  ├─ Wait 2-3 seconds
  ├─ Check if process still running: WaitForSingleObject(pi.hProcess, 0) == TIMEOUT
  ├─ Verify log file: C:\Temp\Gw2FolderHook.log contains "Filesystem redirection active"
  └─ Report success/failure to user
```

### Hook DLL-Side (C++ in Gw2FolderHook.dll)

```
DLL_PROCESS_ATTACH (in DllMain):
  │
  ├─ Initialize critical sections (for logging, key tracking)
  │
  ├─ STEP 1: Log startup
  │    Log("=== Gw2FolderHook DLL loaded (FILESYSTEM REDIRECTION) ===")
  │    Log("Process ID: {GetCurrentProcessId()}")
  │
  ├─ STEP 2: Get original AppData paths (BEFORE hooking)
  │    wchar_t buffer[MAX_PATH]
  │    SHGetFolderPathW(NULL, CSIDL_APPDATA, NULL, 0, buffer)
  │      → g_OriginalRoamingPath = buffer  // e.g., C:\Users\Chris\AppData\Roaming
  │    SHGetFolderPathW(NULL, CSIDL_LOCAL_APPDATA, NULL, 0, buffer)
  │      → g_OriginalLocalPath = buffer    // e.g., C:\Users\Chris\AppData\Local
  │    Log("Original RoamingAppData: {g_OriginalRoamingPath}")
  │    Log("Original LocalAppData: {g_OriginalLocalPath}")
  │
  ├─ STEP 3: Read configuration from environment
  │    GetEnvironmentVariableW("GW2_REDIRECT_ROAMING", buffer, MAX_PATH)
  │      → g_RoamingPath = buffer  // e.g., C:\GW2Profiles\Account1\Roaming
  │    GetEnvironmentVariableW("GW2_REDIRECT_LOCAL", buffer, MAX_PATH)
  │      → g_LocalPath = buffer    // e.g., C:\GW2Profiles\Account1\Local
  │    If either is empty: Log("WARNING: Missing config") and return early
  │    Log("RoamingAppData redirect configured: {g_RoamingPath}")
  │    Log("LocalAppData redirect configured: {g_LocalPath}")
  │
  ├─ STEP 4: Initialize MinHook
  │    MH_Initialize()
  │    Check: status == MH_OK
  │    Log("MinHook initialized")
  │
  ├─ STEP 5: Create hooks (MH_CreateHookApi for each)
  │    MH_CreateHookApi(L"kernel32.dll", "CreateFileW", &Hook_CreateFileW, &g_OriginalCreateFileW)
  │    MH_CreateHookApi(L"kernel32.dll", "CreateDirectoryW", &Hook_CreateDirectoryW, &g_OriginalCreateDirectoryW)
  │    MH_CreateHookApi(L"kernel32.dll", "GetFileAttributesW", &Hook_GetFileAttributesW, &g_OriginalGetFileAttributesW)
  │    MH_CreateHookApi(L"kernel32.dll", "SetFileAttributesW", &Hook_SetFileAttributesW, &g_OriginalSetFileAttributesW)
  │    MH_CreateHookApi(L"kernel32.dll", "DeleteFileW", &Hook_DeleteFileW, &g_OriginalDeleteFileW)
  │    MH_CreateHookApi(L"kernel32.dll", "RemoveDirectoryW", &Hook_RemoveDirectoryW, &g_OriginalRemoveDirectoryW)
  │
  ├─ STEP 6: Enable all hooks
  │    MH_EnableHook(MH_ALL_HOOKS)
  │    Check: status == MH_OK
  │    Log("All filesystem hooks enabled successfully")
  │
  └─ STEP 7: Ready
       Log("Filesystem redirection active - monitoring file operations")
       return TRUE

DLL_PROCESS_DETACH:
  ├─ Log("=== Gw2FolderHook DLL unloading ===")
  ├─ MH_DisableHook(MH_ALL_HOOKS)
  ├─ MH_Uninitialize()
  ├─ Close log file
  └─ Delete critical sections
```

---

## Hooking Strategy

### Path Rewriting Logic

**Core Algorithm (case-insensitive substring replacement):**

```
Function RedirectPath(originalPath):
  if originalPath is empty:
    return originalPath
  
  // Check Roaming redirection
  if ContainsIgnoreCase(originalPath, g_OriginalRoamingPath):
    redirected = ReplaceIgnoreCase(originalPath, g_OriginalRoamingPath, g_RoamingPath)
    Log("REDIRECT: {originalPath} -> {redirected}")
    return redirected
  
  // Check Local redirection
  if ContainsIgnoreCase(originalPath, g_OriginalLocalPath):
    redirected = ReplaceIgnoreCase(originalPath, g_OriginalLocalPath, g_LocalPath)
    Log("REDIRECT: {originalPath} -> {redirected}")
    return redirected
  
  // No redirection needed
  return originalPath
```

**String Operations:**
- `ContainsIgnoreCase`: Convert both strings to lowercase, use `wstring::find()`
- `ReplaceIgnoreCase`: Find position via lowercase comparison, replace in original string

**Normalization Rules:**
- **Trailing slashes:** Not normalized (preserve original formatting)
- **Forward vs backslash:** Both work; Windows APIs accept mixed (observed in PoC log: `Cache/LOG`)
- **UNC paths (`\\server\share`):** Not expected; ignore or log as unhandled
- **Long path prefix (`\\?\`):** Not expected from GW2; if encountered, log as unhandled
- **Relative paths:** GW2 uses absolute paths; reject or pass through unchanged

### Hook Implementation Pattern

**Example: CreateFileW Hook**

```cpp
HANDLE WINAPI Hook_CreateFileW(
    LPCWSTR lpFileName,
    DWORD dwDesiredAccess,
    DWORD dwShareMode,
    LPSECURITY_ATTRIBUTES lpSecurityAttributes,
    DWORD dwCreationDisposition,
    DWORD dwFlagsAndAttributes,
    HANDLE hTemplateFile)
{
    if (lpFileName)
    {
        wstring originalPath(lpFileName);
        wstring redirectedPath = RedirectPath(originalPath);
        
        if (redirectedPath != originalPath)
        {
            // AUTO-CREATE PARENT DIRECTORIES
            size_t lastSlash = redirectedPath.find_last_of(L"\\/");
            if (lastSlash != wstring::npos)
            {
                wstring parentDir = redirectedPath.substr(0, lastSlash);
                g_OriginalCreateDirectoryW(parentDir.c_str(), NULL);
                // Note: Recursive creation handled by CreateDirectoryW hook
            }
            
            // Call original with redirected path
            return g_OriginalCreateFileW(
                redirectedPath.c_str(),
                dwDesiredAccess,
                dwShareMode,
                lpSecurityAttributes,
                dwCreationDisposition,
                dwFlagsAndAttributes,
                hTemplateFile);
        }
    }
    
    // No redirection, pass through
    return g_OriginalCreateFileW(lpFileName, ...);
}
```

**Key Techniques:**
- **Parent directory creation:** Before opening a file, create parent directory tree using `CreateDirectoryW`
- **Recursive directory creation:** `CreateDirectoryW` hook creates parent path segments iteratively
- **Error handling:** If directory creation fails, proceed anyway (file open may fail with better error)
- **Pass-through:** Always call original function with (possibly redirected) path

### APIs Not Hooked (and Why)

**NOT Hooked:**
- `CreateFileA` - GW2 is Unicode-native (all paths are wide-char)
- `FindFirstFileW` / `FindNextFileW` - Not observed in PoC logs; add if needed
- `GetFullPathNameW` - GW2 uses absolute paths; not required
- `MoveFileW` / `CopyFileW` - Rare; add if file moves/copies occur
- `NtCreateFile` (ntdll) - User-mode hooks sufficient; adds complexity
- Registry APIs - Proven ineffective (GW2 bypasses all registry hooks)
- Shell APIs (`SHGetKnownFolderPath`, `SHGetFolderPathW`) - GW2 doesn't call them (or calls before hooks active)

**Add Later If Needed:**
- `FindFirstFileW` / `FindNextFileW` - If directory enumeration needs redirection
- `_wfopen`, `_wopen` - If GW2 uses CRT functions (unlikely, uses Win32 API)

---

## Configuration + Profile Directory Layout

### Configuration Method: **Environment Variables** (Recommended)

**Why Environment Variables:**
- ✅ Inherited automatically by child process
- ✅ No file I/O or parsing needed
- ✅ No permissions issues (in-memory)
- ✅ No race conditions (set before process creation)
- ✅ Simple to debug (`GetEnvironmentStrings` in target process)

**Alternative Rejected:**
- ❌ **Registry:** Requires write permissions; leaves traces; race-prone
- ❌ **Config file:** Requires file I/O; permissions issues; complex parsing
- ❌ **Shared memory:** Requires synchronization; named sections need unique names; overkill

**Environment Variable Names:**
- `GW2_REDIRECT_ROAMING` = `<ProfileRoot>\Roaming` (absolute path)
- `GW2_REDIRECT_LOCAL` = `<ProfileRoot>\Local` (absolute path)

**Example Values:**
```
GW2_REDIRECT_ROAMING=C:\GW2Profiles\Account1\Roaming
GW2_REDIRECT_LOCAL=C:\GW2Profiles\Account1\Local
```

### Profile Directory Structure

**Before GW2 Launch:**
```
<ProfileRoot>\                       (e.g., C:\GW2Profiles\Account1)
├── Roaming\                         [MUST EXIST - create in launcher]
│   └── Guild Wars 2\                [OPTIONAL - GW2 creates on first run]
│       ├── Local.dat                [Created by GW2]
│       ├── GFXSettings.Gw2-64.exe.xml [Created by GW2]
│       └── Settings.json            [Created by GW2]
│
└── Local\                           [MUST EXIST - create in launcher]
    └── Temp\                        [OPTIONAL - GW2 creates on first run]
        └── gw2cache-{GUID}\         [Created by GW2]
            └── user\                [Created by GW2]
                └── Cache\           [Created by GW2]
                    ├── (hundreds of files)
```

**Launcher Responsibility:**
- Create `<ProfileRoot>\Roaming` before launch
- Create `<ProfileRoot>\Local` before launch
- Optionally create `<ProfileRoot>\Local\Temp` (GW2 can create it via hooks)
- Do **NOT** create `Guild Wars 2` subdirectories (GW2 handles this)

**Hook DLL Responsibility:**
- Auto-create parent directories when file operations occur
- Use recursive creation in `CreateDirectoryW` hook

**Permissions:**
- Profile directories must be writable by user (no admin required)
- Avoid `C:\Program Files` or `C:\Windows` as profile roots (write-protected)
- Recommended locations: `C:\GW2Profiles\`, `%USERPROFILE%\Documents\GW2Profiles\`

---

## Logging + Observability

### Log File: `C:\Temp\Gw2FolderHook.log`

**Why this location:**
- `C:\Temp` is typically writable without admin
- Easy to access for debugging
- Persists across runs (append mode)

**Log Format:**
```
[YYYY-MM-DD HH:MM:SS.mmm] <Message>
```

**What to Log:**
1. **Startup:**
   - DLL load event
   - Process ID
   - Original AppData paths detected
   - Configured redirection paths
   - MinHook initialization status
   - Hook creation/enabling results

2. **Redirections:**
   - Every redirected file operation: `REDIRECT: <original> -> <redirected>`
   - First ~20 redirections at full detail, then sample every 100th (reduce log spam)

3. **Errors:**
   - MinHook failures (initialization, hook creation, enable)
   - Missing environment variables
   - Directory creation failures (log but continue)

4. **Shutdown:**
   - DLL unload event

**Log Level Control:**
- Default: INFO (log all redirections)
- Optional: TRACE (log all file operations, even non-redirected) via env var `GW2_HOOK_DEBUG=1`
- Optional: ERROR-only via env var `GW2_HOOK_LOGLEVEL=ERROR`

**Thread Safety:**
- Use `CRITICAL_SECTION` around log writes (already implemented in PoC)
- Flush after every write (`fflush(logFile)`)

**Log Rotation:**
- Manual: User deletes `Gw2FolderHook.log` between tests
- Automatic: Launcher can delete log before launch (optional)
- Size limit: Not implemented (rely on manual cleanup)

### Observability Checklist

**For Successful Launch:**
1. Log shows: `=== Gw2FolderHook DLL loaded (FILESYSTEM REDIRECTION) ===`
2. Log shows: `All filesystem hooks enabled successfully`
3. Log shows: `REDIRECT: ...AppData\Roaming\Guild Wars 2\Local.dat -> <ProfileRoot>\Roaming\Guild Wars 2\Local.dat`
4. Files appear in `<ProfileRoot>\Roaming\Guild Wars 2\`
5. No files created in `%APPDATA%\Guild Wars 2\` during this session

**For Failure Diagnosis:**
- Check if log file exists (if not, DLL didn't load)
- Check if redirections logged (if not, hooks not active)
- Check if files in profile directory (if not, see "Debugging 'no files created'" section)

---

## Failure Modes + Recovery

### Failure Mode 1: DLL Injection Fails

**Symptoms:**
- `CreateRemoteThread` returns `NULL`
- `GetExitCodeThread` returns 0 (LoadLibraryW failed)
- No log file created

**Causes:**
- DLL path incorrect or DLL missing
- Target process architecture mismatch (32-bit vs 64-bit)
- Antivirus blocked injection
- DEP/ASLR issues (rare on x64)

**Recovery:**
- Validate DLL exists before injection
- Log injection steps (VirtualAllocEx address, bytesWritten, thread ID, exit code)
- Retry once with 1-second delay (AV may release block)
- Show user-friendly error: "Failed to inject hook DLL. Check antivirus settings."

### Failure Mode 2: Hook Installation Fails

**Symptoms:**
- Log shows: `ERROR: MH_Initialize failed`
- Log shows: `ERROR: Failed to create/enable hook`
- No redirections logged

**Causes:**
- MinHook incompatible with GW2 code (rare, unlikely)
- DEP prevents trampoline generation
- Memory allocation failure

**Recovery:**
- Log MinHook status codes (all have string representations via `MH_StatusToString`)
- Do NOT proceed if hooks fail; terminate GW2 process
- Show error: "Hook initialization failed. Cannot guarantee profile isolation."

### Failure Mode 3: Hooks Active But Files Not Redirected

**Symptoms:**
- Log shows successful hook enable
- Log shows `REDIRECT:` entries
- BUT: Files still appear in original AppData OR nowhere at all

**Causes:**
- See "Debugging 'no files created' symptom" section (detailed checklist)

**Recovery:**
- Detailed debugging (see section below)
- Fallback: Disable injection, run GW2 normally with warning

### Failure Mode 4: GW2 Crashes After Injection

**Symptoms:**
- Process terminates immediately after `ResumeThread`
- Exit code: non-zero (e.g., 0xC0000005 - access violation)

**Causes:**
- Hook DLL incompatible with specific GW2 build
- Trampoline corrupts GW2 code
- DLL has dependencies not available (MSVC runtime)

**Recovery:**
- Check Event Viewer for crash details (Application log, "Application Error")
- Ensure DLL built with `/MD` (dynamic CRT) matching system
- Try without hooking (disable injection) to confirm GW2 works standalone
- Report issue with GW2 build number

### Failure Mode 5: Multiple Instances Collide

**Symptoms:**
- Second instance fails to start
- Second instance overwrites first instance's `Local.dat`

**Causes:**
- Both instances using same profile directory (launcher bug)
- Environment variables not cleared between launches

**Recovery:**
- Launcher must ensure unique profile directory per instance
- Clear environment variables after each launch: `SetEnvironmentVariable(name, NULL)`
- OR: Use instance-specific variable names: `GW2_REDIRECT_ROAMING_1`, `GW2_REDIRECT_ROAMING_2` (complex, not recommended)

---

## Security + AV/EDR Considerations

### Antivirus / Endpoint Detection

**Known Issues:**
- DLL injection triggers heuristic detection in most AV products
- `CreateRemoteThread` + `LoadLibraryW` pattern is classic malware technique
- Hook DLL modifying API behavior triggers behavioral analysis

**Mitigation Strategies:**

1. **Code Signing:**
   - Sign both `GWxLauncher.exe` and `Gw2FolderHook.dll` with EV certificate
   - Improves trust; reduces false positives
   - Expensive (~$300/year for EV cert)

2. **AV Exclusion Guidance:**
   - Provide clear instructions for users to exclude:
     - `GWxLauncher.exe`
     - `Gw2FolderHook.dll`
     - Profile directories (`C:\GW2Profiles\*`)
   - Document per-AV instructions (Windows Defender, Avast, etc.)

3. **Transparent Operation:**
   - Open-source hook DLL (publish on GitHub)
   - Provide build-from-source instructions
   - No obfuscation or packing
   - Users can verify code themselves

4. **Alternative: AppInit_DLLs Registry:**
   - ❌ **Rejected:** Requires admin, affects all processes, deprecated in Win8+

5. **Alternative: Debugging APIs (NtSuspendProcess):**
   - ❌ **Rejected:** More complex, still triggers AV, requires SeDebugPrivilege

**EDR-Specific Concerns:**
- Corporate EDR (CrowdStrike, Carbon Black, etc.) may block injection even with exclusions
- Users in corporate environments: Launcher may not work without IT approval
- Document limitation clearly: "Not compatible with enterprise EDR unless explicitly allowed"

### User-Mode Only (No Kernel Components)

**Advantages:**
- ✅ No driver signing required
- ✅ No kernel-mode complexity
- ✅ Survives Windows updates (no kernel dependencies)
- ✅ Easier to debug and maintain

**Limitations:**
- ❌ Cannot intercept kernel-mode filesystem operations (not needed for GW2)
- ❌ Cannot block anti-cheat detection (not a goal; GW2 has no anti-cheat DLL)

**GW2-Specific:**
- GW2 does **not** have kernel-mode anti-cheat (unlike Valorant, Apex)
- GW2's account enforcement is server-side (cannot be bypassed by client modifications)
- Our hooks do **not** modify game logic, only filesystem paths

---

## Testing Plan

### Manual Testing Checklist

**Pre-Launch:**
- [ ] Profile directory exists: `C:\GW2Profiles\TestProfile\`
- [ ] `Gw2FolderHook.dll` exists next to launcher
- [ ] `libMinHook.x64.lib` linked into DLL (check DLL size > 50KB)
- [ ] Old AppData location noted: `%APPDATA%\Guild Wars 2\` (timestamp `Local.dat`)

**Launch:**
- [ ] Launcher sets environment variables (verify via `set GW2_` in cmd before launch)
- [ ] Process created suspended (check Task Manager shows 0% CPU briefly)
- [ ] Injection logs show success: "DLL loaded, HMODULE = 0x..." printed
- [ ] Log file created: `C:\Temp\Gw2FolderHook.log`
- [ ] Log shows: "All filesystem hooks enabled successfully"

**Runtime:**
- [ ] GW2 window appears (login screen or character select)
- [ ] Log shows redirections: `REDIRECT: ...Local.dat -> <ProfileRoot>\...`
- [ ] Files created in profile directory:
  - `<ProfileRoot>\Roaming\Guild Wars 2\Local.dat`
  - `<ProfileRoot>\Roaming\Guild Wars 2\GFXSettings.Gw2-64.exe.xml`
  - `<ProfileRoot>\Local\Temp\gw2cache-{GUID}\...` (many files)
- [ ] Old AppData location unchanged (check `Local.dat` timestamp)

**Multi-Instance:**
- [ ] Launch second instance with different profile directory
- [ ] Both instances run simultaneously
- [ ] Each writes to its own profile directory
- [ ] No file conflicts or errors

**Settings Persistence:**
- [ ] Change graphics settings in GW2
- [ ] Close GW2
- [ ] Re-launch with same profile
- [ ] Settings retained (verify `GFXSettings.Gw2-64.exe.xml` updated)

**Login State:**
- [ ] Login to account in GW2
- [ ] Close GW2
- [ ] Re-launch with same profile
- [ ] Still logged in (verify `Local.dat` preserved)

### Automated Testing (Future)

**Unit Tests (C++ for Hook DLL):**
- `RedirectPath()` function with various inputs:
  - Roaming path present
  - Local path present
  - Mixed case paths
  - Paths with trailing slashes
  - Non-matching paths (pass-through)

**Integration Tests (C# for Launcher):**
- Mock GW2 process (simple test executable)
- Inject test DLL
- Verify DLL loaded (check exit code)
- Verify environment variables passed

**System Tests:**
- Automated GW2 launch with profile
- Wait 30 seconds for initialization
- Parse log file for "REDIRECT" count
- Check profile directory for expected files
- Kill GW2 process
- Verify no errors in log

---

## Debugging "No Files Created" Symptom

**Observation from PoC:**
- Log shows: `All filesystem hooks enabled successfully`
- Log shows: `REDIRECT: ...Local.dat -> <ProfileRoot>\Roaming\Guild Wars 2\Local.dat`
- BUT: No files appear in `<ProfileRoot>\Roaming\Guild Wars 2\`

**High-Signal Debugging Checklist:**

### 1. Verify Hook Is Actually Intercepting

**Test:**
```
Add trace logging in Hook_CreateFileW:
  Log("CreateFileW called: {lpFileName} (redirected={redirectedPath != originalPath})")
```

**Expected:** Log shows many `CreateFileW called:` entries for various files

**If missing:** Hooks not active or GW2 uses different API

**Action:** Check if GW2 uses:
- `CreateFileA` (ANSI variant) - Add hook for this
- `NtCreateFile` (native API) - MinHook may not intercept ntdll; add explicit hook
- `_wfopen` / CRT functions - Add hooks for `_wfopen`, `_wopen`

### 2. Check Module Boundary (kernel32 vs KernelBase)

**Issue:** On Windows 10+, `kernel32.dll` forwards to `KernelBase.dll`

**Test:**
```
Add logging in DllMain before MH_CreateHookApi:
  HMODULE hKernel32 = GetModuleHandle(L"kernel32.dll");
  HMODULE hKernelBase = GetModuleHandle(L"KernelBase.dll");
  FARPROC pCreateFileW_K32 = GetProcAddress(hKernel32, "CreateFileW");
  FARPROC pCreateFileW_KB = GetProcAddress(hKernelBase, "CreateFileW");
  Log("CreateFileW in kernel32: {pCreateFileW_K32}");
  Log("CreateFileW in KernelBase: {pCreateFileW_KB}");
```

**Expected:** Both addresses present; may be identical or different

**If different:** Try hooking `KernelBase.dll` instead:
```cpp
MH_CreateHookApi(L"KernelBase.dll", "CreateFileW", ...)
```

### 3. Verify Directory Creation Before File Open

**Issue:** File open may fail if parent directory doesn't exist

**Test:**
```
In Hook_CreateFileW, before calling g_OriginalCreateFileW:
  if (redirectedPath != originalPath) {
    wstring parentDir = GetParentDirectory(redirectedPath);
    DWORD attrs = GetFileAttributesW(parentDir.c_str());
    if (attrs == INVALID_FILE_ATTRIBUTES) {
      Log("WARNING: Parent dir does not exist: {parentDir}");
      Log("Attempting to create: {parentDir}");
      CreateDirectoryRecursive(parentDir);
    }
  }
```

**Expected:** Log shows parent directories being created

**If directories exist but files don't:** GW2 may be checking permissions first

### 4. Check Permissions on Profile Directories

**Test:**
```
Before launch, manually:
  cd /d <ProfileRoot>\Roaming
  echo test > test.txt
  del test.txt
```

**If fails:** Directory not writable

**Actions:**
- Check NTFS permissions (`icacls <ProfileRoot>`)
- Ensure not on network drive or OneDrive-synced folder
- Move profile to `C:\GW2Profiles\` (local disk)

### 5. Verify GW2 Doesn't Use Hard-Coded Paths

**Test:**
```
Search log for non-redirected AppData accesses:
  Log all CreateFileW calls (even non-redirected)
  Filter log for "AppData" without "REDIRECT:"
```

**Expected:** No entries OR only non-GW2 files (e.g., `explorer.exe` queries)

**If found:** GW2 has hard-coded path; may require patching (out of scope) OR is using launcher/patcher process (different PID)

### 6. Check for Lazy File Creation

**Issue:** GW2 may not create `Local.dat` until login attempt

**Test:**
- Let GW2 fully load (wait 60 seconds)
- Attempt to login (or reach character select)
- Check profile directory again

**Expected:** Files created after login UI interaction

**If still missing:** Redirection may not be working

### 7. Verify Hook DLL Stays Loaded

**Test:**
```
Add logging to DLL_PROCESS_DETACH:
  Log("DLL unloading at {time}");
  fflush(logFile);
```

**Check:** When does DLL unload relative to file operations?

**Expected:** DLL unloads only when GW2 exits

**If early unload:** DLL may have been `FreeLibrary`'d by GW2 or crashed

### 8. Check for Relative Paths or Current Directory

**Test:**
```
In Hook_CreateFileW, log current directory:
  wchar_t cwd[MAX_PATH];
  GetCurrentDirectoryW(MAX_PATH, cwd);
  Log("CreateFileW({lpFileName}) CWD={cwd}");
```

**Expected:** All paths are absolute (start with `C:\`)

**If relative:** GW2 may construct paths relative to CWD; may need path canonicalization

### 9. Enable Trace Mode: Log Caller Stack

**Test:**
```
Add stack trace logging (Windows-specific):
  void* stack[10];
  WORD frames = CaptureStackBackTrace(0, 10, stack, NULL);
  for (int i = 0; i < frames; i++) {
    HMODULE hModule;
    GetModuleHandleEx(GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS, (LPCTSTR)stack[i], &hModule);
    wchar_t modName[MAX_PATH];
    GetModuleFileNameW(hModule, modName, MAX_PATH);
    Log("  Frame {i}: {stack[i]} in {modName}");
  }
```

**Use:** Identify which GW2 module calls CreateFileW

**Expected:** Calls from `Gw2-64.exe` or `d3d9.dll` (game modules)

**If from different module:** May need to hook additional modules

### 10. Verify MinHook Trampoline Generation

**Test:**
```
After MH_EnableHook, verify original function pointer:
  if (g_OriginalCreateFileW == NULL) {
    Log("ERROR: Original CreateFileW pointer is NULL!");
  } else {
    Log("Original CreateFileW: {g_OriginalCreateFileW}");
  }
```

**Expected:** Non-NULL pointer to trampoline

**If NULL:** MinHook failed silently; check MH_STATUS return codes

### 11. Cross-Check with ProcMon

**Test:**
- Run ProcMon with filters: `Process Name is Gw2-64.exe` AND `Operation is CreateFile`
- Look for file operations on original AppData paths

**Expected:** ProcMon shows operations on original paths (because GW2 **passes** original path to kernel, but our hook **rewrites** it before kernel sees it)

**If ProcMon shows redirected paths:** Hooks ARE working; issue is elsewhere (permissions, timing, etc.)

**If ProcMon shows original paths AND files created there:** Hooks NOT working; paths not being rewritten

### 12. Check for ERROR_ACCESS_DENIED in Hook

**Test:**
```
In Hook_CreateFileW, after calling g_OriginalCreateFileW:
  HANDLE hFile = g_OriginalCreateFileW(...redirectedPath...);
  if (hFile == INVALID_HANDLE_VALUE) {
    DWORD err = GetLastError();
    Log("CreateFile FAILED: {redirectedPath} Error={err}");
  }
  return hFile;
```

**Expected:** No failures OR failures with ERROR_FILE_NOT_FOUND (normal)

**If ERROR_ACCESS_DENIED:** Profile directory or subfolders not writable

### 13. Verify Timing: Hooks Active Before GW2 Reads Registry

**Issue:** GW2 may cache paths before hooks initialize

**Test:**
- Add timestamp logging in DllMain
- Add timestamp logging for first `REDIRECT:` entry
- Compare: Is there a large gap?

**Expected:** First redirection within ~100ms of hook init

**If large gap (> 1 second):** GW2 may have already cached paths

**Mitigation:** Initialize hooks synchronously in DllMain (already done in PoC)

---

## Future Enhancements

### Enhancement 1: GUI for Profile Management

**Feature:**
- Visual list of profiles
- Create/delete/rename profiles
- Set active profile per launch
- Import/export profiles (backup `Local.dat`)

**Implementation:**
- Add profile manager window in launcher
- Store profile metadata in `profiles.json`
- Profile paths in `C:\GW2Profiles\<Name>\`

### Enhancement 2: Automatic Profile Switching

**Feature:**
- Detect account login from `Local.dat` (parse binary file)
- Auto-select profile based on last-used account
- Prompt user if multiple profiles match

**Implementation:**
- Parse `Local.dat` after login (complex binary format)
- Cache account name → profile mapping
- Show confirmation dialog before launch

### Enhancement 3: Symbolic Link Fallback (Admin-Optional)

**Feature:**
- If injection fails or user prefers, use symlinks instead
- Create symlinks from AppData to profile directories
- Requires one-time admin elevation

**Implementation:**
```csharp
if (IsAdmin) {
  CreateSymbolicLink(
    "%APPDATA%\\Guild Wars 2",
    "<ProfileRoot>\\Roaming\\Guild Wars 2",
    SYMBOLIC_LINK_FLAG_DIRECTORY
  );
}
```

**Pros:**
- No injection required
- No antivirus concerns

**Cons:**
- Requires admin for symlink creation
- Only one profile active at a time (no multi-instance)
- Must switch symlinks between launches

### Enhancement 4: Hook Additional APIs

**If needed for compatibility:**
- `FindFirstFileW` / `FindNextFileW` - Directory enumeration
- `GetFileAttributesExW` - Extended attribute queries
- `MoveFileW` / `MoveFileExW` - File moves
- `CopyFileW` / `CopyFileExW` - File copies
- `_wfopen` / `_wopen` - CRT functions (if GW2 uses them)
- `RegCreateKeyExW` / `RegSetValueExW` - If GW2 writes to registry (not observed)

**Implementation:**
- Add hooks following same pattern as existing
- Test with specific GW2 operations that trigger these APIs

### Enhancement 5: Log Filtering UI

**Feature:**
- GUI tool to parse and filter `Gw2FolderHook.log`
- Show only redirections, errors, or specific paths
- Export filtered log for bug reports

**Implementation:**
- WPF or WinForms app
- Parse log file line-by-line
- Apply regex filters for display

### Enhancement 6: Integrity Verification

**Feature:**
- Hash check for `Gw2FolderHook.dll` before injection
- Detect if DLL was modified or replaced
- Warn user if mismatch

**Implementation:**
- Embed expected SHA-256 hash in launcher
- Compute hash of DLL file at runtime
- Compare and prompt user if different

### Enhancement 7: Crash Dump Analysis

**Feature:**
- Automatically detect GW2 crashes after injection
- Capture minidump (if debugger attached)
- Parse dump for hook-related issues

**Implementation:**
- Use `SetUnhandledExceptionFilter` in hook DLL
- Write minidump on crash (`MiniDumpWriteDump`)
- Launcher checks for dump file after GW2 exit

### Enhancement 8: Profile Syncing (Cloud Backup)

**Feature:**
- Backup `Local.dat` to cloud storage (OneDrive, Dropbox, custom)
- Restore on new machine
- Scheduled backups

**Implementation:**
- Hook DLL triggers backup on `Local.dat` write
- Launcher uploads to cloud API
- Conflict resolution if multiple machines

---

## Appendices

### Appendix A: Win32 API Reference Quick Links

- [CreateProcess (MSDN)](https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-createprocessw)
- [VirtualAllocEx (MSDN)](https://learn.microsoft.com/en-us/windows/win32/api/memoryapi/nf-memoryapi-virtualallocex)
- [WriteProcessMemory (MSDN)](https://learn.microsoft.com/en-us/windows/win32/api/memoryapi/nf-memoryapi-writeprocessmemory)
- [CreateRemoteThread (MSDN)](https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-createremotethread)
- [CreateFileW (MSDN)](https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-createfilew)
- [SHGetFolderPathW (MSDN)](https://learn.microsoft.com/en-us/windows/win32/api/shlobj_core/nf-shlobj_core-shgetfolderpathw)

### Appendix B: MinHook Documentation

- [MinHook GitHub](https://github.com/TsudaKageyu/minhook)
- [MinHook API Reference](https://github.com/TsudaKageyu/minhook/blob/master/README.md)

### Appendix C: ProcMon Filters for Diagnosis

**Recommended ProcMon configuration:**
1. Filter: `Process Name is Gw2-64.exe`
2. Include: `Operation is CreateFile`
3. Include: `Operation is RegOpenKey`
4. Include: `Path contains AppData`
5. Export to CSV for analysis

### Appendix D: Example Log Output (Success Case)

```
[2026-01-12 22:27:38.610] === Gw2FolderHook DLL loaded (FILESYSTEM REDIRECTION) ===
[2026-01-12 22:27:38.610] Process ID: 57220
[2026-01-12 22:27:38.613] Original RoamingAppData: C:\Users\Chris\AppData\Roaming
[2026-01-12 22:27:38.614] Original LocalAppData: C:\Users\Chris\AppData\Local
[2026-01-12 22:27:38.614] RoamingAppData redirect configured: C:\GW2Profiles\RegistryTest\Roaming
[2026-01-12 22:27:38.614] LocalAppData redirect configured: C:\GW2Profiles\RegistryTest\Local
[2026-01-12 22:27:38.614] MinHook initialized
[2026-01-12 22:27:38.664] All filesystem hooks enabled successfully
[2026-01-12 22:27:38.664] Filesystem redirection active - monitoring file operations
[2026-01-12 22:27:39.212] REDIRECT: C:\Users\Chris\AppData\Roaming\Guild Wars 2\Settings.json -> C:\GW2Profiles\RegistryTest\Roaming\Guild Wars 2\Settings.json
[2026-01-12 22:27:40.027] REDIRECT: C:\Users\Chris\AppData\Roaming\Guild Wars 2 -> C:\GW2Profiles\RegistryTest\Roaming\Guild Wars 2
[2026-01-12 22:27:40.036] REDIRECT: C:\Users\Chris\AppData\Roaming\Guild Wars 2\Local.dat -> C:\GW2Profiles\RegistryTest\Roaming\Guild Wars 2\Local.dat
[2026-01-12 22:27:40.320] REDIRECT: C:\Users\Chris\AppData\Roaming\Guild Wars 2\GFXSettings.Gw2-64.exe.xml -> C:\GW2Profiles\RegistryTest\Roaming\Guild Wars 2\GFXSettings.Gw2-64.exe.xml
[... 200+ redirections of cache files ...]
[2026-01-12 22:27:47.736] === Gw2FolderHook DLL unloading ===
```

---

**End of Document**

This design document provides a complete implementation blueprint for integrating the GW2 AppData redirection proof-of-concept into GWxLauncher. Engineers should follow the specified launch sequence, hooking strategy, and debugging checklist to achieve reliable per-profile file isolation without requiring administrator privileges.