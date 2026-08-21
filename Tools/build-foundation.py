#!/usr/bin/env python3
"""Stamp Floor 1 — The Foundation — as Maps/foundation.json."""

from __future__ import annotations

import json
from collections import deque
from pathlib import Path

FIRE = ["Fireball", "FlamePillar", "Melt", "Ignite", "SunLance", "Scald"]
ICE = FIRE + ["Thaw"]
WATER = [
    "Douse",
    "WaterJet",
    "Rain",
    "Flood",
    "Smother",
    "Scald",
    "Swamp",
    "IcePillar",
    "IceSpear",
    "IceWall",
    "Freeze",
    "Snowfall",
    "Snowstorm",
]
EARTH = [
    "HurledStone",
    "EarthPillar",
    "Wall",
    "Quagmire",
    "Pit",
    "Shatter",
    "GraveDust",
    "StonePillar",
]
GOLEM = WATER + EARTH + ["LightningBolt", "ChainLightning", "LavaFlood", "LavaPillar", "Melt"]
PITS = [
    "EarthPillar",
    "Wall",
    "IceWall",
    "Bridge",
    "FlamePillar",
    "IcePillar",
    "VineRise",
    "Hop",
    "Flight",
    "HurledStone",
]
ARROWS = ["EarthPillar", "Wall", "IceWall", "StonePillar", "FlamePillar", "IcePillar", "VineRise", "Menhir", "Bridge"]
WIND = ["Gust", "Gale", "StormCall", "Flight"]
MIND = ["Charm", "Command", "Rage", "Lull", "Terror", "Jolt"]
SPARK = [
    "LightningBolt",
    "LiveFloor",
    "Jolt",
    "BrilliantArc",
    "Blackout",
    "ChainLightning",
    "StormCall",
    "Thunderclap",
]
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


def lesson(x, y, runes, dir="right"):
    return {"type": "lesson", "x": int(x), "y": int(y), "runes": list(runes), "dir": dir}


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

    def must_not(reach, name, xy):
        if xy in reach:
            raise SystemExit(f"can walk to {name} at {xy} — the obstacle is skippable")

    must(closed, "fire altar", world("fire-wing", rooms, 8, 7))
    must(closed, "fire cage face", world("fire-wing", rooms, 5, 7))
    must(closed, "water altar", world("water-wing", rooms, 3, 7))
    must(closed, "water curtain face", world("water-wing", rooms, 8, 7))
    must(closed, "earth altar", world("earth-wing", rooms, 7, 13))
    must(closed, "earth pit lip", world("earth-wing", rooms, 7, 5))
    must_not(closed, "earth stone", world("earth-wing", rooms, 7, 2))
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

    join = world("join-foyer", rooms, 16, 8)
    if join in closed:
        raise SystemExit("Door II is already open — the aspect north wall should stay shut")
    must(open_reach, "join foyer", join)
    must(open_reach, "grove east bank", world("grove-court", rooms, 14, 8))
    must(open_reach, "grove plant lip", world("grove-court", rooms, 11, 8))
    must(open_reach, "grove spark lip", world("grove-court", rooms, 13, 14))
    must_not(open_reach, "grove stone", world("grove-court", rooms, 6, 8))
    must(open_reach, "cistern mouth", world("cistern", rooms, 3, 8))
    must_not(open_reach, "flood stone", world("cistern", rooms, 10, 8))
    must(open_reach, "spark altar", world("spark-cell", rooms, 10, 10))
    must_not(open_reach, "spark stone", world("spark-cell", rooms, 16, 7))
    must(open_reach, "arena floor", world("arena", rooms, 8, 8))
    must(open_reach, "door III", world("door-iii", rooms, 10, 5))


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
            material = mark.get("material") or ""
            cells_xy = mark["cells"]
            stored = "Pit" if kind == "Floor" and material == "Water" else kind
            for i in range(0, len(cells_xy), 2):
                grid[(ox + cells_xy[i], oy + cells_xy[i + 1])] = stored
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
                stamp("Floor", "Moss", cells((21, 6), (22, 6), (24, 6), (25, 6))),
                stamp("Floor", "Plant", cells((21, 5), (25, 5))),
                stamp("Floor", "SaltCrust", cells((22, 7))),
                stamp("Floor", "Vein", cells((24, 7))),
                stamp("Floor", "Hearth", cells((23, 8))),
            ],
            props=[
                {"type": "runes", "x": 4, "y": 8, "runes": ["Fire"], "dir": "right"},
                {"type": "runes", "x": 41, "y": 8, "runes": ["Water"], "dir": "left"},
                {"type": "runes", "x": 10, "y": 3, "runes": ["Earth"], "dir": "up"},
                {"type": "runes", "x": 32, "y": 3, "runes": ["Air"], "dir": "up"},
                {"type": "inscription", "x": 22, "y": 7, "runes": ["Salt"]},
                {"type": "inscription", "x": 24, "y": 7, "runes": ["Mercury"]},
                {"type": "inscription", "x": 23, "y": 8, "runes": ["Sulphur"]},
                {"type": "pillar", "x": 21, "y": 8, "runes": ["Salt"]},
                {"type": "pillar", "x": 25, "y": 8, "runes": ["Mercury"]},
                {"type": "pillar", "x": 23, "y": 5, "runes": ["Sulphur"]},
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
                lesson(7, 4, ["Fire", "Mercury"], "up"),
                {
                    "type": "mite",
                    "x": 3,
                    "y": 3,
                    "displayName": "Ice-thing",
                    "formulaId": "ice-thing",
                    "formula": ["Water", "Salt", "Earth", "Life"],
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
                stamp("Floor", "Hearth", cells((3, 7), (4, 7))),
                stamp("Floor", "Ember", cells((4, 3), (5, 3))),
                stamp("Floor", "Moss", cells((2, 6), (2, 8), (4, 6), (4, 8))),
                stamp("Floor", "Plant", cells((1, 7), (5, 7))),
            ],
            props=[
                {"type": "runes", "x": 3, "y": 7, "runes": ["Water"], "dir": "right"},
                lesson(5, 9, ["Water", "Mercury"], "right"),
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
                    "formula": ["Fire", "Salt", "Life"],
                    "keys": GOLEM,
                    "sprite": "fire-golem",
                    "attack": "golem",
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
                stamp("Pit", "Void", rect(1, 3, 12, 4)),
                stamp("Floor", "SaltCrust", cells((7, 13), (7, 2))),
            ],
            props=[
                {"type": "runes", "x": 7, "y": 13, "runes": ["Earth"], "dir": "up"},
                lesson(4, 13, ["Earth", "Salt"], "right"),
                {
                    "type": "arrows",
                    "x": 7,
                    "y": 11,
                    "displayName": "Arrow volley",
                    "formulaId": "arrow-volley",
                    "formula": ["Earth"],
                    "keys": ARROWS,
                    "cover": [6, 10, 7, 10, 8, 10, 6, 9, 7, 9, 8, 9],
                    "cells": rect(1, 5, 12, 10),
                    "sprite": "arrow-rack",
                    "dir": "south",
                    "note": "Rest stands. The shots break. Then the last step asks for a hop, or a body of rest.",
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
                lesson(4, 12, ["Air", "Mercury"], "right"),
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
                lesson(10, 3, ["Earth", "Salt"], "right"),
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
                lesson(12, 3, ["Fire", "Mercury"], "left"),
                {
                    "type": "mite",
                    "x": 8,
                    "y": 6,
                    "displayName": "Warden",
                    "formulaId": "spirit-warden",
                    "formula": ["Earth", "Salt", "Life"],
                    "keys": ATTACK,
                    "sprite": "warden",
                    "blocking": True,
                    "grant": "spirit-stone",
                    "ensouled": True,
                    "attack": "wizard",
                    "castSeconds": 2,
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
                lesson(3, 2, ["Life", "Sulphur", "Mercury"], "right"),
                {
                    "type": "mite",
                    "x": 8,
                    "y": 6,
                    "displayName": "Stone man",
                    "formulaId": "stone-man",
                    "formula": ["Earth", "Salt", "Life"],
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
                    "formula": ["Earth", "Salt", "Life"],
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
                    "formula": ["Earth", "Salt", "Life"],
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
                    "sprite": "socket-gate",
                    "note": "Body, spirit, and mind take their seats. The living work stands open.",
                },
            ],
        ),
        room(
            id="join-foyer",
            name="The Wrought Cross",
            origin={"x": 22, "y": 81},
            width=34,
            height=16,
            wall="Stone",
            floor="Stone",
            hint="Joins already stand. Grow, burn, freeze. A door wants three stones.",
            stamps=[
                stamp("Floor", "Vein", cells((6, 8), (7, 8))),
                stamp("Floor", "Moss", cells((16, 8), (17, 8))),
                stamp("Floor", "Damp", cells((26, 8), (27, 8))),
                stamp("Floor", "Ice", cells((28, 8))),
                stamp("Floor", "Plant", cells((16, 5), (17, 5))),
            ],
            props=[
                {"type": "runes", "x": 6, "y": 8, "runes": ["Spark"], "dir": "right"},
                {"type": "runes", "x": 16, "y": 8, "runes": ["Plant"], "dir": "right"},
                {"type": "runes", "x": 26, "y": 8, "runes": ["Water"], "dir": "right"},
                {"type": "runes", "x": 28, "y": 8, "runes": ["Ice"], "dir": "right"},
            ],
        ),
        room(
            id="grove-court",
            name="The Living Thicket",
            origin={"x": 0, "y": 81},
            width=18,
            height=16,
            wall="Stone",
            floor="Stone",
            stamps=[
                stamp("Floor", "Grove", cells((1, 7), (1, 8), (2, 7), (2, 8))),
                stamp("Wall", "Stone", rect(3, 1, 3, 6)),
                stamp("Wall", "Stone", rect(3, 9, 3, 14)),
                stamp("Floor", "Stone", cells((3, 7), (3, 8))),
                stamp("Wall", "Timber", rect(4, 1, 4, 6)),
                stamp("Wall", "Grove", cells((4, 7), (4, 8))),
                stamp("Wall", "Plant", rect(4, 9, 4, 14)),
                stamp("Floor", "Plant", rect(6, 1, 6, 14)),
                stamp("Floor", "Moss", cells((6, 7), (6, 8))),
                stamp("Pit", "Void", rect(7, 1, 10, 14)),
                stamp("Floor", "Moss", rect(11, 1, 12, 14)),
                stamp("Floor", "Plant", cells((11, 7), (12, 7), (11, 8), (12, 8))),
                stamp("Floor", "Hearth", cells((14, 11), (15, 11))),
                stamp("Floor", "Plant", cells((14, 8))),
                stamp("Floor", "Damp", cells((14, 5), (15, 5))),
            ],
            props=[
                {"type": "runes", "x": 14, "y": 11, "runes": ["Fire"], "dir": "left"},
                {"type": "runes", "x": 14, "y": 8, "runes": ["Plant"], "dir": "left"},
                {"type": "runes", "x": 14, "y": 5, "runes": ["Water"], "dir": "left"},
                lesson(14, 3, ["Water", "Earth", "Salt", "Life"], "left"),
                {"type": "item", "x": 6, "y": 8, "item": "grove-stone"},
            ],
        ),
        room(
            id="cistern",
            name="The Cistern",
            origin={"x": 59, "y": 81},
            width=20,
            height=16,
            wall="Stone",
            floor="Stone",
            stamps=[
                stamp("Floor", "Damp", cells((3, 8), (4, 8))),
                stamp("Floor", "SaltCrust", cells((3, 10), (4, 10))),
                stamp("Floor", "Ice", cells((3, 6), (4, 6))),
                stamp("Pit", "Water", rect(6, 1, 7, 14)),
                stamp("Floor", "Damp", cells((10, 8), (16, 7), (17, 7))),
            ],
            props=[
                {"type": "runes", "x": 3, "y": 8, "runes": ["Water"], "dir": "right"},
                {"type": "runes", "x": 3, "y": 10, "runes": ["Salt"], "dir": "right"},
                {"type": "runes", "x": 3, "y": 6, "runes": ["Earth"], "dir": "right"},
                {"type": "runes", "x": 4, "y": 6, "runes": ["Ice"], "dir": "right"},
                lesson(2, 10, ["Water", "Salt", "Earth"], "up"),
                {"type": "item", "x": 10, "y": 8, "item": "flood-stone"},
            ],
        ),
        room(
            id="spark-cell",
            name="The Seed of Charge",
            origin={"x": 10, "y": 100},
            width=20,
            height=14,
            wall="Stone",
            floor="Vein",
            stamps=[
                stamp("Floor", "Metal", rect(7, 5, 11, 9)),
                stamp("Floor", "Hearth", cells((3, 3), (4, 3))),
                stamp("Floor", "Scoured", cells((3, 10), (4, 10))),
                stamp("Wall", "Metal", rect(13, 1, 13, 12)),
            ],
            props=[
                {"type": "runes", "x": 10, "y": 10, "runes": ["Spark"], "dir": "left"},
                {"type": "runes", "x": 3, "y": 3, "runes": ["Fire"], "dir": "right"},
                {"type": "runes", "x": 3, "y": 10, "runes": ["Air"], "dir": "right"},
                lesson(3, 6, ["Fire", "Air", "Spark"], "up"),
                {
                    "type": "rod",
                    "x": 9,
                    "y": 7,
                    "displayName": "Live rod",
                    "formulaId": "spark-rod",
                    "formula": ["Fire", "Air"],
                    "keys": SPARK,
                    "sprite": "rod",
                    "note": "The seed already stands. Send it, or write hunger then breath.",
                },
                {
                    "type": "barrier",
                    "x": 13,
                    "y": 7,
                    "displayName": "Charge veil",
                    "formulaId": "charge-veil",
                    "formula": ["Fire", "Air", "Spark"],
                    "keys": SPARK,
                    "cells": rect(13, 1, 13, 12),
                    "clearMaterial": "Vein",
                    "sprite": "charge-curtain",
                    "note": "Hunger given breath, then sent. The veil forgets how to sting.",
                },
                {"type": "item", "x": 16, "y": 7, "item": "spark-stone"},
            ],
        ),
        room(
            id="arena",
            name="The Mixed Court",
            origin={"x": 42, "y": 100},
            width=24,
            height=16,
            wall="Stone",
            floor="Stone",
            stamps=[
                stamp("Floor", "Hearth", cells((16, 4), (17, 4))),
                stamp("Floor", "Vein", cells((11, 8), (12, 8))),
                stamp("Floor", "SaltCrust", cells((5, 4), (5, 11))),
                stamp("Wall", "Stone", cells((9, 6), (9, 9), (14, 6), (14, 9))),
            ],
            props=[
                {"type": "runes", "x": 16, "y": 4, "runes": ["Fire"], "dir": "left"},
                {"type": "runes", "x": 11, "y": 8, "runes": ["Spark"], "dir": "right"},
                {
                    "type": "mite",
                    "x": 5,
                    "y": 4,
                    "displayName": "Fire Golem",
                    "formulaId": "fire-golem",
                    "formula": ["Fire", "Salt"],
                    "keys": GOLEM,
                    "sprite": "fire-golem",
                    "attack": "golem",
                },
                {
                    "type": "mite",
                    "x": 5,
                    "y": 11,
                    "displayName": "Fire Golem",
                    "formulaId": "fire-golem",
                    "formula": ["Fire", "Salt"],
                    "keys": GOLEM,
                    "sprite": "fire-golem",
                    "attack": "golem",
                },
                {
                    "type": "mite",
                    "x": 16,
                    "y": 4,
                    "displayName": "Ember adept",
                    "formulaId": "ember-adept",
                    "formula": ["Fire", "Salt", "Mercury"],
                    "keys": ATTACK,
                    "sprite": "warden",
                    "ensouled": True,
                    "attack": "wizard",
                    "castSeconds": 2,
                    "cast": ["Fire", "Mercury"],
                },
                {
                    "type": "mite",
                    "x": 16,
                    "y": 11,
                    "displayName": "Bolt adept",
                    "formulaId": "bolt-adept",
                    "formula": ["Fire", "Air", "Mercury"],
                    "keys": ATTACK + SPARK,
                    "sprite": "warden",
                    "ensouled": True,
                    "attack": "wizard",
                    "castSeconds": 2.2,
                    "cast": ["Spark", "Mercury"],
                },
                {
                    "type": "mite",
                    "x": 11,
                    "y": 8,
                    "displayName": "Arrow adept",
                    "formulaId": "arrow-adept",
                    "formula": ["Earth", "Mercury"],
                    "keys": ATTACK,
                    "sprite": "warden",
                    "ensouled": True,
                    "attack": "archer",
                    "castSeconds": 1.15,
                    "cast": ["Earth", "Mercury"],
                },
            ],
        ),
        room(
            id="door-iii",
            name="Gate of Joins",
            origin={"x": 26, "y": 118},
            width=22,
            height=10,
            wall="Stone",
            floor="Stone",
            stamps=[
                stamp("Floor", "Crystal", rect(8, 3, 13, 6)),
                stamp("Floor", "Vein", cells((10, 8))),
                stamp("Floor", "Moss", cells((9, 8))),
                stamp("Floor", "Damp", cells((11, 8))),
            ],
            props=[
                {
                    "type": "gate",
                    "x": 10,
                    "y": 5,
                    "displayName": "Gate of Joins",
                    "formulaId": "door-iii",
                    "requires": ["grove-stone", "flood-stone", "spark-stone"],
                    "finishes": True,
                    "sprite": "socket-gate",
                    "note": "Plant, pool, and seed take their seats. The floor opens.",
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
            {"from": "door-ii", "to": "join-foyer", "material": "Stone"},
            {"from": "grove-court", "to": "join-foyer", "material": "Moss"},
            {"from": "join-foyer", "to": "cistern", "material": "Damp"},
            {"from": "grove-court", "to": "spark-cell", "material": "Vein"},
            {"from": "join-foyer", "to": "arena", "material": "Stone"},
            {"from": "join-foyer", "to": "door-iii", "material": "Crystal"},
        ],
    }
    validate(data)
    dest = Path(__file__).resolve().parents[1] / "Assets/Resources/Maps/foundation.json"
    dest.write_text(json.dumps(data, indent=2) + "\n")
    print(f"wrote {dest} ({len(data['rooms'])} rooms)")


if __name__ == "__main__":
    main()
