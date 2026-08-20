# World materials

A running development list, kept beside [`SPELLS.md`](SPELLS.md). Spells are sentences you write. **Materials are sentences the world has already become.** Stamp one on a tile with `MaterialId`; each has its own floor/wall paint and a `WorldMaterial` you can grow later (physics, reactions, map palettes).

The Charter weave reads the **full signature** — roots plus the manifestation — not just Fire or Earth.

Runtime catalog: `Assets/Scripts/World/MaterialCatalog.cs`. Apply with:

```csharp
grid.Set(x, y, TileKind.Floor, MaterialId.Ice);
grid.RoomShell(x0, y0, x1, y1, MaterialId.Stone, MaterialId.Ash);
```

`TileSubstance` still names the first sanctum slice (ash, timber, void…). New maps should use `MaterialId`.

---

## How a material speaks

1. **Signature** is the chain the room writes when you scan across that substance.
2. **Manifestation** is the wrought rune the mix has already become (Ash, Ice, Grove…). Layer runes alone are not enough — timber is Water · Earth · Salt · Plant, not “Earth.”
3. Contiguous runs of the same material collapse to one clause so a floor of ash is one Ash sentence, not eighty copies.
4. **Void** / pits tear the weave. They contribute a gap, not a rune.
5. Locks and world-strings enter the sentence when the scan reaches their tile.

Even rows read left to right. Odd rows read right to left. The Charter grid is that same weave, scrolled sideways. **Only tiles on the screen speak.** A rune that is off-camera cannot be drawn, even if it lives elsewhere in the room.

**Air is ambient.** Anything that still has a floor or a wall is a place that can be breathed. The weave carries Air in almost every view. A screen that is only void — a tear, no room left — has no breath.

---

## In the sanctum now

| Material | Signature | Manifestation | Where |
| --- | --- | --- | --- |
| **Stone** | Earth · Salt · Stone | Stone | Walls, The Drop floor, halls |
| **Ash** | Fire · Plant · Ash | Ash | Ash Court floor |
| **Ember** | Fire · Ash · Ember | Ember | Ash Court bed |
| **Salt crust** | Salt · Earth | Salt | Ash Court / The Drop patches |
| **Timber** | Water · Earth · Salt · Plant | Plant | Wick Chapel, chapel hall |
| **Hearthstone** | Fire · Salt · Earth · Flame | Flame | Chapel / Storm Cell hearths |
| **Moss** | Water · Earth · Salt · Plant · Life · Grove | Grove | Chapel corners |
| **Void** | — (tear) | — | The Drop pits |
| **Vein** | Fire · Air · Spark · Earth | Spark | Storm Cell floor, storm hall |
| **Scoured** | Air · Earth · Dust | Dust | Storm Cell wind-cut stone |
| **Metal** | Fire · Earth · Lava · Metal | Metal | Storm Cell plate |

---

## Ready for later maps

These already have tiles and a class. None of the four tutorial rooms use them yet.

| Material | Signature | Manifestation | Note |
| --- | --- | --- | --- |
| **Ice** | Water · Salt · Earth · Ice | Ice | Hard water. Thaws. Not Death. |
| **Water** | Water · Salt | Water | A pool — yield holding a vessel |
| **Mud** | Water · Earth · Mud | Mud | Soft ground |
| **Sand** | Water · Earth · Mud · Air · Sand | Sand | Mud given breath until it dries |
| **Dust** | Air · Earth · Dust | Dust | Rest that lost its weight |
| **Lava** | Fire · Earth · Lava | Lava | Earth that cannot stay earth |
| **Steam** | Fire · Water · Steam | Steam | Violent hot veil |
| **Glass** | Sand · Flame · Earth · Glass | Glass | Grains, hunger, rest |
| **Crystal** | Earth · Salt · Stone · Water · Crystal | Crystal | Stone grown with yield |
| **Obsidian** | Fire · Earth · Lava · Water · Salt · Obsidian | Obsidian | Hungry earth quenched |
| **Grove** | Water · Earth · Salt · Plant · Life · Grove | Grove | Living plant as a mass |
| **Plant** | Water · Earth · Salt · Plant | Plant | Green cover, not yet Life |
| **Cloud** | Air · Water · Cloud | Cloud | A hanging veil |
| **Rain** | Air · Water · Cloud · Earth · Rain | Rain | The veil drawn down |
| **Snow** | Air · Water · Cloud · Ice · Snow | Snow | The veil given ice’s story |
| **Glacier** | Water · Salt · Earth · Ice · Stone · Glacier | Glacier | Ice given Stone |
| **Acid** | Fire · Water · Steam · Metal · Acid | Acid | Steam forced through Metal |
| **Damp stone** | Water · Earth | Water | Wet rest, not yet mud |

---

## Not listed yet (open)

Oil, gas, bone, flesh, blood, cloth, paper, gold, silver, mercury-as-metal, grave-ice (Water · Salt · Death), shade-stuff. Add a `MaterialId`, a `WorldMaterial` row, a `MaterialPaint`, and a line here. Joins that birth them still live in `MaterialTree` / [`SPELLS.md`](SPELLS.md).

---

## Painting a map

`WorldMaterial` is the hook: name, note, manifestation, signature, floor/wall tones, paint style. Later you can hang collision, spread rules, and reaction tags on the same class without changing how rooms are stamped.

The Grimoire and pause ledger list this catalog next to the fifty spells.
