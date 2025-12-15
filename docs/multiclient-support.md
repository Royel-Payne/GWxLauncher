# GW1 Multiclient Support — Minimal Implementation Guide

This document describes the **minimal, correct method** used to enable **multiple Guild Wars 1 clients** on Windows.

It is written for developers implementing their **own launcher**, independent of any specific codebase.

No files are modified on disk.
All changes are performed **in-memory at process startup**.

---

## Problem Summary

Guild Wars 1 enforces a **single-instance restriction** inside the game process itself.

Attempting to launch a second client normally results in:

* the second process exiting immediately, or
* a silent failure shortly after startup

To support multiclient reliably, the restriction must be bypassed **before the game begins executing**.

---

## High-Level Strategy

1. Launch Guild Wars 1 **in a suspended state**
2. Patch a small function in the game’s memory that enforces single-instance behavior
3. Resume the game’s main thread
4. Continue normal startup (and optional mod injection)

This approach:

* requires no file copying
* requires no admin privileges
* does not persist beyond process lifetime
* is compatible with additional DLL injection

---

## Required Capabilities

Your program must be able to:

* Create a Windows process in a suspended state
* Read memory from a remote process
* Write memory to a remote process
* Resume a suspended thread
* Locate the base address of the target process’s main module
* Perform a byte-pattern scan over a memory buffer

---

## Step-by-Step Method

### 1. Create the GW1 Process Suspended

Use `CreateProcessW` with the `CREATE_SUSPENDED` flag.

You must obtain:

* process handle
* primary thread handle
* process ID

If process creation fails, multiclient support cannot proceed.

---

### 2. Locate the Main Module Base Address

The patch offset is **relative to the main executable image**.

Common approaches:

* **PEB-based method**

  * Call `NtQueryInformationProcess`
  * Read the remote PEB via `ReadProcessMemory`
  * Extract `ImageBaseAddress`

* **Module enumeration**

  * Use `EnumProcessModulesEx`
  * The first module is typically the main executable

Either method is valid as long as the base address is correct.

---

### 3. Read a Fixed Memory Region

Read a fixed block of memory starting at the module base.

Typical values used by existing multiclient tools:

* Read size: `0x48D000` bytes
* Read address: `moduleBase`

Failure to read memory should be treated as a hard failure or a fallback case.

---

### 4. Signature Scan (Pattern Match)

Scan the memory buffer for a known byte signature that identifies the
single-instance enforcement routine.

```
byte[] sigPatch =
{
    0x56, 0x57, 0x68, 0x00, 0x01, 0x00, 0x00, 0x89, 0x85, 0xF4, 0xFE, 0xFF, 0xFF,
    0xC7, 0x00, 0x00, 0x00, 0x00, 0x00
};
```

Important notes:

* Do **not** patch using a hardcoded offset
* The signature must be found dynamically
* The patch address is derived from the signature location

If the signature is not found:

* the client version may be unsupported
* the launcher should not guess or patch blindly

---

### 5. Apply the Patch

Once the signature index `idx` is found:

Compute the patch address:

```
patchAddress = moduleBase + idx - 0x1A
```

Write **exactly 4 bytes** to that address:

```
byte[] payload = { 0x31, 0xC0, 0x90, 0xC3 };
```

Which corresponds to:

* `xor eax, eax`
* `nop`
* `ret`

This forces the enforcement routine to return success immediately.

After writing:

* confirm `WriteProcessMemory` succeeded
* optionally read the bytes back to verify correctness

---

### 6. Resume the Process

Call `ResumeThread` on the primary thread.

At this point:

* the game continues startup normally
* multiple clients can coexist

---

## Interaction With DLL Injection

If your launcher also injects DLLs (Toolbox, gMod, etc.):

Recommended order:

1. Create process suspended
2. Apply multiclient patch
3. Inject any **early-load DLLs**
4. Resume the process
5. Perform any late-stage injections if required

The multiclient patch is **independent** of mod injection.

---

## Error Handling & UX Expectations

A clean launcher must define explicit behavior when patching fails.

Recommended minimum policy:

* If multiclient is enabled:

  * Attempt patch
  * If patch succeeds → continue
  * If patch fails → inform the user clearly

    * optionally abort launch
    * or continue without multiclient (single-client behavior)

Do **not** silently fail.

---

## Constraints & Correctness Notes

* **Bitness must match**

  * 32-bit GW1 process requires correct pointer handling
* **Patch must occur before execution**

  * Patching after resume is unreliable
* **Never assume offsets**

  * Signature scanning is required for safety
* **No persistent state**

  * The patch exists only in memory

---

## Summary

Multiclient support for GW1 is achieved by:

* launching suspended
* performing a minimal in-memory patch
* resuming execution

The method is small, deterministic, and well-contained.

This behavior should be:

* explicit
* user-controlled
* documented
* isolated from unrelated launch logic

---

## Non-Goals

This method does **not**:

* bypass anti-cheat systems
* modify files on disk
* install background services
* persist across reboots
* alter game assets

It is purely a startup-time process modification.

---

# GW2 Multiclient Support — Mutex-Based Reference Method

This document describes the **mutex-based multiclient mechanism** used by Guild Wars 2.

It records:

* how the single-instance mutex is identified
* how a launcher determines whether a launch is blocked
* how the mutex is removed (primary and fallback strategies)
* how `-shareArchive` is used to distinguish launch modes

This method is **distinct from GW1** and does **not** involve memory patching.

---

## Problem Summary

Guild Wars 2 enforces a **single-instance restriction** using a named Windows mutex.

If the mutex already exists:

* a new client launch is blocked
* the game may refuse to start or immediately exit

Multiclient support therefore requires **detecting and neutralizing the mutex** before or during launch.

---

## High-Level Strategy

1. Detect whether the GW2 mutex already exists
2. If present, attempt to remove or invalidate it
3. Launch GW2 with the desired argument mode
4. Optionally monitor for externally-created mutexes

This approach:

* does not modify executable or data files
* relies on standard Windows synchronization primitives
* can be performed without persistent system changes

---

## Mutex Identification

### GW2 Mutex Name

The single-instance mutex used by Guild Wars 2 is:

```
AN-Mutex-Window-Guild Wars 2
```

The presence of this mutex indicates an active or previously-launched GW2 client.

> Note: A similar mutex exists for GW1 (`AN-Mutex-Window-Guild Wars`), but GW1 multiclient is **not** implemented via mutex removal.

---

## Detecting a Blocking Mutex

The reference implementation performs a direct existence check:

* Attempt to open the mutex by name
* If `Mutex.TryOpenExisting(...)` succeeds, the mutex is considered open

This check is used for:

* launch-time decision making
* optional background monitoring

---

## Primary Mutex Removal Strategy (Linked Processes)

The first removal attempt targets **known launcher-managed processes**.

Mechanism:

* Enumerate tracked processes (`LinkedProcess.GetActive()`)
* Filter by matching account type
* For each matching process:

  * confirm it owns a mutex
  * attempt to remove that mutex via `KillMutex(true)`

If any linked process successfully releases its mutex, the launcher may proceed.

Important characteristics:

* The mutex is **not** removed by name
* The operation is performed through process-level interaction

---

## Fallback Removal Strategy (System-Wide Search)

If linked-process removal fails and the mutex remains open, the reference escalates:

1. Attempt a non-elevated system-wide mutex removal
2. If unsuccessful, retry with administrative privileges

Conceptually:

* search for windows/processes associated with the mutex
* attempt to force mutex closure

This fallback exists to handle:

* orphaned mutexes
* GW2 instances launched outside the launcher’s control

---

## Launch Modes and `-shareArchive`

The reference defines two explicit launch modes:

* **Shared mode**

  * includes the `-shareArchive` argument

* **Single mode**

  * omits the `-shareArchive` argument

No additional behavior is implied beyond argument selection.

The launcher must therefore:

* conditionally include `-shareArchive`
* treat this as an explicit, user-controlled choice

---

## Optional External Launch Monitoring

Some implementations include a background monitor that:

* periodically checks for an open GW2 mutex
* attempts removal if detected
* logs failures or escalation attempts

This is **not required** for basic multiclient launching, but documents how external launches can be handled defensively.

---

## Minimal Implementation Checklist

To reproduce this method, a launcher must provide:

1. A known mutex name

   * `AN-Mutex-Window-Guild Wars 2`

2. A way to detect mutex existence

   * `Mutex.TryOpenExisting(...)`

3. One or more mutex removal strategies

   * linked-process removal
   * system-wide fallback (optional, possibly elevated)

4. Argument control

   * include or exclude `-shareArchive` based on launch mode

---

## What This Method Is Not

Based on the reference behavior, GW2 multiclient is **not** achieved by:

* patching `Gw2-64.exe` on disk
* modifying `Gw2.dat`
* injecting code into the GW2 process

It is purely a **mutex-management and argument-selection** strategy.

---

## Summary

GW2 multiclient support is implemented by:

* detecting the GW2 mutex
* removing or invalidating it when necessary
* launching with explicit argument modes

This method should be:

* explicit
* conservative
* clearly surfaced in UX
* kept logically separate from GW1 multiclient logic

---

## GW2 Startup Timing & Mutex Re-Creation

### Why Timing Matters

Guild Wars 2 recreates its single-instance mutex during early startup, not atomically at process creation.

In practice:

- The launcher may clear the mutex successfully
- Launch GW2 with `-shareArchive`
- The mutex may not yet exist when the next launch attempt occurs

If a second launch is attempted too quickly:

- The launcher may not observe a mutex
- The second process may start
- GW2 later recreates the mutex
- One of the instances silently exits

This manifests most clearly during bulk launch scenarios.

---

### Correct Sequencing Model (Observed Behavior)

A reliable GW2 multiclient sequence is:

1. Launch GW2 instance N
2. GW2 performs internal initialization
3. GW2 creates the mutex
4. Launcher observes mutex existence
5. Launcher clears mutex
6. Launch GW2 instance N+1

Any implementation that skips step 4 (observation) is timing-sensitive and unreliable.

---

### Recommended Solution: State-Based Mutex Wait (Not Fixed Delays)

Do not rely on fixed sleeps (for example, `Thread.Sleep(800)`).

Empirical observation shows mutex creation timing varies based on:

- CPU load
- Disk I/O
- Background applications
- System performance

Instead, wait until the mutex is actually observed.

Conceptual approach:

```csharp
while (elapsed < timeout)
{
    if (MutexExists("AN-Mutex-Window-Guild Wars 2"))
        break;

    Sleep(brief_interval);
}
```

This approach:

- Adapts automatically to fast and slow systems
- Eliminates race conditions
- Uses a real synchronization signal
- Avoids guesswork

---

### Bulk Launch Considerations

When launching multiple GW2 profiles in succession:

- Each launch must wait for the previous instance’s mutex to reappear
- Only then can the mutex be safely cleared again
- This applies even if all instances are launcher-managed

Failure to do this can result in:

- Only the first instance launching
- Subsequent instances silently failing
- Inconsistent behavior across machines

---

### Diagnostic Value

Recording mutex timing data is strongly recommended.

Example launch detail:

```
Cleared GW2 mutex in PID 31860
(GW2 mutex observed after 750ms)
```

This provides:

- Proof of correct sequencing
- Visibility into system-dependent timing
- Valuable debugging context

---

### Clarification: Role of `-shareArchive`

The `-shareArchive` argument:

- Enables shared data usage
- Does not control mutex creation timing
- Does not eliminate the need for mutex management
- Must be combined with correct mutex handling

---

### Updated Minimal GW2 Multiclient Checklist

To implement GW2 multiclient robustly, a launcher must:

1. Detect the GW2 mutex
2. Clear the mutex when present
3. Launch GW2 with `-shareArchive` (if enabled)
4. Wait until the mutex is recreated
5. Repeat for subsequent instances
6. Abort only if mutex clearing fails after retries

---

### Why This Matters

Many existing launchers appear to work under light testing but fail under:

- Bulk launch
- Slower systems
- Background load

The state-based mutex wait transforms GW2 multiclient support from “usually works” into “deterministically correct”.

---

### Final Note

This behavior was discovered through real-world testing and timing variance, not static analysis alone.

Documenting it here prevents regressions and explains why the implementation is structured the way it is.
