# Prefabs

Drag these into the Scene. That is the usual Unity path.

Stones start in `Assets/Prefabs/Items`. You can move that folder, rename it, or nest it — Play does not care, and Authoring Place / `GameObject → Rune Magic` find a prefab by file name anywhere under `Assets/Prefabs`. `Gate` and `Door` sit next to them.

`Create / refresh prefabs` writes any missing type (Item, Mite, Torch, Rod, Gate, Electric Gate, Door, Barrier, Plaque, Interact, Crystal, Charm, Adept, stones, and pack enemies). **Interact** is an empty use volume for prayer — no sprite, so you can parent tiles or child sprites for the statue. A Door has closed and open sprites; drag it onto a Gate or Electric Gate **Doors** list. An Electric Gate opens when lightning or charge finds it.

Enemies live in `Assets/Prefabs/Enemies` (Shade, Squire, …). Drag one into the Scene the same way as a stone. The other agent can swap portraits and clips on those prefabs; placement does not depend on the art. Move the folder if you want — Place still finds them by name.
