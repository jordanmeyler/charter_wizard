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
| **Custom** | `custom` | Hunt + slam | Blank body. Mode, slots, gambits, and resistances are yours. |
| **Golem** | `golem` | Golem | Holds ground and slams anyone in reach. Hop or Stoneskin survives it. Earth body. |
| **Warden** | `warden` | Wizard | Writes a fireball for two seconds and commits facing when the sentence starts. Ensouled. |
| **Cultist** | `cultist` | Wizard | Same strike as the Warden. Another robe if you want two casters. |
| **Mite** | `ash-mite` | None | Blank lock. Set formula / attack yourself. |

The Silent Court's stone men are a **Mite** with Id `stone-man`, Attack
**None**, and Blocking on. They do not slam. Charm, Command, **Lull**,
Terror, Jolt, or Rage turn them. Lull puts them to sleep — they flatten,
and you walk over the body. Recast Lull (or another mind sentence that
shares a mark) to wake them.

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

## Animation — frames on the prefab, not a Unity Animator

Unity's usual path for a character is an **Animator Controller** (states
like Idle / Walk / Attack). **The adept uses that.** Hero_22 has
`Idle` / `Walk` / `Cast` / `Hop`. Rebuild it from `Window → Rune Magic
→ Adept Animator` if the controller looks empty.

Enemies do **not**. A lock is a short clip list on `EncounterLock`:

1. Slice a sheet (ElvGames already is). **A** is idle. **C** is slam
   or write. **D** is the unmake.
2. Drag idle slices onto **Idle Frames**.
3. Drag slam / cast slices onto **Attack Frames**.
4. Optional: **Resolve Frames** for the unmake.

Play loops those arrays. Changing how they look is a new set of
slices on that same prefab — not a new controller. Generated painters
stay as a fallback if Portrait and Idle Frames are empty (`Sprite Id`
`enemy-011`, `fire-golem-slam`, `warden-cast`).

Do not put an Animator on a golem or warden.

## Mind, ranges, and attack slots

The same Inspector now has a mind. **Mode** and **Attacks** are how
you write a new body without new C#.

| Mode | What Play does |
|---|---|
| **Auto** | Follows Attack. Golem holds ground. Wizard / Archer stand and write. None wanders. |
| **Hunt** | Close the gap and use the slot that matches the range. |
| **Guard** | Hold the tile. Strike if a slot can reach. |
| **Skirmish** | Keep mid or long. Back off if you step in. |
| **Caster** | Stand and write when a shot or pillar can reach. |
| **Wander** | Walk the room. Gambits can still fire. |

**Close / Mid / Long** are the bands (defaults 1.25 / 4.5 / 8.2). A
slot listed as Close slams inside slam reach. Mid and Long write a
sentence. Distance picks the matching band first, then any slot that
can still reach.

### Attacks (what they do on their own)

**Attacks** is the list they use every breath. Buttons on the Inspector:

| Button | What you get |
|---|---|
| **Add slam** | Close slam. Hop or Stoneskin survives it. |
| **Add fireball** | Mid shot. `Fire · Mercury`. Facing locks when they start writing. |
| **Add arrow** | Long wood arrow. |
| **Add flame-pillar** | Mid pillar. Floor hungers, then a column stands. |
| **Add wall** | Mid earth wall (`Earth · Salt · Earth`) across the line to you. |
| **Add custom (write runes)** | Spell stays empty. Type the sentence. Set **Strike** to Slam, Shot, or Pillar. |

Picking **Attack / spell** on a slot fills **Recipe** from the book
(`Fireball` writes `Fire · Mercury`). **Custom** is how you make a
new attack that is not in that list — write `Spark · Mercury`, or any
chain the catalog knows, or a sentence that is only a shot because you
set Strike to Shot.

Empty **Attacks** still uses the old **Attack** dropdown:

| Attack | Strike | Cast seconds | Empty Cast recipe |
|---|---|---|---|
| **Golem** | Slam in reach | ~0.85s windup | — |
| **Wizard** | Fireball, facing locked when they start writing | 2 | `Fire · Mercury` |
| **Archer** | Wood arrow (`Plant · Salt · Mercury`, power 3) | ~1.15 | `Plant · Salt · Mercury` |
| **None** | Wander only | — | — |

A wizard shows the marks they are writing over their head. Raise a wall
to break the shot, hop over it, get behind them, or wear a flame ward
(`Fire · Salt · Sulphur`).

**Blocking** on a Golem is a solid body — you cannot walk through it.
Wizards leave it off so you can step past while they write. **Lull**
(and stun, freeze, charm) still lets you walk over a blocking body
while they sleep.

## Gambits (if they do this, then write that)

Gambits are **not** the regular attack list. They are answers.

First matching if/then wins, the way FF12 wrote gambits.

`If the player raises a wall, then write flame-pillar.` That is the
Mixed Court lesson. Buttons:

| Button | What you get |
|---|---|
| **If they raise a wall → flame-pillar** | Mixed Court answer. Add this on any caster you want to do it everywhere. |
| **If they raise a wall → wall** | They stand earth of their own. |
| **If close → slam** | A caster who slams when you step in. |
| **Add empty if / then** | Blank row. Set **When**, then **Then spell** or write **Then recipe**. |

A fire wizard with an empty gambit list still answers a wall in the
Mixed Court only. Put the row on the prefab if you want it in every
room.

Other whens: they cast a named spell, they are close / mid / long,
an ally is nearby, this body has a status, the mark has a status.
**Then spell** fills runes the same way an attack slot does. Leave Then
spell on None and write **Then recipe** for a custom sentence. **Once**
spends the row after it fires.

## Nature and resistances

**Nature** Auto still reads the Id (`golem` is earth, `fire-golem` is
fire, an ensouled `warden` is mind). Set Nature yourself if the Id
should not decide the body.

**Load nature defaults into affinities** writes defense, push resist,
and the strike / status columns (0 immune … 5 ruin-weak). Change a
column without rewriting the rest of the row. A stone golem that also
ignores hunger is Defense 4 with Fire set to 0.

## Add another enemy

Yes — no new C# for a new body.

1. `GameObject → Rune Magic → Enemies → Custom`, or duplicate
   **Golem** / **Warden** in `Assets/Prefabs/Enemies`.
2. Change **Name** / **Id**. `golem` is earth. `fire-golem` is fire.
   `warden` is an ensouled watcher. Or set **Nature** directly.
3. Drag another ElvGames facing, or **Fill empty frames from pack**
   after you set **Sprite Id**. **Start from a pack body** copies a
   pack name / formula / frames onto this lock.
4. Set **Mode**, **Attacks**, **Gambits**, **Formula**, **Ensouled**,
   **Blocking**, **Grant**, and resistances.
5. Drop the new prefab in the room.

`GameObject → Rune Magic → Mite` is the same component with no pack
art. Set Sprite Id or drag frames yourself.

A new wood archer does not need new C# — add a Long **Wood arrow**
slot, or set Attack to **Archer**. A body that slams in close and
answers a wall with a flame-pillar is two slots and one gambit. A
custom spark shot is **Add custom**, recipe `Spark · Mercury`, Strike
**Shot**.

## Rooms that want these

| Room | Place |
|---|---|
| Ember Vault | A Golem that slams (the vault's fire-golem can keep `fire-golem` as Id if you want hunger-nature) |
| Gallery of Force | One Warden. Grant the spirit stone. |
| Silent Court | Two Mites, Id `stone-man`, Attack **None**, Blocking on |
| Mixed Court | Two Golems, a Warden or Cultist (Fire · Mercury), a second wizard (`Spark` · `Mercury`), an archer |

See [`FLOOR1.md`](FLOOR1.md) for the lessons. See [`ART.md`](ART.md) if
you are replacing the pack art with your own sheets.
