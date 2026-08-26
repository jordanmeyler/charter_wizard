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
5. Assign gameplay after: `Window → Rune Magic → Tile Properties`.
   Select the layer you just painted (Tiles, Walls, or Environment
   Details), turn on **Paint in Scene view**, pick Kind / Material /
   Cover / Aura / Blocks, and click those cells. The sprite stays.
   Right-click a cell to copy its properties. Check **Write onto Cover
   layer** to stamp ice / fire / aura without changing the walk cell.
6. Select **Cover** and paint ice / fire / lightning / vine / aura over
   those cells if you would rather brush overlays than stamp them.
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
| **Tiles** / **Floor** | Walkable ground, water, pits, doors | Kind + material. Hidden, then baked. |
| **Walls** | Solid walls | Merged as walls. Hidden, then baked. |
| **Cover** / **Coverings** | Ice, fire, vine, aura | Overlay only. Hidden, then baked. |
| **Environment Details** / Decor | Plants, rugs, chairs, statues | Own stamp + optional collision. Hidden, then baked as a detail. |

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
| **Inscription** | floor teaching mark | one rune |
| **Pillar** | standing teaching mark | one rune |
| **Charm** | Free charm | — |
| **Crystal** | spawn / death return | — |
| **Flame Hall** | water-ward lesson at a kindled hall | — |
| **Decor** | look-only prop (not a lock) | sprite |

A layer named **Enviroment Details** (the typo) still counts as Environment Details.

Materials work if you stamp them after painting: select the layer, open `Window → Rune Magic → Tile Properties`, set Kind + Material, click the cells. Walls you never stamp are treated as **Wall / Stone** when they sit on a layer named Walls.

**Environment Details** has its own stamp. Select that layer, stamp **Timber** on a table or **Plant** on a bush. Fire turns that furniture into a pile of ash and leaves the floor — stone stays stone. The coals stay hot, so fire can run from the pile onto a neighboring Plant / Timber / Moss / Grove floor. Stone floors do not catch. A tile named table / chair / bench / bush is guessed as Timber or Plant even if you never stamped it.

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
