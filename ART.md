# Art — how to get past the generated sprites

The dungeon tiles are 16×16 Rogue Adventure slices at 16 PPU. Actors
still fall back to generated painters if you have not dropped a PNG.
Point-filter, snap the camera to 16 PPU. The adept uses a Unity
Animator on Rogue Adventure **Hero_22** (`Idle` / `Walk` / `Cast` /
`Hop`), cropped to **16×16** so one pose is one tile. If the
controller looks empty, open **Window → Rune Magic → Adept Animator**
and click **Build / repair clips**. Then click each state and set
Motion to the matching clip if it says None. Generated painters stay
as a fallback for locks and for the player if that controller is
missing.

A custom still **overrides** a generated clip. The game looks up art in this order:

1. A row in [`Assets/Resources/Catalog/art.json`](Assets/Resources/Catalog/art.json) (`source` PNG or painted `cells`)
2. A named slice in [`Assets/Resources/Catalog/tiles.json`](Assets/Resources/Catalog/tiles.json) (your sprite sheets)
3. A file at `Assets/Resources/Sprites/{id}.png`
4. The built-in painter

**In the Scene, prefer Unity sprites.** Drag a slice onto a Door or Gate
**Portrait**. That skips the painter for that object. Spells look up
clip ids (`fireball-shot`, `fireball`, `fire-shot`, `fx-fire`) on a
`Sprite Sheet` under `Assets/Resources/SpriteSheets/`, or a PNG with
that filename. Particle prefabs go in `Assets/Resources/Fx/` — see
below.

## What to drop in

| Id | Role | Suggested size | Pivot |
| --- | --- | --- | --- |
| `adept` Animator | Hero_22 clips in `Assets/Animations/Adept` | 16×16 | `0.5,0.18` |
| `ash-mite`, `ice-thing`, `fire-golem`, `stone-man`, `warden` | Locks | 48–64 | `0.5,0.32` |
| `fire-golem-slam`, `warden-cast` | Attack frames | same as the actor | same |
| `torch`, `torch-lit`, `rod`, `rod-live`, `charm` | Props | 32–48 | `0.5,0.5` |
| `arrow-shot`, `fireball-shot` | Projectiles | 16–32 | `0.5,0.5` |
| `{spell}-shot`, `{family}-shot`, `fx-{family}` | Player spell body | 16–32 | `0.5,0.5` |
| `stone-fire` … `key-spark` | Pack items | 32 | `0.5,0.5` |
| Gate **Portrait** | Socket lock | any | `0.5,0.5` |

Floors, walls, the door, and dungeon props come from Rogue Adventure (`TileAtlas` / `tiles.json`). Walking surfaces are **stone, dirt, or water**. Ice, fire, and lightning are coverings. Pack enemies (`enemy-001` … `enemy-012`) drop from **GameObject → Rune Magic → Enemies**. Named ids and rects are in [`TILES.md`](TILES.md).

## Ways to get the pictures

**1. Paint them (best control).** [Aseprite](https://www.aseprite.org/), LibreSprite, or the catalog editor for tiny icons. Top-down, no face on the adept — robe and a withheld cowl glow. Keep a short palette (16–32 colours) so they sit on the generated floors.

**2. Buy or borrow a pack, then restyle.** CC0 / paid pixel packs (Kenney, itch.io “top down dungeon”, Lost Garden) get you tiles and bodies fast. Recolor toward the violet robe / warm stone / cold ice already in the game so rooms do not look like three asset stores.

**3. Generate, then clean.** Image models can draft 64px actors if you ask for “top-down pixel art, 64×64, limited palette, no anti-alias, solid outline.” Treat the output as a sketch: snap to a palette, fix the silhouette, throw away mushy frames. Do not ship raw generations as the player sprite.

**4. Commission a small hero set.** One pass for the adept (idle/walk/cast/hop), four enemies, and the shot sprites is enough to judge whether the game can look finished. Tiles can wait.

The catalog pixel grid is for charms and keys. It is the wrong tool for the adept.

## How to install a PNG

```bash
python3 Tools/import-sprite.py ~/art/adept.png --id adept --ppu 16 --pivot 0.5,0.22
```

That copies the file to `Assets/Resources/Sprites/adept.png` and registers `source` in `art.json`. Or drop the PNG in that folder yourself — the id is the filename. Unity must import it as a texture (default is fine; play mode reads it as `Texture2D` and applies point filtering).

In [`Tools/catalog-editor.html`](Tools/catalog-editor.html) you can still paint a still, or import a small PNG into the grid. For anything you care about, use the Resources folder.

## Gate look (Unity Inspector)

Select the **Gate**, not the Door. Drag a slice onto **Portrait**. The
generated gold socket, glow, and floating name stay off while that
field is set. Check **Hide Look** to make the lock invisible — it still
opens the Doors when the pack has the required stones.

## Spells and effects

Casts are still generated painters + code particles until you replace
them. Two Unity paths:

**1. Sprite / sheet (the flying body).**  
`Window → Rune Magic → Sprite Sheet`, or `Create → Rune Magic → Sprite
Sheet`. Save under `Assets/Resources/SpriteSheets/`. Name a clip after
the spell or the element:

| Id | When it is used |
|---|---|
| `fireball-shot` | Fireball, and other Fire shots if no spell-specific clip exists |
| `fireball` | same, second try |
| `fire-shot` | any Fire-family shot |
| `fx-fire` | last Fire fallback |
| `douse-shot`, `water-shot`, `fx-water` | Water, same pattern |
| `arrow-shot` | enemy / volley arrows (already wired) |

A PNG at `Assets/Resources/Sprites/fireball-shot.png` is the same id.

**2. Particle prefab (the trail / burst).**  
Create a Particle System in Unity. Save it as a prefab under
`Assets/Resources/Fx/`:

- `Fx/FireBurst`, `Fx/FireStream`, `Fx/FireLinger`
- or one `Fx/Fire` used for all three

Family names: Fire, Flame, Water, Ice, Earth, Air, Lightning, Spark,
Fog, Poison, Plant, Dark, Light, Steam, Lava. If the prefab is missing,
the generated particles still play.

Recipes (what a spell *does*) stay in
[`Assets/Resources/Catalog/spells.json`](Assets/Resources/Catalog/spells.json)
or `Tools/catalog-editor.html`. That file is not the picture.

## Recommendation

Do not spend more time trying to make the procedural painters “good enough.” Keep them as a fallback so the game always boots. Replace the adept and the four living locks first; if those read, generate or commission the rest against the same palette.
