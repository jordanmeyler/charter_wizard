#!/usr/bin/env python3
"""Paint a 32px dungeon atlas and write Catalog/tiles.json.

Cell (0, 0) is the top-left of the PNG. Unity slices flip Y.
Regenerate with: python3 Tools/build-dungeon-atlas.py
"""

from __future__ import annotations

import json
import struct
import zlib
from pathlib import Path

CELL = 32
COLS = 12
ROWS = 8

# Warm dungeon palette — grey stone, yellow top-left light, earth, water.
CLEAR = (0, 0, 0, 0)
STONE = (86, 84, 90, 255)
STONE_LT = (118, 112, 104, 255)
STONE_DK = (52, 50, 56, 255)
GROUT = (38, 36, 40, 255)
DIRT = (92, 68, 42, 255)
DIRT_LT = (128, 96, 58, 255)
DIRT_DK = (58, 40, 24, 255)
PEBBLE = (110, 108, 102, 255)
WATER = (36, 78, 132, 255)
WATER_LT = (72, 140, 186, 255)
WATER_DK = (18, 42, 78, 255)
WOOD = (118, 72, 36, 255)
WOOD_DK = (72, 42, 20, 255)
WOOD_LT = (156, 102, 52, 255)
MOSS = (46, 92, 38, 255)
MOSS_LT = (78, 132, 52, 255)
BLOOD = (92, 18, 22, 255)
ICE = (168, 210, 230, 255)
ICE_DK = (98, 150, 186, 255)
FIRE = (230, 92, 22, 255)
FIRE_LT = (255, 196, 64, 255)
CHARGE = (240, 220, 70, 255)
POISON = (78, 168, 42, 255)
SMOKE = (210, 210, 214, 255)
METAL = (72, 76, 82, 255)
GOLD = (210, 168, 64, 255)
ASH = (64, 58, 54, 255)
MUD = (72, 54, 32, 255)


def clamp(v, lo=0, hi=255):
    return lo if v < lo else hi if v > hi else v


class Canvas:
    def __init__(self, width, height):
        self.w = width
        self.h = height
        self.px = [CLEAR] * (width * height)

    def set(self, x, y, color):
        if 0 <= x < self.w and 0 <= y < self.h:
            self.px[y * self.w + x] = color

    def get(self, x, y):
        if 0 <= x < self.w and 0 <= y < self.h:
            return self.px[y * self.w + x]
        return CLEAR

    def blend(self, x, y, color):
        if not (0 <= x < self.w and 0 <= y < self.h):
            return
        a = color[3] / 255.0
        if a <= 0:
            return
        under = self.px[y * self.w + x]
        self.px[y * self.w + x] = (
            int(under[0] + (color[0] - under[0]) * a),
            int(under[1] + (color[1] - under[1]) * a),
            int(under[2] + (color[2] - under[2]) * a),
            clamp(int(under[3] + (255 - under[3]) * a)),
        )

    def fill(self, x, y, w, h, color):
        for py in range(y, y + h):
            for px in range(x, x + w):
                self.set(px, py, color)

    def rect(self, x, y, w, h, color):
        for px in range(x, x + w):
            self.set(px, y, color)
            self.set(px, y + h - 1, color)
        for py in range(y, y + h):
            self.set(x, py, color)
            self.set(x + w - 1, py, color)

    def circle(self, cx, cy, r, color, fill=True):
        for y in range(cy - r, cy + r + 1):
            for x in range(cx - r, cx + r + 1):
                d2 = (x - cx) * (x - cx) + (y - cy) * (y - cy)
                if fill and d2 <= r * r:
                    self.blend(x, y, color)
                elif not fill and abs(d2 - r * r) <= r:
                    self.set(x, y, color)

    def line(self, x0, y0, x1, y1, color):
        dx = abs(x1 - x0)
        dy = -abs(y1 - y0)
        sx = 1 if x0 < x1 else -1
        sy = 1 if y0 < y1 else -1
        err = dx + dy
        while True:
            self.set(x0, y0, color)
            if x0 == x1 and y0 == y1:
                break
            e2 = 2 * err
            if e2 >= dy:
                err += dy
                x0 += sx
            if e2 <= dx:
                err += dx
                y0 += sy

    def noise(self, x, y, w, h, color, step, seed):
        rng = seed
        for py in range(y, y + h, 1):
            for px in range(x, x + w, 1):
                rng = (rng * 1103515245 + 12345 + px * 17 + py * 31) & 0x7FFFFFFF
                if rng % step == 0:
                    self.blend(px, py, color)

    def blit(self, dest_x, dest_y, other):
        for y in range(other.h):
            for x in range(other.w):
                c = other.get(x, y)
                if c[3] > 0:
                    self.set(dest_x + x, dest_y + y, c)

    def png(self) -> bytes:
        raw = b""
        for y in range(self.h):
            raw += b"\x00"
            for x in range(self.w):
                raw += bytes(self.px[y * self.w + x])

        def chunk(tag: bytes, data: bytes) -> bytes:
            return struct.pack(">I", len(data)) + tag + data + struct.pack(
                ">I", zlib.crc32(tag + data) & 0xFFFFFFFF
            )

        ihdr = struct.pack(">IIBBBBB", self.w, self.h, 8, 6, 0, 0, 0)
        return b"".join(
            [
                b"\x89PNG\r\n\x1a\n",
                chunk(b"IHDR", ihdr),
                chunk(b"IDAT", zlib.compress(raw, 9)),
                chunk(b"IEND", b""),
            ]
        )


def cell_canvas():
    return Canvas(CELL, CELL)


def cobble(seed=1, cracked=False, mossy=False):
    c = cell_canvas()
    c.fill(0, 0, 32, 32, STONE)
    blocks = [(1, 1, 14, 14), (16, 1, 15, 14), (1, 16, 19, 15), (21, 16, 10, 15)]
    tones = [STONE, STONE_LT, STONE_DK, (96, 90, 84, 255)]
    for i, (x, y, w, h) in enumerate(blocks):
        tone = tones[(i + seed) % len(tones)]
        c.fill(x, y, w, h, tone)
        c.rect(x, y, w, h, GROUT)
        c.fill(x + 1, y + 1, max(1, w - 4), 1, (*STONE_LT[:3], 90))
    c.rect(0, 0, 32, 32, GROUT)
    c.noise(0, 0, 32, 32, (*STONE_LT[:3], 70), 11 + seed, seed * 9)
    if cracked:
        c.line(6, 8, 18, 22, GROUT)
        c.line(18, 22, 26, 14, GROUT)
        c.line(14, 16, 20, 28, STONE_DK)
    if mossy:
        c.blend(4, 22, (*MOSS[:3], 160))
        c.circle(8, 26, 3, (*MOSS[:3], 140))
        c.circle(22, 6, 2, (*MOSS_LT[:3], 120))
    return c


def dirt(seed=1):
    c = cell_canvas()
    c.fill(0, 0, 32, 32, DIRT)
    c.noise(0, 0, 32, 32, (*DIRT_LT[:3], 120), 6, seed)
    c.noise(0, 0, 32, 32, (*DIRT_DK[:3], 140), 8, seed + 3)
    for i, (x, y) in enumerate(((4, 7), (12, 18), (22, 9), (18, 24), (8, 26), (26, 20))):
        tone = PEBBLE if (i + seed) % 2 == 0 else DIRT_DK
        c.circle(x, y, 1 + (i % 2), tone)
    return c


def water(frame=0):
    c = cell_canvas()
    c.fill(0, 0, 32, 32, WATER_DK)
    c.fill(2, 2, 28, 28, WATER)
    shift = frame * 3
    for i in range(4):
        y = 6 + ((i * 6 + shift) % 20)
        c.line(4, y, 28, y, (*WATER_LT[:3], 90))
    c.circle(16, 16, 5, (*WATER_LT[:3], 50))
    c.rect(0, 0, 32, 32, STONE_DK)
    c.rect(1, 1, 30, 30, (*STONE[:3], 180))
    jagged = ((2, 2), (8, 1), (18, 2), (28, 1), (30, 8), (30, 22), (28, 30), (12, 30), (2, 28), (1, 14))
    for x, y in jagged:
        c.set(x, y, STONE)
    return c


def pit():
    c = cell_canvas()
    c.fill(0, 0, 32, 32, (8, 6, 8, 255))
    c.rect(0, 0, 32, 32, STONE_DK)
    c.rect(2, 2, 28, 28, (20, 14, 16, 255))
    c.fill(6, 6, 20, 20, (4, 2, 4, 255))
    c.fill(10, 10, 12, 12, (0, 0, 0, 255))
    c.line(4, 8, 10, 4, STONE)
    c.line(22, 4, 28, 10, STONE)
    return c


def pit_edge():
    c = cobble(2)
    c.fill(0, 18, 32, 14, (4, 2, 4, 255))
    c.line(0, 17, 31, 17, STONE_DK)
    c.line(0, 16, 31, 16, STONE)
    return c


def mud():
    c = dirt(4)
    c.noise(0, 0, 32, 32, (*MUD[:3], 160), 5, 11)
    c.circle(12, 14, 4, (*WATER_DK[:3], 70))
    return c


def ash():
    c = cobble(5)
    c.noise(0, 0, 32, 32, (*ASH[:3], 180), 5, 2)
    c.noise(0, 0, 32, 32, (40, 36, 34, 140), 7, 8)
    return c


def wall(seed=1, mossy=False, cracked=False, cave=False):
    c = cell_canvas()
    c.fill(0, 0, 32, 32, STONE_DK)
    rows = [(0, 0, 32, 10), (0, 11, 32, 10), (0, 22, 32, 10)]
    for i, (x, y, w, h) in enumerate(rows):
        offset = (i % 2) * 8
        c.fill(x, y, w, h, STONE if (i + seed) % 2 == 0 else (76, 74, 80, 255))
        c.line(0, y, 31, y, GROUT)
        c.line(offset, y, offset, y + h - 1, GROUT)
        c.line(offset + 16, y, offset + 16, y + h - 1, GROUT)
        c.fill(2, y + 1, 8, 1, (*STONE_LT[:3], 80))
    c.rect(0, 0, 32, 32, GROUT)
    if mossy:
        c.circle(4, 4, 4, (*MOSS[:3], 170))
        c.line(6, 0, 8, 14, MOSS_LT)
        c.line(10, 0, 9, 18, MOSS)
    if cracked:
        c.line(10, 2, 18, 28, GROUT)
        c.line(18, 16, 26, 30, STONE_DK)
    if cave:
        c.circle(16, 18, 11, (8, 6, 8, 255))
        c.circle(16, 18, 7, (0, 0, 0, 255))
        c.line(8, 8, 12, 4, STONE)
        c.line(22, 6, 26, 10, STONE)
    return c


def wall_corner(inner=True):
    c = wall(3)
    if inner:
        c.fill(0, 0, 12, 32, STONE_DK)
        c.line(12, 0, 12, 31, GROUT)
    else:
        c.fill(20, 0, 12, 12, CLEAR)
        c.line(20, 12, 31, 12, GROUT)
        c.line(20, 0, 20, 12, GROUT)
    return c


def column(broken=False):
    c = cell_canvas()
    c.fill(10, 4 if not broken else 16, 12, 28 if not broken else 16, STONE)
    c.rect(10, 4 if not broken else 16, 12, 28 if not broken else 16, GROUT)
    c.fill(9, 2 if not broken else 14, 14, 4, STONE_LT)
    if not broken:
        c.fill(9, 28, 14, 4, STONE_DK)
        c.line(13, 6, 13, 26, STONE_LT)
    else:
        c.line(10, 16, 16, 12, STONE_DK)
        c.fill(12, 28, 4, 3, STONE_DK)
    return c


def stalagmite():
    c = cell_canvas()
    c.fill(12, 18, 8, 14, STONE)
    c.fill(10, 22, 12, 10, STONE_DK)
    c.line(16, 8, 12, 22, STONE_LT)
    c.line(16, 8, 20, 22, STONE)
    c.set(16, 7, PEBBLE)
    return c


def arch(open_way=True, pillar=False):
    c = cell_canvas()
    c.fill(0, 0, 32, 32, STONE)
    c.rect(0, 0, 32, 32, GROUT)
    if pillar:
        c.fill(4, 0, 8, 32, STONE_LT)
        c.fill(20, 0, 8, 32, STONE_LT)
        c.fill(8, 4, 16, 20, (10, 8, 10, 255) if open_way else WOOD)
        c.rect(4, 0, 8, 32, GROUT)
        c.rect(20, 0, 8, 32, GROUT)
    else:
        c.fill(6, 8, 20, 24, (8, 6, 8, 255) if open_way else STONE_DK)
        c.circle(16, 12, 10, STONE)
        if open_way:
            c.circle(16, 14, 7, (8, 6, 8, 255))
        else:
            c.fill(8, 10, 16, 20, WOOD)
        c.fill(3, 6, 6, 26, STONE_LT)
        c.fill(23, 6, 6, 26, STONE_LT)
        c.circle(6, 10, 2, (70, 66, 62, 255))
        c.circle(26, 10, 2, (70, 66, 62, 255))
    return c


def door(open_way=False):
    """32x64 leaf so the adept (64px) reads as able to pass."""
    c = Canvas(32, 64)
    c.fill(0, 0, 32, 64, STONE)
    c.fill(2, 2, 28, 60, STONE_LT)
    c.rect(0, 0, 32, 64, GROUT)
    c.fill(3, 4, 6, 56, STONE)
    c.fill(23, 4, 6, 56, STONE)
    c.circle(7, 12, 2, (70, 66, 62, 255))
    c.circle(25, 12, 2, (70, 66, 62, 255))
    if open_way:
        c.fill(9, 10, 14, 48, (6, 4, 8, 255))
        c.fill(20, 12, 6, 44, WOOD)
        c.line(21, 14, 21, 52, WOOD_DK)
    else:
        c.fill(9, 10, 14, 48, WOOD)
        c.line(16, 10, 16, 57, WOOD_DK)
        c.line(9, 26, 22, 26, WOOD_DK)
        c.line(9, 42, 22, 42, WOOD_DK)
        c.line(11, 12, 11, 55, WOOD_LT)
        c.circle(20, 36, 2, GOLD)
        c.set(20, 36, (240, 210, 110, 255))
        c.rect(9, 10, 14, 48, WOOD_DK)
    return c


def torch(kind="lit"):
    c = cell_canvas()
    c.fill(0, 8, 32, 24, STONE)
    c.rect(0, 8, 32, 24, GROUT)
    c.fill(13, 14, 6, 10, METAL)
    if kind == "empty":
        c.rect(13, 14, 6, 10, STONE_DK)
        return c
    c.fill(14, 8, 4, 10, WOOD)
    if kind == "lit":
        c.circle(16, 7, 4, FIRE)
        c.circle(16, 5, 2, FIRE_LT)
        c.set(16, 3, (255, 240, 180, 255))
    else:
        c.fill(14, 6, 4, 4, WOOD_DK)
    return c


def brazier(lit=True):
    c = cell_canvas()
    c.line(8, 28, 16, 16, METAL)
    c.line(24, 28, 16, 16, METAL)
    c.line(16, 28, 16, 16, METAL)
    c.fill(10, 14, 12, 5, METAL)
    c.rect(10, 14, 12, 5, STONE_DK)
    if lit:
        c.circle(16, 10, 5, FIRE)
        c.circle(16, 8, 3, FIRE_LT)
        c.set(16, 5, (255, 240, 160, 255))
    return c


def overlay_moss(variant=0):
    c = cell_canvas()
    spots = (
        ((6, 22), (12, 26), (8, 18), (20, 24))
        if variant == 0
        else ((4, 8), (10, 4), (18, 10), (24, 6), (14, 14))
    )
    for x, y in spots:
        c.circle(x, y, 3 + variant, (*MOSS[:3], 170))
        c.circle(x + 1, y - 1, 2, (*MOSS_LT[:3], 120))
    if variant:
        c.line(10, 0, 12, 16, (*MOSS_LT[:3], 200))
        c.line(18, 0, 16, 20, (*MOSS[:3], 180))
    return c


def overlay_vine():
    c = cell_canvas()
    c.line(8, 0, 10, 28, (*MOSS[:3], 210))
    c.line(20, 0, 18, 24, (*MOSS_LT[:3], 200))
    c.circle(10, 12, 2, (*MOSS_LT[:3], 180))
    c.circle(18, 18, 2, (*MOSS[:3], 180))
    c.circle(12, 22, 2, (*MOSS_LT[:3], 160))
    return c


def overlay_crack(n=0):
    c = cell_canvas()
    paths = [
        [(6, 8), (14, 16), (22, 12), (26, 22)],
        [(4, 16), (16, 10), (20, 24)],
        [(10, 4), (12, 18), (18, 28)],
    ][n % 3]
    for i in range(len(paths) - 1):
        c.line(*paths[i], *paths[i + 1], (*GROUT[:3], 220))
    return c


def overlay_seal():
    c = cell_canvas()
    c.circle(16, 16, 10, (*ICE_DK[:3], 0))
    c.circle(16, 16, 10, (*GOLD[:3], 180), fill=False)
    c.circle(16, 16, 6, (*GOLD[:3], 160), fill=False)
    c.line(16, 6, 16, 26, (*GOLD[:3], 160))
    c.line(6, 16, 26, 16, (*GOLD[:3], 160))
    c.line(9, 9, 23, 23, (*GOLD[:3], 120))
    c.line(23, 9, 9, 23, (*GOLD[:3], 120))
    return c


def overlay_blood():
    c = cell_canvas()
    c.circle(14, 16, 5, (*BLOOD[:3], 180))
    c.circle(20, 12, 3, (*BLOOD[:3], 150))
    c.circle(10, 22, 2, (*BLOOD[:3], 140))
    c.set(24, 18, BLOOD)
    c.set(8, 12, BLOOD)
    return c


def overlay_ice():
    c = cell_canvas()
    c.fill(2, 2, 28, 28, (*ICE[:3], 90))
    c.line(6, 8, 14, 24, (*ICE_DK[:3], 180))
    c.line(18, 6, 24, 22, (255, 255, 255, 100))
    c.rect(3, 3, 26, 26, (*ICE[:3], 80))
    return c


def overlay_metal():
    c = cell_canvas()
    c.fill(4, 4, 24, 24, (*METAL[:3], 140))
    c.rect(4, 4, 24, 24, (*STONE_LT[:3], 160))
    c.line(6, 8, 26, 8, (200, 200, 210, 80))
    return c


def overlay_plant():
    c = overlay_moss(0)
    c.circle(16, 14, 4, (*MOSS_LT[:3], 160))
    c.line(16, 18, 16, 28, (*MOSS[:3], 200))
    return c


def fx_blob(color, radius=8):
    c = cell_canvas()
    c.circle(16, 16, radius, (*color[:3], 140))
    c.circle(16, 14, max(2, radius - 3), (*color[:3], 90))
    return c


def fx_fire():
    c = cell_canvas()
    c.circle(16, 20, 7, (*FIRE[:3], 140))
    c.circle(16, 14, 5, (*FIRE_LT[:3], 160))
    c.circle(12, 18, 3, (*FIRE[:3], 120))
    c.circle(20, 18, 3, (*FIRE[:3], 120))
    c.set(16, 8, (255, 240, 180, 200))
    return c


def fx_smoke(big=False):
    c = cell_canvas()
    c.circle(14, 20, 5 if big else 3, (*SMOKE[:3], 90))
    c.circle(18, 14, 4 if big else 2, (*SMOKE[:3], 80))
    c.circle(16, 8, 3 if big else 2, (*SMOKE[:3], 60))
    return c


def fx_ripple(frame=0):
    c = cell_canvas()
    r = 5 + frame * 3
    c.circle(16, 16, r, (*WATER_LT[:3], 0), fill=False)
    for a in range(-r, r + 1):
        for b in range(-r, r + 1):
            d = abs(a * a + b * b - r * r)
            if d <= r:
                c.blend(16 + a, 16 + b, (*WATER_LT[:3], 140))
    return c


def bush(big=False):
    c = cell_canvas()
    r = 9 if big else 7
    c.circle(16, 18, r, (*MOSS[:3], 230))
    c.circle(12, 16, r - 2, (*MOSS_LT[:3], 180))
    c.circle(20, 16, r - 3, (*MOSS[:3], 200))
    c.fill(15, 24, 3, 6, DIRT_DK)
    return c


def ice_object(kind="fountain"):
    c = cell_canvas()
    if kind == "chest":
        c.fill(8, 16, 16, 10, STONE_DK)
        c.fill(7, 14, 18, 12, (*ICE[:3], 180))
        c.rect(7, 14, 18, 12, ICE_DK)
        c.line(7, 20, 24, 20, ICE_DK)
        c.circle(16, 20, 1, GOLD)
    else:
        c.fill(10, 20, 12, 8, STONE)
        c.fill(12, 12, 8, 10, STONE_LT)
        c.fill(8, 8, 16, 16, (*ICE[:3], 170))
        c.rect(8, 8, 16, 16, ICE_DK)
        c.line(16, 6, 12, 18, (230, 240, 255, 160))
    return c


def water_fountain():
    c = cell_canvas()
    c.fill(10, 20, 12, 8, STONE)
    c.fill(12, 14, 8, 8, STONE_LT)
    c.circle(16, 12, 5, WATER)
    c.circle(16, 10, 3, WATER_LT)
    return c


def lightning_vial():
    c = cell_canvas()
    c.fill(12, 10, 8, 16, (40, 70, 120, 200))
    c.rect(12, 10, 8, 16, (80, 120, 180, 255))
    c.fill(13, 12, 6, 10, CHARGE)
    c.line(16, 6, 14, 12, CHARGE)
    c.line(16, 6, 19, 12, CHARGE)
    c.line(14, 8, 18, 10, (255, 255, 180, 255))
    return c


def lightning_pillar():
    c = column(False)
    c.line(12, 6, 18, 14, CHARGE)
    c.line(18, 14, 12, 22, CHARGE)
    c.line(12, 22, 20, 28, CHARGE)
    c.circle(16, 5, 3, (*CHARGE[:3], 180))
    return c


def lightning_splash():
    c = cell_canvas()
    c.circle(16, 20, 4, (40, 80, 140, 180))
    c.line(16, 18, 10, 6, CHARGE)
    c.line(16, 18, 22, 8, CHARGE)
    c.line(16, 18, 16, 4, (255, 255, 200, 255))
    return c


def hook_statue():
    c = cell_canvas()
    c.fill(12, 14, 8, 16, STONE_DK)
    c.fill(10, 10, 12, 6, STONE)
    c.line(16, 10, 22, 4, STONE_LT)
    c.line(22, 4, 24, 8, STONE)
    return c


def ring_mount():
    c = wall(1)
    c.circle(16, 16, 6, METAL, fill=False)
    c.circle(16, 16, 5, METAL, fill=False)
    c.circle(16, 16, 2, GOLD)
    return c


def tall_place(sheet: Canvas, col: int, top_row: int, sprite: Canvas):
    """Place a 32x64 sprite occupying (col, top_row) and the row below."""
    sheet.blit(col * CELL, top_row * CELL, sprite)


TILES = []


def add(tid, col, row, w=1, h=1, pivot="0.5,0.5", kind="tile", note=""):
    TILES.append(
        {
            "id": tid,
            "col": col,
            "row": row,
            "w": w,
            "h": h,
            "pivot": pivot,
            "kind": kind,
            "note": note,
        }
    )


def build():
    sheet = Canvas(COLS * CELL, ROWS * CELL)
    sheet.fill(0, 0, sheet.w, sheet.h, (0, 0, 0, 255))

    paints = {
        (0, 0): cobble(1),
        (1, 0): cobble(2),
        (2, 0): dirt(1),
        (3, 0): dirt(2),
        (4, 0): water(0),
        (5, 0): water(1),
        (6, 0): pit(),
        (7, 0): pit_edge(),
        (8, 0): cobble(3, cracked=True),
        (9, 0): mud(),
        (10, 0): ash(),
        (11, 0): dirt(5),
        (0, 1): wall(1),
        (1, 1): wall(2),
        (2, 1): wall_corner(True),
        (3, 1): wall_corner(False),
        (4, 1): wall(1, mossy=True),
        (5, 1): wall(2, cave=True),
        (6, 1): wall(3, cave=True),
        (7, 1): wall(1, cracked=True),
        (8, 1): wall(4),
        (9, 1): column(False),
        (10, 1): column(True),
        (11, 1): stalagmite(),
        (0, 2): arch(True, False),
        (3, 2): arch(True, True),
        (4, 2): torch("lit"),
        (5, 2): torch("empty"),
        (6, 2): torch("unlit"),
        (7, 2): brazier(True),
        (8, 2): brazier(False),
        (9, 2): ring_mount(),
        (10, 2): hook_statue(),
        (0, 3): arch(False, False),
        (3, 3): ice_object("fountain"),
        (4, 3): ice_object("chest"),
        (5, 3): water_fountain(),
        (6, 3): lightning_vial(),
        (7, 3): lightning_pillar(),
        (8, 3): lightning_splash(),
        (0, 4): overlay_moss(0),
        (1, 4): overlay_moss(1),
        (2, 4): overlay_vine(),
        (3, 4): overlay_crack(0),
        (4, 4): overlay_crack(1),
        (5, 4): overlay_crack(2),
        (6, 4): overlay_seal(),
        (7, 4): overlay_blood(),
        (8, 4): overlay_ice(),
        (9, 4): overlay_metal(),
        (10, 4): overlay_plant(),
        (11, 4): overlay_moss(0),
        (0, 5): fx_fire(),
        (1, 5): fx_blob(POISON, 8),
        (2, 5): fx_smoke(False),
        (3, 5): fx_smoke(True),
        (4, 5): fx_blob(CHARGE, 7),
        (5, 5): fx_blob(WATER_LT, 7),
        (6, 5): fx_blob(MOSS_LT, 7),
        (7, 5): fx_ripple(0),
        (8, 5): fx_ripple(1),
        (9, 5): bush(False),
        (10, 5): bush(True),
        (11, 5): fx_blob(FIRE, 6),
    }
    for (col, row), sprite in paints.items():
        sheet.blit(col * CELL, row * CELL, sprite)

    door_closed = door(False)
    door_open = door(True)
    tall_place(sheet, 1, 2, door_closed)
    tall_place(sheet, 2, 2, door_open)

    catalog = [
        ("floor-stone", 0, 0, 1, 1, "0.5,0.5", "floor", "Solid dungeon cobble. Default walk."),
        ("floor-stone-b", 1, 0, 1, 1, "0.5,0.5", "floor", "Cobble variant."),
        ("floor-dirt", 2, 0, 1, 1, "0.5,0.5", "floor", "Loose earth."),
        ("floor-dirt-b", 3, 0, 1, 1, "0.5,0.5", "floor", "Dirt variant."),
        ("floor-water", 4, 0, 1, 1, "0.5,0.5", "floor", "Stone-rimmed pool. Drowns."),
        ("floor-water-b", 5, 0, 1, 1, "0.5,0.5", "floor", "Water frame 2."),
        ("pit", 6, 0, 1, 1, "0.5,0.5", "floor", "Square pit."),
        ("pit-edge", 7, 0, 1, 1, "0.5,0.5", "floor", "Drop lip."),
        ("floor-cracked", 8, 0, 1, 1, "0.5,0.5", "floor", "Broken cobble. Covering baked in."),
        ("floor-mud", 9, 0, 1, 1, "0.5,0.5", "floor", "Dirt after water."),
        ("floor-ash", 10, 0, 1, 1, "0.5,0.5", "floor", "What fire leaves."),
        ("floor-pebble", 11, 0, 1, 1, "0.5,0.5", "floor", "Pebble dirt."),
        ("wall", 0, 1, 1, 1, "0.5,0.5", "wall", "Brick wall."),
        ("wall-b", 1, 1, 1, 1, "0.5,0.5", "wall", "Brick variant."),
        ("wall-corner-in", 2, 1, 1, 1, "0.5,0.5", "wall", "Inner corner."),
        ("wall-corner-out", 3, 1, 1, 1, "0.5,0.5", "wall", "Outer corner."),
        ("wall-moss", 4, 1, 1, 1, "0.5,0.5", "wall", "Mossy wall."),
        ("wall-cave", 5, 1, 1, 1, "0.5,0.5", "wall", "Cave mouth."),
        ("wall-cave-b", 6, 1, 1, 1, "0.5,0.5", "wall", "Cave mouth variant."),
        ("wall-crack", 7, 1, 1, 1, "0.5,0.5", "wall", "Cracked brick."),
        ("wall-c", 8, 1, 1, 1, "0.5,0.5", "wall", "Third brick variant."),
        ("pillar", 9, 1, 1, 1, "0.5,0.35", "prop", "Stone column."),
        ("pillar-broken", 10, 1, 1, 1, "0.5,0.3", "prop", "Broken stump."),
        ("stalagmite", 11, 1, 1, 1, "0.5,0.28", "prop", "Cave spike."),
        ("arch", 0, 2, 1, 1, "0.5,0.4", "door", "Open stone arch / door jamb."),
        ("door", 1, 2, 1, 2, "0.5,0.22", "door", "The one wooden door. 32x64, adept-sized."),
        ("door-open", 2, 2, 1, 2, "0.5,0.22", "door", "Same arch, leaf swung."),
        ("arch-pillar", 3, 2, 1, 1, "0.5,0.4", "door", "Square-pillar arch."),
        ("torch-lit", 4, 2, 1, 1, "0.5,0.45", "prop", "Lit wall torch."),
        ("torch-empty", 5, 2, 1, 1, "0.5,0.45", "prop", "Empty sconce."),
        ("torch-unlit", 6, 2, 1, 1, "0.5,0.45", "prop", "Unlit torch."),
        ("brazier-lit", 7, 2, 1, 1, "0.5,0.3", "prop", "Standing flame."),
        ("brazier", 8, 2, 1, 1, "0.5,0.3", "prop", "Cold brazier."),
        ("ring-mount", 9, 2, 1, 1, "0.5,0.5", "prop", "Wall ring."),
        ("hook-statue", 10, 2, 1, 1, "0.5,0.28", "prop", "Hooked stone."),
        ("arch-shut", 0, 3, 1, 1, "0.5,0.4", "door", "Sealed arch (jamb when shut)."),
        ("ice-fountain", 3, 3, 1, 1, "0.5,0.28", "prop", "Fountain caged in ice."),
        ("ice-chest", 4, 3, 1, 1, "0.5,0.3", "prop", "Chest / cask in ice."),
        ("water-fountain", 5, 3, 1, 1, "0.5,0.28", "prop", "Living fountain."),
        ("lightning-vial", 6, 3, 1, 1, "0.5,0.35", "prop", "Spark in glass."),
        ("lightning-pillar", 7, 3, 1, 1, "0.5,0.28", "prop", "Charged column. Use as storm rod."),
        ("lightning-splash", 8, 3, 1, 1, "0.5,0.35", "prop", "Spark leaving the vial."),
        ("cover-moss", 0, 4, 1, 1, "0.5,0.5", "cover", "Moss on stone or dirt."),
        ("cover-moss-b", 1, 4, 1, 1, "0.5,0.5", "cover", "Hanging moss."),
        ("cover-vine", 2, 4, 1, 1, "0.5,0.5", "cover", "Vines. Burns."),
        ("cover-crack", 3, 4, 1, 1, "0.5,0.5", "cover", "Large crack."),
        ("cover-crack-b", 4, 4, 1, 1, "0.5,0.5", "cover", "Medium crack."),
        ("cover-crack-c", 5, 4, 1, 1, "0.5,0.5", "cover", "Hairline crack."),
        ("cover-seal", 6, 4, 1, 1, "0.5,0.5", "cover", "Runic floor seal."),
        ("cover-blood", 7, 4, 1, 1, "0.5,0.5", "cover", "Blood splatter."),
        ("cover-ice", 8, 4, 1, 1, "0.5,0.5", "cover", "Ice after a freeze. Not a room floor."),
        ("cover-metal", 9, 4, 1, 1, "0.5,0.5", "cover", "Metal plate on stone."),
        ("cover-plant", 10, 4, 1, 1, "0.5,0.5", "cover", "Green growth."),
        ("cover-grove", 11, 4, 1, 1, "0.5,0.5", "cover", "Living mass."),
        ("fx-fire", 0, 5, 1, 1, "0.5,0.5", "fx", "Hunger on a tile."),
        ("fx-poison", 1, 5, 1, 1, "0.5,0.5", "fx", "Miasma / toxic gas."),
        ("fx-smoke", 2, 5, 1, 1, "0.5,0.5", "fx", "Smoke puff."),
        ("fx-smoke-b", 3, 5, 1, 1, "0.5,0.5", "fx", "Larger smoke."),
        ("fx-charge", 4, 5, 1, 1, "0.5,0.5", "fx", "Spark walking the floor."),
        ("fx-wet", 5, 5, 1, 1, "0.5,0.5", "fx", "Water on stone."),
        ("fx-grow", 6, 5, 1, 1, "0.5,0.5", "fx", "Growth tick."),
        ("fx-ripple", 7, 5, 1, 1, "0.5,0.5", "fx", "Water ripple."),
        ("fx-ripple-b", 8, 5, 1, 1, "0.5,0.5", "fx", "Wider ripple."),
        ("bush", 9, 5, 1, 1, "0.5,0.28", "prop", "Shrub."),
        ("bush-b", 10, 5, 1, 1, "0.5,0.28", "prop", "Larger shrub."),
        ("fx-ember", 11, 5, 1, 1, "0.5,0.5", "fx", "Low ember wash."),
    ]
    for row in catalog:
        add(*row)

    aliases = {
        "torch": "torch-unlit",
        "torch-lit": "torch-lit",
        "rod": "lightning-pillar",
        "rod-live": "lightning-pillar",
        "ice-block": "ice-chest",
        "tile-fire": "fx-fire",
        "tile-poison": "fx-poison",
        "tile-fog": "fx-smoke-b",
        "tile-charge": "fx-charge",
        "tile-wet": "fx-wet",
        "tile-grow": "fx-grow",
        "door": "door",
        "door-open": "door-open",
    }

    root = Path(__file__).resolve().parents[1]
    png_path = root / "Assets/Resources/Sprites/dungeon-atlas.png"
    json_path = root / "Assets/Resources/Catalog/tiles.json"
    png_path.parent.mkdir(parents=True, exist_ok=True)
    png_path.write_bytes(sheet.png())
    payload = {
        "note": "Dungeon atlas. Floors are stone, dirt, or water. Ice, fire, and lightning are coverings, props, or FX — they swap onto a base tile. One door (door / door-open), 32x64 so the adept fits. Drop a replacement PNG on the same 32px grid and keep this catalog.",
        "source": "Sprites/dungeon-atlas",
        "cell": CELL,
        "columns": COLS,
        "rows": ROWS,
        "pixelsPerUnit": 32,
        "tiles": TILES,
        "aliases": [{"id": k, "tile": v} for k, v in aliases.items()],
        "bases": {
            "stone": ["floor-stone", "floor-stone-b", "floor-cracked"],
            "dirt": ["floor-dirt", "floor-dirt-b", "floor-pebble", "floor-mud", "floor-ash"],
            "water": ["floor-water", "floor-water-b"],
        },
        "covers": {
            "Moss": "cover-moss",
            "Plant": "cover-plant",
            "Grove": "cover-grove",
            "Ice": "cover-ice",
            "Snow": "cover-ice",
            "Glacier": "cover-ice",
            "Metal": "cover-metal",
            "Crystal": "cover-seal",
            "Ash": None,
            "Mud": None,
            "Acid": "fx-poison",
            "Damp": "fx-wet",
        },
    }
    json_path.write_text(json.dumps(payload, indent=2) + "\n")
    print(f"wrote {png_path.relative_to(root)} ({sheet.w}x{sheet.h})")
    print(f"wrote {json_path.relative_to(root)} ({len(TILES)} tiles)")


if __name__ == "__main__":
    build()
