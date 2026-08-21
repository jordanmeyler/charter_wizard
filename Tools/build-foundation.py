#!/usr/bin/env python3
"""Stamp Floor 1 — The Foundation — as Maps/foundation.json."""

from __future__ import annotations

import json
from pathlib import Path

FIRE = ["Fireball", "FlamePillar", "Melt", "Ignite", "SunLance", "Scald"]
ICE = FIRE + ["Thaw"]
WATER = ["Douse", "WaterJet", "Rain", "Flood", "Smother"]
GOLEM = WATER + ["Gale", "Quagmire", "Wall", "Pit", "Bridge", "IcePillar"]
PITS = [
    "Wall",
    "Bridge",
    "FlamePillar",
    "IcePillar",
    "VineRise",
    "Hop",
    "Flight",
    "HurledStone",
]
ARROWS = ["Wall", "FlamePillar", "IcePillar", "VineRise", "Bridge"]
POISON = ["Gale", "StormCall", "Flight"]
MIND = ["Rage", "Lull", "Terror", "Command", "Jolt"]
ATTACK = [
    "Fireball",
    "Douse",
    "WaterJet",
    "HurledStone",
    "LightningBolt",
    "IceSpear",
    "Gale",
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


def validate(data):
    boxes = []
    for r in data["rooms"]:
        box = (
            r["id"],
            r["origin"]["x"],
            r["origin"]["y"],
            r["origin"]["x"] + r["width"] - 1,
            r["origin"]["y"] + r["height"] - 1,
        )
        boxes.append(box)
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
        east = dst["origin"]["x"] > src["origin"]["x"]
        north = dst["origin"]["y"] > src["origin"]["y"]
        if not east and not north:
            raise SystemExit(f"hall {hall['from']}→{hall['to']} is not east/north")
    spawn = data["spawn"]
    ante = next(r for r in data["rooms"] if r["id"] == "antechamber")
    if not (
        ante["origin"]["x"] < spawn["x"] < ante["origin"]["x"] + ante["width"] - 1
        and ante["origin"]["y"] < spawn["y"] < ante["origin"]["y"] + ante["height"] - 1
    ):
        raise SystemExit("spawn is not inside the antechamber")


def main():
    rooms = [
        room(
            id="antechamber",
            name="Antechamber",
            origin={"x": 16, "y": 0},
            width=13,
            height=11,
            wall="Stone",
            floor="Stone",
            hint="Read the labelled Fire. Hunger sent is the first verb.",
            exit="north",
            stamps=[
                stamp("Floor", "Hearth", cells((4, 3), (5, 3), (4, 4), (5, 4), (6, 4))),
                stamp("Floor", "Timber", cells((5, 7), (6, 7), (7, 7), (6, 8))),
            ],
            props=[
                {
                    "type": "runes",
                    "x": 4,
                    "y": 4,
                    "runes": ["Fire"],
                    "dir": "right",
                },
                {
                    "type": "barrier",
                    "x": 6,
                    "y": 8,
                    "displayName": "Rope",
                    "formulaId": "rope",
                    "formula": ["Fire"],
                    "keys": FIRE,
                    "sprite": "torch",
                    "note": "Hunger finds the rope. The portcullis forgets why it was shut.",
                },
            ],
        ),
        room(
            id="hub",
            name="The Cross",
            origin={"x": 16, "y": 15},
            width=29,
            height=13,
            wall="Stone",
            floor="Stone",
            stamps=[
                stamp("Floor", "SaltCrust", rect(13, 5, 15, 7)),
                stamp("Floor", "Hearth", cells((4, 6), (24, 6))),
            ],
            props=[
                {
                    "type": "runes",
                    "x": 3,
                    "y": 3,
                    "runes": ["Fire"],
                    "dir": "right",
                },
                {
                    "type": "runes",
                    "x": 25,
                    "y": 3,
                    "runes": ["Water"],
                    "dir": "left",
                },
                {
                    "type": "runes",
                    "x": 3,
                    "y": 9,
                    "runes": ["Earth"],
                    "dir": "right",
                },
                {
                    "type": "runes",
                    "x": 25,
                    "y": 9,
                    "runes": ["Air"],
                    "dir": "left",
                },
            ],
        ),
        room(
            id="fire-wing",
            name="The Frozen Hall",
            origin={"x": 0, "y": 15},
            width=13,
            height=13,
            wall="Stone",
            floor="Ice",
            stamps=[
                stamp("Wall", "Ice", rect(10, 2, 11, 10)),
                stamp(
                    "Wall",
                    "Ice",
                    cells((2, 8), (3, 8), (4, 8), (2, 9), (4, 9), (2, 10), (3, 10), (4, 10)),
                ),
                stamp("Floor", "Hearth", cells((3, 2), (4, 2), (3, 3), (7, 11))),
            ],
            props=[
                {"type": "runes", "x": 3, "y": 3, "runes": ["Fire"], "dir": "right"},
                {
                    "type": "barrier",
                    "x": 10,
                    "y": 6,
                    "displayName": "Ice seal",
                    "formulaId": "ice-seal",
                    "formula": ["Water", "Salt", "Earth"],
                    "keys": ICE,
                    "cells": rect(10, 2, 11, 10),
                    "clearMaterial": "Stone",
                    "sprite": "torch",
                    "note": "Hunger finds the hard water. The seal remembers yield.",
                },
                {
                    "type": "mite",
                    "x": 7,
                    "y": 6,
                    "displayName": "Ice-thing",
                    "formulaId": "ice-thing",
                    "formula": ["Water", "Salt", "Earth"],
                    "keys": ICE,
                    "sprite": "ash-mite",
                },
                {
                    "type": "barrier",
                    "x": 3,
                    "y": 9,
                    "displayName": "Ice cage",
                    "formulaId": "ice-cage",
                    "formula": ["Water", "Salt", "Earth"],
                    "keys": ICE,
                    "cells": [2, 8, 3, 8, 4, 8, 2, 9, 4, 9, 2, 10, 3, 10, 4, 10],
                    "clearMaterial": "Stone",
                    "sprite": "torch",
                    "note": "The cage thaws. A stone of hunger sits free.",
                },
                {"type": "item", "x": 3, "y": 9, "item": "fire-stone"},
            ],
        ),
        room(
            id="water-wing",
            name="The Ember Vault",
            origin={"x": 49, "y": 15},
            width=17,
            height=13,
            wall="Stone",
            floor="Stone",
            stamps=[
                stamp("Wall", "Hearth", rect(1, 2, 2, 10)),
                stamp("Pit", "Water", rect(10, 3, 15, 9)),
                stamp("Floor", "Hearth", cells((8, 2), (5, 10), (6, 10))),
                stamp("Floor", "Ember", cells((4, 10), (7, 10))),
            ],
            props=[
                {"type": "runes", "x": 8, "y": 2, "runes": ["Water"], "dir": "right"},
                {
                    "type": "barrier",
                    "x": 2,
                    "y": 6,
                    "displayName": "Flame curtain",
                    "formulaId": "flame-curtain",
                    "formula": ["Fire"],
                    "keys": WATER,
                    "cells": rect(1, 2, 2, 10),
                    "clearMaterial": "Stone",
                    "sprite": "torch",
                    "note": "Yield thrown. The curtain forgets how to burn.",
                },
                {
                    "type": "mite",
                    "x": 5,
                    "y": 6,
                    "displayName": "Fire Golem",
                    "formulaId": "fire-golem",
                    "formula": ["Fire", "Salt"],
                    "keys": GOLEM,
                    "sprite": "ash-mite",
                    "blocking": True,
                },
                {
                    "type": "barrier",
                    "x": 13,
                    "y": 6,
                    "displayName": "Sunken basin",
                    "formulaId": "sunken-basin",
                    "formula": ["Water"],
                    "keys": ["Flood", "WaterJet", "Rain", "IcePillar", "Douse"],
                    "cells": rect(10, 3, 15, 9),
                    "clearMaterial": "Water",
                    "grant": "water-stone",
                    "sprite": "rod",
                    "note": "The basin rises. A stone of yield comes up with it.",
                },
            ],
        ),
        room(
            id="earth-wing",
            name="The Arrow Gauntlet",
            origin={"x": 33, "y": 0},
            width=13,
            height=11,
            wall="Stone",
            floor="Stone",
            stamps=[
                stamp("Pit", "Void", rect(3, 7, 9, 8)),
                stamp("Wall", "Metal", rect(1, 3, 4, 6)),
                stamp("Wall", "Metal", rect(8, 3, 11, 6)),
                stamp("Wall", "Metal", cells((5, 5), (6, 5), (7, 5))),
                stamp("Floor", "SaltCrust", cells((6, 2), (6, 9))),
            ],
            props=[
                {"type": "runes", "x": 6, "y": 9, "runes": ["Earth"], "dir": "up"},
                {
                    "type": "chasm",
                    "x": 6,
                    "y": 7,
                    "displayName": "Entry gap",
                    "formulaId": "earth-gap",
                    "keys": PITS,
                },
                {
                    "type": "barrier",
                    "x": 6,
                    "y": 5,
                    "displayName": "Arrow volley",
                    "formulaId": "arrow-volley",
                    "formula": ["Earth"],
                    "keys": ARROWS,
                    "cells": [5, 5, 6, 5, 7, 5],
                    "clearMaterial": "Stone",
                    "sprite": "rod",
                    "note": "Rest stands in the way of the arrows. The hall can be walked.",
                },
                {"type": "item", "x": 6, "y": 2, "item": "earth-stone"},
            ],
        ),
        room(
            id="air-wing",
            name="The Sundered Heights",
            origin={"x": 0, "y": 32},
            width=13,
            height=13,
            wall="Stone",
            floor="Scoured",
            stamps=[
                stamp("Pit", "Void", rect(1, 8, 11, 9)),
                stamp("Wall", "Steam", rect(1, 10, 11, 10)),
                stamp("Floor", "Vein", cells((3, 3), (6, 3))),
            ],
            props=[
                {"type": "runes", "x": 3, "y": 3, "runes": ["Air"], "dir": "right"},
                {
                    "type": "chasm",
                    "x": 6,
                    "y": 8,
                    "displayName": "The rift",
                    "formulaId": "air-rift",
                    "keys": PITS,
                },
                {
                    "type": "barrier",
                    "x": 6,
                    "y": 10,
                    "displayName": "Poison veil",
                    "formulaId": "poison-veil",
                    "formula": ["Air"],
                    "keys": POISON,
                    "cells": rect(1, 10, 11, 10),
                    "clearMaterial": "Scoured",
                    "sprite": "rod",
                    "note": "Breath going pushes the veil out. The chamber can be entered.",
                },
                {"type": "item", "x": 6, "y": 11, "item": "air-stone"},
            ],
        ),
        room(
            id="door-i",
            name="Gate of Elements",
            origin={"x": 16, "y": 32},
            width=29,
            height=13,
            wall="Stone",
            floor="Stone",
            exit="north",
            stamps=[
                stamp("Pit", "Void", rect(1, 1, 8, 11)),
                stamp("Pit", "Water", rect(20, 1, 27, 11)),
                stamp("Floor", "Hearth", cells((14, 3), (13, 3), (15, 3))),
                stamp("Floor", "Stone", cells((3, 12), (4, 12), (5, 12), (23, 12), (24, 12), (25, 12))),
            ],
            props=[
                {
                    "type": "gate",
                    "x": 14,
                    "y": 10,
                    "displayName": "Gate of Elements",
                    "formulaId": "door-i",
                    "requires": ["fire-stone", "water-stone", "earth-stone", "air-stone"],
                    "sprite": "rod",
                    "note": "Four sockets take four stones. The gate of elements opens.",
                },
            ],
        ),
        room(
            id="aspect-foyer",
            name="Aspect Antechamber",
            origin={"x": 16, "y": 49},
            width=29,
            height=13,
            wall="Stone",
            floor="Stone",
            stamps=[
                stamp("Floor", "SaltCrust", cells((7, 6), (8, 6))),
                stamp("Floor", "Vein", cells((14, 6), (15, 6))),
                stamp("Floor", "Hearth", cells((21, 6), (22, 6))),
            ],
            props=[
                {"type": "runes", "x": 7, "y": 6, "runes": ["Salt"], "dir": "right"},
                {"type": "runes", "x": 14, "y": 6, "runes": ["Mercury"], "dir": "right"},
                {"type": "runes", "x": 21, "y": 6, "runes": ["Sulphur"], "dir": "right"},
            ],
        ),
        room(
            id="body-sanctum",
            name="The Standing Stone",
            origin={"x": 0, "y": 49},
            width=13,
            height=13,
            wall="Stone",
            floor="Stone",
            stamps=[
                stamp("Pit", "Void", rect(1, 8, 11, 9)),
                stamp("Floor", "SaltCrust", cells((6, 3), (6, 11), (5, 11), (7, 11))),
            ],
            props=[
                {"type": "runes", "x": 6, "y": 3, "runes": ["Salt"], "dir": "up"},
                {
                    "type": "chasm",
                    "x": 6,
                    "y": 8,
                    "displayName": "The standing gap",
                    "formulaId": "body-gap",
                    "keys": PITS,
                },
                {"type": "item", "x": 6, "y": 11, "item": "body-stone"},
            ],
        ),
        room(
            id="spirit-sanctum",
            name="The Gallery of Force",
            origin={"x": 49, "y": 49},
            width=17,
            height=13,
            wall="Stone",
            floor="Stone",
            stamps=[
                stamp("Floor", "Vein", cells((8, 6), (3, 3), (9, 6))),
                stamp("Wall", "Metal", cells((6, 4), (10, 4), (6, 8), (10, 8))),
            ],
            props=[
                {"type": "runes", "x": 3, "y": 3, "runes": ["Mercury"], "dir": "right"},
                {
                    "type": "mite",
                    "x": 8,
                    "y": 6,
                    "displayName": "Warden",
                    "formulaId": "spirit-warden",
                    "formula": ["Earth", "Salt"],
                    "keys": ATTACK,
                    "sprite": "ash-mite",
                    "blocking": True,
                    "grant": "spirit-stone",
                },
            ],
        ),
        room(
            id="mind-sanctum",
            name="The Silent Court",
            origin={"x": 16, "y": 66},
            width=29,
            height=13,
            wall="Stone",
            floor="Stone",
            stamps=[
                stamp("Wall", "Stone", cells(*[(13, y) for y in range(3, 10)] + [(15, y) for y in range(3, 10)])),
                stamp("Floor", "Crystal", cells((14, 11), (8, 3))),
            ],
            props=[
                {"type": "runes", "x": 8, "y": 3, "runes": ["Sulphur"], "dir": "right"},
                {
                    "type": "mite",
                    "x": 14,
                    "y": 4,
                    "displayName": "Stone man",
                    "formulaId": "stone-man",
                    "formula": ["Earth", "Salt"],
                    "keys": MIND,
                    "sprite": "ash-mite",
                    "blocking": True,
                },
                {
                    "type": "mite",
                    "x": 14,
                    "y": 6,
                    "displayName": "Stone man",
                    "formulaId": "stone-man",
                    "formula": ["Earth", "Salt"],
                    "keys": MIND,
                    "sprite": "ash-mite",
                    "blocking": True,
                },
                {
                    "type": "mite",
                    "x": 14,
                    "y": 8,
                    "displayName": "Stone man",
                    "formulaId": "stone-man",
                    "formula": ["Earth", "Salt"],
                    "keys": MIND,
                    "sprite": "ash-mite",
                    "blocking": True,
                },
                {"type": "item", "x": 14, "y": 11, "item": "mind-stone"},
            ],
        ),
        room(
            id="door-ii",
            name="Gate of Aspects",
            origin={"x": 16, "y": 83},
            width=21,
            height=11,
            wall="Stone",
            floor="Stone",
            exit="north",
            stamps=[
                stamp("Floor", "Crystal", rect(8, 4, 12, 6)),
                stamp("Floor", "SaltCrust", cells((10, 8))),
            ],
            props=[
                {
                    "type": "gate",
                    "x": 10,
                    "y": 7,
                    "displayName": "Gate of Aspects",
                    "formulaId": "door-ii",
                    "requires": ["body-stone", "spirit-stone", "mind-stone"],
                    "finishes": True,
                    "sprite": "rod",
                    "note": "Body, spirit, and mind take their seats. The floor opens.",
                },
            ],
        ),
    ]

    data = {
        "id": "foundation",
        "name": "The Foundation",
        "spawn": {"x": 22, "y": 2},
        "rooms": rooms,
        "halls": [
            {"from": "antechamber", "to": "hub", "material": "Stone"},
            {"from": "earth-wing", "to": "hub", "material": "Stone"},
            {"from": "fire-wing", "to": "hub", "material": "Ice"},
            {"from": "hub", "to": "water-wing", "material": "Stone"},
            {"from": "fire-wing", "to": "air-wing", "material": "Scoured"},
            {"from": "hub", "to": "door-i", "material": "Stone"},
            {"from": "air-wing", "to": "door-i", "material": "Scoured"},
            {"from": "door-i", "to": "aspect-foyer", "material": "Stone"},
            {"from": "body-sanctum", "to": "aspect-foyer", "material": "SaltCrust"},
            {"from": "aspect-foyer", "to": "spirit-sanctum", "material": "Vein"},
            {"from": "aspect-foyer", "to": "mind-sanctum", "material": "Stone"},
            {"from": "mind-sanctum", "to": "door-ii", "material": "Crystal"},
        ],
    }
    validate(data)
    dest = Path(__file__).resolve().parents[1] / "Assets/Resources/Maps/foundation.json"
    dest.write_text(json.dumps(data, indent=2) + "\n")
    print(f"wrote {dest} ({len(data['rooms'])} rooms)")


if __name__ == "__main__":
    main()
