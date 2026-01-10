# Technical Guide: Automating Keyboard Input to CEF (Chromium Embedded Framework) Applications

## Executive Summary

This document describes the technical challenges and solutions for automating keyboard input to applications using CEF (Chromium Embedded Framework), specifically focusing on the Guild Wars 2 launcher. The key discovery: **CEF requires hardware scan codes in addition to virtual key codes for SendInput to work**.

## Problem Statement

Standard Windows automation techniques (`SendMessage`, `PostMessage`) do not work with CEF-based applications. Even using `SendInput` (the recommended API for keyboard simulation) fails without proper scan codes.

### Why Standard Approaches Fail

1. **SendMessage/PostMessage with WM_KEYDOWN/WM_KEYUP**: CEF ignores window messages sent directly to the window handle
2. **SendMessage/PostMessage with WM_CHAR**: CEF ignores character messages
3. **SendInput without scan codes**: CEF/Chromium validates input against hardware expectations and ignores synthetic input missing scan codes

## The Solution: SendInput with Hardware Scan Codes

### Core Concept

Physical keyboard input sends two pieces of information:
- **Virtual Key Code (VK)**: Logical key identifier (e.g., `VK_TAB = 0x09`)
- **Scan Code**: Hardware-specific code representing the physical key position

CEF validates both to distinguish between legitimate hardware input and synthetic/scripted input. By including scan codes via `MapVirtualKey`, we make `SendInput` indistinguishable from real keyboard input.

### Implementation

#### 1. Add MapVirtualKey P/Invoke

```csharp
// In NativeMethods.cs or equivalent
[DllImport("user32.dll")]
internal static extern uint MapVirtualKey(uint uCode, uint uMapType);
```

#### 2. Implement SendKey with Scan Codes

```csharp
private static void SendKey(ushort vk, bool keyUp)
{
    INPUT[] inputs = new INPUT[1];
    inputs[0] = new INPUT
    {
        type = INPUT_KEYBOARD,  // 1 = keyboard input
        U = new InputUnion
        {
            ki = new KEYBDINPUT
            {
                wVk = vk,
                wScan = (ushort)MapVirtualKey(vk, 0), // CRITICAL: Get hardware scan code
                dwFlags = keyUp ? KEYEVENTF_KEYUP : 0,
                time = 0,
                dwExtraInfo = IntPtr.Zero
            }
        }
    };

    SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
}
```

**Key Parameters:**
- `MapVirtualKey(vk, 0)`: Converts virtual key to scan code (MAPVK_VK_TO_VSC)
- `wScan`: **Must be set** - CEF rejects input without valid scan codes
- `dwFlags`: `0` for key down, `KEYEVENTF_KEYUP` (0x0002) for key up

#### 3. Send Complete Key Presses

CEF requires proper key press sequences (DOWN ? UP):

```csharp
// Tab navigation example (14 tabs to reach email field in GW2 launcher)
for (int i = 0; i < 14; i++)
{
    SendKey(VK_TAB, keyUp: false);  // Key down
    Thread.Sleep(10);                // Brief delay
    SendKey(VK_TAB, keyUp: true);   // Key up
    Thread.Sleep(10);                // Brief delay
}
```

**Critical Rules:**
- Always send KEYDOWN followed by KEYUP
- Include small delays (10-20ms) between events
- Don't send multiple KEYDOWN without intervening KEYUP

#### 4. Text Input via Clipboard (Ctrl+V)

For password/email fields, use clipboard paste with scan codes:

```csharp
private static bool TryTypeViaClipboard(IntPtr hwnd, string text)
{
    // Save current clipboard
    string? savedClip = Clipboard.ContainsText() ? Clipboard.GetText() : null;
    
    // Set text to clipboard
    Clipboard.SetText(text);
    Thread.Sleep(50);

    // Send Ctrl+V with scan codes
    SendKey(VK_CONTROL, keyUp: false);
    Thread.Sleep(10);
    SendKey((ushort)'V', keyUp: false);
    Thread.Sleep(10);
    SendKey((ushort)'V', keyUp: true);
    Thread.Sleep(10);
    SendKey(VK_CONTROL, keyUp: true);
    
    Thread.Sleep(100); // Allow app to process paste

    // Restore clipboard
    if (savedClip != null)
        Clipboard.SetText(savedClip);
    else
        Clipboard.Clear();

    return true;
}
```

#### 5. Unicode Text Fallback

For non-ASCII characters or when clipboard fails:

```csharp
private static void SendUnicodeChar(char c)
{
    INPUT[] inputs = new INPUT[2];

    inputs[0] = new INPUT
    {
        type = INPUT_KEYBOARD,
        U = new InputUnion
        {
            ki = new KEYBDINPUT
            {
                wVk = 0,                    // No virtual key
                wScan = c,                  // Character itself is the "scan"
                dwFlags = KEYEVENTF_UNICODE,
                time = 0,
                dwExtraInfo = IntPtr.Zero
            }
        }
    };

    inputs[1] = new INPUT
    {
        type = INPUT_KEYBOARD,
        U = new InputUnion
        {
            ki = new KEYBDINPUT
            {
                wVk = 0,
                wScan = c,
                dwFlags = KEYEVENTF_UNICODE | KEYEVENTF_KEYUP,
                time = 0,
                dwExtraInfo = IntPtr.Zero
            }
        }
    };

    SendInput(2, inputs, Marshal.SizeOf(typeof(INPUT)));
}
```

## Critical Prerequisites

### 1. Window Focus

**SendInput only works on the focused window.** Before any keyboard automation:

```csharp
private static bool ForceAndHoldForeground(IntPtr hwnd, int holdStableMs, int timeoutMs)
{
    var sw = Stopwatch.StartNew();
    while (sw.ElapsedMilliseconds < timeoutMs)
    {
        ForceForeground(hwnd);
        Thread.Sleep(60);
        
        if (IsForegroundStable(hwnd, holdStableMs))
            return true;

        Thread.Sleep(80);
    }
    return false;
}

private static void ForceForeground(IntPtr hwnd)
{
    if (IsIconic(hwnd))
        ShowWindow(hwnd, SW_RESTORE);

    SetForegroundWindow(hwnd);
    BringWindowToTop(hwnd);

    // Thread input attachment for reliable focus
    IntPtr fg = GetForegroundWindow();
    uint fgTid = GetWindowThreadProcessId(fg, out _);
    uint myTid = GetCurrentThreadId();
    uint targetTid = GetWindowThreadProcessId(hwnd, out _);

    AttachThreadInput(myTid, fgTid, true);
    AttachThreadInput(myTid, targetTid, true);
    try
    {
        SetForegroundWindow(hwnd);
        SetFocus(hwnd);
    }
    finally
    {
        AttachThreadInput(myTid, targetTid, false);
        AttachThreadInput(myTid, fgTid, false);
    }
}
```

### 2. Wait for Modifier Keys to Be Released

User might have modifier keys pressed when automation starts:

```csharp
private static bool WaitForModifierKeysUp(int timeoutMs)
{
    var sw = Stopwatch.StartNew();
    while (sw.ElapsedMilliseconds < timeoutMs)
    {
        if (!IsKeyDown(VK_SHIFT) && 
            !IsKeyDown(VK_CONTROL) && 
            !IsKeyDown(VK_MENU))
            return true;

        Thread.Sleep(50);
    }
    return false;
}

private static bool IsKeyDown(int vk)
{
    return (GetAsyncKeyState(vk) & 0x8000) != 0;
}
```

### 3. Child Process Priority (Optional but Recommended)

Boost CEF renderer process priority to reduce input lag:

```csharp
// Find CefHost.exe or CoherentUI_Host.exe child processes
var childProcesses = Process.GetProcessesByName("CefHost")
    .Where(p => GetParentProcessId(p.Id) == parentProcessId)
    .ToList();

foreach (var child in childProcesses)
{
    try 
    { 
        child.PriorityClass = ProcessPriorityClass.High; 
    }
    catch { }
}
```

## Common Pitfalls and Solutions

### Pitfall 1: Missing Scan Codes

**Symptom:** SendInput calls succeed but nothing happens in the CEF application.

**Solution:** Always use `MapVirtualKey` to get scan codes:
```csharp
wScan = (ushort)MapVirtualKey(vk, 0)  // NOT wScan = 0
```

### Pitfall 2: Incomplete Key Sequences

**Symptom:** Random characters appear or input is skipped.

**Solution:** Always send complete DOWN?UP sequences:
```csharp
SendKey(vk, keyUp: false);  // DOWN
Thread.Sleep(10);
SendKey(vk, keyUp: true);   // UP
```

### Pitfall 3: SendInput to Unfocused Window

**Symptom:** Input works intermittently or not at all.

**Solution:** Verify focus before every input operation:
```csharp
if (GetForegroundWindow() != targetHwnd)
{
    if (!ForceAndHoldForeground(targetHwnd, 250, 2000))
        throw new Exception("Lost focus");
}
```

### Pitfall 4: Using SendMessage/PostMessage

**Symptom:** No input reaches CEF.

**Solution:** CEF ignores window messages for keyboard input. **Always use SendInput with scan codes.**

### Pitfall 5: Window Disabled (WS_DISABLED)

**Symptom:** SendInput stops working after disabling window.

**Solution:** Don't disable the window. CEF needs to be enabled to receive input. Rely on automation speed instead.

## Architecture-Specific Notes

### CEF Multi-Process Architecture

CEF applications spawn multiple processes:
- **Browser Process**: Main window (e.g., "ArenaNet" class)
- **Renderer Processes**: CefHost.exe instances
- **GPU Process**: Hardware acceleration
- **Utility Processes**: Networking, etc.

**You target the main browser window** - SendInput routes through the OS input queue to the focused window, which CEF internally routes to the correct renderer process.

### Coordinate Systems

For mouse clicks in CEF (if needed):

**Window-Relative Coordinates for SendMessage:**
```csharp
GetWindowRect(hwnd, out RECT windowRect);
int windowW = windowRect.Right - windowRect.Left;
int windowH = windowRect.Bottom - windowRect.Top;

// Coordinates relative to window top-left (0,0)
int x = (int)(windowW * 0.5);  // 50% from left
int y = (int)(windowH * 0.5);  // 50% from top

uint coord = (uint)((y << 16) | x);
SendMessage(hwnd, WM_LBUTTONDOWN, 1, coord);
SendMessage(hwnd, WM_LBUTTONUP, 0, coord);
```

## Complete Example: GW2 Launcher Auto-Login

```csharp
public bool AutomateLogin(IntPtr gw2Hwnd, string email, string password)
{
    const ushort VK_TAB = 0x09;
    const ushort VK_RETURN = 0x0D;

    try
    {
        // 1. Ensure window has focus
        if (!ForceAndHoldForeground(gw2Hwnd, 250, 5000))
            throw new Exception("Failed to get window focus");

        // 2. Wait for modifier keys to be released
        if (!WaitForModifierKeysUp(5000))
            throw new Exception("Modifier keys held down");

        // 3. Click empty area to clear focus (optional)
        // Uses SendMessage with window-relative coordinates
        uint emptyCoord = GetEmptyAreaCoord(gw2Hwnd);
        SendMessage(gw2Hwnd, WM_LBUTTONDOWN, 1, emptyCoord);
        SendMessage(gw2Hwnd, WM_LBUTTONUP, 0, emptyCoord);
        Thread.Sleep(50);

        // 4. Tab to email field (14 tabs for CEF launcher)
        for (int i = 0; i < 14; i++)
        {
            SendKey(VK_TAB, keyUp: false);
            Thread.Sleep(10);
            SendKey(VK_TAB, keyUp: true);
            Thread.Sleep(10);
        }
        Thread.Sleep(100);

        // 5. Enter email via clipboard
        if (!TryTypeViaClipboard(gw2Hwnd, email))
            throw new Exception("Failed to enter email");
        Thread.Sleep(100);

        // 6. Tab to password field
        SendKey(VK_TAB, keyUp: false);
        Thread.Sleep(10);
        SendKey(VK_TAB, keyUp: true);
        Thread.Sleep(100);

        // 7. Enter password via clipboard
        if (!TryTypeViaClipboard(gw2Hwnd, password))
            throw new Exception("Failed to enter password");
        Thread.Sleep(250);

        // 8. Press Enter to submit
        SendKey(VK_RETURN, keyUp: false);
        Thread.Sleep(15);
        SendKey(VK_RETURN, keyUp: true);

        return true;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Auto-login failed: {ex.Message}");
        return false;
    }
}
```

## Testing and Validation

### Verify Scan Code Generation

```csharp
// Test MapVirtualKey
uint scanTab = MapVirtualKey(0x09, 0);    // Should return ~15 (0x0F)
uint scanEnter = MapVirtualKey(0x0D, 0);  // Should return ~28 (0x1C)
uint scanA = MapVirtualKey(0x41, 0);      // Should return ~30 (0x1E)

Console.WriteLine($"Tab scan: {scanTab:X}");
Console.WriteLine($"Enter scan: {scanEnter:X}");
Console.WriteLine($"A scan: {scanA:X}");
```

### Manual Testing

1. Launch the CEF application
2. Ensure it has focus
3. Run automation
4. **Expected:** Text appears in fields, navigation works
5. **If fails:** Check event viewer, enable detailed logging

### Troubleshooting Checklist

- [ ] Window has focus (`GetForegroundWindow() == targetHwnd`)
- [ ] Scan codes are non-zero (`wScan != 0`)
- [ ] Complete key sequences (DOWN followed by UP)
- [ ] Delays between key events (10-20ms)
- [ ] Window is NOT disabled (`WS_DISABLED` not set)
- [ ] Modifier keys are released before automation
- [ ] SendInput returns success (not zero)

## Performance Considerations

### Timing Guidelines

- **Between key events**: 10-20ms
- **After key sequence**: 50-100ms
- **After clipboard paste**: 100-150ms
- **After form submission**: 200-500ms

### Reliability Improvements

1. **Retry logic**: Retry focus acquisition if lost
2. **Verification**: Sample screen pixels to verify UI state
3. **Graceful degradation**: Fall back to Unicode input if clipboard fails
4. **User interference detection**: Monitor for modifier key presses during automation

## References

### Windows API Documentation

- [SendInput](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-sendinput)
- [MapVirtualKey](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-mapvirtualkeyw)
- [INPUT Structure](https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-input)
- [KEYBDINPUT Structure](https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-keybdinput)

### CEF Resources

- [Chromium Embedded Framework](https://bitbucket.org/chromiumembedded/cef)
- [CEF Input Handling](https://magpcss.org/ceforum/viewtopic.php?f=6&t=12641)

### Related Projects

- [Gw2Launcher](https://github.com/Healix/Gw2Launcher) - Reference implementation

## Conclusion

Automating keyboard input to CEF applications requires:

1. **SendInput** (not SendMessage/PostMessage)
2. **Hardware scan codes** via MapVirtualKey (critical!)
3. **Window focus** before any input
4. **Complete key sequences** (DOWN ? UP)
5. **Proper timing** between events

The scan code requirement is the key differentiator - without it, CEF/Chromium treats input as potentially malicious and ignores it. Including scan codes makes synthetic input indistinguishable from physical keyboard input.

---

**Document Version:** 1.0  
**Last Updated:** January 2026  
**Author:** Technical analysis of GW2 auto-login implementation  
**License:** MIT (adapt freely for your projects)
