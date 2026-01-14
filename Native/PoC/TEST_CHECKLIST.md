# GW2 AppData Redirection PoC - Test Checklist

Use this checklist to track your testing progress and document results.

---

## Setup Phase

- [ ] **Downloaded MinHook library**
  - Downloaded from: https://github.com/TsudaKageyu/minhook/releases
  - Version: _______________
  - Placed at: `Gw2FolderHook\MinHook\lib\libMinHook.x64.lib`
  - Verified file exists: YES / NO

- [ ] **Built the PoC successfully**
  - Build command used: `Build.bat` / Other: _______________
  - Build output location: `Build\`
  - Files created:
    - [ ] Gw2AppDataRedirectPoC.exe
    - [ ] Gw2FolderHook.dll

---

## Initial Test (Single Profile)

**Test Date:** _______________  
**GW2 Version:** _______________  
**GW2 Path:** _______________  
**Profile Path:** _______________

- [ ] **Launch test passed**
  - Console showed "? SUCCESS!": YES / NO
  - GW2 launched normally: YES / NO
  - No crashes: YES / NO

- [ ] **Injection verification**
  - Hook log exists at `C:\Temp\Gw2FolderHook.log`: YES / NO
  - Log shows initialization: YES / NO
  - Log shows SHGetKnownFolderPath calls: YES / NO
  - Log shows redirected paths: YES / NO

- [ ] **File redirection verification**
  - Changed a setting in GW2: Graphics preset / Other: _______________
  - Files created in profile folder: YES / NO
    - [ ] `Roaming\Guild Wars 2\Local.dat`
    - [ ] `Roaming\Guild Wars 2\GFXSettings*.xml`
    - [ ] `Local\ArenaNet\Guild Wars 2\Settings.json`
  - Real AppData untouched: YES / NO
  - Re-launched with same profile: YES / NO
  - Settings persisted correctly: YES / NO

**Result:** ? SUCCESS / ? FAILED / ?? PARTIAL

**Notes:**
```
(Add any observations, errors, or issues here)






```

---

## Multiple Profiles Test

**Test Date:** _______________

### Profile 1
- Path: _______________
- Graphics Preset: _______________

### Profile 2
- Path: _______________
- Graphics Preset: _______________

- [ ] **Both instances launched successfully**
  - Profile 1 running: YES / NO
  - Profile 2 running: YES / NO
  - No interference between instances: YES / NO

- [ ] **Settings isolation verified**
  - Profile 1 settings file checked: YES / NO
  - Profile 2 settings file checked: YES / NO
  - Settings are different: YES / NO
  - Re-launch preserves correct settings per profile: YES / NO

**Result:** ? SUCCESS / ? FAILED / ?? PARTIAL

**Notes:**
```
(Add any observations about multi-instance behavior)






```

---

## Advanced Testing (Optional)

### Account Switching
- [ ] Logged into different accounts in same profile
- [ ] Account data persisted correctly: YES / NO
- [ ] Notes: _______________

### Game Updates
- [ ] Tested with game update/patch: YES / NO
- [ ] Update succeeded: YES / NO
- [ ] Settings preserved after update: YES / NO
- [ ] Notes: _______________

### Long-term Stability
- [ ] Run duration: _____ hours
- [ ] Number of launches: _____
- [ ] Any crashes or issues: YES / NO
- [ ] Notes: _______________

### Anti-cheat Detection
- [ ] Played for extended period: YES / NO
- [ ] Any warnings or bans: YES / NO
- [ ] Notes: _______________

---

## Performance Impact

- [ ] Measured injection time: _____ seconds
- [ ] GW2 launch time (normal): _____ seconds
- [ ] GW2 launch time (with hook): _____ seconds
- [ ] Noticeable performance impact: YES / NO
- [ ] Frame rate impact: YES / NO / N/A

---

## Troubleshooting Log

### Issues Encountered

1. **Issue:** _______________
   - **Solution:** _______________
   - **Resolved:** YES / NO

2. **Issue:** _______________
   - **Solution:** _______________
   - **Resolved:** YES / NO

3. **Issue:** _______________
   - **Solution:** _______________
   - **Resolved:** YES / NO

---

## Final Assessment

### Overall Result

- [ ] ? **Complete Success** - Ready for integration
- [ ] ?? **Partial Success** - Works but needs refinement
- [ ] ? **Failed** - Alternative approach needed

### Success Criteria Met

- [ ] Injection works reliably
- [ ] Files redirected correctly
- [ ] Multiple profiles isolated
- [ ] No UAC/admin required
- [ ] No crashes or instability
- [ ] Performance acceptable
- [ ] Anti-cheat compatible

### Recommendation

- [ ] **Proceed with integration** into main launcher
- [ ] **Needs more testing** before integration
- [ ] **Consider alternative approach**
- [ ] **Abandon this approach**

---

## Integration Readiness

If proceeding with integration, check off these items:

- [ ] Code review completed
- [ ] Edge cases identified and documented
- [ ] UI mockups for profile isolation settings
- [ ] Migration plan for existing profiles
- [ ] User documentation drafted
- [ ] Testing plan for integrated version
- [ ] Rollback plan if issues arise
- [ ] License compliance verified (MinHook BSD license)

---

## Next Actions

**Immediate:**
1. _______________
2. _______________
3. _______________

**Short-term (this week):**
1. _______________
2. _______________
3. _______________

**Long-term (if integrating):**
1. _______________
2. _______________
3. _______________

---

## Sign-off

**Tested by:** _______________  
**Date:** _______________  
**Verdict:** _______________

**Additional Comments:**
```











```

---

**Remember to commit this checklist with your findings for future reference!**
