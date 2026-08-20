# Rune Magic

A 2D top-down puzzle-RPG. You read the runic field, compose a spell, and turn a lock. Combat is not a damage race.

The living design reference is [`DESIGN.md`](DESIGN.md) (v0.3). This repository is a Unity 6.3 project with a playable sanctum slice of that system.

## What is implemented

The first room is a closed courtyard. Runes orbit you. Two soulless locks wait. A discarded Free charm teaches by revealing its recipe.

| Design rule | In this slice |
| --- | --- |
| Casting is perception, not position | Runes stream around the player. You compose from the field. |
| The field is constant | Novice and master see the same seven runes. |
| No XP / levels | The grimoire only stores recipes you have borrowed or composed. |
| Enemy = lock, spell = key | The right spell unmakes the encounter instantly. |
| Material × Aspect | Fire × Mercury is not Fire × Salt. |
| Stable / violent blends | Two materials join before the aspect is applied. |
| Charter vs Free | Tab switches stance per cast. Free can leave blanks. |
| Free is never required | Both locks have Charter keys. Free is a risky shortcut and a teacher. |
| Free items reveal runes | Walk the orange charm to learn Fireball and how to read the moth. |

Primordial runes, Aether, souls, and items-as-gates are catalogued in data and left closed, matching the hard gate in the design.

## Open and play

1. Install [Unity Hub](https://unity.com/download) and **Unity 6.3 LTS** (`6000.3.22f1` or a nearby 6.3).
2. Open this folder in Hub (`Assets`, `Packages`, `ProjectSettings`).
3. Open `Assets/Scenes/Main.unity` and press Play.

### Controls

- **WASD** move
- **Click** a circling rune, or press **1–7**
- **Tab** or **Q** Bound (Charter) / Unbound (Free)
- **F** or **Enter** cast
- **C** clear slots
- **G** grimoire

### The two locks

1. **Cinder Moth** `{Fire · Mercury}` — quenched by **Water × Mercury** (Water-jet) or **Water × Salt** (Ice-wall).
2. **Clay Sentinel** `{Earth · Salt}` — scattered by **Air × Mercury** (Gale), or blend **Air + Earth → Dust** then **Mercury** (Scatter-dust).

Walk the charm first if you want the moth's formula interpreted for you. You can also ignore it and compose from first principles.

## Where to grow the trees

You said the deep tree work comes next. These are the files to extend:

| File | What to add |
| --- | --- |
| `Assets/Scripts/Magic/RuneCatalog.cs` | New rune names, families, meanings |
| `Assets/Scripts/Magic/MaterialTree.cs` | Second/third-tier blends |
| `Assets/Scripts/Magic/SpellGrammar.cs` | New Material × Aspect recipes |
| `Assets/Scripts/Magic/CastResolver.cs` | Charter / Free / later Primordial rules |
| `Assets/Scripts/Magic/Grimoire.cs` | Knowledge, not stats |
| `Assets/Scripts/Field/RuneField.cs` | What currently streams in the matrix |
| `DESIGN.md` | The source of truth |

## Not in this slice (on purpose)

- Full material tree and ternary nodes such as Plant
- Primordial access, soul-work, ensouled casters
- Wards, mediums, and the Primordial-gate item
- Overworld, dialogue, and Magnum Opus world-tint
- Field-economy scarcity (Aether is absent here)

Those stay open threads in `DESIGN.md` until you ratify them.
