# Rune Magic

A 2D top-down puzzle-RPG. You read the runic field, compose a spell, and turn a lock. Combat is not a damage race. Terrain is made of the same materials as spells.

The living design reference is [`DESIGN.md`](DESIGN.md) (v0.17). The eleven basic runes and the written story-chains are in [`SPELLS.md`](SPELLS.md). World materials — tiles you can stamp, with full rune sentences, flammability, and conductivity — are in [`MATERIALS.md`](MATERIALS.md). Floor 1 — the Foundation — is specified in [`FLOOR1.md`](FLOOR1.md) and boots from `Assets/Resources/Maps/foundation.json`. Joins become their own runes (Fire · Air → Spark). Salt stands a body (walls, pillars). Sulphur is the wildcard. Life marks a living recipe. Death is reserved for Free / grave-work. This repository is a Unity 6.3 project.

## What is implemented

**The Foundation** is the first floor. You spawn in a hub with four open elemental rooms attached — Fire, Water, Earth, Air. Walk in, read the altar (the mark sits beside a flame, water, rock, or gale), use that element’s spells on the obstacle, and take the stone. Around the crystal the floor is carved **body · spirit · mind**. Door I (the hub’s north door) wants the four element stones. Past it, three aspect sanctums teach Salt, Mercury, and Sulphur. Door II wants only those three stones. Doors gate on **possession, not sequence**.

Tiles are **materials** and each material keeps a full rune sentence. Those glyphs stay folded while you walk. **Space** opens the Charter: a wall of marks you have remembered, and a sideways-scrolling weave of everything on screen. **Play** sight shows only the marks; **Develop** (F1) shows names and the written book. Enemies and terrain are both locks.

| Room | Lock | Intended keys | What it teaches |
| --- | --- | --- | --- |
| **The Cross** | Gate of Elements (north) | The four element stones | Choice. Four labelled roots. |
| **The Frozen Hall** | Ice cage (ice-thing optional) | Melt, Fireball, Flame-pillar, Ignite | Fire: heat, melt, burn. |
| **The Ember Vault** | Flame curtain (golem slams) | Douse (`Water · Mercury`), Water-jet, Rain, Flood | Water: douse, fill, cool. Hop or Stoneskin the slam. |
| **The Arrow Gauntlet** | Real arrow shots, then a pit either side of the stone | Earth-pillar (`Earth · Salt`), then walk around | Earth: rest given a body. Shots kill; a wall or Stoneskin breaks them. |
| **The Sundered Heights** | Green poison fog | Gust (`Air · Mercury`), Gale | Air: a simple wind clears the room. |
| **The Standing Stone** | A gap | Earth-pillar (`Earth · Salt`), Wall, Bridge; Hop if you know Air | Salt stands a body. |
| **The Gallery of Force** | Wizard (2s fireball) | A sent element (Fireball, Douse, Hurled stone, Lightning…) | Mercury sends. Wall, hop, or get behind the shot, then unmake. |
| **The Silent Court** | Stone men (they block a short aisle) | Rage, Command, Lull, Terror, Jolt | Sulphur reaches a mind. |
| **Gate of Aspects** | Three sockets | Body, Spirit, and Mind stones | This section’s keys only. The floor opens. |

The old four-room slice (`sanctum`) is still in `Assets/Resources/Maps/`. Point `index.json` at it to boot that map.

Douse, Command, Gust, and Earth-pillar are new ordinary sentences written for this floor. You cannot swim; water is a wall until you freeze it, span it, or boil it dry.

| Design rule | In this slice |
| --- | --- |
| Casting is perception, not position | Runes live on their own layer, visible only in the Charter. The weave is what is on screen, alternating rows, scrolled sideways. You cannot draw a rune that is off-camera. Click a cell to draw it. |
| The world is the same materials | Each `MaterialId` has its own tile paint and a full signature (timber is Water · Earth · Salt · Plant, not a lone root). Ice, lava, grove, and the rest are already in the catalog for later maps. The Grimoire lists them beside the spells. |
| Enemy = lock, spell = key | The right spell unmakes the encounter instantly. No HP. Statuses still land, and bodies can strike back. |
| Terrain = lock | Rope, ice, flame, poison, pits, and socketed gates accept keys the same way a creature does. |
| Chains, not pairs | The catalog spells resolve in play. Fire is Fire · Mercury. Lightning is Fire · Air · Mercury. The written order is the sentence. The Grimoire lists them all; click a name to string it. |
| Joins are runes | Fire · Air → Spark. Spark · Air → Lightning. Short tutorial strings still work as a fallback. |
| Free is never required | Every lock has a Charter key. Free fills a blank, unscrambles a valid bag of runes, leans on attunement, and cannot be stored. |

## Open and play

1. Install [Unity Hub](https://unity.com/download) and **Unity 6.3 LTS** (`6000.3.22f1` or a nearby 6.3).
2. Open this folder in Hub (`Assets`, `Packages`, `ProjectSettings`).
3. Open `Assets/Scenes/Main.unity` and press Play. Leave the scene as the camera and light — the adept, rooms, and locks spawn at runtime from `Assets/Resources/Maps/foundation.json`. Do not place a character in the scene.

### Controls

You only move and cast. The adept is the hooded figure with a violet glow. A gold ring marks the nearest lock; it turns cyan while you aim.

- **WASD** / arrows move
- **Space** opens the Charter — a wall of remembered marks, and the room’s weave in a scrolling grid. Space again closes it. The weave is not visible while you walk. In the world, each altar shows the **mark beside a picture** (flame, water, rock, gale). Body, spirit, and mind are carved around the crystal.
- **Click a cell in the weave** to draw that rune (only in the Charter).
- **Right-click** (or Shift-click) a mark to **remember** it. Remembered marks sit on the wall so you can string them without hunting the weave. The wall does not fill itself.
- **F1** (or **Play** / **Develop** on the bar) toggles sight. **Play** is the game: abstract marks, no names or element colours. **Develop** is the working ledger: names, letters, recipes, and the full book.
- The **top-right** panel lists the last ten attempted casts. ○ held together; ✕ fizzled. Charter shows the marks. Free blocks them.
- In the Charter: **click wall runes** or weave cells to string them (up to 8). In Play the wall is only what you have kept. In Develop the eleven are named and listed. Only runes **on screen** light up. **Air is ambient** wherever a room still holds breath. **You** are always mind · body · soul. Walk to bring other missing runes into view. Two runes birth a join or wait; a finished spell is a sentence. Then **Charter Cast**, **Store**, or **Free Cast**.
- **Charter Cast** (F / Enter): the recipe must already be written, in order. Wrong or scrambled strings fizzle.
- **Free Cast** (X): fills up to one missing rune (the budget can rise later), and can unscramble a valid bag of runes into a written sentence. Several matches → attunement-weighted pick. Free cannot be stored.
- **Store** (R): holds one Charter sentence. Store is the benefit of using Charter.
- After a cast: **click the world**. The chain already wrote the form (Earth stands as a pillar; a wall then asks for a second click; Fire · Mercury flies; hop and flight stay on you). Esc cancels and keeps the string.
- The **bottom bar** shows the stored Charter spell. Click the slot or press **F** / **Enter** in the world to aim it.
- **I** (or **Pack** on the bar) opens the pack. Stones, charms, and other key items sit there. Click one to look at it. Arrows move the selection. Esc or I closes.
- **Grimoire** on the bottom bar (or **Esc** / **G**): in Play, marks you have kept; in Develop, every written spell and your Free attunement.
- **Backspace** / **C** unmake the last rune

Casts are visible: Shot flies, Pillar rises, Spread wells from the feet, Remote forms at the click. Each work is particles and light of its element — fire is embers, water is droplets, lightning is a jagged arc. Fog and poison mist hang until another element tears them. Walls and pillars stay as masonry or a column; water melts a basic earth wall. An unwritten or scrambled Charter string fizzles. Free fills a blank or unscrambles a valid bag of runes; used types grow. The right key unmakes the lock and opens the door east.

Walk into a pit and you return to the last safe floor. A slam, an arrow, a wizard’s fireball, or a floor that is already hunger **kills** you — you wake at the **spawn crystal**. A pillar or wall fills a hollow (a wall is start-to-stop: a span over the drop, a barrier on the floor). Hop leaps a few tiles and can clear a shot. Flight lets you walk over pits for a short while.

**Wards** are `Element · Salt · Sulphur`, held on you. Water douses Fire (water ward stops fireballs). Fire scorches Earth. Earth stands against Air, and Stoneskin also stops a physical blow. Air dries Water. Only one ward stands at a time.

Spells are single-target, area, or self. Status chips name what holds on you and on them. Water a plant and it grows. Fire spreads onto flammable tiles and dies against water and ice. Charge runs metal and wet stone.

## Building levels and scenes

You do not place rooms in `Main.unity`. Maps are JSON. Floor 1 is `Assets/Resources/Maps/foundation.json` (regenerate with `python3 Tools/build-foundation.py`). The old four-room slice is `sanctum.json`. `Assets/Resources/Maps/index.json` chooses which map boots (`startup`).

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
| `FLOOR1.md` | Floor 1 design (open hub, four elemental rooms, staged doors) |
| `Assets/Scripts/World/SanctumLayout.cs` | Coded fallback if JSON is missing |
| `Assets/Scripts/World/MaterialCatalog.cs` | New materials, signatures, and tile paints |
| `MATERIALS.md` | Running material list (beside the spell book) |
| `Assets/Scripts/Field/RuneTapestry.cs` | Charter-only room weave (scroll + alternating rows) |
| `Assets/Scripts/Field/RoomSentence.cs` | How a room is read as a continuous sequence |
| `Assets/Scripts/Field/RuneStringSource.cs` | Ordered world-sentences in the field |
| `Assets/Scripts/World/TileTypes.cs` | New tile kinds |
| `Assets/Scripts/Magic/RuneCatalog.cs` | New rune names, families, meanings |
| `Assets/Scripts/Magic/GlyphView.cs` | Play vs Develop sight (F1) |
| `Assets/Scripts/Magic/RuneMemory.cs` | Remembered wall marks; later keep-conditions by rune depth |
| `Assets/Scripts/Presentation/RuneMark.cs` | Abstract Play-mode marks |
| `Assets/Scripts/Presentation/RuneSign.cs` | Mark + nature picture (flame, water, rock, gale, body, spirit, mind) |
| `Assets/Scripts/World/RuneStele.cs` | Floor inscriptions and aspect pillars |
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
- Ice bridges over water, gas/oil explosions, and the rest of the later reaction list
- Attunement decay / off-focus wither, a Free-store item, Primordial access, soul-work
- A real death / last-rites pass (this slice respawns at the crystal)
- Passive item-wards and mediums, and the Primordial-gate item
- Overworld, dialogue, and Magnum Opus world-tint

Those stay open threads in `DESIGN.md` until you ratify them.
