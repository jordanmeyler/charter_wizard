# Rune Magic

A 2D top-down puzzle-RPG. You read the runic field, compose a spell, and turn a lock. Combat is not a damage race. Terrain is made of the same materials as spells.

The living design reference is [`DESIGN.md`](DESIGN.md) (v0.13). The eleven basic runes and fifty story-chains are in [`SPELLS.md`](SPELLS.md). World materials — tiles you can stamp, with full rune sentences — are in [`MATERIALS.md`](MATERIALS.md). Joins become their own runes (Fire · Air → Spark). Salt stands a body (walls, pillars). Sulphur is the wildcard. Life marks a living recipe. Death is reserved for Free / grave-work. This repository is a Unity 6.3 project with a four-room sanctum slice of that system.

## What is implemented

Four rooms, east to west of the spawn. Tiles are **materials** (ash, timber, hearthstone, void, spark-vein…) and each material keeps a full rune sentence. Those glyphs stay folded while you walk. **Space** opens the Charter: the eleven writeable runes, and a sideways-scrolling weave of everything in the room. Enemies and terrain are both locks. You read the glyphs there, then weave a key.

| Room | Lock | Intended keys | What it teaches |
| --- | --- | --- | --- |
| **Ash Court** | Ash Mite `{Fire · Salt}` | Almost any formed offensive spell | The most basic lock. Many keys. |
| **Wick Chapel** | Cold torch `{Plant · Dry wick}` | Flame-pillar (Fire · Salt · Earth), Melt (Fire · Mercury), Ignite (Fire · Sulphur · Salt), Snuff / Smother | Terrain is a lock. Salt stands fire as a pillar or a wick. Mercury sends hunger *into* the wick. |
| **The Drop** | Pit / missing Earth | Wall (Earth · Salt · Earth: click start and stop), any pillar, Hop (Air · Salt · Air, Self), Flight, Bridge | Traversal. Rest that stands fills a hollow or bars the floor. Breath given a body leaps or flies. |
| **Storm Cell** | Storm rod `{Spark · waiting}` | Lightning (Fire · Air · Air · Mercury, Shot), Live-floor (Fire · Air · Salt, Spread), Jolt (Fireball + Sulphur) | A join becomes a rune, then the chain continues. Sulphur turns the same spark into a different work. |

Walk the orange Free charm in Ash Court if you want Fireball written for you. You can ignore it and compose from first principles.

| Design rule | In this slice |
| --- | --- |
| Casting is perception, not position | Runes live on their own layer, visible only in the Charter. The weave is what is on screen, alternating rows, scrolled sideways. You cannot draw a rune that is off-camera. Click a cell to draw it. |
| The world is the same materials | Each `MaterialId` has its own tile paint and a full signature (timber is Water · Earth · Salt · Plant, not a lone root). Ice, lava, grove, and the rest are already in the catalog for later maps. The Grimoire lists them beside the spells. |
| Enemy = lock, spell = key | The right spell unmakes the encounter instantly. No HP. |
| Terrain = lock | Torch, pit, and rod accept keys the same way the mite does. |
| Chains, not pairs | The fifty catalog spells resolve in play. Fireball is Fire · Air · Mercury — a moving spark, no Salt. The Grimoire lists them all; click a name to string it. |
| Joins are runes | Fire · Air → Spark. Spark · Air → Lightning. Short tutorial strings still work as a fallback. |
| Free is never required | Every lock has a Charter key. Free fills a blank, leans on attunement, and cannot be stored. |

## Open and play

1. Install [Unity Hub](https://unity.com/download) and **Unity 6.3 LTS** (`6000.3.22f1` or a nearby 6.3).
2. Open this folder in Hub (`Assets`, `Packages`, `ProjectSettings`).
3. Open `Assets/Scenes/Main.unity` and press Play. Leave the scene as the camera and light — the adept, rooms, and locks spawn at runtime. Do not place a character in the scene.

### Controls

You only move and cast. The adept is the hooded figure with a violet glow. A gold ring marks the nearest lock; it turns cyan while you aim.

- **WASD** / arrows move
- **Space** opens the Charter — a wall of the eleven, and the room’s weave in a scrolling grid. Space again closes it. The weave is not visible while you walk.
- **Click a cell in the weave** to draw that rune (only in the Charter).
- In the Charter: **click wall runes** or weave cells to string them (up to 8). The wall is the eleven basic runes, but only those **on screen** light up. **Air is ambient** wherever a room still holds breath. **Mercury is ambient** because the adept is ensouled — a soul is what lets a living being work magic. Walk to bring other missing runes into view. The grid is the camera’s sentence, manifestations included. Two runes birth a join or wait; a finished spell is a sentence. Then **Charter Cast**, **Store**, or **Free Cast**.
- **Charter Cast** (F / Enter): the recipe must already be written. Wrong strings fizzle.
- **Free Cast** (X): fills up to one missing rune (the budget can rise later). Several matches → attunement-weighted pick. Free cannot be stored.
- **Store** (R): holds one Charter sentence. Store is the benefit of using Charter.
- After a cast: **click the world**. The chain already wrote the form (Earth stands as a pillar; a wall then asks for a second click; Mercury without breath is remote; hop and flight stay on you). Esc cancels and keeps the string.
- The **bottom bar** shows the stored Charter spell. Click the slot or press **F** / **Enter** in the world to aim it.
- **Grimoire** on the bottom bar (or **Esc** / **G**) lists every written spell and your Free attunement.
- **Backspace** / **C** unmake the last rune

Casts are visible: Shot flies, Pillar rises, Spread wells from the feet, Remote forms at the click. An unwritten Charter string fizzles. Free fills a blank; used types grow. The right key unmakes the lock and opens the door east.

Walk into a pit and you return to the last safe floor. A pillar or wall fills that hollow (a wall is start-to-stop: a span over the drop, a barrier on the floor). Hop leaps a few tiles. Flight lets you walk over pits for a short while.

## Where to grow the trees

| File | What to add |
| --- | --- |
| `Assets/Scripts/World/SanctumLayout.cs` | New rooms and tile layouts |
| `Assets/Scripts/World/MaterialCatalog.cs` | New materials, signatures, and tile paints |
| `MATERIALS.md` | Running material list (beside the spell book) |
| `Assets/Scripts/Field/RuneTapestry.cs` | Charter-only room weave (scroll + alternating rows) |
| `Assets/Scripts/Field/RoomSentence.cs` | How a room is read as a continuous sequence |
| `Assets/Scripts/Field/RuneStringSource.cs` | Ordered world-sentences in the field |
| `Assets/Scripts/World/TileTypes.cs` | New tile kinds |
| `Assets/Scripts/Magic/RuneCatalog.cs` | New rune names, families, meanings |
| `Assets/Scripts/Magic/MaterialTree.cs` | Second/third-tier blends |
| `Assets/Scripts/Magic/SpellGrammar.cs` | New Material × Aspect × Formation recipes (keep them sparse) |
| `Assets/Scripts/Magic/SpellShape.cs` | How a written form is aimed (range, lock radius) |
| `SPELLS.md` | Primary runes, family mixing rules, full spell list |
| `DESIGN.md` | The source of truth |

## Not in this slice (on purpose)

- Full material tree, ternary nodes, and `material-codex.html`
- Cascading environmental reactions (conduction, spreading burn, ice bridges over water)
- Attunement decay / off-focus wither, a Free-store item, Primordial access, soul-work, ensouled casters
- Wards, mediums, and the Primordial-gate item
- Overworld, dialogue, and Magnum Opus world-tint

Those stay open threads in `DESIGN.md` until you ratify them.
