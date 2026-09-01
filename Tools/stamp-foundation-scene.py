#!/usr/bin/env python3
"""Paint Floor 1 into Assets/Scenes/Main.unity and drop placeable objects."""

from __future__ import annotations

import json
from collections import defaultdict
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SCENE = ROOT / "Assets" / "Scenes" / "Main.unity"
FOUNDATION = ROOT / "Assets" / "Resources" / "Maps" / "foundation.json"
TILES = ROOT / "Assets" / "Tiles"

ENEMY_GUID = "61bf0193c5968b0fe905c2e42b5e7f7c"
ITEM_GUID = "b5a4b106858840aa9dc12dda44dd9bf2"
DECOR_GUID = "2b8d0f4a6c184e7b9d1e3f5a7c9b2345"
PLAQUE_GUID = "813811c7afd64d4f83d7773870e8043f"
CRYSTAL_GUID = "072a4e1835f6ad94e270c1b38405e6f7"
TORCH_GUID = "7efda3cc1e3945d0a3c074e14475aefc"
GATE_GUID = "b96f9601482f4e9ca74087f8f18e20b7"
CHARGE_GATE_GUID = "e7a1c3f509284b6d8e2f4a6b8c0d1e2f"


def load_guids() -> dict[str, str]:
    guids = {}
    for path in TILES.rglob("*.asset.meta"):
        name = path.name.replace(".asset.meta", "")
        for line in path.read_text().splitlines():
            if line.startswith("guid:"):
                guids[name] = line.split(":", 1)[1].strip()
                break
    return guids


def brush(kind: str, material: str, guids: dict[str, str]) -> str | None:
    kind = (kind or "Floor").title()
    material = (material or "Stone").replace(" ", "")
    if kind == "Wall":
        name = f"Wall-{material}"
    elif kind == "Door":
        name = "Door"
    elif kind == "Pit" or material == "Void":
        name = "Pit"
    elif kind == "Bridge":
        name = "Bridge"
    else:
        name = f"Floor-{material}"
    if name not in guids:
        name = "Floor-Stone" if kind != "Wall" else "Wall-Stone"
    return guids.get(name)


def overlay_name(aura: str, cover: str) -> str | None:
    cover_key = (cover or "").strip().lower()
    aura_key = (aura or "").strip().lower()
    covers = {
        "ice": "Cover-Ice",
        "fire": "Cover-Fire",
        "lightning": "Cover-Lightning",
        "water": "Cover-Water",
        "vine": "Cover-Vine",
        "cracks": "Cover-Cracks",
        "crack": "Cover-Cracks",
        "seal": "Cover-Seal",
        "miasma": "Cover-Miasma",
        "poison": "Cover-Miasma",
        "fog": "Cover-Fog",
    }
    auras = {
        "fire": "Aura-Fire",
        "miasma": "Aura-Miasma",
        "poison": "Aura-Miasma",
        "fog": "Aura-Fog",
    }
    if cover_key:
        return covers.get(cover_key)
    return auras.get(aura_key)


def find_room(rooms, ident):
    for room in rooms:
        if room.get("id") == ident:
            return room
    return None


def paint_exit(cells, origin, width, height, exit_dir, door_guid):
    mid_x = origin[0] + width // 2
    mid_y = origin[1] + height // 2
    ox, oy = origin
    if exit_dir == "west":
        coords = [(ox, mid_y - 1), (ox, mid_y), (ox, mid_y + 1)]
    elif exit_dir == "north":
        coords = [(mid_x - 1, oy + height - 1), (mid_x, oy + height - 1), (mid_x + 1, oy + height - 1)]
    elif exit_dir == "south":
        coords = [(mid_x - 1, oy), (mid_x, oy), (mid_x + 1, oy)]
    else:
        coords = [(ox + width - 1, mid_y - 1), (ox + width - 1, mid_y), (ox + width - 1, mid_y + 1)]
    for x, y in coords:
        cells[(x, y)] = door_guid


def paint_room(room, cells, overlays, guids):
    origin = (room["origin"]["x"], room["origin"]["y"])
    width = max(3, int(room.get("width", 13)))
    height = max(3, int(room.get("height", 11)))
    wall = brush("Wall", room.get("wall", "Stone"), guids)
    floor = brush("Floor", room.get("floor", "Stone"), guids)
    for y in range(height):
        for x in range(width):
            edge = x == 0 or y == 0 or x == width - 1 or y == height - 1
            cells[(origin[0] + x, origin[1] + y)] = wall if edge else floor
    exit_dir = (room.get("exit") or "none").lower()
    if exit_dir not in ("", "none"):
        paint_exit(cells, origin, width, height, exit_dir, brush("Door", room.get("wall", "Stone"), guids))
    for stamp in room.get("stamps") or []:
        kind = stamp.get("kind", "Floor")
        material = stamp.get("material", "Stone")
        guid = brush(kind, material, guids)
        cover = overlay_name(stamp.get("aura", ""), stamp.get("cover", ""))
        coords = stamp.get("cells") or []
        for i in range(0, len(coords) - 1, 2):
            pos = (origin[0] + coords[i], origin[1] + coords[i + 1])
            cells[pos] = guid
            if cover and cover in guids:
                overlays[pos] = guids[cover]


def paint_hall(data, hall, cells, overlays, floor_guid, wall_guid, fire_guid):
    frm = find_room(data["rooms"], hall.get("from"))
    to = find_room(data["rooms"], hall.get("to"))
    if not frm or not to:
        return
    fx, fy = frm["origin"]["x"], frm["origin"]["y"]
    tx, ty = to["origin"]["x"], to["origin"]["y"]
    fw, fh = frm["width"], frm["height"]
    tw, th = to["width"], to["height"]
    kindle = (hall.get("hazard") or "").lower() == "fire"
    half = 1
    if tx > fx + fw - 1:
        y0 = max(fy + 1, ty + 1)
        y1 = min(fy + fh - 2, ty + th - 2)
        mid = (y0 + y1) // 2 if y0 <= y1 else ty + th // 2
        stamp_hall(cells, overlays, fx + fw, tx - 1, mid, half, True, floor_guid, wall_guid, fire_guid if kindle else None)
        for d in range(-1, 2):
            cells[(fx + fw - 1, mid + d)] = floor_guid
            cells[(tx, mid + d)] = floor_guid
        return
    if ty > fy + fh - 1:
        x0 = max(fx + 1, tx + 1)
        x1 = min(fx + fw - 2, tx + tw - 2)
        mid = (x0 + x1) // 2 if x0 <= x1 else tx + tw // 2
        stamp_hall(cells, overlays, fy + fh, ty - 1, mid, half, False, floor_guid, wall_guid, fire_guid if kindle else None)
        for d in range(-1, 2):
            cells[(mid + d, fy + fh - 1)] = floor_guid
            cells[(mid + d, ty)] = floor_guid


def stamp_hall(cells, overlays, gap0, gap1, mid, half, east_west, floor_guid, wall_guid, fire_guid):
    if gap0 > gap1:
        return
    for along in range(gap0, gap1 + 1):
        for side in range(-half - 1, half + 2):
            x = along if east_west else mid + side
            y = mid + side if east_west else along
            if abs(side) <= half:
                cells[(x, y)] = floor_guid
                if fire_guid:
                    overlays[(x, y)] = fire_guid
            elif (x, y) not in cells:
                cells[(x, y)] = wall_guid


def emit_tilemap(cells: dict, origin_line_indent: str = "  ") -> tuple[str, list[str]]:
    if not cells:
        return "  m_Tiles: []\n", []
    used = []
    index = {}
    for guid in cells.values():
        if guid not in index:
            index[guid] = len(used)
            used.append(guid)
    tiles = []
    for (x, y), guid in sorted(cells.items()):
        idx = index[guid]
        tiles.append(
            f"""  - first: {{x: {x}, y: {y}, z: 0}}
    second:
      serializedVersion: 2
      m_TileIndex: {idx}
      m_TileSpriteIndex: {idx}
      m_TileMatrixIndex: 0
      m_TileColorIndex: 0
      m_TileObjectToInstantiateIndex: 65535
      dummyAlignment: 0
      m_AllTileFlags: 1073741825"""
        )
    assets = "\n".join(
        f"""  - m_RefCount: {sum(1 for g in cells.values() if g == guid)}
    m_Data: {{fileID: 11400000, guid: {guid}, type: 2}}"""
        for guid in used
    )
    sprites = "\n".join(
        f"""  - m_RefCount: {sum(1 for g in cells.values() if g == guid)}
    m_Data: {{fileID: 0}}"""
        for guid in used
    )
    min_x = min(x for x, _ in cells)
    min_y = min(y for _, y in cells)
    max_x = max(x for x, _ in cells)
    max_y = max(y for _, y in cells)
    body = (
        "  m_Tiles:\n"
        + "\n".join(tiles)
        + "\n  m_AnimatedTiles: {}\n"
        + "  m_TileAssetArray:\n"
        + assets
        + "\n  m_TileSpriteArray:\n"
        + sprites
        + """
  m_TileMatrixArray:
  - m_RefCount: """
        + str(len(cells))
        + """
    m_Data:
      e00: 1
      e01: 0
      e02: 0
      e03: 0
      e10: 0
      e11: 1
      e12: 0
      e13: 0
      e20: 0
      e21: 0
      e22: 1
      e23: 0
      e30: 0
      e31: 0
      e32: 0
      e33: 1
  m_TileColorArray:
  - m_RefCount: """
        + str(len(cells))
        + """
    m_Data: {r: 1, g: 1, b: 1, a: 1}
  m_TileObjectToInstantiateArray: []
"""
    )
    return body, [str(min_x), str(min_y), str(max_x - min_x + 1), str(max_y - min_y + 1)]


ENEMY_MAP = {
    "ash-mite": ("Shade", "shade", "enemy-001", ["Fire", "Salt", "Life"], "", 0),
    "ice-thing": ("Wisp", "wisp", "enemy-004", ["Air", "Salt", "Life"], "", 0),
    "fire-golem": ("Golem", "golem", "enemy-011", ["Earth", "Salt", "Fire"], "golem", 1),
    "stone-man": ("Squire", "squire", "enemy-002", ["Earth", "Salt", "Life"], "golem", 1),
    "warden": ("Warden", "warden", "enemy-012", ["Fire", "Sulphur", "Mercury"], "wizard", 1),
    "spirit-warden": ("Cultist", "cultist", "enemy-012", ["Fire", "Sulphur", "Life"], "wizard", 1),
}


def match_enemy(prop):
    key = (prop.get("formulaId") or prop.get("sprite") or prop.get("displayName") or "").lower()
    for ident, spec in ENEMY_MAP.items():
        if ident in key:
            return spec
    return ENEMY_MAP["ash-mite"]


def yaml_list(values) -> str:
    if not values:
        return "[]"
    return "\n" + "\n".join(f"  - {v}" for v in values)


def emit_objects(data, next_id: int) -> tuple[str, list[int]]:
    chunks = []
    child_ids = []
    spawn = data.get("spawn") or {"x": 39, "y": 26}

    def add_object(name, script_guid, pos, fields: str):
        nonlocal next_id
        go, tr, sr, mb = next_id, next_id + 1, next_id + 2, next_id + 3
        next_id += 4
        child_ids.append(tr)
        chunks.append(
            f"""--- !u!1 &{go}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: {tr}}}
  - component: {{fileID: {sr}}}
  - component: {{fileID: {mb}}}
  m_Layer: 0
  m_Name: {name}
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!4 &{tr}
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go}}}
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: {pos[0] + 0.5}, y: {pos[1] + 0.5}, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: 210000002}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
--- !u!212 &{sr}
SpriteRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go}}}
  m_Enabled: 1
  m_CastShadows: 0
  m_ReceiveShadows: 0
  m_DynamicOccludee: 1
  m_StaticShadowCaster: 0
  m_MotionVectors: 1
  m_LightProbeUsage: 1
  m_ReflectionProbeUsage: 1
  m_RayTracingMode: 0
  m_RayTraceProcedural: 0
  m_RenderingLayerMask: 1
  m_RendererPriority: 0
  m_Materials:
  - {{fileID: 10754, guid: 0000000000000000f000000000000000, type: 0}}
  m_StaticBatchInfo:
    firstSubMesh: 0
    subMeshCount: 0
  m_StaticBatchRoot: {{fileID: 0}}
  m_ProbeAnchor: {{fileID: 0}}
  m_LightProbeVolumeOverride: {{fileID: 0}}
  m_ScaleInLightmap: 1
  m_ReceiveGI: 1
  m_PreserveUVs: 0
  m_IgnoreNormalsForChartDetection: 0
  m_ImportantGI: 0
  m_StitchLightmapSeams: 1
  m_SelectedEditorRenderState: 0
  m_MinimumChartSize: 4
  m_AutoUVMaxDistance: 0.5
  m_AutoUVMaxAngle: 89
  m_LightmapParameters: {{fileID: 0}}
  m_SortingLayerID: 0
  m_SortingLayer: 0
  m_SortingOrder: 5
  m_Sprite: {{fileID: 0}}
  m_Color: {{r: 1, g: 1, b: 1, a: 1}}
  m_FlipX: 0
  m_FlipY: 0
  m_DrawMode: 0
  m_Size: {{x: 1, y: 1}}
  m_AdaptiveModeThreshold: 0.5
  m_SpriteTileMode: 0
  m_WasSpriteAssigned: 0
  m_MaskInteraction: 0
  m_SpriteSortPoint: 0
--- !u!114 &{mb}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {go}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {script_guid}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
{fields}"""
        )

    extras = [
        ((spawn["x"] + 2, spawn["y"]), ENEMY_MAP["ash-mite"]),
        ((spawn["x"] + 3, spawn["y"] + 1), ENEMY_MAP["stone-man"]),
        ((spawn["x"] + 1, spawn["y"] - 1), ENEMY_MAP["ice-thing"]),
    ]
    for pos, spec in extras:
        name, ident, sprite, formula, attack, blocking = spec
        fields = (
            f"  authoredName: {name}\n"
            f"  authoredId: {ident}\n"
            f"  spriteId: {sprite}\n"
            f"  portrait: {{fileID: 0}}\n"
            f"  idleFrames: []\n"
            f"  resolveFrames: []\n"
            f"  resolveClip: \n"
            f"  formula:{yaml_list(formula)}\n"
            f"  keys: []\n"
            f"  authoredEnsouled: 0\n"
            f"  authoredBlocking: {blocking}\n"
            f"  grant: \n"
            f"  attack: {attack}\n"
            f"  authoredCastSeconds: 2\n"
            f"  cast: []\n"
        )
        add_object(name, ENEMY_GUID, pos, fields)

    for room in data.get("rooms") or []:
        origin = (room["origin"]["x"], room["origin"]["y"])
        for prop in room.get("props") or []:
            typ = (prop.get("type") or "").lower()
            pos = (origin[0] + int(prop.get("x", 0)), origin[1] + int(prop.get("y", 0)))
            if typ in ("mite", "lock"):
                name, ident, sprite, formula, attack, blocking = match_enemy(prop)
                fields = (
                    f"  authoredName: {prop.get('displayName') or name}\n"
                    f"  authoredId: {ident}\n"
                    f"  spriteId: {sprite}\n"
                    f"  portrait: {{fileID: 0}}\n"
                    f"  idleFrames: []\n"
                    f"  resolveFrames: []\n"
                    f"  resolveClip: \n"
                    f"  formula:{yaml_list(prop.get('formula') or formula)}\n"
                    f"  keys: []\n"
                    f"  authoredEnsouled: {1 if prop.get('ensouled') else 0}\n"
                    f"  authoredBlocking: {blocking}\n"
                    f"  grant: {prop.get('grant') or ''}\n"
                    f"  attack: {prop.get('attack') or attack}\n"
                    f"  authoredCastSeconds: {prop.get('castSeconds') or 2}\n"
                    f"  cast: []\n"
                )
                add_object(prop.get("displayName") or name, ENEMY_GUID, pos, fields)
            elif typ == "item":
                fields = (
                    f"  catalogId: {prop.get('item') or ''}\n"
                    f"  displayName: \n"
                    f"  spriteId: {prop.get('sprite') or ''}\n"
                    f"  portrait: {{fileID: 0}}\n"
                    f"  idleFrames: []\n"
                    f"  changeFrames: []\n"
                    f"  changeClip: \n"
                    f"  changeFps: 10\n"
                    f"  material: 0\n"
                    f"  matter: \n"
                    f"  fragile: 0\n"
                    f"  keys: []\n"
                    f"  teachesSpell: \n"
                    f"  note: \n"
                    f"  look: \n"
                )
                add_object(prop.get("item") or "Item", ITEM_GUID, pos, fields)
            elif typ in ("decor", "pillar", "stele"):
                fields = (
                    f"  spriteId: {prop.get('sprite') or 'pillar'}\n"
                    f"  portrait: {{fileID: 0}}\n"
                    f"  blocking: {1 if prop.get('blocking') else 0}\n"
                    f"  look: {prop.get('note') or ''}\n"
                )
                add_object("Decor", DECOR_GUID, pos, fields)
            elif typ in ("plaque", "inscription") and prop.get("text"):
                fields = (
                    f"  text: {json.dumps(prop.get('text'))}\n"
                    f"  spriteId: plaque\n"
                    f"  portrait: {{fileID: 0}}\n"
                )
                add_object("Plaque", PLAQUE_GUID, pos, fields)
            elif typ == "torch":
                fields = (
                    "  authoredName: Cold Torch\n"
                    "  authoredId: cold-torch\n"
                    "  authoredSprite: torch\n"
                    "  authoredSpriteLit: torch-lit\n"
                    "  portrait: {fileID: 0}\n"
                    "  idleFrames: []\n"
                    "  litFrames: []\n"
                    "  keys: []\n"
                )
                add_object("Torch", TORCH_GUID, pos, fields)
            elif typ == "gate":
                req = prop.get("requires") or []
                fields = (
                    f"  authoredName: {prop.get('displayName') or 'Gate'}\n"
                    f"  authoredId: gate\n"
                    f"  requires:{yaml_list(req)}\n"
                    f"  finishes: {1 if prop.get('finishes') else 0}\n"
                    f"  note: {prop.get('note') or ''}\n"
                    f"  spriteId: socket-gate\n"
                    f"  portrait: {{fileID: 0}}\n"
                    f"  idleFrames: []\n"
                    f"  doorCells: []\n"
                )
                add_object(prop.get("displayName") or "Gate", GATE_GUID, pos, fields)
            elif typ in ("charge-gate", "electric-gate"):
                fields = (
                    f"  authoredName: {prop.get('displayName') or 'Electric Gate'}\n"
                    f"  authoredId: electric-gate\n"
                    f"  finishes: {1 if prop.get('finishes') else 0}\n"
                    f"  note: {prop.get('note') or ''}\n"
                    f"  keys: []\n"
                    f"  spriteId: rod\n"
                    f"  spriteLit: rod-live\n"
                    f"  portrait: {{fileID: 0}}\n"
                    f"  idleFrames: []\n"
                    f"  liveFrames: []\n"
                    f"  hideLook: 1\n"
                    f"  doorCells: []\n"
                    f"  sensorCells: []\n"
                )
                add_object(prop.get("displayName") or "Electric Gate", CHARGE_GATE_GUID, pos, fields)

    fields = (
        "  spriteId: spawn-crystal\n"
        "  portrait: {fileID: 0}\n"
        "  idleFrames: []\n"
    )
    add_object("Crystal", CRYSTAL_GUID, (spawn["x"], spawn["y"]), fields)
    return "\n".join(chunks) + "\n", child_ids


def replace_tilemap_block(text: str, component_id: str, body: str, size: list[str]) -> str:
    marker = f"--- !u!1839735485 &{component_id}"
    start = text.find(marker)
    if start < 0:
        raise SystemExit(f"missing tilemap {component_id}")
    next_start = text.find("\n--- !u!", start + len(marker))
    block = text[start:next_start if next_start >= 0 else None]
    # keep header through m_Enabled / m_GameObject, replace from m_Tiles
    head_end = block.find("  m_Tiles:")
    if head_end < 0:
        raise SystemExit("tilemap missing m_Tiles")
    head = block[:head_end]
    tail_start = block.find("  m_AnimationFrameRate:")
    if tail_start < 0:
        raise SystemExit("tilemap missing animation frame rate")
    tail = block[tail_start:]
    tail = tail.replace("  m_Origin: {x: 0, y: 0, z: 0}", f"  m_Origin: {{x: {size[0]}, y: {size[1]}, z: 0}}")
    tail = tail.replace("  m_Size: {x: 0, y: 0, z: 1}", f"  m_Size: {{x: {size[2]}, y: {size[3]}, z: 1}}")
    new_block = head + body + tail
    if next_start >= 0:
        return text[:start] + new_block + text[next_start:]
    return text[:start] + new_block


def main() -> None:
    data = json.loads(FOUNDATION.read_text())
    guids = load_guids()
    cells = {}
    overlays = {}
    for room in data.get("rooms") or []:
        paint_room(room, cells, overlays, guids)
    floor_guid = brush("Floor", "Stone", guids)
    wall_guid = brush("Wall", "Stone", guids)
    fire_guid = guids.get("Aura-Fire") or guids.get("Cover-Fire")
    for hall in data.get("halls") or []:
        paint_hall(data, hall, cells, overlays, floor_guid, wall_guid, fire_guid)

    floor_body, floor_size = emit_tilemap(cells)
    cover_body, cover_size = emit_tilemap(overlays)
    objects, child_ids = emit_objects(data, 310000001)
    text = SCENE.read_text()
    text = replace_tilemap_block(text, "210000013", floor_body, floor_size)
    text = replace_tilemap_block(text, "210000023", cover_body, cover_size)

    spawn = data.get("spawn") or {"x": 39, "y": 26}
    text = text.replace(
        "  m_LocalPosition: {x: 2.5, y: 5.5, z: 0}",
        f"  m_LocalPosition: {{x: {spawn['x'] + 0.5}, y: {spawn['y'] + 0.5}, z: 0}}",
    )

    children = "\n".join(
        ["  - {fileID: 210000012}", "  - {fileID: 210000022}", "  - {fileID: 210000032}"]
        + [f"  - {{fileID: {cid}}}" for cid in child_ids]
    )
    old = """  m_Children:
  - {fileID: 210000012}
  - {fileID: 210000022}
  - {fileID: 210000032}"""
    text = text.replace(old, "  m_Children:\n" + children)

    # insert objects before SceneRoots
    roots = text.find("--- !u!1660057539 &9223372036854775807")
    if roots < 0:
        raise SystemExit("missing scene roots")
    text = text[:roots] + objects + text[roots:]
    SCENE.write_text(text)
    print(f"stamped {len(cells)} walk cells, {len(overlays)} overlays, {len(child_ids)} objects")


if __name__ == "__main__":
    main()
