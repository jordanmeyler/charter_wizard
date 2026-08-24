# Tiles

Maps are Unity Tilemaps. You paint them in the Scene view. Play bakes
each `WorldPaintTile` into a live `WorldTile`.

## Authoring

1. Open `Assets/Scenes/Main.unity`. The scene already has **Map** (a
   Grid), **Tiles**, **Cover**, and **Spawn**.
2. `Window → 2D → Tile Palette` → Open Palette → **Rune Palette**.
   If the palette is missing, `Window → Rune Magic → Create Tile Palette`.
3. Select the **Tiles** object and paint walk cells (stone floor, water,
   walls, pits, doors).
4. Select **Cover** and paint ice / fire / lightning / vine / aura over
   those cells. Overlay brushes do not replace the walk family.
5. Click a tile asset in `Assets/Tiles` to set **material**, **kind**,
   **cover**, and **aura**. Duplicate an asset to make a new brush.
   `Create → Rune Magic → Map Tile` also works.
6. Place objects with `GameObject → Rune Magic → Item / Decor / Mite / Torch…`
   and set catalog id, material, formula, or sprite on the Inspector.

Play hides the editor Tilemap renderers and builds the live grid from
what you painted. JSON under `Assets/Resources/Maps/` is leftover and
is not loaded unless Level Authoring is set to **Named Map**.

| Folder | Brushes |
|---|---|
| `Assets/Tiles/Floor` | One floor per `MaterialId` (Stone, Dirt, Water, Ice…) |
| `Assets/Tiles/Wall` | One wall per material |
| `Assets/Tiles/Special` | Pit, Door, Bridge |
| `Assets/Tiles/Cover` | Ice / fire / lightning / vine overlays and fire / miasma / fog auras |

The live dungeon art is the sprite sheets under
`Assets/Resources/Sprites/`. `tiles.json` names every slice the game
uses. `TileAtlas` cuts those pixels at runtime (Unity's y-axis starts
at the bottom of the PNG). Leave a tile's Sprite blank to use the atlas
slice for that material.

The old painted `dungeon-atlas.png` is unused now.

## Sheets

| File | What it is |
|---|---|
| `pixellab-A-modular-top-down-pixel-art-d-1787590789217.png` | Floors, walls, the wooden door, torches, braziers, pillars, vines, cracks, bushes, water, miasma |
| `pixellab-fantastic-can-we-just-add-ice--1787592603251.png` | Ice overlays, ice props, lightning vial / rod, dirt variants, fire / lava FX |
| `pixellab-the-furniture-should-be-ancien-1787593737338.png` | Ancient furniture, statues, shelves, water altar |
| `sprite_sheet_env_1.png` | Extra env mix (not required by the current map) |

Each tile is about 32–39px (the sheets are 256×256, not a clean 32 grid).
Rects in `tiles.json` are Unity texture space: `x`, `y` from the
**bottom-left**.

## Floors (walk on these)

Only three walkable families. Ice / fire / lightning are never the
floor itself.

| Id | Sheet | What it looks like |
|---|---|---|
| `floor-stone` | modular `(3,209)` | Dungeon cobble |
| `floor-cracked` | modular `(45,209)` | Worn cobble |
| `floor-dirt` | modular `(87,209)` | Packed earth |
| `floor-water` | modular `(215,209)` | Water pool |
| `pit` | modular `(172,209)` | Open pit |
| `pit-edge` | modular `(130,209)` | Pit rim |

Aliases: `floor`, `floor-hearth`, `floor-ice`, `floor-vein`,
`floor-crystal`, `floor-ember` all resolve to stone or dirt so old map
stamps keep working. The actual ice/fire/lightning look is a **cover**.

## Walls and the door

| Id | Sheet | Notes |
|---|---|---|
| `wall` | modular `(3,163)` | Solid dungeon wall |
| `wall-moss` | modular `(130,163)` | Mossy outer wall |
| `wall-cave` | modular `(172,163)` | Cave wall |
| `wall-fissure` | modular `(215,163)` | Cracked wall |
| `door` / `arch-shut` | modular `(45,119)` | **The** wooden door |
| `door-open` | modular `(3,119)` | Open arch (no leaf) |
| `arch-pillar` | modular `(87,119)` | Stone pillar arch |
| `pillar` | modular `(160,75)` | Stone pillar |
| `pillar-broken` | modular `(192,75)` | Broken stump |
| `stalagmite` | modular `(224,75)` | Rock cluster |

One door sprite. The exit is still three cells wide so a closed gate
seals the hall; only the **center** cell draws the wooden leaf. The
two jambs draw stone wall.

## Coverings (element swaps)

Freeze / burn / charge / flood swap the covering, not the walk family.

| Cover | Sprite | Source |
|---|---|---|
| `ice` | `cover-ice` | ice sheet `(3,209)` — ice over stone |
| `fire` | `cover-fire` | ice sheet `(160,0)` — lava / fire over dirt |
| `lightning` | `cover-lightning` | modular `(192,39)` — gold seal overlay |
| `water` | `cover-water` | modular water tile |
| `vine` | `cover-vine` | modular `(64,41)` |
| `cracks` | `cover-cracks` | modular `(95,39)` |
| `seal` | `cover-seal` | modular `(192,39)` |

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
(`torch`, `brazier`, `pillar`…). The JSON stamp form is leftover.

## Adding a tile later

1. Open the sheet in any image tool. Note the pixel rect. Remember
   Unity `y` is measured from the **bottom**.
2. Add a row to `tiles.json`:

```json
{ "id": "my-tile", "source": "Sprites/the-sheet", "x": 45, "y": 119, "width": 39, "height": 37 }
```

3. Use `"sprite": "my-tile"` on a decor stamp, or `TileAtlas.Get("my-tile")`.
