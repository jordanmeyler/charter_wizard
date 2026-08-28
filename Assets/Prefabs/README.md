# Prefabs

Drag these into the Scene. That is the usual Unity path.

`Assets/Prefabs/Items` has one prefab per stone (Fire Stone, Water Stone, …). `Gate` and `Door` sit next to them. `Window → Rune Magic → Authoring → Place` and `GameObject → Rune Magic` instantiate the same files.

`Create / refresh prefabs` writes any missing type (Item, Mite, Torch, Rod, Gate, Door, Barrier, Plaque, Crystal, Charm, Adept, and the stones). A Door has closed and open sprites; drag it onto a Gate's **Doors** list.

Pack enemies are not prefab files — place them with **GameObject → Rune Magic → Enemies** or the Authoring window. `Main.unity` already has a few next to spawn.
