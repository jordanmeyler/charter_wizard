# Rune Magic

A 2D top-down puzzle-RPG. You read the runic field, compose a spell, and turn a lock. Combat is not a damage race. Terrain is made of the same materials as spells.

The living design reference is [`DESIGN.md`](DESIGN.md) (v0.10). The eleven basic runes and fifty story-chains are in [`SPELLS.md`](SPELLS.md). Joins become their own runes (Fire · Air → Spark). Life marks a living recipe. Death is reserved for Free / grave-work. This repository is a Unity 6.3 project with a four-room sanctum slice of that system.

## What is implemented

Four rooms, east to west of the spawn. Tiles are substances (ash, timber, hearthstone, void, spark-vein…) and each substance speaks runes into a **living tapestry** that drifts over the room. Enemies and terrain are both locks. You read the glyphs, then weave a key.

| Room | Lock | Intended keys | What it teaches |
| --- | --- | --- | --- |
| **Ash Court** | Ash Mite `{Fire · Salt}` | Almost any formed offensive spell | The most basic lock. Many keys. |
| **Wick Chapel** | Cold torch `{Plant · Dry wick}` | Fire × Mercury · Shot, Fire × Salt · Pillar, Fire × Sulphur · Spread — or Snuff / Smother | Terrain is a lock. Fire wants a form, then a place. |
| **The Drop** | Pit / missing Earth | Earth × Mercury · Shot, Earth × Salt · Pillar or Remote, Earth × Life · Pillar | Traversal. You aim the earth; it does not auto-bridge. |
| **Storm Cell** | Storm rod `{Spark · waiting}` | Spark × Mercury · Shot (Fire + Air first — Spark is its own rune), or Live-floor / Jolt / Brilliant-arc | A join becomes a rune, then the chain continues. |

Walk the orange Free charm in Ash Court if you want Fireball written for you. You can ignore it and compose from first principles.

| Design rule | In this slice |
| --- | --- |
| Casting is perception, not position | Runes live on their own layer. They drift and weave from what the room is made of; they are not stamped on a tile. Space stills the tapestry into a Charter wall. Click a glyph to draw it. |
| The world is the same materials | Ash, timber, hearth, ember, damp stone, spark-vein, wind-scoured stone, moss, iron, salt crust, and void each speak a short rune signature. The HUD shows the tapestry reading, not just the floor name. |
| Enemy = lock, spell = key | The right spell unmakes the encounter instantly. No HP. |
| Terrain = lock | Torch, pit, and rod accept keys the same way the mite does. |
| Chains, not pairs | The fifty catalog spells resolve in play. Fireball is Fire · Air · Salt · Mercury. The Grimoire lists them all; click a name to string it. |
| Joins are runes | Fire · Air → Spark. Spark · Air → Lightning. Short tutorial strings still work as a fallback. |
| Free is never required | Every lock has a Charter key. Free fills a blank, leans on attunement, and cannot be stored. |

## Open and play

1. Install [Unity Hub](https://unity.com/download) and **Unity 6.3 LTS** (`6000.3.22f1` or a nearby 6.3).
2. Open this folder in Hub (`Assets`, `Packages`, `ProjectSettings`).
3. Open `Assets/Scenes/Main.unity` and press Play. Leave the scene as the camera and light — the adept, rooms, and locks spawn at runtime. Do not place a character in the scene.

### Controls

You only move and cast. The adept is the hooded figure with a violet glow. A gold ring marks the nearest lock; it turns cyan while you aim.

- **WASD** / arrows move
- **Space** stills the tapestry into the Charter — a wall of runes over the world. Space again closes it.
- **Click a drifting glyph** to draw that rune from the weave (opens the Charter if it is folded).
- In the Charter: **click wall runes** or world glyphs to string them (up to 8). The wall is the eleven basic runes plus what the room is holding. Two runes birth a join or wait; a finished spell is a sentence. Then **Charter Cast**, **Store**, or **Free Cast**.
- **Charter Cast** (F / Enter): the recipe must already be written. Wrong strings fizzle.
- **Free Cast** (X): fills up to one missing rune (the budget can rise later). Several matches → attunement-weighted pick. Free cannot be stored.
- **Store** (R): holds one Charter sentence. Store is the benefit of using Charter.
- After a cast: pick **Shot / Pillar / Spread / Remote** (only the forms that string can take), then **click the world**. Esc cancels and keeps the string.
- The **bottom bar** shows the stored Charter spell. Click the slot or press **F** / **Enter** in the world to aim it.
- **Grimoire** on the bottom bar (or **Esc** / **G**) lists every written spell and your Free attunement.
- **Backspace** / **C** unmake the last rune

Casts are visible: Shot flies, Pillar rises, Spread wells from the feet, Remote forms at the click. An unwritten Charter string fizzles. Free fills a blank; used types grow. The right key unmakes the lock and opens the door east.

Walk into a pit and you return to the last safe floor.

## Where to grow the trees

| File | What to add |
| --- | --- |
| `Assets/Scripts/World/SanctumLayout.cs` | New rooms and tile layouts |
| `Assets/Scripts/World/TileSubstance.cs` | New floor substances and the runes they speak |
| `Assets/Scripts/Field/RuneTapestry.cs` | How the living rune layer weaves |
| `Assets/Scripts/Field/RuneStringSource.cs` | Ordered world-sentences in the field |
| `Assets/Scripts/World/TileTypes.cs` | New tile kinds / substance names |
| `Assets/Scripts/Magic/RuneCatalog.cs` | New rune names, families, meanings |
| `Assets/Scripts/Magic/MaterialTree.cs` | Second/third-tier blends |
| `Assets/Scripts/Magic/SpellGrammar.cs` | New Material × Aspect × Formation recipes (keep them sparse) |
| `Assets/Scripts/Magic/SpellShape.cs` | Which formations a material × aspect may even attempt |
| `SPELLS.md` | Primary runes, family mixing rules, full spell list |
| `DESIGN.md` | The source of truth |

## Not in this slice (on purpose)

- Full material tree, ternary nodes, and `material-codex.html`
- Cascading environmental reactions (conduction, spreading burn, ice bridges over water)
- Attunement decay / off-focus wither, a Free-store item, Primordial access, soul-work, ensouled casters
- Wards, mediums, and the Primordial-gate item
- Overworld, dialogue, and Magnum Opus world-tint

Those stay open threads in `DESIGN.md` until you ratify them.
