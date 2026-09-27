# Release Audit — 2026-09-27

## Passed

- Unity scripts compile and the Game scene reports a valid 14-room run.
- Fallback JSON parses successfully.
- Build Settings contain Bootstrap, MainMenu, and Game in the correct order.
- New Input System is enabled.
- No Moth API key is stored in the project source or settings.
- Windows x64 and WebGL modules are targeted by the submission builder.
- Product metadata updated to version 1.0.0 and publisher Tuan Anh Bui.
- In-game controls, accessibility notes, credits, engine roles, and presentation script are documented.
- 2026 “quantum BRAIDing” theme is addressed explicitly.
- Unity release validation passed: 3 enabled scenes and a valid 14-room fallback run.
- All 4 Edit Mode tests passed on 2026-09-27 (0 failed, 0 skipped).
- WebGL and Windows x64 release builds completed successfully.
- Windows ZIP integrity test passed with no compressed-data errors.

## Release artifacts

- `SubmissionBuilds/TheUnobservedRooms-WebGL.zip` — upload as an HTML5 game on itch.io (17 MB).
- `SubmissionBuilds/TheUnobservedRooms-Windows.zip` — optional downloadable Windows build (35 MB).
- WebGL SHA-256: `d480b972e269f671e06d28264c3549fe1c148f4b3d6cf7be18f19ef63388135c`
- Windows SHA-256: `e1ea53284025d7b237cc065c1672844132f674f5f607b6c4d14bd32712686b55`

## Must be completed by the submitter

1. Confirm the credit-name spelling.
2. Run completed Moth engine jobs and replace the `demo-*` provenance if verified engine use is being claimed.
3. Test the WebGL ZIP after upload and the Windows ZIP after extraction on a Windows machine.
4. Capture at least three screenshots and a complete gameplay video.
5. Upload the game, video, screenshots, and credits to itch.io.

## Recommended final smoke test

- Start from MainMenu.
- Open a threshold and confirm coherence decreases.
- Collect a shard and confirm coherence increases.
- Calibrate both A and B; verify one control alone cannot unlock the exit.
- Match the final target and confirm the exit turns cyan.
- Confirm the Surveyor slows while centered in view.
- Win once, lose once, and retry with R.
- Verify the Quantum Run Report appears.
