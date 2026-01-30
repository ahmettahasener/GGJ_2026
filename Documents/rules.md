# Project Rules & Guidelines

## 1. General Principles
- **Game Engine:** Unity 2022.3 LTS (or later).
- **Language:** C#.
- **Design Pattern:** Composition over Inheritance. Use ScriptableObjects for data (Masks, GameEvents).
- **Comments:** Code comments should be in English. Explanations in chat can be Turkish.

## 2. Coding Standards
- **Variables:** `camelCase` (e.g., `currentElectricity`).
- **Methods:** `PascalCase` (e.g., `StartNewDay`).
- **Classes:** `PascalCase` (e.g., `GameManager`).
- **Serialized Fields:** Always use `[SerializeField] private` instead of public variables for Inspector references.
- **Null Checks:** Always check for null before accessing components (Use `TryGetComponent` where applicable).

## 3. Workflow (Strict)
- **Analyze First:** Before writing code, analyze the impact on existing systems.
- **Ask for Confirmation:** If a task involves creating a new Manager or a complex algorithm (like the Fuse logic), explain the logic and ask "Shall I proceed?"
- **Modular Scripts:** Do NOT write monolithic scripts. 
  - `PlayerController` handles movement.
  - `InteractionSystem` handles Raycasting.
  - `ResourceManager` handles stats.
  - `MachineBase` handles generic machine logic.

## 4. Specific Mechanics Rules
- **Mask System:** Masks MUST be implemented using `ScriptableObject` so we can easily create new ones in the Editor.
- **Mini-Game:** The Radio mini-game logic should be separate from the Radio interaction logic.
- **State Machine:** Use a simple Enum based State Machine for the Game Loop (`DayPhase`, `NightPhase`, `SleepPhase`).