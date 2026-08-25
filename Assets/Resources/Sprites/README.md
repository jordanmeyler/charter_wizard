# Sprites

16px tiles live in `Rogue/`. Enemy strips live in `Enemies/`. Named slices are in `Catalog/tiles.json` and `Catalog/art.json`.

The grid is one world unit per 16×16 tile (16 PPU). Paint the map in `Assets/Scenes/Main.unity`. Drop enemies with **GameObject → Rune Magic → Enemies**.

`python3 Tools/import-sprite.py path/to/file.png --id adept --ppu 16`
