# GW2 Multiclient Method Documentation (Reference Implementation)

This document records the **exact multiclient mechanism** shown in the provided `mcpatch.cs` reference source.

It answers:
- how the mutex is identified
- how the launcher decides a mutex is blocking a new launch
- how the launcher attempts to remove the mutex (normal + elevated fallback)
- how `-sharedArchive` relates to “shared mode” vs “single” mode

This document does **not** cover GW1 (different technique) and does **not** assume anything beyond what the source explicitly shows.

---

## 1) Mutex identification (GW2)

The reference identifies the Guild Wars 2 single-instance mutex by name and opens it directly.

### Mutex name used for GW2
- `"AN-Mutex-Window-Guild Wars 2"` 

### How it is checked
The code attempts to open the mutex:

- `Mutex.TryOpenExisting("AN-Mutex-Window-Guild Wars 2", out mutex)`

If it succeeds, the mutex is considered present/open, and the launcher treats it as a launch blocker.

### GW1 mutex (for completeness in the same reference file)
- `"AN-Mutex-Window-Guild Wars"` 

---

## 2) Detecting a blocking mutex

### Direct “is it open?” check
The reference uses `IsMutexOpen(type)` which calls `GetMutex(...)` and returns `true` if it can open it. 

This is used both for:
- launch-time logic
- “external launch monitoring” logic

---

## 3) Primary mutex removal strategy (linked-process kill)

### KillMutex(AccountType type)
The reference tries to kill mutexes associated with already-known “linked” processes.

Mechanism:
- Iterate active linked processes: `foreach (LinkedProcess p in LinkedProcess.GetActive())` 
- For matching account types: `if ((type & p.Account.Type) != 0)` 
- Attempt: `p.HasMutex && p.KillMutex(true)`

This returns a boolean `killed` indicating whether any mutex kill succeeded.

**Important note:** The mutex “kill” here is not performed by name; it’s performed by interacting with tracked processes (`LinkedProcess`) and invoking their mutex-kill capability. 

---

## 4) Fallback mutex removal strategy (system-wide search)

If `KillMutex(type)` does not clear the mutex, the reference escalates to a broader approach:

- `Util.ProcessUtil.KillMutexWindow(a, false)` (non-admin attempt) 
- If still not successful, it logs an “as admin” attempt and calls:  
  `Util.ProcessUtil.KillMutexWindow(account.Settings.Type, true)`

This fallback is invoked when:
- a mutex is detected open, AND
- linked-process kill is insufficient, OR the mutex remains open after kill attempts 

The monitoring loop explicitly logs when:
- an open mutex is detected, and
- it cannot be closed 

---

## 5) Relationship to `-sharedArchive` (shared vs single launch mode)

The reference defines two launch modes with an explicit difference:

- `LaunchMode.Launch`: “launched with all arguments” 
- `LaunchMode.LaunchSingle`: “launched without -sharedArchive” 

This establishes that, in this design:
- **Shared mode** includes `-sharedArchive`
- **Single mode** excludes `-sharedArchive`

No other semantics are assumed here; the file only documents the difference in terms of arguments.

---

## 6) External launch monitoring (optional, but relevant)

The reference includes an “external launch monitor” that periodically checks for an open mutex and attempts to close it.

Key parts:
- the monitor loops every 3 seconds (`await Task.Delay(3000)`) 
- it checks `IsMutexOpen(t)` and logs “Open mutex … detected” 
- it then attempts to close via `KillMutex(t)` and `Util.ProcessUtil.KillMutexWindow(...)` fallback 

This is not required for core multiclient launching, but it documents how the reference handles *other* launchers starting GW2 outside its control. 

---

## 7) Minimal implementation checklist (as shown by this source)

If you were implementing GW2 multiclient **using the same strategy** shown here, you would minimally need:

1) A known mutex name:
   - `"AN-Mutex-Window-Guild Wars 2"` 

2) A way to check if it exists:
   - `Mutex.TryOpenExisting(...)` 

3) A way to remove/neutralize the mutex:
   - process-linked removal (`LinkedProcess` / `KillMutex`) 
   - fallback system-wide removal (`Util.ProcessUtil.KillMutexWindow(..., adminFlag)`) 

4) Argument control:
   - support “with `-sharedArchive`” vs “without `-sharedArchive`” based on a launch mode flag

---

## 8) What this method is NOT

Based on this file, GW2 multiclient is **not implemented** by:
- patching `Gw2-64.exe` on disk
- modifying `Gw2.dat` on disk

The mechanism shown is:
- mutex detection/removal, and
- argument-mode selection (`-sharedArchive` vs not). 

