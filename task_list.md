# Project Task List

## [X] Milestone 1: Base Architecture & Managers
- [x] **GameManager:** Implement Singleton pattern, define Game States (MaskSelection, Gameplay, EndNight).
- [x] **ResourceManager:** Implement resource tracking (Electricity, Sanity, Distance, Frequency) and Events.
- [x] **SoundManager:** Set up basic structure for SFX.
- [x] **MaskData:** Create ScriptableObject definition for Masks.

## [X] Milestone 2: Interaction System
- [x] **IInteractable:** Define the interface (`OnInteract`, `OnExit`).
- [x] **PlayerInteract:** Implement Raycast system and 'E' key interaction.
- [x] **MachineBase:** Create abstract base class for machines (Electricity consumption).
- [x] **OverlayUI:** Implement Reticle and Interaction Prompts.
- [x] **PlayerInteract:** Refactored for optional Camera Focus (Masks/Doors don't lock camera).
- [x] **Auto-Exit:** Implemented auto-exit for Machines (Radio, Radar, Medicine, FuseBox) after task completion.
- [x] **Visuals:** Implemented dynamic, billboarded text descriptions for Masks on hover.
- [x] **Electricity:** Refactored consumption to "One-Time Cost" at interaction start. Fuse check is now immediate.
- [x] **Notification System:** Implemented Sliding InfoPopup for resource changes (Electricity, Sanity, Distance, Frequency).

## [X] Milestone 3: Machine Mechanics
- [x] **RadioMachine:** Refactored to 10s gravity-based minigame with World Canvas.
- [x] **RadarMachine:** Implement Distance pushback and Fuse risk.
- [x] **MedicineMachine:** Implement Sanity restoration.
- [x] **DashboardUI:** Connect UI to ResourceManager events.
- [x] **FuseBox:** Implement random break chance and repair mechanic.

## [X] Milestone 4: Mask System & Game Loop Integration
- [x] **MaskManager:** Implement random selection logic.
- [x] **Mask Effects:** Apply passive buffs/debuffs based on selection.
- [x] **Night Cycle:** Implement turn transitions and end-of-night calculations (Sanity drop, Monster move).
