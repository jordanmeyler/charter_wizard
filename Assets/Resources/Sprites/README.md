# Sprites

Drop a PNG here whose filename matches a sprite id (`adept.png`, `fire-golem.png`, `adept-walk.png`). The game loads it at boot and uses point filtering.

`dungeon-atlas.png` is the floor/wall/door/prop sheet. Named slices live in `Catalog/tiles.json`. See [`TILES.md`](../../../TILES.md).

`python3 Tools/import-sprite.py path/to/file.png --id adept`

See [`ART.md`](../../../ART.md) for sizes, pivots, and where to get better art.
