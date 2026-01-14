// Gw2FolderHook.cpp
// Proof of Concept: Per-Process AppData Redirection for Guild Wars 2 (FILESYSTEM HOOKS)
//
// This DLL hooks Windows filesystem APIs to redirect AppData file operations
// for the current process only. It's injected into GW2 before the game initializes
// to ensure all file operations go through our hooks.
//
// Strategy: Let GW2 discover AppData paths however it wants (registry, env vars, etc.),
// then intercept at the filesystem boundary and rewrite paths containing "AppData\Roaming"
// or "AppData\Local" to use profile-specific directories.

#include <windows.h>
#include <shlobj.h>
#include <string>
#include <fstream>
#include <mutex>
#include <ctime>
#include <algorithm>
#include "MinHook/include/MinHook.h"

// ============================================================================
// Global State
// ============================================================================

// Original AppData paths detected from system
static std::wstring g_OriginalRoamingPath;  // e.g., C:\Users\Chris\AppData\Roaming
static std::wstring g_OriginalLocalPath;    // e.g., C:\Users\Chris\AppData\Local

// Redirected paths (read from environment variables on startup)
static std::wstring g_RedirectRoamingPath;  // e.g., C:\Temp\GW2Test\Profile1\Roaming
static std::wstring g_RedirectLocalPath;    // e.g., C:\Temp\GW2Test\Profile1\Local

static std::wstring g_LogFilePath;

// Original function pointers (saved by MinHook)
typedef HANDLE(WINAPI* CreateFileW_t)(
    LPCWSTR lpFileName,
    DWORD dwDesiredAccess,
    DWORD dwShareMode,
    LPSECURITY_ATTRIBUTES lpSecurityAttributes,
    DWORD dwCreationDisposition,
    DWORD dwFlagsAndAttributes,
    HANDLE hTemplateFile);

typedef BOOL(WINAPI* CreateDirectoryW_t)(
    LPCWSTR lpPathName,
    LPSECURITY_ATTRIBUTES lpSecurityAttributes);

typedef DWORD(WINAPI* GetFileAttributesW_t)(
    LPCWSTR lpFileName);

typedef BOOL(WINAPI* SetFileAttributesW_t)(
    LPCWSTR lpFileName,
    DWORD dwFileAttributes);

typedef BOOL(WINAPI* DeleteFileW_t)(
    LPCWSTR lpFileName);

typedef BOOL(WINAPI* RemoveDirectoryW_t)(
    LPCWSTR lpPathName);

static CreateFileW_t g_OriginalCreateFileW = nullptr;
static CreateDirectoryW_t g_OriginalCreateDirectoryW = nullptr;
static GetFileAttributesW_t g_OriginalGetFileAttributesW = nullptr;
static SetFileAttributesW_t g_OriginalSetFileAttributesW = nullptr;
static DeleteFileW_t g_OriginalDeleteFileW = nullptr;
static RemoveDirectoryW_t g_OriginalRemoveDirectoryW = nullptr;

// Thread safety for logging
static std::mutex g_LogMutex;
static int g_RedirectCount = 0;

// ============================================================================
// String Utilities
// ============================================================================

// Case-insensitive string search
bool ContainsIgnoreCase(const std::wstring& haystack, const std::wstring& needle)
{
    std::wstring haystackLower = haystack;
    std::wstring needleLower = needle;
    
    std::transform(haystackLower.begin(), haystackLower.end(), haystackLower.begin(), ::towlower);
    std::transform(needleLower.begin(), needleLower.end(), needleLower.begin(), ::towlower);
    
    return haystackLower.find(needleLower) != std::wstring::npos;
}

// Case-insensitive string replace (first occurrence)
std::wstring ReplaceIgnoreCase(const std::wstring& original, const std::wstring& oldStr, const std::wstring& newStr)
{
    std::wstring originalLower = original;
    std::wstring oldStrLower = oldStr;
    
    std::transform(originalLower.begin(), originalLower.end(), originalLower.begin(), ::towlower);
    std::transform(oldStrLower.begin(), oldStrLower.end(), oldStrLower.begin(), ::towlower);
    
    size_t pos = originalLower.find(oldStrLower);
    if (pos == std::wstring::npos)
        return original;
    
    std::wstring result = original;
    result.replace(pos, oldStr.length(), newStr);
    return result;
}

// ============================================================================
// Logging Utilities
// ============================================================================

void LogMessage(const std::wstring& message, bool isError = false)
{
    std::lock_guard<std::mutex> lock(g_LogMutex);

    try
    {
        std::wstring logPath = isError && !g_LogFilePath.empty()
            ? g_LogFilePath.substr(0, g_LogFilePath.find_last_of(L'.')) + L"_error.log"
            : g_LogFilePath;

        if (logPath.empty())
            logPath = L"C:\\Temp\\Gw2FolderHook.log";

        std::wofstream logFile(logPath, std::ios::app);
        if (logFile.is_open())
        {
            // Get timestamp
            time_t now = time(nullptr);
            tm timeInfo;
            localtime_s(&timeInfo, &now);
            wchar_t timeStr[32];
            wcsftime(timeStr, sizeof(timeStr) / sizeof(wchar_t), L"%Y-%m-%d %H:%M:%S", &timeInfo);

            logFile << L"[" << timeStr << L"] " << message << std::endl;
            logFile.close();
        }
    }
    catch (...)
    {
        // Silently fail - we can't risk crashing the host process
    }
}

void LogError(const std::wstring& message)
{
    LogMessage(L"ERROR: " + message, true);
}

// ============================================================================
// Path Redirection Logic
// ============================================================================

std::wstring RedirectPath(const std::wstring& originalPath)
{
    if (originalPath.empty())
        return originalPath;
    
    // Check if path contains Roaming AppData
    if (ContainsIgnoreCase(originalPath, g_OriginalRoamingPath))
    {
        std::wstring redirected = ReplaceIgnoreCase(originalPath, g_OriginalRoamingPath, g_RedirectRoamingPath);
        
        // Log first 20 redirections, then every 100th to reduce spam
        g_RedirectCount++;
        if (g_RedirectCount <= 20 || (g_RedirectCount % 100) == 0)
        {
            LogMessage(L"REDIRECT [" + std::to_wstring(g_RedirectCount) + L"]: " + originalPath + L" -> " + redirected);
        }
        
        return redirected;
    }
    
    // Check if path contains Local AppData
    if (ContainsIgnoreCase(originalPath, g_OriginalLocalPath))
    {
        std::wstring redirected = ReplaceIgnoreCase(originalPath, g_OriginalLocalPath, g_RedirectLocalPath);
        
        g_RedirectCount++;
        if (g_RedirectCount <= 20 || (g_RedirectCount % 100) == 0)
        {
            LogMessage(L"REDIRECT [" + std::to_wstring(g_RedirectCount) + L"]: " + originalPath + L" -> " + redirected);
        }
        
        return redirected;
    }
    
    // No redirection needed
    return originalPath;
}

// Recursively create parent directories
void EnsureDirectoryExists(const std::wstring& path)
{
    if (path.empty() || path.length() < 3)
        return;
    
    // Find last slash
    size_t lastSlash = path.find_last_of(L"\\/");
    if (lastSlash == std::wstring::npos || lastSlash < 3)
        return;
    
    std::wstring parentDir = path.substr(0, lastSlash);
    
    // Check if parent exists
    DWORD attrs = g_OriginalGetFileAttributesW(parentDir.c_str());
    if (attrs == INVALID_FILE_ATTRIBUTES)
    {
        // Parent doesn't exist, create it recursively
        EnsureDirectoryExists(parentDir);
        
        // Now create this directory
        g_OriginalCreateDirectoryW(parentDir.c_str(), NULL);
    }
}

// ============================================================================
// Hooked Functions - Filesystem APIs
// ============================================================================

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
        std::wstring originalPath(lpFileName);
        std::wstring redirectedPath = RedirectPath(originalPath);
        
        if (redirectedPath != originalPath)
        {
            // Ensure parent directory exists
            EnsureDirectoryExists(redirectedPath);
            
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
    return g_OriginalCreateFileW(lpFileName, dwDesiredAccess, dwShareMode, 
        lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
}

BOOL WINAPI Hook_CreateDirectoryW(
    LPCWSTR lpPathName,
    LPSECURITY_ATTRIBUTES lpSecurityAttributes)
{
    if (lpPathName)
    {
        std::wstring originalPath(lpPathName);
        std::wstring redirectedPath = RedirectPath(originalPath);
        
        if (redirectedPath != originalPath)
        {
            // Ensure parent directory exists
            EnsureDirectoryExists(redirectedPath);
            
            // Call original with redirected path
            return g_OriginalCreateDirectoryW(redirectedPath.c_str(), lpSecurityAttributes);
        }
    }
    
    return g_OriginalCreateDirectoryW(lpPathName, lpSecurityAttributes);
}

DWORD WINAPI Hook_GetFileAttributesW(LPCWSTR lpFileName)
{
    if (lpFileName)
    {
        std::wstring originalPath(lpFileName);
        std::wstring redirectedPath = RedirectPath(originalPath);
        
        if (redirectedPath != originalPath)
        {
            return g_OriginalGetFileAttributesW(redirectedPath.c_str());
        }
    }
    
    return g_OriginalGetFileAttributesW(lpFileName);
}

BOOL WINAPI Hook_SetFileAttributesW(LPCWSTR lpFileName, DWORD dwFileAttributes)
{
    if (lpFileName)
    {
        std::wstring originalPath(lpFileName);
        std::wstring redirectedPath = RedirectPath(originalPath);
        
        if (redirectedPath != originalPath)
        {
            return g_OriginalSetFileAttributesW(redirectedPath.c_str(), dwFileAttributes);
        }
    }
    
    return g_OriginalSetFileAttributesW(lpFileName, dwFileAttributes);
}

BOOL WINAPI Hook_DeleteFileW(LPCWSTR lpFileName)
{
    if (lpFileName)
    {
        std::wstring originalPath(lpFileName);
        std::wstring redirectedPath = RedirectPath(originalPath);
        
        if (redirectedPath != originalPath)
        {
            return g_OriginalDeleteFileW(redirectedPath.c_str());
        }
    }
    
    return g_OriginalDeleteFileW(lpFileName);
}

BOOL WINAPI Hook_RemoveDirectoryW(LPCWSTR lpPathName)
{
    if (lpPathName)
    {
        std::wstring originalPath(lpPathName);
        std::wstring redirectedPath = RedirectPath(originalPath);
        
        if (redirectedPath != originalPath)
        {
            return g_OriginalRemoveDirectoryW(redirectedPath.c_str());
        }
    }
    
    return g_OriginalRemoveDirectoryW(lpPathName);
}

// ============================================================================
// Hook Installation
// ============================================================================

bool InstallHooks()
{
    LogMessage(L"========================================");
    LogMessage(L"Gw2FolderHook v0.2.0 - Initializing (FILESYSTEM REDIRECTION)");
    LogMessage(L"========================================");
    LogMessage(L"Process ID: " + std::to_wstring(GetCurrentProcessId()));

    // STEP 1: Detect original AppData paths BEFORE hooking
    wchar_t buffer[MAX_PATH];
    
    if (SHGetFolderPathW(NULL, CSIDL_APPDATA, NULL, 0, buffer) == S_OK)
    {
        g_OriginalRoamingPath = buffer;
        LogMessage(L"Original RoamingAppData: " + g_OriginalRoamingPath);
    }
    else
    {
        LogError(L"Failed to get original Roaming AppData path!");
    }
    
    if (SHGetFolderPathW(NULL, CSIDL_LOCAL_APPDATA, NULL, 0, buffer) == S_OK)
    {
        g_OriginalLocalPath = buffer;
        LogMessage(L"Original LocalAppData: " + g_OriginalLocalPath);
    }
    else
    {
        LogError(L"Failed to get original Local AppData path!");
    }

    // STEP 2: Read configuration from environment variables
    if (GetEnvironmentVariableW(L"GW2_REDIRECT_ROAMING", buffer, MAX_PATH) > 0)
    {
        g_RedirectRoamingPath = buffer;
        LogMessage(L"RoamingAppData redirect configured: " + g_RedirectRoamingPath);
    }
    else
    {
        LogError(L"Environment variable GW2_REDIRECT_ROAMING not set!");
    }

    if (GetEnvironmentVariableW(L"GW2_REDIRECT_LOCAL", buffer, MAX_PATH) > 0)
    {
        g_RedirectLocalPath = buffer;
        LogMessage(L"LocalAppData redirect configured: " + g_RedirectLocalPath);
    }
    else
    {
        LogError(L"Environment variable GW2_REDIRECT_LOCAL not set!");
    }

    if (GetEnvironmentVariableW(L"GW2_HOOK_LOG", buffer, MAX_PATH) > 0)
    {
        g_LogFilePath = buffer;
    }

    // Verify we have at least one redirect configured
    if (g_RedirectRoamingPath.empty() && g_RedirectLocalPath.empty())
    {
        LogError(L"No redirection paths configured - aborting hook installation");
        return false;
    }

    // STEP 3: Initialize MinHook
    MH_STATUS status = MH_Initialize();
    if (status != MH_OK)
    {
        LogError(L"MH_Initialize failed: " + std::to_wstring(status));
        return false;
    }

    LogMessage(L"MinHook initialized successfully");

    // STEP 4: Create hooks for filesystem APIs
    status = MH_CreateHookApi(
        L"kernel32.dll",
        "CreateFileW",
        &Hook_CreateFileW,
        reinterpret_cast<LPVOID*>(&g_OriginalCreateFileW));

    if (status != MH_OK)
    {
        LogError(L"Failed to create hook for CreateFileW: " + std::to_wstring(status));
        MH_Uninitialize();
        return false;
    }
    LogMessage(L"Created hook for CreateFileW");

    status = MH_CreateHookApi(
        L"kernel32.dll",
        "CreateDirectoryW",
        &Hook_CreateDirectoryW,
        reinterpret_cast<LPVOID*>(&g_OriginalCreateDirectoryW));

    if (status != MH_OK)
    {
        LogError(L"Failed to create hook for CreateDirectoryW: " + std::to_wstring(status));
    }
    else
    {
        LogMessage(L"Created hook for CreateDirectoryW");
    }

    status = MH_CreateHookApi(
        L"kernel32.dll",
        "GetFileAttributesW",
        &Hook_GetFileAttributesW,
        reinterpret_cast<LPVOID*>(&g_OriginalGetFileAttributesW));

    if (status != MH_OK)
    {
        LogError(L"Failed to create hook for GetFileAttributesW: " + std::to_wstring(status));
    }
    else
    {
        LogMessage(L"Created hook for GetFileAttributesW");
    }

    status = MH_CreateHookApi(
        L"kernel32.dll",
        "SetFileAttributesW",
        &Hook_SetFileAttributesW,
        reinterpret_cast<LPVOID*>(&g_OriginalSetFileAttributesW));

    if (status != MH_OK)
    {
        LogError(L"Failed to create hook for SetFileAttributesW: " + std::to_wstring(status));
    }
    else
    {
        LogMessage(L"Created hook for SetFileAttributesW");
    }

    status = MH_CreateHookApi(
        L"kernel32.dll",
        "DeleteFileW",
        &Hook_DeleteFileW,
        reinterpret_cast<LPVOID*>(&g_OriginalDeleteFileW));

    if (status != MH_OK)
    {
        LogError(L"Failed to create hook for DeleteFileW: " + std::to_wstring(status));
    }
    else
    {
        LogMessage(L"Created hook for DeleteFileW");
    }

    status = MH_CreateHookApi(
        L"kernel32.dll",
        "RemoveDirectoryW",
        &Hook_RemoveDirectoryW,
        reinterpret_cast<LPVOID*>(&g_OriginalRemoveDirectoryW));

    if (status != MH_OK)
    {
        LogError(L"Failed to create hook for RemoveDirectoryW: " + std::to_wstring(status));
    }
    else
    {
        LogMessage(L"Created hook for RemoveDirectoryW");
    }

    // STEP 5: Enable all hooks
    status = MH_EnableHook(MH_ALL_HOOKS);
    if (status != MH_OK)
    {
        LogError(L"MH_EnableHook failed: " + std::to_wstring(status));
        MH_Uninitialize();
        return false;
    }

    LogMessage(L"All filesystem hooks enabled successfully");
    LogMessage(L"========================================");
    LogMessage(L"Filesystem redirection active - monitoring file operations");
    LogMessage(L"========================================");

    return true;
}

void UninstallHooks()
{
    LogMessage(L"========================================");
    LogMessage(L"Uninstalling filesystem hooks...");
    LogMessage(L"Total redirections: " + std::to_wstring(g_RedirectCount));
    LogMessage(L"========================================");

    MH_DisableHook(MH_ALL_HOOKS);
    MH_Uninitialize();

    LogMessage(L"Hooks uninstalled successfully");
}

// ============================================================================
// DLL Entry Point
// ============================================================================

BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved)
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        // Don't do heavy work in DllMain - but hooking early is critical
        // We'll take the risk here since timing is everything
        DisableThreadLibraryCalls(hModule);
        
        if (!InstallHooks())
        {
            // Hook installation failed - log it but don't prevent DLL load
            // (Prevents process crash if we return FALSE here)
            LogError(L"DLL_PROCESS_ATTACH: Hook installation failed!");
        }
        break;

    case DLL_PROCESS_DETACH:
        UninstallHooks();
        break;

    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
        break;
    }
    return TRUE;
}
