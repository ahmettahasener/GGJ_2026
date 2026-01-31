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

## [X] Milestone 3: Machine Mechanics
- [x] **RadioMachine:** Implement Frequency mechanism and Mini-game stub.
- [x] **RadarMachine:** Implement Distance pushback and Fuse risk.
- [x] **MedicineMachine:** Implement Sanity restoration.
- [x] **DashboardUI:** Connect UI to ResourceManager events.
- [x] **FuseBox:** Implement random break chance and repair mechanic.

## [X] Milestone 4: Mask System & Game Loop Integration
- [x] **MaskManager:** Implement random selection logic.
- [x] **Mask Effects:** Apply passive buffs/debuffs based on selection.
- [x] **Night Cycle:** Implement turn transitions and end-of-night calculations (Sanity drop, Monster move).
