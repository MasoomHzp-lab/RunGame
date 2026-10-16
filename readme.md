# RunGame

A 3D endless-runner style game built in **Unity** (C#). The player's running is driven by an external REST API, so a client app can start runs, send step commands, and read game state back.

## Project Structure

- `Assets/Scripts/`: core gameplay and API code
  - `ApiModels.cs`: serializable models for API responses (`ApiResponse`, `TherapySnapshot` with state, score, progress, speed, active side, running flag, and queued command count)
  - `RunCommand.cs`: a run instruction (duration, power, threshold, target side, step interval). Duration is converted from ms to seconds and inputs are clamped to safe minimums.
  - `StepCommand.cs`, `StepResult.cs`: step-level command and result types
  - `MainThreadDispatcher.cs`: thread-safe queue that runs actions from background threads (e.g. HTTP callbacks) on Unity's main thread
  - `GroundLooper.cs`: loops ground tiles to create the endless track
  - `SimpleFollowCamera.cs`: camera that follows the player
  - `Movement/`, `Score/`, `UI/`, `gameplay bonus/`, `birds/`, `core/`, `enums/`: gameplay modules
  - `Test/`: test scripts
- `Assets/Animation`, `Assets/Material`, `Assets/Texture`, `Assets/Sprites`: art and animation assets (character FBX, walking animation, denim textures)
- `Assets/Scenes`, `Assets/prefabs`, `Assets/UI`, `Assets/Fonts`, `Assets/Audio`: scenes, prefabs, UI, fonts, music
- `Assets/InputSystem_Actions.inputactions`: Unity Input System action map
- `ProjectSettings/`, `Packages/`: Unity project and package configuration
- `RunGame.slnx`: Visual Studio solution file

## Key Technical Features

- **Remote control via REST API**: the game exposes run state (score, progress, speed, isRunning, queued commands) and accepts commands from a client.
- **Coin goal**: a target coin count triggers a congratulation when reached; coin count and a progress bar are displayed in the UI.
- **Movement control from the game**: minimum power, the speed value used for movement, and the step cadence are read on the game side rather than sent by the client.
- **Music and run control from the client**: background music and run execution can be triggered remotely.
- **Thread safety**: API callbacks arrive off the main thread, so they are queued through `MainThreadDispatcher` before touching Unity objects.
- **Animation**: walking and step animation timing is tuned to consecutive steps.

## Requirements

- Unity (version matching `ProjectSettings/ProjectVersion.txt`)
- Git LFS (character FBX files are tracked with LFS, see `.gitattributes`)

## Getting Started

1. Install Git LFS and run `git lfs pull` after cloning.
2. Open the project folder in Unity Hub.
3. Open a scene from `Assets/Scenes` and press Play.

## Notes

- `RunGameLV.rar` (~52 MB) is a large archive committed to the repo. Consider moving it to Git LFS or a release asset.
- This README was written from the source structure and key scripts. Scene-level and gameplay details may need updating as the project changes.
