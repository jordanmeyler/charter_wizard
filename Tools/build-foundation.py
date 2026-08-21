#!/usr/bin/env python3
"""Stamp Floor 1 — The Foundation — as Maps/foundation.json."""

from __future__ import annotations

import json
from collections import deque
from pathlib import Path

FIRE = ["Fireball", "FlamePillar", "Melt", "Ignite", "SunLance", "Scald"]
ICE = FIRE + ["Thaw"]
WATER = ["Douse", "WaterJet", "Rain", "Flood", "Smother"]
GOLEM = WATER + ["Gale", "Quagmire", "Wall", "Pit", "Bridge", "IcePillar"]
PITS = [
    "EarthPillar",
    "Wall",
    "Bridge",
    "FlamePillar",
    "IcePillar",
    "VineRise",
    "Hop",
    "Flight",
    "HurledStone",
]
ARROWS = ["EarthPillar", "Wall", "StonePillar", "FlamePillar", "IcePillar", "VineRise", "Menhir", "Bridge"]
WIND = ["Gust", "Gale", "StormCall", "Flight"]
MIND = ["Rage", "Lull", "Terror", "Command", "Jolt"]
ATTACK = [
    "Fireball",
    "Douse",
    "WaterJet",
    "HurledStone",
    "LightningBolt",
    "IceSpear",
    "Gale",
    "Gust",
    "SunLance",
    "Scald",
]


def cells(*pairs):
    out = []
    for x, y in pairs:
        out.extend([int(x), int(y)])
    return out


def rect(x0, y0, x1, y1):
    out = []
    for y in range(y0, y1 + 1):
        for x in range(x0, x1 + 1):
            out.extend([x, y])
    return out


def stamp(kind, material, coords):
    return {"kind": kind, "material": material, "cells": coords}


def room(**kwargs):
    spec = {
        "exit": "none",
        "stamps": [],
        "props": [],
    }
    spec.update(kwargs)
    return spec


def box(r):
    return (
        r["origin"]["x"],
        r["origin"]["y"],
        r["width"],
        r["height"],
    )


def validate(data):
    boxes = []
    for r in data["rooms"]:
        packed = (
            r["id"],
            r["origin"]["x"],
            r["origin"]["y"],
            r["origin"]["x"] + r["width"] - 1,
            r["origin"]["y"] + r["height"] - 1,
        )
        boxes.append(packed)
    for i, a in enumerate(boxes):
        for b in boxes[i + 1 :]:
            if a[1] <= b[3] and b[1] <= a[3] and a[2] <= b[4] and b[2] <= a[4]:
                raise SystemExit(f"overlap {a[0]} vs {b[0]}")
    ids = {r["id"] for r in data["rooms"]}
    for hall in data["halls"]:
        if hall["from"] not in ids or hall["to"] not in ids:
            raise SystemExit(f"bad hall {hall}")
        src = next(r for r in data["rooms"] if r["id"] == hall["from"])
        dst = next(r for r in data["rooms"] if r["id"] == hall["to"])
        east = dst["origin"]["x"] > src["origin"]["x"] + src["width"] - 1
        north = dst["origin"]["y"] > src["origin"]["y"] + src["height"] - 1
        if not east and not north:
            raise SystemExit(f"hall {hall['from']}→{hall['to']} is not east/north of a gap")
    spawn = data["spawn"]
    hub = next(r for r in data["rooms"] if r["id"] == "hub")
    if not (
        hub["origin"]["x"] < spawn["x"] < hub["origin"]["x"] + hub["width"] - 1
        and hub["origin"]["y"] < spawn["y"] < hub["origin"]["y"] + hub["height"] - 1
    ):
        raise SystemExit("spawn is not inside the hub")
    assert_walkable(data)


def world(room_id, rooms, lx, ly):
    spec = next(r for r in rooms if r["id"] == room_id)
    return spec["origin"]["x"] + lx, spec["origin"]["y"] + ly


def assert_walkable(data):
    grid = simulate(data)
    rooms = data["rooms"]
    start = (data["spawn"]["x"], data["spawn"]["y"])
    closed = flood(grid, start, doors_open=False)
    open_reach = flood(grid, start, doors_open=True)

    def must(reach, name, xy):
        if xy not in reach:
            raise SystemExit(f"cannot walk to {name} at {xy}")

    must(closed, "fire altar", world("fire-wing", rooms, 8, 7))
    must(closed, "fire cage face", world("fire-wing", rooms, 5, 7))
    must(closed, "water altar", world("water-wing", rooms, 3, 7))
    must(closed, "water curtain face", world("water-wing", rooms, 8, 7))
    must(closed, "earth altar", world("earth-wing", rooms, 7, 13))
    must(closed, "earth stone approach", world("earth-wing", rooms, 7, 3))
    must(closed, "air altar", world("air-wing", rooms, 7, 12))
    must(closed, "air fog lip", world("air-wing", rooms, 7, 9))
    must(closed, "door I", world("hub", rooms, 23, 13))

    foyer = world("aspect-foyer", rooms, 15, 6)
    if foyer in closed:
        raise SystemExit("Door I is already open — the hub north wall should stay shut")

    must(open_reach, "aspect foyer", foyer)
    must(open_reach, "body altar", world("body-sanctum", rooms, 12, 6))
    must(open_reach, "body pit lip", world("body-sanctum", rooms, 8, 6))
    must(open_reach, "spirit altar", world("spirit-sanctum", rooms, 3, 3))
    must(open_reach, "warden approach", world("spirit-sanctum", rooms, 6, 6))
    must(open_reach, "mind altar", world("mind-sanctum", rooms, 3, 6))
    must(open_reach, "mind aisle mouth", world("mind-sanctum", rooms, 6, 6))
    must(open_reach, "door II", world("door-ii", rooms, 9, 7))


def simulate(data):
    max_x = max(r["origin"]["x"] + r["width"] + 2 for r in data["rooms"])
    max_y = max(r["origin"]["y"] + r["height"] + 2 for r in data["rooms"])
    grid = {}
    by_id = {r["id"]: r for r in data["rooms"]}
    for spec in data["rooms"]:
        ox, oy, w, h = box(spec)
        for y in range(oy, oy + h):
            for x in range(ox, ox + w):
                edge = x in (ox, ox + w - 1) or y in (oy, oy + h - 1)
                grid[(x, y)] = "Wall" if edge else "Floor"
        for mark in spec.get("stamps") or []:
            kind = mark["kind"]
            cells_xy = mark["cells"]
            for i in range(0, len(cells_xy), 2):
                grid[(ox + cells_xy[i], oy + cells_xy[i + 1])] = kind
        exit_dir = (spec.get("exit") or "none").lower()
        if exit_dir != "none":
            mx, my = ox + w // 2, oy + h // 2
            if exit_dir == "north":
                spots = ((mx - 1, oy + h - 1), (mx, oy + h - 1), (mx + 1, oy + h - 1))
            elif exit_dir == "south":
                spots = ((mx - 1, oy), (mx, oy), (mx + 1, oy))
            elif exit_dir == "west":
                spots = ((ox, my - 1), (ox, my), (ox, my + 1))
            else:
                spots = ((ox + w - 1, my - 1), (ox + w - 1, my), (ox + w - 1, my + 1))
            for spot in spots:
                grid[spot] = "Door"
    for hall in data["halls"]:
        connect(grid, by_id[hall["from"]], by_id[hall["to"]])
    return grid


def connect(grid, src, dst):
    fx, fy, fw, fh = box(src)
    tx, ty, tw, th = box(dst)
    half = 1
    if tx > fx + fw - 1:
        y0 = max(fy + 1, ty + 1)
        y1 = min(fy + fh - 2, ty + th - 2)
        mid = (y0 + y1) // 2 if y0 <= y1 else ty + th // 2
        stamp_hall(grid, fx + fw, tx - 1, mid, half, True)
        for dy in range(-half, half + 1):
            open_passage(grid, fx + fw - 1, mid + dy)
            open_passage(grid, tx, mid + dy)
        return
    if ty > fy + fh - 1:
        x0 = max(fx + 1, tx + 1)
        x1 = min(fx + fw - 2, tx + tw - 2)
        mid = (x0 + x1) // 2 if x0 <= x1 else tx + tw // 2
        stamp_hall(grid, fy + fh, ty - 1, mid, half, False)
        for dx in range(-half, half + 1):
            open_passage(grid, mid + dx, fy + fh - 1)
            open_passage(grid, mid + dx, ty)


def stamp_hall(grid, gap0, gap1, mid, half, east_west):
    if gap0 > gap1:
        return
    for along in range(gap0, gap1 + 1):
        for side in range(-half - 1, half + 2):
            x = along if east_west else mid + side
            y = mid + side if east_west else along
            if abs(side) <= half:
                open_passage(grid, x, y)
            else:
                seal_edge(grid, x, y)


def open_passage(grid, x, y):
    if grid.get((x, y)) == "Door":
        return
    grid[(x, y)] = "Floor"


def seal_edge(grid, x, y):
    kind = grid.get((x, y))
    if kind in ("Floor", "Bridge", "Door"):
        return
    grid[(x, y)] = "Wall"


def flood(grid, start, doors_open):
    walk = {"Floor", "Bridge"}
    if doors_open:
        walk = walk | {"Door"}
    seen = set()
    q = deque([start])
    seen.add(start)
    while q:
        x, y = q.popleft()
        for nx, ny in ((x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1)):
            if (nx, ny) in seen:
                continue
            if grid.get((nx, ny)) not in walk:
                continue
            seen.add((nx, ny))
            q.append((nx, ny))
    return seen


def main():
    rooms = [
        room(
            id="hub",
            name="The Cross",
            origin={"x": 16, "y": 19},
            width=46,
            height=16,
            wall="Stone",
            floor="Stone",
            hint="Four open rooms. Four elements. The north door wants their stones.",
            exit="north",
            stamps=[
                stamp("Floor", "Hearth", cells((4, 8), (5, 8), (41, 8), (40, 8))),
                stamp("Floor", "SaltCrust", cells((10, 3), (11, 3))),
                stamp("Floor", "Scoured", cells((32, 3), (33, 3))),
                stamp("Floor", "Crystal", rect(21, 12, 25, 13)),
            ],
            props=[
                {"type": "runes", "x": 4, "y": 8, "runes": ["Fire"], "dir": "right"},
                {"type": "runes", "x": 41, "y": 8, "runes": ["Water"], "dir": "left"},
                {"type": "runes", "x": 10, "y": 3, "runes": ["Earth"], "dir": "up"},
                {"type": "runes", "x": 32, "y": 3, "runes": ["Air"], "dir": "up"},
                {
                    "type": "gate",
                    "x": 23,
                    "y": 13,
                    "displayName": "Gate of Elements",
                    "formulaId": "door-i",
                    "requires": ["fire-stone", "water-stone", "earth-stone", "air-stone"],
                    "sprite": "socket-gate",
                    "note": "Four sockets take four stones. The north door opens.",
                },
            ],
        ),
        room(
            id="fire-wing",
            name="The Frozen Hall",
            origin={"x": 0, "y": 21},
            width=13,
            height=14,
            wall="Stone",
            floor="Ice",
            stamps=[
                stamp("Wall", "Ice", cells((2, 6), (3, 6), (4, 6), (2, 7), (4, 7), (2, 8), (3, 8), (4, 8))),
                stamp("Floor", "Hearth", cells((8, 7), (7, 7), (8, 6))),
            ],
            props=[
                {"type": "runes", "x": 8, "y": 7, "runes": ["Fire"], "dir": "left"},
                {
                    "type": "mite",
                    "x": 3,
                    "y": 3,
                    "displayName": "Ice-thing",
                    "formulaId": "ice-thing",
                    "formula": ["Water", "Salt", "Earth"],
                    "keys": ICE,
                    "sprite": "ice-thing",
                },
                {
                    "type": "barrier",
                    "x": 3,
                    "y": 7,
                    "displayName": "Ice cage",
                    "formulaId": "ice-cage",
                    "formula": ["Water", "Salt", "Earth"],
                    "keys": ICE,
                    "cells": [2, 6, 3, 6, 4, 6, 2, 7, 4, 7, 2, 8, 3, 8, 4, 8],
                    "clearMaterial": "Stone",
                    "sprite": "ice-block",
                    "note": "Hunger finds the hard water. A stone of fire sits free.",
                },
                {"type": "item", "x": 3, "y": 7, "item": "fire-stone"},
            ],
        ),
        room(
            id="water-wing",
            name="The Ember Vault",
            origin={"x": 65, "y": 21},
            width=13,
            height=14,
            wall="Stone",
            floor="Stone",
            stamps=[
                stamp("Wall", "Hearth", rect(9, 1, 9, 12)),
                stamp("Pit", "Water", cells((2, 2), (3, 2), (2, 11), (3, 11))),
                stamp("Floor", "Hearth", cells((3, 7), (4, 7))),
                stamp("Floor", "Ember", cells((4, 3), (5, 3))),
            ],
            props=[
                {"type": "runes", "x": 3, "y": 7, "runes": ["Water"], "dir": "right"},
                {
                    "type": "barrier",
                    "x": 9,
                    "y": 7,
                    "displayName": "Flame curtain",
                    "formulaId": "flame-curtain",
                    "formula": ["Fire"],
                    "keys": WATER,
                    "cells": rect(9, 1, 9, 12),
                    "clearMaterial": "Stone",
                    "sprite": "flame-curtain",
                    "note": "Yield thrown. The curtain forgets how to burn.",
                },
                {
                    "type": "mite",
                    "x": 6,
                    "y": 3,
                    "displayName": "Fire Golem",
                    "formulaId": "fire-golem",
                    "formula": ["Fire", "Salt"],
                    "keys": GOLEM,
                    "sprite": "fire-golem",
                },
                {"type": "item", "x": 11, "y": 7, "item": "water-stone"},
            ],
        ),
        room(
            id="earth-wing",
            name="The Arrow Gauntlet",
            origin={"x": 22, "y": 0},
            width=14,
            height=16,
            wall="Stone",
            floor="Stone",
            stamps=[
                stamp("Pit", "Void", cells((6, 2), (8, 2))),
                stamp("Floor", "SaltCrust", cells((7, 13), (7, 2))),
            ],
            props=[
                {"type": "runes", "x": 7, "y": 13, "runes": ["Earth"], "dir": "up"},
                {
                    "type": "arrows",
                    "x": 7,
                    "y": 11,
                    "displayName": "Arrow volley",
                    "formulaId": "arrow-volley",
                    "formula": ["Earth"],
                    "keys": ARROWS,
                    "cover": [6, 10, 7, 10, 8, 10, 6, 9, 7, 9, 8, 9],
                    "cells": rect(1, 1, 12, 10),
                    "sprite": "arrow-rack",
                    "note": "Rest stands. The shots break. Walk around the body you raised.",
                },
                {"type": "item", "x": 7, "y": 2, "item": "earth-stone"},
            ],
        ),
        room(
            id="air-wing",
            name="The Sundered Heights",
            origin={"x": 42, "y": 0},
            width=14,
            height=16,
            wall="Stone",
            floor="Scoured",
            stamps=[
                stamp("Floor", "Vein", cells((7, 12), (7, 2))),
                stamp("Floor", "Acid", rect(1, 1, 12, 8)),
            ],
            props=[
                {"type": "runes", "x": 7, "y": 12, "runes": ["Air"], "dir": "up"},
                {
                    "type": "fog",
                    "x": 7,
                    "y": 5,
                    "displayName": "Poison fog",
                    "formulaId": "poison-fog",
                    "formula": ["Air"],
                    "keys": WIND,
                    "cells": rect(1, 1, 12, 8),
                    "sprite": "poison-fog",
                    "note": "Breath sent. The foul air forgets the room.",
                },
                {"type": "item", "x": 7, "y": 2, "item": "air-stone"},
            ],
        ),
        room(
            id="aspect-foyer",
            name="Aspect Antechamber",
            origin={"x": 25, "y": 38},
            width=30,
            height=12,
            wall="Stone",
            floor="Stone",
            stamps=[
                stamp("Floor", "SaltCrust", cells((5, 6), (6, 6))),
                stamp("Floor", "Vein", cells((14, 6), (15, 6))),
                stamp("Floor", "Hearth", cells((23, 6), (24, 6))),
            ],
            props=[
                {"type": "runes", "x": 5, "y": 6, "runes": ["Salt"], "dir": "right"},
                {"type": "runes", "x": 14, "y": 6, "runes": ["Mercury"], "dir": "right"},
                {"type": "runes", "x": 23, "y": 6, "runes": ["Sulphur"], "dir": "right"},
            ],
        ),
        room(
            id="body-sanctum",
            name="The Standing Stone",
            origin={"x": 4, "y": 38},
            width=16,
            height=12,
            wall="Stone",
            floor="Stone",
            stamps=[
                stamp("Pit", "Void", rect(6, 1, 7, 10)),
                stamp("Floor", "SaltCrust", cells((12, 6), (3, 6))),
            ],
            props=[
                {"type": "runes", "x": 12, "y": 6, "runes": ["Salt"], "dir": "left"},
                {
                    "type": "chasm",
                    "x": 6,
                    "y": 6,
                    "displayName": "The standing gap",
                    "formulaId": "body-gap",
                    "keys": PITS,
                },
                {"type": "item", "x": 3, "y": 6, "item": "body-stone"},
            ],
        ),
        room(
            id="spirit-sanctum",
            name="The Gallery of Force",
            origin={"x": 31, "y": 53},
            width=16,
            height=12,
            wall="Stone",
            floor="Stone",
            stamps=[
                stamp("Floor", "Vein", cells((3, 3), (8, 6), (12, 6))),
                stamp("Floor", "Hearth", cells((4, 8), (5, 8))),
            ],
            props=[
                {"type": "runes", "x": 3, "y": 3, "runes": ["Mercury"], "dir": "right"},
                {"type": "runes", "x": 4, "y": 8, "runes": ["Fire"], "dir": "right"},
                {
                    "type": "mite",
                    "x": 8,
                    "y": 6,
                    "displayName": "Warden",
                    "formulaId": "spirit-warden",
                    "formula": ["Earth", "Salt"],
                    "keys": ATTACK,
                    "sprite": "warden",
                    "blocking": True,
                    "grant": "spirit-stone",
                },
            ],
        ),
        room(
            id="mind-sanctum",
            name="The Silent Court",
            origin={"x": 59, "y": 38},
            width=16,
            height=12,
            wall="Stone",
            floor="Stone",
            stamps=[
                stamp("Wall", "Stone", rect(6, 5, 14, 5)),
                stamp("Wall", "Stone", rect(6, 7, 14, 7)),
                stamp("Floor", "Crystal", cells((3, 6), (13, 6))),
                stamp("Floor", "Hearth", cells((3, 3), (4, 3))),
                stamp("Floor", "Damp", cells((3, 9), (4, 9))),
                stamp("Floor", "SaltCrust", cells((3, 8), (4, 8))),
            ],
            props=[
                {"type": "runes", "x": 3, "y": 3, "runes": ["Salt"], "dir": "right"},
                {"type": "runes", "x": 3, "y": 6, "runes": ["Sulphur"], "dir": "right"},
                {"type": "runes", "x": 3, "y": 9, "runes": ["Mercury"], "dir": "right"},
                {"type": "runes", "x": 5, "y": 3, "runes": ["Fire"], "dir": "right"},
                {"type": "runes", "x": 5, "y": 9, "runes": ["Water"], "dir": "right"},
                {
                    "type": "mite",
                    "x": 8,
                    "y": 6,
                    "displayName": "Stone man",
                    "formulaId": "stone-man",
                    "formula": ["Earth", "Salt"],
                    "keys": MIND,
                    "sprite": "stone-man",
                    "blocking": True,
                },
                {
                    "type": "mite",
                    "x": 10,
                    "y": 6,
                    "displayName": "Stone man",
                    "formulaId": "stone-man",
                    "formula": ["Earth", "Salt"],
                    "keys": MIND,
                    "sprite": "stone-man",
                    "blocking": True,
                },
                {
                    "type": "mite",
                    "x": 12,
                    "y": 6,
                    "displayName": "Stone man",
                    "formulaId": "stone-man",
                    "formula": ["Earth", "Salt"],
                    "keys": MIND,
                    "sprite": "stone-man",
                    "blocking": True,
                },
                {"type": "item", "x": 13, "y": 6, "item": "mind-stone"},
            ],
        ),
        room(
            id="door-ii",
            name="Gate of Aspects",
            origin={"x": 30, "y": 68},
            width=18,
            height=10,
            wall="Stone",
            floor="Stone",
            exit="north",
            stamps=[
                stamp("Floor", "Crystal", rect(7, 4, 11, 6)),
                stamp("Floor", "SaltCrust", cells((9, 8))),
            ],
            props=[
                {
                    "type": "gate",
                    "x": 9,
                    "y": 7,
                    "displayName": "Gate of Aspects",
                    "formulaId": "door-ii",
                    "requires": ["body-stone", "spirit-stone", "mind-stone"],
                    "finishes": True,
                    "sprite": "socket-gate",
                    "note": "Body, spirit, and mind take their seats. The floor opens.",
                },
            ],
        ),
    ]

    data = {
        "id": "foundation",
        "name": "The Foundation",
        "spawn": {"x": 39, "y": 26},
        "rooms": rooms,
        "halls": [
            {"from": "fire-wing", "to": "hub", "material": "Ice"},
            {"from": "hub", "to": "water-wing", "material": "Stone"},
            {"from": "earth-wing", "to": "hub", "material": "Stone"},
            {"from": "air-wing", "to": "hub", "material": "Scoured"},
            {"from": "hub", "to": "aspect-foyer", "material": "Stone"},
            {"from": "body-sanctum", "to": "aspect-foyer", "material": "SaltCrust"},
            {"from": "aspect-foyer", "to": "mind-sanctum", "material": "Stone"},
            {"from": "aspect-foyer", "to": "spirit-sanctum", "material": "Vein"},
            {"from": "spirit-sanctum", "to": "door-ii", "material": "Crystal"},
        ],
    }
    validate(data)
    dest = Path(__file__).resolve().parents[1] / "Assets/Resources/Maps/foundation.json"
    dest.write_text(json.dumps(data, indent=2) + "\n")
    print(f"wrote {dest} ({len(data['rooms'])} rooms)")


if __name__ == "__main__":
    main()
