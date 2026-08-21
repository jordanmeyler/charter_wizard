# Rune Magic

A 2D top-down puzzle-RPG. You read the runic field, compose a spell, and turn a lock. Combat is not a damage race. Terrain is made of the same materials as spells.

The living design reference is [`DESIGN.md`](DESIGN.md) (v0.14). The eleven basic runes and the written story-chains are in [`SPELLS.md`](SPELLS.md). World materials — tiles you can stamp, with full rune sentences — are in [`MATERIALS.md`](MATERIALS.md). Joins become their own runes (Fire · Air → Spark). Salt stands a body (walls, pillars). Sulphur is the wildcard. Life marks a living recipe. Death is reserved for Free / grave-work. This repository is a Unity 6.3 project with a four-room sanctum slice of that system.

## What is implemented

Four rooms, east to west of the spawn. Tiles are **materials** (ash, timber, hearthstone, void, spark-vein…) and each material keeps a full rune sentence. Those glyphs stay folded while you walk. **Space** opens the Charter: the eleven writeable runes, and a sideways-scrolling weave of everything in the room. Enemies and terrain are both locks. You read the glyphs there, then weave a key.

| Room | Lock | Intended keys | What it teaches |
| --- | --- | --- | --- |
| **Ash Court** | Ash Mite `{Fire · Salt · Life}` | Almost any formed offensive spell | A living creature writes its recipe. Life stays Life. The adept is mind · body · soul. |
| **Wick Chapel** | Cold torch `{Plant · Dry wick}` | Flame-pillar (Fire · Salt · Earth), Melt (Fire · Salt · Mercury), Ignite (Fire · Sulphur · Salt), Snuff / Smother | Terrain is a lock. Salt stands fire as a pillar or a wick. A stood fire-body is what goes *into* the wick. |
| **The Drop** | Pit / missing Earth | Wall (Earth · Salt · Earth: click start and stop), any pillar, Hop (Air · Salt · Air, Self), Flight, Bridge | Traversal. Rest that stands fills a hollow or bars the floor. Breath given a body leaps or flies. Order is the sentence: Hop and Flight use the same ideas in a different order. |
| **Storm Cell** | Storm rod `{Spark · waiting}` | Lightning (Fire · Air · Mercury, Shot), Live-floor (Fire · Air · Salt, Spread), Jolt (Lightning + Sulphur) | Fire given breath and sent is a bolt. A join can stand as Lightning, then Mercury sends it. |

Walk the orange Free charm in Ash Court if you want Fireball written for you. You can ignore it and compose from first principles.

| Design rule | In this slice |
| --- | --- |
| Casting is perception, not position | Runes live on their own layer, visible only in the Charter. The weave is what is on screen, alternating rows, scrolled sideways. You cannot draw a rune that is off-camera. Click a cell to draw it. |
| The world is the same materials | Each `MaterialId` has its own tile paint and a full signature (timber is Water · Earth · Salt · Plant, not a lone root). Ice, lava, grove, and the rest are already in the catalog for later maps. The Grimoire lists them beside the spells. |
| Enemy = lock, spell = key | The right spell unmakes the encounter instantly. No HP. |
| Terrain = lock | Torch, pit, and rod accept keys the same way the mite does. |
| Chains, not pairs | The catalog spells resolve in play. Fire is Fire · Mercury. Lightning is Fire · Air · Mercury. The written order is the sentence. The Grimoire lists them all; click a name to string it. |
| Joins are runes | Fire · Air → Spark. Spark · Air → Lightning. Short tutorial strings still work as a fallback. |
| Free is never required | Every lock has a Charter key. Free fills a blank, unscrambles a valid bag of runes, leans on attunement, and cannot be stored. |

## Open and play

1. Install [Unity Hub](https://unity.com/download) and **Unity 6.3 LTS** (`6000.3.22f1` or a nearby 6.3).
2. Open this folder in Hub (`Assets`, `Packages`, `ProjectSettings`).
3. Open `Assets/Scenes/Main.unity` and press Play. Leave the scene as the camera and light — the adept, rooms, and locks spawn at runtime from `Assets/Resources/Maps/sanctum.json`. Do not place a character in the scene.

### Controls

You only move and cast. The adept is the hooded figure with a violet glow. A gold ring marks the nearest lock; it turns cyan while you aim.

- **WASD** / arrows move
- **Space** opens the Charter — a wall of the eleven, and the room’s weave in a scrolling grid. Space again closes it. The weave is not visible while you walk.
- **Click a cell in the weave** to draw that rune (only in the Charter).
- In the Charter: **click wall runes** or weave cells to string them (up to 8). The wall is the eleven basic runes, but only those **on screen** light up. **Air is ambient** wherever a room still holds breath. **You** are always mind · body · soul. A living creature on screen writes its recipe as written (the ash mite is Fire · Salt · Life). Walk to bring other missing runes into view. Two runes birth a join or wait; a finished spell is a sentence. Then **Charter Cast**, **Store**, or **Free Cast**.
- **Charter Cast** (F / Enter): the recipe must already be written, in order. Wrong or scrambled strings fizzle.
- **Free Cast** (X): fills up to one missing rune (the budget can rise later), and can unscramble a valid bag of runes into a written sentence. Several matches → attunement-weighted pick. Free cannot be stored.
- **Store** (R): holds one Charter sentence. Store is the benefit of using Charter.
- After a cast: **click the world**. The chain already wrote the form (Earth stands as a pillar; a wall then asks for a second click; Fire · Mercury flies; hop and flight stay on you). Esc cancels and keeps the string.
- The **bottom bar** shows the stored Charter spell. Click the slot or press **F** / **Enter** in the world to aim it.
- **Grimoire** on the bottom bar (or **Esc** / **G**) lists every written spell and your Free attunement.
- **Backspace** / **C** unmake the last rune

Casts are visible: Shot flies, Pillar rises, Spread wells from the feet, Remote forms at the click. An unwritten or scrambled Charter string fizzles. Free fills a blank or unscrambles a valid bag of runes; used types grow. The right key unmakes the lock and opens the door east.

Walk into a pit and you return to the last safe floor. A pillar or wall fills that hollow (a wall is start-to-stop: a span over the drop, a barrier on the floor). Hop leaps a few tiles. Flight lets you walk over pits for a short while.

## Building levels and scenes

You do not place rooms in `Main.unity`. Maps are JSON. The four-room sanctum is `Assets/Resources/Maps/sanctum.json`. `Assets/Resources/Maps/index.json` chooses which map boots (`startup`).

| Tool | Use it for |
| --- | --- |
| [`Tools/map-editor.html`](Tools/map-editor.html) | Browser painter — rooms, materials, pits, doors, plaques, rune-strings, locks, halls. Export JSON and drop it in `Assets/Resources/Maps/`. |
| Unity `Window → Rune Magic → Map Painter` | Same JSON inside the editor. Left-click stamps a tile; Shift-click places a prop; Alt-click clears. |
| `Assets/Scripts/World/MapFile.cs` / `MapBuilder.cs` | The format and the runtime stamp. |

A room is a shell (wall + floor) plus stamps (any cell that is not the default) plus props (`plaque`, `runes`, `charm`, `mite`, `torch`, `rod`, `chasm`, `item`). Halls connect two room ids. Lock keys can be omitted — the builder uses the tutorial presets.

Tiles are still painted in code (`SpriteFactory`): each material has its own cobble/plank/vein treatment, and floors vary by world position. Walls sit a little taller than the floor. Rooms wash the camera; locks carry a soft glow. You can replace or add sprites yourself — see Catalog below.

## Catalog — recipes, sprites, items

**Yes: one master file controls the recipes.** The game loads [`Assets/Resources/Catalog/spells.json`](Assets/Resources/Catalog/spells.json) at boot. That file is the written story-chains plus the joins (Fire · Air → Spark). [`SPELLS.md`](SPELLS.md) is the prose companion; if they disagree, the JSON wins in play. `SpellCodex.cs` is only a fallback if the JSON is missing.

[`Assets/Resources/Catalog/art.json`](Assets/Resources/Catalog/art.json) is sprites and items. A custom sprite can reuse a built-in id (`adept`, `ash-mite`, `charm`, `torch`, `rod`) to replace it, or a new id you assign on an item or a map prop.

| Tool | Use it for |
| --- | --- |
| [`Tools/catalog-editor.html`](Tools/catalog-editor.html) | Edit recipes, joins, pixel sprites, and items. Export `spells.json` / `art.json` back into `Assets/Resources/Catalog/`. |
| Unity `Window → Rune Magic → Catalog` | Jump to those files |

A new recipe needs a **recipe** sentence and a **work** effect (`Fireball`, `Hop`, `Wall`…). Work is the coded verb it reuses. A new lock key is the spell `id`. A new item is an `art.json` row; place it on a map with `"type": "item", "item": "your-id"` or set `"sprite"` on a mite/torch.

## Where to grow the trees

| File | What to add |
| --- | --- |
| `Assets/Resources/Maps/` | New maps (JSON). Point `index.json` at the one to boot |
| `Tools/map-editor.html` | Paint those maps without opening Unity |
| `Assets/Scripts/World/SanctumLayout.cs` | Coded fallback if JSON is missing |
| `Assets/Scripts/World/MaterialCatalog.cs` | New materials, signatures, and tile paints |
| `MATERIALS.md` | Running material list (beside the spell book) |
| `Assets/Scripts/Field/RuneTapestry.cs` | Charter-only room weave (scroll + alternating rows) |
| `Assets/Scripts/Field/RoomSentence.cs` | How a room is read as a continuous sequence |
| `Assets/Scripts/Field/RuneStringSource.cs` | Ordered world-sentences in the field |
| `Assets/Scripts/World/TileTypes.cs` | New tile kinds |
| `Assets/Scripts/Magic/RuneCatalog.cs` | New rune names, families, meanings |
| `Assets/Scripts/Magic/MaterialTree.cs` | Second/third-tier blends |
| `Assets/Resources/Catalog/spells.json` | Master recipes and joins (what the game actually casts) |
| `Assets/Resources/Catalog/art.json` | Custom sprites and items |
| `Tools/catalog-editor.html` | Paint sprites and rewrite recipes without opening C# |
| `Assets/Scripts/Magic/SpellGrammar.cs` | Legacy compressed pair recipes (fallback only) |
| `Assets/Scripts/Magic/SpellShape.cs` | How a written form is aimed (range, lock radius) |
| `SPELLS.md` | Prose companion to the JSON book |
| `DESIGN.md` | The source of truth |

## Not in this slice (on purpose)

- Full material tree, ternary nodes, and `material-codex.html`
- Cascading environmental reactions (conduction, spreading burn, ice bridges over water)
- Attunement decay / off-focus wither, a Free-store item, Primordial access, soul-work, ensouled casters
- Wards, mediums, and the Primordial-gate item
- Overworld, dialogue, and Magnum Opus world-tint

Those stay open threads in `DESIGN.md` until you ratify them.
