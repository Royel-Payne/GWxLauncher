# GW1 Multiclient Support — Minimal Implementation Guide

This document describes the **minimal, correct method** used to enable **multiple Guild Wars 1 clients** on Windows.

It is written for developers implementing their **own launcher**, not tied to any specific codebase.

No files are modified on disk.  
All changes are performed **in-memory at process startup**.

---

## Problem Summary

Guild Wars 1 enforces a **single-instance restriction** inside the game process itself.

Attempting to launch a second client normally results in:
- the second process exiting immediately, or
- a silent failure after startup

To support multiclient reliably, the restriction must be bypassed **before the game begins executing**.

---

## High-Level Strategy

1. Launch Guild Wars 1 **in a suspended state**
2. Patch a small function in the game’s memory that enforces single-instance behavior
3. Resume the game’s main thread
4. Continue normal startup (and optional mod injection)

This approach:
- requires no file copying
- requires no admin privileges
- does not persist beyond process lifetime
- is compatible with additional DLL injection

---

## Required Capabilities

Your program must be able to:

- Create a Windows process in a suspended state
- Read memory from a remote process
- Write memory to a remote process
- Resume a suspended thread
- Locate the base address of the target process’s main module
- Perform a byte-pattern scan over a memory buffer

---

## Step-by-Step Method

### 1. Create the GW1 Process Suspended

Use `CreateProcessW` with the `CREATE_SUSPENDED` flag.

You must obtain:
- process handle
- primary thread handle
- process ID

If process creation fails, multiclient support cannot proceed.

---

### 2. Locate the Main Module Base Address

The patch offset is **relative to the main executable image**.

Common approaches:

- **PEB-based method**
  - Call `NtQueryInformationProcess`
  - Read the remote PEB via `ReadProcessMemory`
  - Extract `ImageBaseAddress`

- **Module enumeration**
  - Use `EnumProcessModulesEx`
  - The first module is typically the main executable

Either method is valid as long as the base address is correct.

---

### 3. Read a Fixed Memory Region

Read a fixed block of memory starting at the module base.

Typical values used by existing multiclient tools:
- Read size: `0x48D000` bytes
- Read address: `moduleBase`

Failure to read memory should be treated as a hard failure or a fallback case.

---

### 4. Signature Scan (Pattern Match)

Scan the memory buffer for a known byte signature that identifies the
single-instance enforcement routine.

byte[] sigPatch =
{
    0x56, 0x57, 0x68, 0x00, 0x01, 0x00, 0x00, 0x89, 0x85, 0xF4, 0xFE, 0xFF, 0xFF,
    0xC7, 0x00, 0x00, 0x00, 0x00, 0x00
};

Important notes:
- Do **not** patch using a hardcoded offset
- The signature must be found dynamically
- The offset is derived from the signature location

If the signature is not found:
- the client version may be unsupported
- the launcher should not guess or patch blindly

---

### 5. Apply the Patch

Once the signature index `idx` is found:

Compute the patch address:

patchAddress = moduleBase + idx - 0x1A

Write **exactly 4 bytes** to that address:

31 C0 90 C3

```
Payload to write:
byte[] payload = { 0x31, 0xC0, 0x90, 0xC3 };
```

Which corresponds to:

- `xor eax, eax`
- `nop`
- `ret`

This forces the enforcement routine to return success immediately.

After writing:
- confirm `WriteProcessMemory` succeeded
- optionally read the bytes back to verify correctness

---

### 6. Resume the Process

Call `ResumeThread` on the primary thread.

At this point:
- the game continues startup normally
- multiple clients can coexist

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

- If multiclient is enabled:
  - Attempt patch
  - If patch succeeds → continue
  - If patch fails → inform the user clearly
    - optionally abort launch
    - or continue without multiclient (single-client behavior)

Do **not** silently fail.

---

## Constraints & Correctness Notes

- **Bitness must match**
  - 32-bit GW1 process requires correct pointer handling
- **Patch must occur before execution**
  - Patching after resume is unreliable
- **Never assume offsets**
  - Signature scanning is required for safety
- **No persistent state**
  - The patch exists only in memory

---

## Summary

Multiclient support for GW1 is achieved by:

- launching suspended
- performing a minimal in-memory patch
- resuming execution

The method is small, deterministic, and well-contained.

This behavior should be:
- explicit
- user-controlled
- documented
- isolated from unrelated launch logic

---

## Non-Goals

This method does **not**:
- bypass anti-cheat systems
- modify files on disk
- install background services
- persist across reboots
- alter game assets

It is purely a startup-time process modification.

