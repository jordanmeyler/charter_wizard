#!/usr/bin/env python3
"""Point Rune Magic tile brushes at already-sliced ElvGames sprites.

Also fills the Tile Palette sprite cache and replaces the invisible
Foundation stamp in Main.unity with a 13×11 room at the origin so the
Scene view shows tiles immediately.
"""

from __future__ import annotations

import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
ELV = ROOT / "Assets" / "ElvGames" / "Rogue Adventure" / "Tilesets"
TILES = ROOT / "Assets" / "Tiles"
SCENE = ROOT / "Assets" / "Scenes" / "Main.unity"
PALETTE = TILES / "Palettes" / "Rune Palette.prefab"

# WorldPaintTile name → ElvGames tile asset (already has the correct sprite ref).
BIND = {
    "Floor-Stone": "Crypt/Tiles/RA_Crypt_1.asset",
    "Floor-Dirt": "Caverns/Tiles/RA_Cavern_4.asset",
    "Floor-Water": "Caverns/Tiles/RA_Cavern_24.asset",
    "Floor-Mud": "Caverns/Tiles/RA_Cavern_16.asset",
    "Floor-Ash": "Hell/Tiles/RA_Hell_20.asset",
    "Floor-Ice": "Sanctuary/Tiles/RA_Sanctuary_0.asset",
    "Floor-Lava": "Hell/Tiles/RA_Hell_21.asset",
    "Floor-Moss": "Jungle/Tiles/RA_Jungle_0.asset",
    "Floor-Plant": "Jungle/Tiles/RA_Jungle_1.asset",
    "Floor-Grove": "Jungle/Tiles/RA_Jungle_2.asset",
    "Wall-Stone": "Crypt/Tiles/RA_Crypt_31.asset",
    "Wall-Moss": "Jungle/Tiles/RA_Jungle_10.asset",
    "Wall-Ice": "Sanctuary/Tiles/RA_Sanctuary_16.asset",
    "Wall-Lava": "Hell/Tiles/RA_Hell_8.asset",
    "Door": "Crypt/Tiles/RA_Crypt_80.asset",
    "Pit": "Caverns/Tiles/RA_Cavern_1.asset",
    "Bridge": "Caverns/Tiles/RA_Cavern_40.asset",
    "Cover-Ice": "Sanctuary/Tiles/RA_Sanctuary_0.asset",
    "Cover-Fire": "Hell/Tiles/RA_Hell_21.asset",
    "Cover-Water": "Caverns/Tiles/RA_Cavern_24.asset",
    "Cover-Vine": "Jungle/Tiles/RA_Jungle_5.asset",
    "Cover-Lightning": "Atlantis/Tiles/RA_Atlantis_20.asset",
    "Cover-Seal": "Atlantis/Tiles/RA_Atlantis_16.asset",
    "Cover-Cracks": "Crypt/Tiles/RA_Crypt_50.asset",
    "Aura-Fire": "Hell/Tiles/RA_Hell_22.asset",
    "Aura-Miasma": "Jungle/Tiles/RA_Jungle_3.asset",
    "Aura-Fog": "Hell/Tiles/RA_Hell_10.asset",
}

SPRITE_RE = re.compile(r"m_Sprite: \{fileID: ([-\d]+), guid: ([0-9a-f]+), type: 3\}")
GUID_RE = re.compile(r"^guid: ([0-9a-f]+)$", re.M)

FLOOR_GUID = "00dbb2a295950db4ba2dd7a66d236c6d"
WALL_GUID = "34b12542e58fdd948a4f36bd9a823ab1"
ROOM_W = 13
ROOM_H = 11


def sprite_ref(rel: str) -> str | None:
    path = ELV / rel
    if not path.is_file():
        folder = path.parent
        stem = path.stem.rsplit("_", 1)[0]
        if folder.is_dir():
            matches = sorted(folder.glob(stem + "_*.asset"))
            if matches:
                path = matches[0]
            else:
                return None
        else:
            return None
    found = SPRITE_RE.search(path.read_text())
    return found.group(0) if found else None


def bind_asset(asset: Path, ref: str) -> bool:
    text = asset.read_text()
    if "m_Sprite:" not in text:
        return False
    new = re.sub(r"m_Sprite: \{fileID: [^}]+\}", ref, text, count=1)
    if new == text:
        return False
    asset.write_text(new)
    return True


def bind_brushes() -> dict[str, str]:
    refs: dict[str, str] = {}
    for name, rel in BIND.items():
        ref = sprite_ref(rel)
        if ref:
            refs[name] = ref
        else:
            print(f"missing pack tile {rel}")

    stone = refs.get("Floor-Stone")
    wall = refs.get("Wall-Stone")
    changed = 0
    for asset in TILES.rglob("*.asset"):
        name = asset.stem
        ref = refs.get(name)
        if ref is None:
            if name.startswith("Wall-"):
                ref = wall
            elif name.startswith(("Floor-", "Cover-", "Aura-")):
                ref = stone
        if ref is None:
            continue
        if bind_asset(asset, ref):
            changed += 1
            print(f"bound {asset.relative_to(ROOT)}", flush=True)
    print(f"updated {changed} tile brushes", flush=True)
    return refs


def guid_to_sprite() -> dict[str, str]:
    mapping: dict[str, str] = {}
    for asset in TILES.rglob("*.asset"):
        meta = asset.with_suffix(".asset.meta")
        if not meta.is_file():
            continue
        guid_m = GUID_RE.search(meta.read_text())
        sprite_m = SPRITE_RE.search(asset.read_text())
        if guid_m and sprite_m:
            mapping[guid_m.group(1)] = sprite_m.group(0)
    return mapping


def sprite_data_line(ref: str) -> str:
    inner = ref.split("m_Sprite: ", 1)[1]
    return f"    m_Data: {inner}"


def update_palette(guid_sprites: dict[str, str]) -> None:
    text = PALETTE.read_text()
    start = text.find("  m_TileAssetArray:\n")
    end = text.find("  m_TileSpriteArray:", start)
    matrix = text.find("  m_TileMatrixArray:", end)
    if start < 0 or end < 0 or matrix < 0:
        raise SystemExit("palette tile arrays not found")
    asset_block = text[start:end]
    guids = re.findall(r"guid: ([0-9a-f]+)", asset_block)
    lines = ["  m_TileSpriteArray:"]
    missing = 0
    for guid in guids:
        ref = guid_sprites.get(guid)
        if ref is None:
            missing += 1
            lines.append("  - m_RefCount: 1")
            lines.append("    m_Data: {fileID: 0}")
        else:
            lines.append("  - m_RefCount: 1")
            lines.append(sprite_data_line(ref))
    array = "\n".join(lines) + "\n"
    text = text[:end] + array + text[matrix:]
    text = re.sub(
        r"m_TileIndex: (\d+)\n      m_TileSpriteIndex: 4294967295",
        r"m_TileIndex: \1\n      m_TileSpriteIndex: \1",
        text,
    )
    PALETTE.write_text(text)
    print(f"palette sprites: {len(guids) - missing}/{len(guids)}")


def tile_cell(x: int, y: int, index: int) -> str:
    return (
        f"  - first: {{x: {x}, y: {y}, z: 0}}\n"
        f"    second:\n"
        f"      serializedVersion: 2\n"
        f"      m_TileIndex: {index}\n"
        f"      m_TileSpriteIndex: {index}\n"
        f"      m_TileMatrixIndex: 0\n"
        f"      m_TileColorIndex: 0\n"
        f"      m_TileObjectToInstantiateIndex: 65535\n"
        f"      dummyAlignment: 0\n"
        f"      m_AllTileFlags: 1073741825\n"
    )


def identity_matrix() -> str:
    return (
        "    e00: 1\n"
        "    e01: 0\n"
        "    e02: 0\n"
        "    e03: 0\n"
        "    e10: 0\n"
        "    e11: 1\n"
        "    e12: 0\n"
        "    e13: 0\n"
        "    e20: 0\n"
        "    e21: 0\n"
        "    e22: 1\n"
        "    e23: 0\n"
        "    e30: 0\n"
        "    e31: 0\n"
        "    e32: 0\n"
        "    e33: 1\n"
    )


def starter_tiles_block(floor_ref: str, wall_ref: str) -> str:
    cells: list[str] = []
    floors = walls = 0
    for y in range(ROOM_H):
        for x in range(ROOM_W):
            edge = x == 0 or y == 0 or x == ROOM_W - 1 or y == ROOM_H - 1
            if edge:
                walls += 1
                cells.append(tile_cell(x, y, 1))
            else:
                floors += 1
                cells.append(tile_cell(x, y, 0))
    total = floors + walls
    return (
        "  m_Tiles:\n"
        + "".join(cells)
        + "  m_AnimatedTiles: {}\n"
        + "  m_TileAssetArray:\n"
        + f"  - m_RefCount: {floors}\n"
        + f"    m_Data: {{fileID: 11400000, guid: {FLOOR_GUID}, type: 2}}\n"
        + f"  - m_RefCount: {walls}\n"
        + f"    m_Data: {{fileID: 11400000, guid: {WALL_GUID}, type: 2}}\n"
        + "  m_TileSpriteArray:\n"
        + f"  - m_RefCount: {floors}\n"
        + sprite_data_line(floor_ref)
        + "\n"
        + f"  - m_RefCount: {walls}\n"
        + sprite_data_line(wall_ref)
        + "\n"
        + "  m_TileMatrixArray:\n"
        + f"  - m_RefCount: {total}\n"
        + "    m_Data:\n"
        + identity_matrix().replace("    e", "      e")
        + "  m_TileColorArray:\n"
        + f"  - m_RefCount: {total}\n"
        + "    m_Data: {r: 1, g: 1, b: 1, a: 1}\n"
        + "  m_TileObjectToInstantiateArray: []\n"
        + "  m_AnimationFrameRate: 1\n"
        + "  m_Color: {r: 1, g: 1, b: 1, a: 1}\n"
        + f"  m_Origin: {{x: 0, y: 0, z: 0}}\n"
        + f"  m_Size: {{x: {ROOM_W}, y: {ROOM_H}, z: 1}}\n"
        + "  m_TileAnchor: {x: 0.5, y: 0.5, z: 0}\n"
        + "  m_TileOrientation: 0\n"
        + "  m_TileOrientationMatrix:\n"
        + identity_matrix()
    )


def empty_cover_block() -> str:
    return (
        "  m_Tiles: []\n"
        "  m_AnimatedTiles: {}\n"
        "  m_TileAssetArray: []\n"
        "  m_TileSpriteArray: []\n"
        "  m_TileMatrixArray: []\n"
        "  m_TileColorArray: []\n"
        "  m_TileObjectToInstantiateArray: []\n"
        "  m_AnimationFrameRate: 1\n"
        "  m_Color: {r: 1, g: 1, b: 1, a: 1}\n"
        "  m_Origin: {x: 0, y: 0, z: 0}\n"
        "  m_Size: {x: 0, y: 0, z: 1}\n"
        "  m_TileAnchor: {x: 0.5, y: 0.5, z: 0}\n"
        "  m_TileOrientation: 0\n"
        "  m_TileOrientationMatrix:\n"
        + identity_matrix()
    )


def replace_tilemap_body(scene: str, file_id: int, body: str) -> str:
    header = f"--- !u!1839735485 &{file_id}\n"
    start = scene.find(header)
    if start < 0:
        raise SystemExit(f"tilemap {file_id} not found")
    tiles_at = scene.find("\n  m_Tiles:", start)
    next_doc = scene.find("\n--- ", tiles_at)
    if tiles_at < 0 or next_doc < 0:
        raise SystemExit(f"tilemap {file_id} body not found")
    return scene[: tiles_at + 1] + body + scene[next_doc + 1 :]


def drop_stamped_objects(scene: str) -> str:
    parts = re.split(r"(?m)^--- ", scene)
    header = parts[0]
    kept = []
    dropped = 0
    for part in parts[1:]:
        first = part.split("\n", 1)[0]
        found = re.search(r"&(\d+)", first)
        file_id = int(found.group(1)) if found else 0
        if 310000000 <= file_id < 400000000:
            dropped += 1
            continue
        kept.append("--- " + part)
    print(f"removed {dropped} stamped scene objects")
    return header + "".join(kept)


def replace_children(scene: str) -> str:
    marker = "--- !u!4 &210000002\n"
    start = scene.find(marker)
    kids = scene.find("  m_Children:\n", start)
    father = scene.find("  m_Father:", kids)
    if start < 0 or kids < 0 or father < 0:
        raise SystemExit("Map children not found")
    return (
        scene[:kids]
        + "  m_Children:\n"
        + "  - {fileID: 210000012}\n"
        + "  - {fileID: 210000022}\n"
        + "  - {fileID: 210000032}\n"
        + scene[father:]
    )


def replace_local_position(scene: str, file_id: int, xyz: str) -> str:
    marker = f"--- !u!4 &{file_id}\n"
    start = scene.find(marker)
    pos = scene.find("  m_LocalPosition:", start)
    end = scene.find("\n", pos)
    if start < 0 or pos < 0:
        raise SystemExit(f"transform {file_id} not found")
    return scene[:pos] + f"  m_LocalPosition: {xyz}" + scene[end:]


def reset_scene(floor_ref: str, wall_ref: str) -> None:
    print("rewriting Main.unity…", flush=True)
    scene = SCENE.read_text()
    scene = drop_stamped_objects(scene)
    scene = replace_tilemap_body(scene, 210000013, starter_tiles_block(floor_ref, wall_ref))
    scene = replace_tilemap_body(scene, 210000023, empty_cover_block())
    scene = replace_children(scene)
    scene = replace_local_position(scene, 963194228, "{x: 6.5, y: 5.5, z: -10}")
    scene = replace_local_position(scene, 210000032, "{x: 2.5, y: 5.5, z: 0}")
    SCENE.write_text(scene)
    print(f"wrote starter {ROOM_W}x{ROOM_H} room in {SCENE.relative_to(ROOT)}", flush=True)


def main() -> None:
    refs = bind_brushes()
    floor = refs.get("Floor-Stone")
    wall = refs.get("Wall-Stone")
    if not floor or not wall:
        raise SystemExit("need Floor-Stone and Wall-Stone pack sprites")
    update_palette(guid_to_sprite())
    reset_scene(floor, wall)


if __name__ == "__main__":
    main()
