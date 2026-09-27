# Milestone 01 Foundation

## Outcome

The project can load a bundled verified run, validate the contract, and assemble a visible graph of graybox room floors. A backend provider exists behind the same interface but remains disabled until a safe proxy URL is configured.

## Verification in Unity

1. Open the Test Runner and run Edit Mode tests.
2. Open `MainMenu.unity` and press Play.
3. Select `Start Offline Run`.
4. Confirm the Console reports `run_demo_001` with eight rooms.
5. Confirm eight floor nodes appear in the Game scene without overlapping grid cells.

## Known limitations

- The scene is a top-down graybox visualization, not yet first person.
- Edges are not yet rendered as corridors.
- Thresholds, room stability, entangled switches, exit evaluation, HUD, and the Surveyor begin in Milestone 02 and 03.
- The project compiles in Unity `6000.6.3f1`; all four Edit Mode tests pass.
