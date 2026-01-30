# Technical Architecture

## Core Managers
1. **GameManager (Singleton):** Manages the Game Loop (States: MaskSelection, Gameplay, EndNight). Handles day/night transition logic.
2. **ResourceManager:** Holds state data (Electricity, Sanity, Distance, Frequency). Uses C# Actions/Events (`OnElectricityChanged`, `OnSanityChanged`) to update UI.
3. **SoundManager:** Handles SFX for machines and paranoid events.

## Interaction System
- **IInteractable (Interface):** All machines implement this.
  - `void OnInteract()`
  - `void OnExit()`
- **PlayerInteract:** Raycasts from camera center. If hits `IInteractable`, shows prompt.

## Machine Hierarchy
- **MachineBase (Abstract Class):** Inherits from MonoBehaviour, implements IInteractable.
  - Fields: `electricityCost`, `machineName`.
  - Virtual Methods: `UseMachine()`, `ConsumeElectricity()`.
- **Specific Machines:** `RadioMachine`, `RadarMachine`, `MedicineMachine` inherit from `MachineBase`.

## Mask System
- **MaskData (ScriptableObject):**
  - `maskName`, `description`, `icon`.
  - `MaskType` (Enum) or specific float modifiers.
- **MaskManager:** Handles the logic of selecting 3 random masks and applying the chosen one's effects via `GameManager`.

## UI System
- **DashboardUI:** Listens to `ResourceManager` events and updates the in-game computer screen texts/bars.
- **OverlayUI:** Reticle, interaction prompts ("Press E"), and Mask Selection Panel.