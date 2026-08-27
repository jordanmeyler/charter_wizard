# Tiles

Maps are Unity Tilemaps. You paint them in the Scene view. Play bakes
each `WorldPaintTile` into a live `WorldTile`.

## Authoring

1. Open `Assets/Scenes/Main.unity`. The scene already has **Map** (a
   Grid), **Tiles**, **Environment Details**, **Cover**, and **Spawn**,
   plus a 13×11 stone room at the origin so you can see tiles immediately.
2. In the Scene view, turn on **2D**. Select **Map** and press **F** to
   frame the room. The camera and spawn sit inside that room.
3. `Window → 2D → Tile Palette` → Open Palette → **Rune Palette**.
   If the palette is blank, `Window → Rune Magic → Bind Pack Sprites`.
   If the palette is missing, `Window → Rune Magic → Create Tile Palette`.
4. Select the **Tiles** object and paint the look — Rune Palette or any
   ElvGames palette. Erase or overwrite the starter room as you like.
   Cells you leave empty are the drop at Play. Paint walls and
   floor where they should stand; raise a pillar or wall to cross.
5. Assign gameplay after: `Window → Rune Magic → Tile Properties`.
   Select the layer you just painted (Tiles, Walls, or Environment
   Details), turn on **Paint in Scene view**, pick Kind / Material /
   Cover / Aura / Blocks, and click those cells. The sprite stays.
   Right-click a cell to copy its properties. Check **Write onto Cover
   layer** to stamp ice / fire / aura without changing the walk cell.
   Stamps glow in the **Scene** view (not Game). Nothing to turn on.
   If colours are missing, click the Scene tab, turn **Gizmos** on at
   the top-right of that view, and keep Play off. A **Rune Stamps**
   panel sits in the Scene view; `Window → Rune Magic → Show Stamps`
   should be checked. Each material has its own colour. Cells with
   no glow are look only.
6. Select **Cover** and paint ice / fire / lightning / vine / aura over
   those cells if you would rather brush overlays than stamp them.
   Miasma and fog are see-through (about 40%). Check **Opacity** in
   Tile Properties to fade any Cover tile, or to make the veil denser.
   Select **Environment Details** for plants and furniture that sit on
   the floor. Check **Blocks** and drag across a cluster to give that
   group collision.
7. Click a tile asset in `Assets/Tiles` to set **material**, **kind**,
   **cover**, and **aura** on a shared brush. Duplicate an asset to make
   a new brush. `Create → Rune Magic → Map Tile` also works.
8. Place objects with `GameObject → Rune Magic → Item / Decor / Enemy / Torch…`
   and set catalog id, material, formula, or sprite on the Inspector.
   Pack enemies are under `GameObject → Rune Magic → Enemies`.
9. Leave **Stamp Foundation Into Scene** alone unless you want the old
   generated Floor 1 dumped back onto the Tilemap.

Play hides the editor Tilemap renderers and builds the live grid from
what you painted. JSON under `Assets/Resources/Maps/` is leftover and
is not loaded unless Level Authoring is set to **Named Map**.

## Layers

Keep extra Tilemaps as **children of Map**. Play merges them by name.

| Child name | What to paint | Play |
|---|---|---|
| **Tiles** / **Floor** | Walkable ground, water, pits, doors | Kind + material. Empty cells become pits. Hidden, then baked. |
| **Walls** | Solid walls | Merged as walls. Hidden, then baked. |
| **Cover** / **Coverings** | Ice, fire, vine, aura | Overlay only. Hidden, then baked. |
| **Environment Details** / Decor | Plants, rugs, chairs, statues | Own stamp + optional collision. Hidden, then baked as a detail. |

## First area puzzles

Stamp gameplay onto the rooms you painted, then drop the stones.
Do **not** run **Stamp Foundation** — that dumps the old generated
floor over your tiles.

Open `Window → Rune Magic → Tile Properties`. Select the layer
first (**Tiles** for walk / pit / ice walls, **Cover** or
**Write onto Cover layer** for miasma). Turn on **Paint in Scene
view**. Uncheck the stamps you do not want so
a click only writes Kind, or only Aura. Empty Tiles cells are
already pits at Play (magenta “blank” glow). Stamp **Kind = Pit**
only when you painted a hole look that would otherwise stay floor.

### 1. Fire — fire stone frozen in ice

Ice has to *block*. Cover-Ice is only a look; you still walk it.
Stamp a ring (or U) of **ice walls**, leave one floor cell in the
middle for the stone.

1. Select **Tiles** (or **Walls**).
2. Tile Properties: check **Kind** = `Wall`, **Material** = `Ice`.
   Uncheck Cover / Aura / Blocks.
3. Click the cells around the alcove. Leave the centre as floor.
4. Optional look: check **Cover** = `Ice`, **Write onto Cover
   layer**, and stamp the same cells (and the inner floor) so the
   ice reads as ice.
5. `GameObject → Rune Magic → Item`. Snap to the inner cell.
   Inspector: `catalogId` = `fire-stone` (sprite `stone-fire` if
   you want it shown in the editor).
6. Optional lock sprite: `GameObject → Rune Magic → Barrier` on
   that same cell. `authoredName` = `Ice cage`, formula `Water`
   and `Earth`, `spriteId` = `ice-block`. List **Cover Cells** as
   the ice-wall cells. Fireball / Melt / Ignite / a flame-pillar
   still melt ice walls even without the Barrier — the Barrier is
   a target you can click. Melted ice leaves a water covering on
   the floor that was under the wall. Fire will not run across
   that water.

The player needs a **Fire** mark in reach so they can write
Fireball. `GameObject → Rune Magic → Inscription` or **Pillar**,
Inspector `authoredRune` = Fire, near the room mouth. Do not stamp
fire on Environment Details and do not expect a painted torch tile
to burn things. A torch is `GameObject → Rune Magic → Torch` — a
lock you light with a spell. It sits in the Hierarchy like the
stone, not on a tile layer. Painted fire (Cover = Fire or Aura =
Fire) is scenery at rest. Hunger only runs after a player or NPC
spell starts it, or after a covering a spell left behind (melt
water, spell-fire on a bush).

### 2. Air — miasma, then the air stone

Miasma is not a wall. Walking in it throws you back to the last
safe floor. **Gust** (`Air · Mercury`) or Gale clears it.

1. Select **Tiles** so you can see the air room.
2. Tile Properties: uncheck Kind / Material. Check **Aura** =
   `Miasma`. Turn on **Write onto Cover layer**. Miasma is
   see-through by default (about 40%). Check **Opacity** and
   drag the slider if you want it thinner or more solid.
3. Click (or drag) the floor you want fouled — the path to the
   stone, not the doorway if you want them to step in and get
   thrown back. Paint any foggy tile on **Cover** first if you
   want your own art under the veil.
4. `GameObject → Rune Magic → Item` on the far side of the fog.
   `catalogId` = `air-stone`.
5. Optional: `GameObject → Rune Magic → Fog` on those cells if
   you want a named lock Gust can target. Painted aura alone
   already throws you back and vents when air is sent.

### 3. Earth — pits and the drop

Unpainted Tiles cells are the drop. Walk off the floor, or through
a hole you erased, and you return to the last safe floor. No
Chasm object. Hop (`Air · Salt · Air`), Earth-pillar
(`Earth · Salt`), or a wall drawn across the gap still crosses.

**To make a pit**

- Erase floor on **Tiles** (or never paint those cells). That is
  enough. The Scene glow marks blank space as a pit.
- Or paint a hole look (Cavern pit / void tiles), then stamp
  **Kind = Pit**, **Material = Void** so Play does not treat that
  art as floor.

Keep a closed hole under 4×4 or water work can fill it. A hole
that opens onto the map edge is part of the outer void and will
not fill.

1. Leave or erase the crossing — one cell is a small hop; two or
   three in a line is a short gap.
2. `GameObject → Rune Magic → Item` on the far ledge.
   `catalogId` = `earth-stone`.

Painted walls stay walls. Pillars and wall spells fill pits as
walkable spans. A later pass will cap how far a bridge can run
over a drop before it collapses.

Plaques, altars, and teaching columns come after these three
play.

## Sprites on runes, items, and effects

You control the picture from the Inspector. Nothing here is a tile layer.

**Runes (Inscription / Pillar / Rune).**
Every catalog rune can be an inscription — roots, joins, and reserved names. `Window → Rune Magic → Inscriptions` (or Authoring → Inscriptions **Place**): click a mark, then click a tile in the Scene view. Right-click removes. The Inspector on an inscription is the same grid. With nothing else set, a **floating mark** is the whole picture — no slab, shaft, or base. Hover = Floor (lower) or Pillar (a little higher). Drag your art onto **Portrait** when you have a palette of your own. Or type a **Sprite Id**. In Play, click a floating mark to draw it into the Charter.

**Items, torches, plaques, barriers, fog.**
Same pattern. Select the object. Drag a sprite onto **Portrait**, or set `spriteId` / `catalogId` (items use `fire-stone` and can take `spriteId` = `stone-fire` to show that art in the editor).

**Tile covers and auras (ice look, fire look, miasma, water after a melt).**
These are tiles, not objects.

1. Select **Cover** in the Hierarchy.
2. Paint any ice / fire / water / fog tile from any palette — that *is* the picture.
3. Or stamp in Tile Properties: **Cover** = Ice / Fire / Water, **Write onto Cover layer**. Play then uses the catalog sprites `cover-ice`, `cover-fire`, `cover-water`.

Miasma is **Aura** = Miasma on the Cover layer. The sick look is the runtime overlay (`tile-poison`); you can still paint a foggy tile on Cover if you want your own art under it.

**Spell leftovers** (wet floor after melt, hunger on a bush) draw `tile-wet` / `tile-fire` from the catalog. To change those globally, add or replace those ids in `Assets/Resources/Catalog` / the sprite sheets. A Cover tile you painted stays as the floor look; the wet/fire glow sits on top.

Do **not** make a Tilemap for interactables. Puzzle pieces are GameObjects: `GameObject → Rune Magic → …`. A tile cannot hold a formula, key list, or inventory id.

| Place | Old puzzle job | Inspector |
|---|---|---|
| **Item** | fire-stone, water-stone, earth-stone, air-stone, body / mind / grove / flood / spark stones, ice-cask | `catalogId` |
| **Mite** / **Enemy** | ice-thing, ash-mite, and the rest | formula, keys, sprite |
| **Torch** | cold torch | keys |
| **Rod** | storm rod | keys |
| **Gate** | Gate of Elements | `requires` item ids |
| **Barrier** | ice cage | formula, cover cells, clear material |
| **Chasm** | pit lock | nearby pits, or list cells |
| **Arrows** | arrow volley | dir, cover cells |
| **Fog** | poison / gust room | cover cells |
| **Plaque** | wall text | text |
| **Rune** | written sentence on the field | runes, dir |
| **Inscription** | floating teaching mark | any catalog rune |
| **Pillar** | same mark, a little higher | any catalog rune |
| **Charm** | Free charm | — |
| **Crystal** | spawn / death return | — |
| **Flame Hall** | water-ward lesson at a kindled hall | — |
| **Decor** | look-only prop (not a lock) | sprite |

A layer named **Enviroment Details** (the typo) still counts as Environment Details.

Materials work if you stamp them after painting: select the layer, open `Window → Rune Magic → Tile Properties`, set Kind + Material, click the cells. Walls you never stamp are treated as **Wall / Stone** when they sit on a layer named Walls.

**Environment Details** has its own stamp. Select that layer, stamp **Timber** on a table or **Plant** on a bush. A standing torch or painted fire does not catch those bushes — the room is at rest. A player or NPC spell that starts a fire can then run into Plant / Timber / Moss / Grove and leave hot coals. Stone floors do not catch. A tile named table / chair / bench / bush is guessed as Timber or Plant even if you never stamped it.

Collision is a separate stamp. Select **Environment Details**, check only **Blocks** in Tile Properties, and drag across a group of tables or statues. Those cells block walking. Tables, chairs, statues, crates, and pillars are guessed as blocking if you never stamped them; rugs and grass are not. When a blocking table burns to ash, you can walk over the pile. Cover and Aura still apply to that cell (ice, fire, vine, kindled).

`GameObject → Rune Magic → Decor` is still look-only art. Burning or blocking furniture has to be an Environment Details **tile**.

The grid is **16×16** (16 PPU, one cell = one tile). ElvGames Tile
palettes also paint — Play keeps that sprite and guesses wall / door /
water from the tile name unless you stamped properties.

| Folder | Brushes |
|---|---|
| `Assets/Tiles/Floor` | One floor per `MaterialId` (Stone, Dirt, Water, Ice…) |
| `Assets/Tiles/Wall` | One wall per material |
| `Assets/Tiles/Special` | Pit, Door, Bridge |
| `Assets/Tiles/Cover` | Ice / fire / lightning / vine overlays and fire / miasma / fog auras |

Each brush in `Assets/Tiles` already has a sliced ElvGames sprite
assigned (Crypt stone, Hell lava, Jungle moss, and so on). Drag a
different `RA_*` tile from `Assets/ElvGames/Rogue Adventure/Tilesets`
onto **Sprite** if you want another look. Play keeps that sprite.

The atlas under `Assets/Resources/Sprites/Rogue/` is only a fallback for
brushes that still have a blank Sprite. Enemies are 32×32 strips in
`Sprites/Enemies/`.

Regenerate slices with `python3 Tools/build-rogue-atlas.py`.

## Sheets

| File | What it is |
|---|---|
| `Rogue/RA_Crypt.png` | Hub stone floors, brick walls, door, furniture |
| `Rogue/RA_Hell.png` | Fire / lava cover, scorched rock, braziers |
| `Rogue/RA_Cavern.png` | Dirt, water, pits, cave walls, bridges |
| `Rogue/RA_Sanctuary.png` | Ice cover, pillars, torches, altars |
| `Rogue/RA_Jungle.png` | Moss, vines, plants, bushes |
| `Rogue/RA_Atlantis.png` | Seals, lightning, charged props |
| `Enemies/Enemy_001.png` … `012` | Placeable pack enemies (`enemy-001` …) |

Each tile is 16×16 at 16 PPU. Rects in `tiles.json` are Unity texture
space: `x`, `y` from the **bottom-left**.

## Floors (walk on these)

Only three walkable families. Ice / fire / lightning are never the
floor itself.

| Id | Sheet | What it looks like |
|---|---|---|
| `floor-stone` | Crypt | Dungeon cobble |
| `floor-cracked` | Crypt | Worn cobble |
| `floor-dirt` | Cavern | Packed earth |
| `floor-water` | Cavern | Water pool |
| `pit` | Cavern | Open pit |
| `pit-edge` | Cavern | Pit rim |

Aliases: `floor`, `floor-hearth`, `floor-ice`, `floor-vein`,
`floor-crystal`, `floor-ember` all resolve to stone or dirt so old map
stamps keep working. The actual ice/fire/lightning look is a **cover**.

## Walls and the door

| Id | Sheet | Notes |
|---|---|---|
| `wall` | Crypt | Solid dungeon wall |
| `wall-moss` | Jungle | Mossy outer wall |
| `wall-cave` | Cavern | Cave wall |
| `wall-fissure` | Hell | Cracked volcanic wall |
| `door` / `arch-shut` | Crypt | **The** wooden door |
| `door-open` | Crypt | Open arch (no leaf) |
| `arch-pillar` | Sanctuary | Stone pillar arch |
| `pillar` | Sanctuary | Stone pillar |
| `pillar-broken` | Crypt | Broken stump |
| `stalagmite` | Cavern | Rock cluster |

One door sprite. The exit is still three cells wide so a closed gate
seals the hall; only the **center** cell draws the wooden leaf. The
two jambs draw stone wall.

## Coverings (element swaps)

Freeze / burn / charge / flood swap the covering, not the walk family.

| Cover | Sprite | Source |
|---|---|---|
| `ice` | `cover-ice` | Sanctuary — ice over stone |
| `fire` | `cover-fire` | Hell — lava / fire |
| `lightning` | `cover-lightning` | Atlantis — charged seal |
| `water` | `cover-water` | Cavern water tile |
| `vine` | `cover-vine` | Jungle vines |
| `cracks` | `cover-cracks` | Crypt cracks |
| `seal` | `cover-seal` | Atlantis seal |

## Props (reuse these)

| Id | What |
|---|---|
| `torch` / `torch-lit` | Wall torch — modular `(0,79)` / `(64,79)` |
| `brazier` / `brazier-lit` | Standing brazier — modular `(128,75)` / `(96,75)` |
| `bush` / `bush-bloom` | Greenery — modular bottom row |
| `water-ripple` | Water sparkle |
| `ice-fountain` | Frozen fountain — ice sheet |
| `ice-chest` / `ice-vessel` | Iced bust |
| `rod` / `lightning-rod` / `lightning-pillar` | Charged column |
| `lightning-vial` | Sparking jar |
| `statue` / `statue-gold` | Ancient statues — furniture sheet |
| `bookshelf` | Shelf of tomes |
| `bench` / `chair` / `table` | Furniture |
| `water-fountain` | Glowing water altar |

Place decor with `GameObject → Rune Magic → Decor` and set the sprite id
(`torch`, `brazier`, `pillar`…). Place pack enemies with
`GameObject → Rune Magic → Enemies`. The JSON stamp form is leftover.

## Fixing a bad slice (by hand)

The atlas rects in `tiles.json` were guessed. Do **not** edit JSON if you
can drag a sprite — the ElvGames pack is already sliced in Unity.

1. Open `Assets/Tiles/Floor/Floor-Stone` (or Wall-Stone, Door, Pit…).
2. In the Project window open
   `Assets/ElvGames/Rogue Adventure/Tilesets/Crypt/Tiles`
   (Hell, Caverns, Sanctuary, Jungle, Atlantis have the same layout).
3. Drag the tile you want onto **Sprite**. The painted map updates.
4. Repeat for each brush you care about. Play uses that sprite.
5. If every brush is blank again, run `Window → Rune Magic → Bind Pack Sprites`.

To fix a `tiles.json` rect instead (only needed when Sprite is left blank):

1. Open `Assets/Resources/Sprites/Rogue/RA_Crypt.png` (or Hell / Cavern…).
2. Measure the 16×16 cell. Unity `y` starts at the **bottom-left** of
   the PNG, not the top.
3. Edit that row’s `x`, `y`, `width`, `height` in
   `Assets/Resources/Catalog/tiles.json`.
4. `floor-stone` is the stone floor, `wall` is the stone wall, `door`
   is the leaf. Save. Play again.

## Adding a tile later

1. Open the sheet in any image tool. Note the pixel rect. Remember
   Unity `y` is measured from the **bottom**.
2. Add a row to `tiles.json`:

```json
{ "id": "my-tile", "source": "Sprites/Rogue/RA_Crypt", "x": 32, "y": 240, "width": 16, "height": 16 }
```

3. Use `"sprite": "my-tile"` on a decor stamp, or `TileAtlas.Get("my-tile")`.
