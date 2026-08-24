# Dungeon tiles

Named slices from [`Assets/Resources/Catalog/tiles.json`](Assets/Resources/Catalog/tiles.json) on [`Assets/Resources/Sprites/dungeon-atlas.png`](Assets/Resources/Sprites/dungeon-atlas.png). 32px cells. Cell `(0, 0)` is the top-left of the PNG. Rebuild the sheet with `python3 Tools/build-dungeon-atlas.py`.

Walking surfaces are **stone, dirt, or water**. Ice, fire, and lightning are not room floors. They sit as coverings, props, or FX and **swap** onto a base tile when an element interacts (freeze water → ice cover; burn moss → ash; charge metal → spark FX).

There is **one door**: `door` / `door-open`, 32×64, pivot `0.5,0.22` — the same feet as the adept. Halls stay three tiles wide so a closed gate still seals; only the centre leaf is the wooden door. The sides are stone jambs (`arch` / `arch-shut`).

Map stamps may set `"cover": "moss"` / `"seal"` / `"crack"` / `"blood"` / `"ice"` / `"vine"` on a stone or dirt cell. Live tile state (fire, wet, charge, miasma, growth) still paints `fx-*` overlays.

Decor props in a map:

```json
{ "type": "decor", "x": 4, "y": 8, "sprite": "brazier-lit", "blocking": true }
```

## Floors

| Id | Use |
| --- | --- |
| `floor-stone`, `floor-stone-b`, `floor-cracked` | Dungeon cobble |
| `floor-dirt`, `floor-dirt-b`, `floor-pebble` | Loose earth |
| `floor-water`, `floor-water-b` | Pool. Drowns until frozen |
| `floor-mud` | Dirt after water |
| `floor-ash` | What fire leaves of a covering |
| `pit`, `pit-edge` | Hollow and lip |

## Walls

| Id | Use |
| --- | --- |
| `wall`, `wall-b`, `wall-c`, `wall-crack` | Brick |
| `wall-corner-in`, `wall-corner-out` | Corners |
| `wall-moss` | Aged brick |
| `wall-cave`, `wall-cave-b` | Cave mouth |

## Door

| Id | Use |
| --- | --- |
| `door` | Closed wooden leaf in a stone arch |
| `door-open` | Same arch, leaf swung |
| `arch`, `arch-shut`, `arch-pillar` | Jambs / open stone |

## Coverings

| Id | Use |
| --- | --- |
| `cover-moss`, `cover-moss-b`, `cover-vine`, `cover-plant`, `cover-grove` | Green. Burns. Water grows plant |
| `cover-crack`, `cover-crack-b`, `cover-crack-c` | Wear |
| `cover-seal` | Runic slab |
| `cover-blood` | Splatter |
| `cover-ice` | After a freeze — not a painted room floor |
| `cover-metal` | Conductive plate on stone |

## Effects

| Id | Use |
| --- | --- |
| `fx-fire`, `fx-ember` | Hunger on a tile |
| `fx-poison` | Miasma / toxic gas |
| `fx-smoke`, `fx-smoke-b` | Smoke / fog |
| `fx-charge` | Spark walking the floor |
| `fx-wet` | Water on stone |
| `fx-grow` | Growth tick |
| `fx-ripple`, `fx-ripple-b` | Water motion |

## Props

| Id | Use |
| --- | --- |
| `pillar`, `pillar-broken`, `stalagmite`, `hook-statue` | Stone bodies |
| `torch-lit`, `torch-unlit`, `torch-empty`, `brazier-lit`, `brazier` | Light |
| `ice-fountain`, `ice-chest` | Ice objects. Fire or shatter ends them |
| `water-fountain` | Living yield |
| `lightning-vial`, `lightning-pillar`, `lightning-splash` | Spark objects. The live rod uses the pillar |
| `bush`, `bush-b` | Shrubs |
| `ring-mount` | Wall ring |

Aliases keep old ids working: `torch` → `torch-unlit`, `rod` / `rod-live` → `lightning-pillar`, `ice-block` → `ice-chest`, `tile-fire` → `fx-fire`, and the other `tile-*` FX names.
