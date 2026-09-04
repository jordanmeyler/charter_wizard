#!/usr/bin/env python3
"""Copy Rogue Adventure sheets into Resources and write tiles.json + enemy art.

16px tiles at 16 PPU fill one world cell. Regenerate with:
    python3 Tools/build-rogue-atlas.py
"""

from __future__ import annotations

import json
import shutil
import uuid
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
PACK = ROOT / "Assets" / "ElvGames" / "Rogue Adventure"
SPRITES = ROOT / "Assets" / "Resources" / "Sprites"
ROGUE = SPRITES / "Rogue"
ENEMIES = SPRITES / "Enemies"
TILES = ROOT / "Assets" / "Resources" / "Catalog" / "tiles.json"
ART = ROOT / "Assets" / "Resources" / "Catalog" / "art.json"

SHEETS = {
    "RA_Crypt": PACK / "Tilesets" / "Crypt" / "Sprites" / "RA_Crypt.png",
    "RA_Hell": PACK / "Tilesets" / "Hell" / "Sprites" / "RA_Hell.png",
    "RA_Cavern": PACK / "Tilesets" / "Caverns" / "Sprites" / "RA_Cavern.png",
    "RA_Sanctuary": PACK / "Tilesets" / "Sanctuary" / "Sprites" / "RA_Sanctuary.png",
    "RA_Jungle": PACK / "Tilesets" / "Jungle" / "Sprites" / "RA_Jungle.png",
    "RA_Atlantis": PACK / "Tilesets" / "Atlantis" / "Sprites" / "RA_Atlantis.png",
}

SIZES = {
    "RA_Crypt": (512, 256),
    "RA_Hell": (256, 256),
    "RA_Cavern": (512, 512),
    "RA_Sanctuary": (512, 352),
    "RA_Jungle": (512, 752),
    "RA_Atlantis": (768, 512),
}

ENEMY_NAMES = [
    ("enemy-001", "Shade"),
    ("enemy-002", "Squire"),
    ("enemy-003", "Crawler"),
    ("enemy-004", "Wisp"),
    ("enemy-005", "Brute"),
    ("enemy-006", "Imp"),
    ("enemy-007", "Skeleton"),
    ("enemy-008", "Cultist"),
    ("enemy-009", "Bat"),
    ("enemy-010", "Serpent"),
    ("enemy-011", "Golem"),
    ("enemy-012", "Warden"),
]


def src(name: str) -> str:
    return f"Sprites/Rogue/{name}"


def cell(sheet: str, cx: int, cy: int, w: int = 1, h: int = 1) -> dict:
    tw, th = SIZES[sheet]
    size = 16
    return {
        "source": src(sheet),
        "x": cx * size,
        "y": th - (cy + h) * size,
        "width": w * size,
        "height": h * size,
    }


def tile(id: str, sheet: str, cx: int, cy: int, kind: str, note: str, pivot="0.5,0.5", w=1, h=1) -> dict:
    row = {
        "id": id,
        "kind": kind,
        "note": note,
        "pivot": pivot,
        "col": 0,
        "row": 0,
        "w": w,
        "h": h,
    }
    row.update(cell(sheet, cx, cy, w, h))
    return row


def write_meta(path: Path, ppu: int = 16) -> None:
    meta = path.with_suffix(path.suffix + ".meta")
    guid = uuid.uuid4().hex
    meta.write_text(
        f"""fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 12
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 1
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMipmapLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 0
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 2
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spritePixelsToUnits: {ppu}
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  cookieLightType: 0
  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
    physicsShape: []
    bones: []
    spriteID: {guid[:16]}
    internalID: 0
    vertices: []
    indices: 
    edges: []
    weights: []
    secondaryTextures: []
    nameFileIdTable: {{}}
  mipmapLimitGroupName: 
  pSDRemoveMatte: 0
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""
    )


def copy_sheets() -> None:
    ROGUE.mkdir(parents=True, exist_ok=True)
    ENEMIES.mkdir(parents=True, exist_ok=True)
    for name, path in SHEETS.items():
        if not path.is_file():
            raise SystemExit(f"Missing {path}")
        dest = ROGUE / f"{name}.png"
        shutil.copy2(path, dest)
        write_meta(dest, 16)
        print(f"copied {dest.relative_to(ROOT)}")

    for i in range(1, 13):
        src_png = PACK / "Enemies" / f"Enemy_{i:03d}_A.png"
        if not src_png.is_file():
            raise SystemExit(f"Missing {src_png}")
        dest = ENEMIES / f"Enemy_{i:03d}.png"
        shutil.copy2(src_png, dest)
        write_meta(dest, 16)
        print(f"copied {dest.relative_to(ROOT)}")


def build_tiles() -> dict:
    tiles = [
        tile("floor-stone", "RA_Crypt", 2, 0, "floor", "Crypt cobble."),
        tile("floor-stone-b", "RA_Crypt", 5, 0, "floor", "Worn crypt cobble."),
        tile("floor-cracked", "RA_Crypt", 3, 8, "floor", "Cracked crypt stone."),
        tile("floor-dirt", "RA_Cavern", 4, 0, "floor", "Packed cave earth."),
        tile("floor-dirt-b", "RA_Cavern", 8, 0, "floor", "Rougher cave earth."),
        tile("floor-pebble", "RA_Cavern", 10, 1, "floor", "Pebble cave floor."),
        tile("floor-mud", "RA_Cavern", 16, 16, "floor", "Dark wet earth."),
        tile("floor-ash", "RA_Hell", 6, 5, "floor", "Scorched rock."),
        tile("floor-water", "RA_Cavern", 24, 0, "floor", "Cave pool."),
        tile("floor-water-b", "RA_Cavern", 24, 8, "floor", "Second pool frame."),
        tile("pit", "RA_Cavern", 1, 0, "floor", "Open pit."),
        tile("pit-edge", "RA_Cavern", 2, 0, "floor", "Pit rim."),
        tile("wall", "RA_Crypt", 1, 1, "wall", "Crypt brick."),
        tile("wall-b", "RA_Crypt", 2, 1, "wall", "Crypt brick variant."),
        tile("wall-c", "RA_Crypt", 1, 7, "wall", "Dark crypt brick."),
        tile("wall-crack", "RA_Crypt", 3, 1, "wall", "Cracked crypt wall."),
        tile("wall-moss", "RA_Jungle", 1, 33, "wall", "Mossy jungle stone."),
        tile("wall-ice", "RA_Sanctuary", 2, 1, "wall", "Ice wall. Same sanctuary face as ice-shot freeze."),
        tile("wall-cave", "RA_Cavern", 1, 1, "wall", "Cave wall."),
        tile("wall-cave-b", "RA_Cavern", 2, 2, "wall", "Broken cave wall."),
        tile("wall-fissure", "RA_Hell", 1, 2, "wall", "Volcanic fissure wall."),
        tile("wall-corner-in", "RA_Crypt", 4, 1, "wall", "Inner corner."),
        tile("wall-corner-out", "RA_Crypt", 6, 1, "wall", "Outer corner."),
        tile("arch", "RA_Crypt", 10, 2, "door", "Open crypt arch."),
        tile("door", "RA_Crypt", 8, 8, "door", "Wooden crypt door."),
        tile("door-open", "RA_Crypt", 10, 2, "door", "Open doorway."),
        tile("arch-shut", "RA_Crypt", 1, 1, "door", "Shut jamb — brick."),
        tile("arch-pillar", "RA_Sanctuary", 2, 1, "door", "Sanctuary pillar arch."),
        tile("bridge", "RA_Cavern", 8, 7, "floor", "Wood plank bridge."),
        tile("pillar", "RA_Sanctuary", 2, 0, "prop", "Stone pillar.", "0.5,0.2"),
        tile("pillar-broken", "RA_Crypt", 12, 12, "prop", "Broken stump.", "0.5,0.2"),
        tile("stalagmite", "RA_Cavern", 8, 4, "prop", "Rock spike.", "0.5,0.2"),
        tile("torch-lit", "RA_Sanctuary", 0, 21, "prop", "Lit sanctuary torch.", "0.5,0.2"),
        tile("torch-empty", "RA_Sanctuary", 2, 21, "prop", "Empty sconce.", "0.5,0.2"),
        tile("torch-unlit", "RA_Sanctuary", 1, 21, "prop", "Unlit torch.", "0.5,0.2"),
        tile("brazier-lit", "RA_Hell", 8, 13, "prop", "Lit brazier.", "0.5,0.2"),
        tile("brazier", "RA_Hell", 10, 13, "prop", "Cold brazier.", "0.5,0.2"),
        tile("ring-mount", "RA_Crypt", 14, 8, "prop", "Iron mount."),
        tile("cover-moss", "RA_Jungle", 0, 31, "cover", "Moss patch."),
        tile("cover-moss-b", "RA_Jungle", 2, 31, "cover", "Thicker moss."),
        tile("cover-vine", "RA_Jungle", 4, 33, "cover", "Vines."),
        tile("cover-plant", "RA_Jungle", 0, 36, "cover", "Leafy growth."),
        tile("cover-grove", "RA_Jungle", 1, 36, "cover", "Dense grove."),
        tile("cover-crack", "RA_Crypt", 3, 8, "cover", "Floor crack."),
        tile("cover-crack-b", "RA_Crypt", 4, 8, "cover", "Branching crack."),
        tile("cover-crack-c", "RA_Crypt", 5, 8, "cover", "Hairline crack."),
        tile("cover-seal", "RA_Atlantis", 16, 1, "cover", "Cyan floor seal."),
        tile("cover-blood", "RA_Crypt", 20, 8, "cover", "Blood splatter."),
        tile("cover-ice", "RA_Sanctuary", 0, 0, "cover", "Ice over stone. Same sanctuary ice as Floor-Ice / Cover-Ice."),
        tile("cover-ice-b", "RA_Sanctuary", 1, 0, "cover", "Ice variant."),
        tile("cover-metal", "RA_Atlantis", 18, 3, "cover", "Metal plate."),
        tile("cover-fire", "RA_Hell", 6, 5, "cover", "Lava / fire."),
        tile("cover-lightning", "RA_Atlantis", 20, 1, "cover", "Charged seal."),
        tile("cover-water", "RA_Cavern", 24, 0, "cover", "Water covering."),
        tile("bush", "RA_Jungle", 0, 32, "prop", "Shrub.", "0.5,0.2"),
        tile("bush-b", "RA_Jungle", 3, 32, "prop", "Denser shrub.", "0.5,0.2"),
        tile("fx-ripple", "RA_Cavern", 26, 0, "fx", "Water ripple."),
        tile("fx-ripple-b", "RA_Cavern", 26, 8, "fx", "Flowing water."),
        tile("fx-wet", "RA_Atlantis", 16, 7, "fx", "Wet sheen."),
        tile("fx-poison", "RA_Jungle", 0, 36, "fx", "Green vapor."),
        tile("fx-smoke", "RA_Hell", 8, 6, "fx", "Haze."),
        tile("fx-smoke-b", "RA_Hell", 10, 6, "fx", "Wider haze."),
        tile("fx-grow", "RA_Jungle", 2, 36, "fx", "Growth swirl."),
        tile("fx-fire", "RA_Hell", 8, 5, "fx", "Hunger / lava core."),
        tile("fx-ember", "RA_Hell", 10, 5, "fx", "Molten wash."),
        tile("fx-charge", "RA_Atlantis", 22, 2, "fx", "Spark on a tile."),
        tile("ice-fountain", "RA_Sanctuary", 0, 1, "prop", "Ice fountain.", "0.5,0.2"),
        tile("ice-chest", "RA_Sanctuary", 8, 7, "prop", "Iced chest.", "0.5,0.2"),
        tile("lightning-vial", "RA_Atlantis", 14, 8, "prop", "Sparking jar.", "0.5,0.2"),
        tile("lightning-pillar", "RA_Atlantis", 12, 3, "prop", "Charged column.", "0.5,0.2"),
        tile("lightning-splash", "RA_Atlantis", 22, 5, "prop", "Spark splash."),
        tile("hook-statue", "RA_Crypt", 18, 12, "prop", "Hooked stone.", "0.5,0.2"),
        tile("water-fountain", "RA_Atlantis", 4, 3, "prop", "Water altar.", "0.5,0.2"),
        tile("bookshelf", "RA_Crypt", 16, 12, "prop", "Tomes.", "0.5,0.2"),
        tile("bookshelf-tall", "RA_Crypt", 18, 11, "prop", "Tall shelf.", "0.5,0.15", w=1, h=2),
        tile("bench", "RA_Crypt", 14, 12, "prop", "Stone bench.", "0.5,0.2"),
        tile("chair", "RA_Crypt", 15, 12, "prop", "Seat.", "0.5,0.2"),
        tile("statue", "RA_Sanctuary", 4, 2, "prop", "Standing figure.", "0.5,0.2"),
        tile("statue-hood", "RA_Crypt", 20, 12, "prop", "Hooded statue.", "0.5,0.2"),
        tile("statue-head", "RA_Hell", 4, 12, "prop", "Beast skull.", "0.5,0.2"),
        tile("statue-seated", "RA_Sanctuary", 6, 2, "prop", "Seated figure.", "0.5,0.2"),
    ]
    aliases = [
        {"id": "torch", "tile": "torch-unlit"},
        {"id": "rod", "tile": "lightning-pillar"},
        {"id": "rod-live", "tile": "lightning-pillar"},
        {"id": "lightning-rod", "tile": "lightning-pillar"},
        {"id": "pillar-ice", "tile": "wall-ice"},
        {"id": "ice-block", "tile": "ice-chest"},
        {"id": "ice-thing", "tile": "ice-fountain"},
        {"id": "ice-vessel", "tile": "ice-chest"},
        {"id": "tile-fire", "tile": "fx-fire"},
        {"id": "tile-poison", "tile": "fx-poison"},
        {"id": "tile-fog", "tile": "fx-smoke-b"},
        {"id": "tile-charge", "tile": "fx-charge"},
        {"id": "tile-wet", "tile": "fx-wet"},
        {"id": "tile-grow", "tile": "fx-grow"},
        {"id": "cover-cracks", "tile": "cover-crack"},
        {"id": "cover-lava", "tile": "cover-fire"},
        {"id": "water-ripple", "tile": "fx-ripple"},
        {"id": "bush-bloom", "tile": "bush-b"},
        {"id": "statue-gold", "tile": "statue-head"},
        {"id": "urn", "tile": "chair"},
        {"id": "crate", "tile": "bench"},
        {"id": "table", "tile": "bench"},
        {"id": "floor", "tile": "floor-stone"},
        {"id": "floor-hearth", "tile": "floor-stone"},
        {"id": "floor-ice", "tile": "floor-stone"},
        {"id": "floor-vein", "tile": "floor-stone"},
        {"id": "floor-crystal", "tile": "floor-stone"},
        {"id": "floor-ember", "tile": "floor-dirt"},
    ]
    return {
        "note": "16px Rogue Adventure slices. Unity y from the bottom. Floors are stone, dirt, or water.",
        "source": src("RA_Crypt"),
        "cell": 16,
        "pixelsPerUnit": 16,
        "tiles": tiles,
        "aliases": aliases,
    }


def merge_art() -> None:
    art = {"note": "", "sprites": [], "items": []}
    if ART.is_file():
        art = json.loads(ART.read_text())
        art.setdefault("sprites", [])
        art.setdefault("items", [])

    kept = []
    for row in art["sprites"]:
        ident = (row.get("id") or "").lower()
        if ident.startswith("enemy-"):
            continue
        kept.append(row)

    for i, (ident, name) in enumerate(ENEMY_NAMES, start=1):
        kept.append(
            {
                "id": ident,
                "source": f"Sprites/Enemies/Enemy_{i:03d}",
                "x": 0,
                "y": 128,
                "width": 32,
                "height": 32,
                "frames": 6,
                "fps": 8,
                "pixelsPerUnit": 16,
                "pivot": "0.5,0.18",
                "note": f"{name} idle from Rogue Adventure Enemy_{i:03d}.",
            }
        )

    art["note"] = (
        "Custom sprites and items. Rogue Adventure enemies are enemy-001 … enemy-012. "
        "Drop Assets/Resources/Sprites/{id}.png or add a sprites[] row. See ART.md."
    )
    art["sprites"] = kept
    ART.write_text(json.dumps(art, indent=2) + "\n")
    print(f"wrote {ART.relative_to(ROOT)} ({len(kept)} sprites)")


def main() -> None:
    copy_sheets()
    data = build_tiles()
    TILES.write_text(json.dumps(data, indent=2) + "\n")
    print(f"wrote {TILES.relative_to(ROOT)} ({len(data['tiles'])} tiles)")
    merge_art()


if __name__ == "__main__":
    main()
