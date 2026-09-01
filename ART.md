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

A custom still or clip **overrides** a generated painter. One lookup,
used for conjured walls, bridges, pillars, leftovers, covers, and
shots. Floor / Wall **stamps** still keep the tileset you painted —
these ids are only for what a spell stands.

1. A **Look** (`Create → Rune Magic → Look`, or `Window → Rune Magic → Looks`). Drag Unity-sliced sprites onto **Frames**. One sprite is a still; several loop at **FPS**.
2. A **Sprite Sheet** under `Assets/Resources/SpriteSheets/` — same thing, or drag sprites onto a clip's **Sprites** array.
3. A row in [`Assets/Resources/Catalog/art.json`](Assets/Resources/Catalog/art.json), or a PNG at `Assets/Resources/Sprites/{id}.png`
4. A named slice in [`Assets/Resources/Catalog/tiles.json`](Assets/Resources/Catalog/tiles.json) (pack default)
5. The built-in painter

**In the Scene, prefer Unity sprites.** Slice the texture in the Sprite
Editor, then assign the slices. Drag a slice onto a Door or Gate
**Portrait** to skip the painter for that object. Particle prefabs go
in `Assets/Resources/Fx/` — see below.

## Looks (the Unity path)

`Window → Rune Magic → Looks` → pick an id → **Create Look asset**.
Open it, drag sprites onto **Frames**, set **FPS** if it should loop.

| Id | What Play draws |
|---|---|
| `wall` / `wall-ice` / `wall-timber` / `wall-plant` | Spell wall |
| `bridge` / `bridge-ice` | Spell span |
| `pillar` / `pillar-ice` | Earth-pillar / ice column |
| `floor-dirt` / `floor-stone` / `floor-water` | Leftover dirt, conjured floor |
| `pit` / `door` / `door-open` | Pit and baked door |
| `cover-ice` / `cover-fire` / `cover-vine` | Cover sheen (ice-shot, freeze) |
| `tile-fire` / `tile-wet` / `tile-charge` | Spell leftover glow |
| `fireball-shot` / `douse-shot` / `fx-fire` | Flying shot |

Material-specific ids win (`wall-ice` before `wall`). Timber walls stay
the tree painter until you assign `wall-timber`. Empty Frames means
“use the next fallback.”

`python3 Tools/import-sprite.py file.png --id wall-ice` still works —
that is the PNG / art.json path, same id.

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

Select the **Gate**, not the Door. **Hide Look** is on by default — the
lock draws nothing. Paint your tiles on the Tilemap (any number of
cells). The Gate does not take a 2×2 stamp.

To put one picture on the Gate itself, uncheck **Hide Look** and drag a
slice onto **Portrait**.

## Spells and effects

Casts are still generated painters + code particles until you replace
them. Same Look ids as walls and leftovers.

**1. Look or sheet (the flying body).**  
`Window → Rune Magic → Looks` → create `fireball-shot`, drag sprites.
Or `Create → Rune Magic → Sprite Sheet` under `Assets/Resources/SpriteSheets/`.
Name the clip after the spell or the element:

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
