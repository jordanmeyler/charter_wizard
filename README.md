# Charter Wizard

A first Unity game: a tiny 3D courtyard where you play as a wizard and collect glowing charter runes.

This repository is a complete Unity 6 project. Open it in the Unity Editor, press Play, and the game builds itself.

## What you need

1. [Unity Hub](https://unity.com/download)
2. **Unity 6.3 LTS** (`6000.3.22f1` or any nearby 6.3 release)
3. A free Unity Personal license when Hub asks for one

The editor runs on your computer. This repo is the project files and C# gameplay code.

## Open and play

1. Clone or download this repo.
2. In Unity Hub, choose **Open** and select this folder (the one that contains `Assets`, `Packages`, and `ProjectSettings`).
3. If Hub offers to download Unity 6.3, let it.
4. When the editor finishes importing, open `Assets/Scenes/Main.unity` if it is not already open.
5. Press the Play button.

Controls:

- **WASD** or arrow keys to run
- **Space** to hop
- Walk into the cyan runes to collect them
- **R** after you win to play again

## Where the game lives

| File | What it does |
| --- | --- |
| `Assets/Scripts/Bootstrap.cs` | Starts the game automatically when you press Play |
| `Assets/Scripts/GameDirector.cs` | Builds the courtyard, wizard, and runes; tracks the score |
| `Assets/Scripts/WizardController.cs` | Movement and jumping |
| `Assets/Scripts/RunePickup.cs` | Spinning collectibles |
| `Assets/Scenes/Main.unity` | Empty 3D scene (camera + sunlight). The level is created in code |

The courtyard uses Unity primitives (cubes, capsules, spheres) so there are no art packs to import.

## Easy first tweaks

Open `GameDirector.cs` and change:

- `RuneCount` — how many runes you need
- `ArenaRadius` — how spread out they are

Open `WizardController.cs` and change:

- `moveSpeed`
- `jumpSpeed`

Then press Play again.

## Next ideas

- Add a timer and try to beat your best time
- Spawn a second kind of pickup
- Swap the capsule wizard for a model from the Unity Asset Store
- Add sound when a rune is collected

If you want help with any of those, say which one you want next.
