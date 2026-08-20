# Rune Magic

A 2D top-down puzzle-RPG. You read the runic field, compose a spell, and turn a lock. Combat is not a damage race. Terrain is made of the same materials as spells.

The living design reference is [`DESIGN.md`](DESIGN.md) (v0.6). This repository is a Unity 6.3 project with a four-room sanctum slice of that system.

## What is implemented

Four rooms, east to west of the spawn. Tiles carry an element. Enemies and terrain are both locks.

| Room | Lock | Intended keys | What it teaches |
| --- | --- | --- | --- |
| **Ash Court** | Ash Mite `{Fire · Salt}` | Almost any formed offensive spell | The most basic lock. Many keys. |
| **Wick Chapel** | Cold torch `{Plant · Dry wick}` | Fire × Mercury, Salt, or Sulphur | Terrain is a lock. Fire wants a form. |
| **The Drop** | Pit / missing Earth | Earth × Mercury (spirit) or Earth × Salt | Traversal. Ground + spirit throws a bridge. |
| **Storm Cell** | Storm rod `{Spark · waiting}` | Spark × Mercury, Salt, or Sulphur | Blend first: Fire + Air → Spark, then an aspect. |

Walk the orange Free charm in Ash Court if you want Fireball written for you. You can ignore it and compose from first principles.

| Design rule | In this slice |
| --- | --- |
| Casting is perception, not position | Space opens a Charter wall over the world. You compose from the field, not from a tile you stand on. |
| The world is the same materials | Floor, wall, wood, fire-stone, spark-stone, and pit tiles each carry an element. The HUD names the tile underfoot. |
| Enemy = lock, spell = key | The right spell unmakes the encounter instantly. No HP. |
| Terrain = lock | Torch, pit, and rod accept keys the same way the mite does. |
| Material × Aspect | Fire × Mercury is not Fire × Salt. Form matters. |
| Stable blends | Fire + Air → Spark before a lightning bolt will form. |
| Free is never required | Every lock has a Charter key. Free is a risky shortcut and a teacher. |

## Open and play

1. Install [Unity Hub](https://unity.com/download) and **Unity 6.3 LTS** (`6000.3.22f1` or a nearby 6.3).
2. Open this folder in Hub (`Assets`, `Packages`, `ProjectSettings`).
3. Open `Assets/Scenes/Main.unity` and press Play.

### Controls

You only move and cast.

- **WASD** move
- **Space** open the Charter — a wall of runes over the world. Space again closes it.
- In the Charter: **click runes** to string them (up to 8). **Cast** now, or **Store** one form to hold.
- **F** / **Enter** release the held spell (or Cast while the Charter is open)
- **R** Store while the Charter is open
- **Backspace** / **C** unmake the last rune
- **Tab** / **Q** Bound (Charter) / Unbound (Free) — only while the Charter is open
- **Esc** pause and show every written spell recipe (developer ledger)

Walk into a pit and you return to the last safe floor.

## Where to grow the trees

| File | What to add |
| --- | --- |
| `Assets/Scripts/World/SanctumLayout.cs` | New rooms and tile layouts |
| `Assets/Scripts/World/TileTypes.cs` | New tile kinds / element names |
| `Assets/Scripts/Magic/RuneCatalog.cs` | New rune names, families, meanings |
| `Assets/Scripts/Magic/MaterialTree.cs` | Second/third-tier blends |
| `Assets/Scripts/Magic/SpellGrammar.cs` | New Material × Aspect recipes |
| `DESIGN.md` | The source of truth |

## Not in this slice (on purpose)

- Full material tree, ternary nodes, and `material-codex.html`
- Cascading environmental reactions (conduction, spreading burn, ice bridges over water)
- Free attunement, Primordial access, soul-work, ensouled casters
- Wards, mediums, and the Primordial-gate item
- Overworld, dialogue, and Magnum Opus world-tint

Those stay open threads in `DESIGN.md` until you ratify them.
