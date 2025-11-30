# Club Fungal - AI Agent Instructions

## Architecture Overview

- **Service-Oriented Design**: Core logic in ScriptableObject services inheriting from `GURUService` (e.g., `SnapshotService`, `UnitControllerService`).
- **Data Flow**: Services reference each other (e.g., `SnapshotService` uses `UnitControllerService.Controllers`). Persistence via `LocalData` (JSON) and Unity assets.
- **Component Hierarchy**: Units/builds parented under `RoomController` (UnitParent/BuildParent). Repositioning triggers auto-save via `RepositionableEditor`.

## Key Patterns

- **Asset Loading**: Use `GURUStyler.LoadAsset<T>(typeName)` for ScriptableObjects (e.g., `GURUStyler.LoadAsset<SnapshotService>("SnapshotService")`). Assumes assets exist; no null checks.
- **Movement**: Call `controller.Teleport(position, parent)` for precise positioning (updates transform and parent).
- **Snapshots**: `SnapshotInstance` assets store unit positions. Load/save via `SnapshotService` methods.
- **Editor UI**: Extend `GURUEditor` with `DrawContent()`. Use `GURUStyler.DrawGuruSection()` for styled sections with service buttons.

## Workflows

- **Build**: Unity iOS build via Xcode (see `Builds/` folder). Use `process_symbols.sh` for symbols.
- **Debugging**: Editor scripts auto-reposition units. Check `Logs/` for Unity output.
- **Persistence**: JSON in `LocalData.JsonFile`. Assets for snapshots. Auto-save on quit/scene unload.

## Conventions

- **Naming**: Services end with "Service", editors with "Editor". Use `GURU` prefix for custom classes.
- **Initialization**: Services initialize in `OnInitialize()` or `OnSceneLoaded()`.
- **Error Handling**: Minimal; assume valid state. Use `Debug.Log()` for feedback.
- **Coding Style**: Write code without null checks; assume all dependencies are available.

## Examples

- Load service: `var service = GURUStyler.LoadAsset<SnapshotService>("SnapshotService"); service.SaveSnapshot();`
- Teleport unit: `controller.Teleport(newPos, room.UnitParent);`
- Asset selector: Use `AssetSelectorComponent<T>` for drag-drop with history.

Reference: `Assets/Snapshots/`, `Assets/Editor/`, `Assets/Units/Scripts/`
