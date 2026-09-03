# Enemies

Place them like any other Unity prefab. The lock, the picture, and the
strike are all on `EncounterLock` — one Inspector.

Golem and Warden are already dressed. Drag them in and press Play.

## Place

Three ways. They all drop the same prefab from `Assets/Prefabs/Enemies`.

1. Drag **Golem** or **Warden** from `Assets/Prefabs/Enemies` into the
   Scene view. Snap with `GameObject → Rune Magic → Snap Selection To Grid`.
2. `GameObject → Rune Magic → Enemies → Golem` (or **Stone Golem**,
   same prefab) / **Warden**. **Cultist** is a second wizard.
3. `Window → Rune Magic → Authoring` → **Place** next to the name.

The body sits on the 16×16 grid. One cell is one tile.

| Prefab | Id | Attack | What Play does |
|---|---|---|---|
| **Golem** | `golem` | Golem | Slams anyone in reach. Hop or Stoneskin survives it. Earth body. |
| **Warden** | `warden` | Wizard | Writes a fireball for two seconds and commits facing when the sentence starts. Ensouled. |
| **Cultist** | `cultist` | Wizard | Same strike as the Warden. Another robe if you want two casters. |
| **Mite** | `ash-mite` | None | Blank lock. Set formula / attack yourself. |

The Silent Court's stone men are a **Mite** with Id `stone-man`, Attack
**None**, and Blocking on. They do not slam. Charm, Command, Lull,
Terror, Jolt, or Rage turn them.

Empty **Keys** means the usual mite list (Fireball, Charm, Command, …).
**Grant item** is a catalog id dropped when the lock turns (`mind-stone`).

## Connect sprites

The usual Unity path: slice a texture, drag the slices onto the
component. Generated painters stay as a fallback.

1. Open `Assets/ElvGames/Rogue Adventure/Enemies`.
2. Each enemy is four sheets. **A** is idle. **B** is walk. **C** is
   slam or cast. **D** is the resolve (the body coming apart).
3. The sheets are already sliced — unfold `Enemy_011_A` in the Project
   window and you see `Enemy_011_A_0` … Frames **0–5** are one facing.
4. Select the Golem (or Warden) in the Scene or the prefab.
5. Drag `Enemy_011_A_0` onto **Portrait**. The Scene view shows it.
6. Drag `Enemy_011_A_0` … `A_5` onto **Idle Frames**. Play loops them.
7. Drag `Enemy_011_C_0` … `C_5` onto **Attack Frames**. The slam (or
   the wizard's write) plays those.
8. Optional: drag `Enemy_011_D_*` onto **Resolve Frames** so the unmake
   plays a clip before the object goes.

Golem is `Enemy_011`. Warden is `Enemy_012`. Cultist is `Enemy_008`.

**Fill empty frames from pack** on the Inspector does steps 5–8 for
the current **Sprite Id** (`enemy-011`, `enemy-012`, …). **Replace
frames from pack** overwrites what you already dragged. The same bind
runs from `Window → Rune Magic → Bind Enemy Sprites` and from
**Bind Pack Sprites** (empty slots only, so your own art stays).

**Sprite Id** is the fallback if Portrait and Idle Frames are empty.
Pack ids are `enemy-001` … `enemy-012`. Named painters (`fire-golem`,
`stone-man`, `warden`) still work. **Idle Clip** / **Attack Clip** are
only needed when you want a sheet id instead of dragged frames
(`fire-golem-slam`, `warden-cast`).

Do not assign the unsliced strips under `Assets/Resources/Sprites/Enemies`
to Portrait — those are the catalog fallback, 16 PPU, and they read as
two tiles tall. The ElvGames slices are 32 PPU and sit on one cell.

## Set the attack

On the same Inspector, **Attack**:

| Attack | Strike | Cast seconds | Empty Cast recipe |
|---|---|---|---|
| **Golem** | Slam in reach | ~0.85s windup | — |
| **Wizard** | Fireball, facing locked when they start writing | 2 | `Fire · Mercury` |
| **Archer** | Arrow | ~1.15 | `Earth · Mercury` |
| **None** | Wander only | — | — |

A wizard shows the marks they are writing over their head. Raise a wall
to break the shot, hop over it, get behind them, or wear a flame ward
(`Fire · Salt · Sulphur`). In the Mixed Court a fire wizard answers a
wall with a flame-pillar.

To write a different sentence, fill **Cast recipe** (`Spark`, `Mercury`
for the bolt adept; `Earth`, `Salt`, `Mercury` for the arrow adept).

**Blocking** on a Golem is a solid body — you cannot walk through it.
Wizards leave it off so you can step past while they write.

## Add another

1. Duplicate **Golem** or **Warden** in `Assets/Prefabs/Enemies`.
2. Change **Name** / **Id**. `golem` is earth. `fire-golem` is fire.
   `warden` is an ensouled watcher.
3. Drag another ElvGames facing, or **Fill empty frames from pack**
   after you set **Sprite Id**.
4. Set **Attack**, **Formula**, **Ensouled**, **Blocking**, **Grant**.
5. Drop the new prefab in the room.

`GameObject → Rune Magic → Mite` is the same component with no pack
art. Set Sprite Id or drag frames yourself.

## Rooms that want these

| Room | Place |
|---|---|
| Ember Vault | A Golem that slams (the vault's fire-golem can keep `fire-golem` as Id if you want hunger-nature) |
| Gallery of Force | One Warden. Grant the spirit stone. |
| Silent Court | Two Mites, Id `stone-man`, Attack **None**, Blocking on |
| Mixed Court | Two Golems, a Warden or Cultist (Fire · Mercury), a second wizard (`Spark` · `Mercury`), an archer |

See [`FLOOR1.md`](FLOOR1.md) for the lessons. See [`ART.md`](ART.md) if
you are replacing the pack art with your own sheets.
