#!/usr/bin/env python3
"""Copy a PNG into Assets/Resources/Sprites and register it in art.json."""

from __future__ import annotations

import argparse
import json
import shutil
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SPRITES = ROOT / "Assets" / "Resources" / "Sprites"
ART = ROOT / "Assets" / "Resources" / "Catalog" / "art.json"


def main() -> None:
    parser = argparse.ArgumentParser(description="Install a PNG as a named game sprite.")
    parser.add_argument("png", type=Path, help="Source PNG (or other image Unity can import)")
    parser.add_argument("--id", required=True, help="Sprite id, e.g. adept or adept-walk")
    parser.add_argument("--ppu", type=float, default=16.0, help="Pixels per unit (default 16)")
    parser.add_argument("--pivot", default="0.5,0.5", help="Pivot as x,y (adept: 0.5,0.22)")
    args = parser.parse_args()

    source = args.png.expanduser().resolve()
    if not source.is_file():
        raise SystemExit(f"No file at {source}")

    sprite_id = args.id.strip()
    if not sprite_id:
        raise SystemExit("Sprite id is empty")

    SPRITES.mkdir(parents=True, exist_ok=True)
    dest = SPRITES / f"{sprite_id}{source.suffix.lower() or '.png'}"
    shutil.copy2(source, dest)

    art = {"note": "", "sprites": [], "items": []}
    if ART.is_file():
        art = json.loads(ART.read_text())
        art.setdefault("sprites", [])
        art.setdefault("items", [])

    row = None
    for item in art["sprites"]:
        if (item.get("id") or "").lower() == sprite_id.lower():
            row = item
            break
    if row is None:
        row = {"id": sprite_id}
        art["sprites"].append(row)

    row["id"] = sprite_id
    row["source"] = f"Sprites/{sprite_id}"
    row["pixelsPerUnit"] = args.ppu
    row["pivot"] = args.pivot
    ART.write_text(json.dumps(art, indent=2) + "\n")
    print(f"Installed {dest.relative_to(ROOT)}")
    print(f"Registered {sprite_id} → {row['source']} (ppu {args.ppu}, pivot {args.pivot})")


if __name__ == "__main__":
    main()
