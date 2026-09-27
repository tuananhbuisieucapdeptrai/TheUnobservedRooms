# The Unobserved Rooms

A short first-person quantum-horror puzzle made in Unity for Global Quantum Game Jam 2026 and the Moth Quantum showcase.

The 2026 theme is **quantum BRAIDing**. The game interprets braiding as two spatially separated control histories whose operations must be interwoven into one valid final state while the player physically braids a route through uncertain rooms.

## Play

1. Open the project with Unity 6.6 (`6000.6.3f1`).
2. Open `Assets/_Game/Scenes/MainMenu.unity`.
3. Enter Play Mode and choose **Start Offline Run**.

Controls: WASD move, mouse look, Shift sprint, hold E interact, F flashlight, Esc release cursor, R retry after a result.

## Objective

Navigate a 14-room generated facility, spend coherence to observe uncertain thresholds, collect stabilizer shards, match entangled switches A and B to their target, manage the Surveyor by keeping it in view, and reach the exit aperture.

## Quantum engines

- `labyrinth-v1`: room graph and route topology.
- `graph-v1`: entangled switch relationship and target state.
- `comet-qrng-v1`: lighting, reward placement, and anomaly-event values.

The included offline run is a deterministic cached demonstration payload so the game remains playable without network access. The result screen displays the declared engine provenance, execution mode, job IDs, and payload hash. Before claiming verified live Moth execution, replace the `demo-*` identifiers in `valid_demo.json` with exports from completed platform jobs.

## Build

Use **Tools → The Unobserved Rooms → Build Submission Packages**. Unity creates WebGL and Windows builds under `SubmissionBuilds/` and packages the Windows build as a ZIP.

No API key is stored in the Unity client. Production Moth calls should be made by a backend service that keeps credentials server-side.

## Release status

- Unity version: 6000.6.3f1
- Game version: 1.0.0
- Publisher: Tuan Anh Bui
- Enabled scenes: Bootstrap, MainMenu, Game
- Platforms prepared: WebGL and Windows x64
