# Prefabs

Drag these into the Scene. That is the usual Unity path.

Stones start in `Assets/Prefabs/Items`. You can move that folder, rename it, or nest it — Play does not care, and Authoring Place / `GameObject → Rune Magic` find a prefab by file name anywhere under `Assets/Prefabs`. `Gate` and `Door` sit next to them.

Each stone has a **Description** (pack inspect and `You see`) and a **Pickup line**. Edit those on the prefab, on a placed instance, or all at once in `Window → Rune Magic → Catalog`. Empty fields fall back to the catalog row in `art.json`.

`Create / refresh prefabs` writes any missing type (Item, Mite, Torch, Rod, Gate, Electric Gate, Door, Barrier, Plaque, Interact, Crystal, Charm, Adept, stones, pack enemies, and Custom). **Interact** is an empty use volume for prayer — no sprite, so you can parent tiles or child sprites for the statue. A Door has closed and open sprites; drag it onto a Gate or Electric Gate **Doors** list. An Electric Gate opens when lightning or charge finds it.

Enemies live in `Assets/Prefabs/Enemies` (Golem, Warden, Shade, **Custom**, …). Drag one into the Scene the same way as a stone. **Custom** is a blank Hunt + slam you dress in the Inspector (mode, close / mid / long slots, gambits, resistances). Golem, Warden, and Cultist already have ElvGames idle / attack slices on Portrait, Idle Frames, and Attack Frames. Drag different slices, or run **Fill empty frames from pack** on the Inspector. `Window → Rune Magic → Bind Enemy Sprites` fills any pack enemy that is still blank.

To keep a dressed body: set **Name**, tweak resistances / attacks / gambits, then **Save as prefab** on the Inspector. That writes `Assets/Prefabs/Enemies/{Name}.prefab`. Authoring lists extras under **Saved enemies**. See [`ENEMIES.md`](../../ENEMIES.md). Move the folder if you want — Place still finds them by name.
