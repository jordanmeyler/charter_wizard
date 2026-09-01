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
   Cells you leave empty are the drop at Play. A painted look is **not**
   floor until you stamp **Kind = Floor** or use a **Floor-** brush.
   Raise a pillar or wall to cross a drop.
5. Assign gameplay after: `Window → Rune Magic → Tile Properties`.
   Select the layer you just painted (Tiles, Walls, or Environment
   Details), turn on **Paint in Scene view**, pick Kind / Material /
   Cover / Blocks, and click those cells. The sprite stays.
   Right-click a cell to copy its properties. Check **Write onto Cover
   layer** to stamp ice / fire / miasma without changing the walk cell.
   Stamps glow in the **Scene** view (not Game). Nothing to turn on.
   If colours are missing, click the Scene tab, turn **Gizmos** on at
   the top-right of that view, and keep Play off. A **Rune Stamps**
   panel sits in the Scene view; `Window → Rune Magic → Show Stamps`
   should be checked. Each material has its own colour. Cells with
   no glow are look only.
6. Select **Cover** and paint ice / fire / lightning / vine / miasma over
   those cells if you would rather brush overlays than stamp them.
   Covers are the live layer. **Floor-Fire** and **Wall-Fire** go on
   Tiles / Walls — rest materials, like stone.
   Miasma and fog are see-through (about 40%). Check **Opacity** in
   Tile Properties to fade any Cover tile, or to make the veil denser.
   Select **Environment Details** for plants and furniture that sit on
   the floor. Check **Blocks** and drag across a cluster to give that
   group collision.
7. Click a tile asset in `Assets/Tiles` to set **material**, **kind**,
   and **cover** on a shared brush. Duplicate an asset to make
   a new brush. `Create → Rune Magic → Map Tile` also works.
8. Drag prefabs from `Assets/Prefabs` (Items/Fire Stone, Enemies/Shade,
   Gate, Door) or press Place in `Window → Rune Magic → Authoring`.
   The GameObject menu instantiates those same prefabs.
9. Leave **Stamp Foundation Into Scene** alone unless you want the old
   generated Floor 1 dumped back onto the Tilemap.

Play hides the editor Tilemap renderers and builds the live grid from
what you painted. JSON under `Assets/Resources/Maps/` is leftover and
is not loaded unless Level Authoring is set to **Named Map**.

## Layers

Keep extra Tilemaps as **children of Map**. Play merges them by name.
A cell is **floor only** when a Floor brush or **Kind = Floor** stamp
says so. Looks on any layer — Tiles, Floor 2, Environment Details —
are not walkable. Extra **Floor** / **Tiles** children are more
levels of the same grid: stamp Floor on the cells you can walk.

| Child name | What to paint | Play |
|---|---|---|
| **Tiles** / **Floor** / **Floor 2** | Looks, then Floor / pit / door stamps | Floor only where stamped. Empty cells become pits. Hidden, then baked. |
| **Walls** | Solid walls | Unstamped cells on this layer stay walls. Hidden, then baked. |
| **Cover** / **Coverings** | Ice, fire, vine, miasma, fog | Overlay: look, work, and weave. Hidden, then baked. |
| **Environment Details** / Decor | Plants, rugs, chairs, statues | Look + optional Blocks. A Floor stamp here still makes that cell walkable. Hidden, then baked. |

## First area puzzles

Stamp gameplay onto the rooms you painted, then drop the stones.
Do **not** run **Stamp Foundation** — that dumps the old generated
floor over your tiles.

Open `Window → Rune Magic → Tile Properties`. Select the layer
first (**Tiles** for walk / pit / ice walls, **Cover** or
**Write onto Cover layer** for miasma). Turn on **Paint in Scene
view**. Uncheck the stamps you do not want so
a click only writes Kind, or only Cover. Empty Tiles cells are
already pits at Play (magenta “blank” glow). Stamp **Kind = Pit**
only when you painted a hole look that would otherwise stay floor.

### 1. Fire — fire stone frozen in ice

Ice has to *block*. Cover-Ice is walkable and answers Ice
(Water · Earth) in the weave — the same mark as an Ice inscription.
It does not stop you. Stamp walls for a cage.
Stamp a ring (or U) of **ice walls**, leave one floor cell in the
middle for the stone.

1. Select **Tiles** (or **Walls**).
2. Tile Properties: check **Kind** = `Wall`, **Material** = `Ice`.
   Uncheck Cover / Blocks.
3. Click the cells around the alcove. Leave the centre as floor.
4. Optional look: check **Cover** = `Ice`, **Write onto Cover
   layer**, and stamp the same cells (and the inner floor) so the
   ice reads as ice.
5. Drag `Assets/Prefabs/Items/Fire Stone` onto the inner cell.
6. Optional lock sprite: `GameObject → Rune Magic → Barrier` on
   that same cell. `authoredName` = `Ice cage`, formula `Water`
   and `Earth`, `spriteId` = `ice-block`. List **Cover Cells** as
   the ice-wall cells. Fireball / Melt / Ignite / a flame-pillar
   still melt ice walls even without the Barrier — the Barrier is
   a target you can click. Melted ice leaves a water covering on
   the floor that was under the wall. Fire will not run across
   that water.

The player needs a **Fire** mark in reach so they can write
Fireball. **Floor-Fire** / **Wall-Fire** are walk stamps, like
stone or dirt: hunger seated in the tile, at rest. The tileset
stays. They do not spread and they do not kindle. **Cover =
Fire** (Tile Properties, Write onto Cover layer, or the
Cover-Fire brush) is the live layer — it can catch and interact
once a spell starts work. The weave speaks Fire from either.
Click the cover mark to draw the rune. A fireball, a spreading
burn, or oil that a spell left will find the cover. Aura-Fire
still kindles a hall. Ice cover melts when hunger crosses it.
Oil or metal stamped on the Cover layer is the same: fuel or a
path for the spark, not a reaction that starts itself.
`GameObject → Rune Magic → Inscription` or **Pillar** still works
if you want a floating mark instead. Do not stamp fire on
Environment Details and do not expect a painted torch tile
to burn things. A torch is `GameObject → Rune Magic → Torch` — a
lock you light with a spell. It sits in the Hierarchy like the
stone, not on a tile layer. Floor and wall stamps sit at rest.
Covers are the live layer: they can catch, melt, and interact
once a spell starts work. Hunger only runs after a player or
NPC spell starts it, after a covering a spell left behind
(melt water, spell-fire on a bush), or when you paint the
**Aura-Fire** brush (a kindled hall). When a vegetable body
burns out, Play adds an ash covering, fire cover wears off, and a
plant or timber floor becomes dirt (look and Earth). Masonry
stays stone under the ash. A burned item ashes the cell under it.

### 2. Air — miasma, then the air stone

Miasma is not a wall. Walking in it throws you back to the last
safe floor. **Wind** (`Air · Mercury`) or Gale clears it.

1. Select **Tiles** so you can see the air room.
2. Tile Properties: uncheck Kind. On **Cover** (or **Write onto
   Cover layer**), pick the **Miasma** mark. Material =
   `Miasma` on that layer is the same stamp. The cell speaks
   Cloud · Acid · Miasma. Miasma is see-through by default
   (about 40%). Check **Opacity** and drag the slider if you
   want it thinner or more solid.
3. Click (or drag) the floor you want fouled — the path to the
   stone, not the doorway if you want them to step in and get
   thrown back. Paint any foggy tile on **Cover** first if you
   want your own art under the veil.
4. Drag `Assets/Prefabs/Items/Air Stone` onto the far side of the fog.
5. Optional: `GameObject → Rune Magic → Fog` on those cells if
   you want a named lock Wind can target. Painted Cover-Miasma
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
2. Drag `Assets/Prefabs/Items/Earth Stone` onto the far ledge.

Painted walls stay walls. Pillars and wall spells fill pits as
two-tile walkable spans. Standard earth and ice must find a floor
or a wall at each end, or the span falls. Metal hangs without a
far bank. Ice freezes water without banks; earth only muds it.

Plaques, altars, and teaching columns come after these three
play.

## Sprites on runes, items, and effects

You control the picture from the Inspector. Nothing here is a tile layer.

**Runes (Inscription / Pillar / Rune).**
Every catalog rune can be an inscription — roots, joins, and reserved names. `Window → Rune Magic → Inscriptions` (or Authoring → Inscriptions **Place**): click a mark, then click a tile in the Scene view. Right-click removes. The Inspector on an inscription is the same grid. With nothing else set, a **floating mark** is the whole picture — no slab, shaft, or base. Hover = Floor (lower) or Pillar (a little higher). Drag your art onto **Portrait** when you have a palette of your own. Or type a **Sprite Id**. In Play, click a floating mark to draw it into the Charter.

**Items, torches, plaques, barriers, fog, doors.**
Same pattern. Stones already have their id and sprite on the prefab. For a custom Item, drag a sprite onto **Portrait**, or set `spriteId` / `catalogId`. A Door has two portraits — closed and open.

**Tile covers (ice, fire, miasma, water after a melt).**
These are tiles, not objects. Floor and wall are at rest.
A cover is the live layer: look, work, and the same catalog
mark as an inscription. Ice is Water · Earth. **Fire cover**
can catch and interact once a spell starts hunger (click the
mark to draw Fire). Ice melts, oil fuels, metal conducts.
**Floor-Fire / Wall-Fire** are rest matter, like stone — they
do not spread on their own. Vine cover speaks Plant — Vine is
a climbing shot (`Plant · Mercury`), not a rune, and hunger
can run it as a wick. When hunger finishes the fuel, fire cover
wears off and ash covers the cell (Ash · Fire · Plant). A plant
or timber walk becomes dirt (look and Earth); masonry stays.
Click any spoken cover — fire, ice, ash — to draw that rune.
Miasma is Cloud · Acid, Fog is Cloud. A kindled hall is the
**Aura-Fire** brush.

1. Select **Cover** in the Hierarchy.
2. Paint any ice / fire / water / fog tile from any palette — that *is* the sheen.
3. Or stamp in Tile Properties: pick the **mark** (same chips as Inscriptions), **Write onto Cover layer**. Play draws that generated sign on the cell. Click the cover to draw the rune into the Charter. Painted sheen stays underneath when you supplied one (`cover-ice`, `tile-poison` for miasma).

**Spell leftovers** (wet floor after melt, hunger on a bush) draw `tile-wet` / `tile-fire` from the catalog. To change those globally, add or replace those ids in `Assets/Resources/Catalog` / the sprite sheets. A Cover tile you painted stays as the floor look; the wet/fire glow sits on top.

Do **not** make a Tilemap for interactables. Puzzle pieces are GameObjects: `GameObject → Rune Magic → …`. A tile cannot hold a formula, key list, or inventory id.

## Stones, the lock, and the door

These are prefabs. Drag them from `Assets/Prefabs` into the Scene, or press **Place** in `Window → Rune Magic → Authoring`. `GameObject → Rune Magic` instantiates the same files. Stones can live in any folder under Prefabs.

### 1. Orbs / stones

Drag one from `Assets/Prefabs` (they ship in `Items/`):

| Prefab | Catalog id |
|---|---|
| Fire Stone | `fire-stone` |
| Water Stone | `water-stone` |
| Earth Stone | `earth-stone` |
| Air Stone | `air-stone` |
| Body Stone | `body-stone` |
| Spirit Stone | `spirit-stone` |
| Mind Stone | `mind-stone` |
| Grove Stone | `grove-stone` |
| Flood Stone | `flood-stone` |
| Spark Stone | `spark-stone` |

Snap it to a floor cell. Walking onto it puts that stone in the pack. The blank **Item** prefab is only for a new catalog row.

### 2. The lock (Gate)

Drag `Assets/Prefabs/Gate`. Sit it in front of the door. This is the
**lock**, not the leaf — **Requires** lives here. Inspector:

- `authoredName` — what the adept reads
- `requires` — pack item ids (`fire-stone`, `water-stone`, …). Not objects you attach.
- `doors` — drag Door objects here
- `note` — flavour when the stones seat
- `finishes` — check only if this lock ends the floor
- **Hide Look** — on by default. No picture. Paint any tiles you want on the Tilemap; the Gate does not stamp a 2×2.
- **Portrait** — optional single sprite if you want a picture on the Gate itself. Uncheck Hide Look first.

Walk up to the Gate holding every required stone and it turns.

The default look is a generated painter. Same path as the Door: drag a
slice from your sheet onto **Portrait**. Scene view shows it immediately.

### 2b. The electric gate

Drag `Assets/Prefabs/Electric Gate` (or `GameObject → Rune Magic →
Electric Gate`). Same Doors list as a stone gate. A bolt, a spark
sentence, live-floor, or charge walking onto its cells opens those
doors. Sensor Cells are the tiles that take the spark — leave them
empty to use the gate’s own cell. Hide Look is on by default;
paint the look on the Tilemap. Uncheck it and set Portrait if you
want a picture on the lock itself.

### 3. The door (an object)

Drag `Assets/Prefabs/Door` onto the floor gap in the wall — not a Door stamp on the Tilemap. Inspector:

- **Start State** — `Closed` or `Open`
- **Closed Portrait** / **Closed Sprite Id** (`door`) — the shut leaf
- **Open Portrait** / **Open Sprite Id** (`door-open`) — the open way
- **Block Width / Height** — how many cells the leaf covers (1×1 is one cell; 3×1 seals a three-wide hall)

Drag your own closed and open sprites onto the two Portrait fields. The Scene gizmo is amber when shut, green when open. Toggle **Start State** to preview the other picture.

On the Gate or Electric Gate, drag that Door into **Doors**. When the lock turns, those objects open: collider drops, the open sprite shows, walking and shots go through.

If **Doors** and **Door Cells** are both empty, the lock opens any Door standing within about four tiles.

Tile `Kind = Door` still works (list those cells on the Gate as **Door Cells**). Prefer the Door object when you want your own open and closed art.

| Place | Old puzzle job | Inspector |
|---|---|---|
| **Fire Stone** … | orbs — drag `Assets/Prefabs/Items` | already set |
| **Mite** / **Enemy** | ice-thing, ash-mite, and the rest | formula, keys, sprite |
| **Torch** | cold torch | keys |
| **Rod** | storm rod | keys |
| **Gate** | Gate of Elements | `requires` item ids, **Doors** |
| **Electric Gate** | spark lock | lightning / charge opens **Doors** |
| **Door** | wooden leaf with open / closed states | start state, closed / open sprites |
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

Materials work if you stamp them after painting: select the layer, open `Window → Rune Magic → Tile Properties`, set Kind + Material, click the cells. **Kind = Floor** (or a Floor-Stone brush) is the only way a cell becomes walkable floor. Plant, timber, water, and fire stamps keep the tileset sprite they sit on — they do not swap in Floor-Plant / Floor-Timber / Floor-Fire pack art. **Floor-Fire** and **Wall-Fire** are rest matter, like stone: a fire source that will not spread until a player or NPC spell starts work. **Cover-Fire** is the live layer and can catch. Walls you never stamp are treated as **Wall / Stone** when they sit on a layer named Walls. Extra Floor / Tiles layers merge into the same walk grid — stamp Floor on each level you want to stand on.

**Environment Details** has its own stamp. Select that layer, stamp **Timber** on a table or **Plant** on a bush. A standing torch or painted fire does not catch those bushes — the room is at rest. A player or NPC spell that starts a fire can then run into Plant / Timber / Moss / Grove. When the fuel is spent the fire cover wears off, ash covers the cell, and a plant or timber floor becomes dirt (look and Earth). Stone floors do not catch; a burned table on stone leaves ash on the cobble. A tile named table / chair / bench / bush is guessed as Timber or Plant even if you never stamped it.

Collision is a separate stamp. Select **Environment Details**, check only **Blocks** in Tile Properties, and drag across a group of tables or statues. Those cells block walking. Tables, chairs, statues, crates, and pillars are guessed as blocking if you never stamped them; rugs and grass are not. A detail is never a floor unless you stamp **Kind = Floor** on that cell. When a blocking table burns, the ash covering stays, the walk becomes dirt if it was plant or timber, and you can walk over it. Cover still applies to that cell (ice, fire, vine, ash, miasma).

`GameObject → Rune Magic → Decor` is still look-only art. Burning or blocking furniture has to be an Environment Details **tile**.

The grid is **16×16** (16 PPU, one cell = one tile). ElvGames Tile
palettes also paint — Play keeps that sprite as a look. It will guess
wall / door / pit / bridge from the tile name, but it will **not**
guess floor. Stamp Kind = Floor (or paint a Floor brush) to walk.

| Folder | Brushes |
|---|---|
| `Assets/Tiles/Floor` | One floor per `MaterialId` (Stone, Dirt, Water, Ice…) |
| `Assets/Tiles/Wall` | One wall per material |
| `Assets/Tiles/Special` | Pit, Door, Bridge |
| `Assets/Tiles/Cover` | Ice / fire / lightning / vine / miasma / fog overlays |

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

One door sprite. Prefer `GameObject → Rune Magic → Door` for a leaf
you can open and close. A three-wide hall still wants stone jambs;
set the Door **Block Width** to 3, or drop one Door on the centre
cell. Tile `Kind = Door` is the leftover baked leaf.

## Coverings (element swaps)

Freeze / burn / charge / flood swap the covering, not the walk family.
Each spoken cover uses the same generated mark as that rune.

| Cover | Speaks | Sheen | Mark |
|---|---|---|---|
| `ice` | Ice · Water · Earth | Sanctuary — ice over stone | Ice |
| `fire` | Fire | Hell — lava / fire | Fire (mark only; Aura-Fire kindles) |
| `lightning` | Lightning · Spark · Air | Atlantis — charged seal | Lightning |
| `water` | Water | Cavern water tile | Water |
| `vine` | Plant · Water · Salt · Earth | Jungle vines | Plant |
| `ash` | Ash · Fire · Plant | Scorched rock over the tile | Ash |
| `miasma` | Miasma · Cloud · Acid | Poison veil | Miasma |
| `fog` | Cloud · Air · Water | Cloud veil | Cloud |
| `cracks` | — (look only) | Crypt cracks | — |
| `seal` | — (look only) | Atlantis seal | — |

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
