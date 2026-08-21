# World materials

A running development list, kept beside [`SPELLS.md`](SPELLS.md). Spells are sentences you write. **Materials are sentences the world has already become.** Stamp one on a tile with `MaterialId`; each has its own floor/wall paint and a `WorldMaterial` you can grow later (physics, reactions, map palettes).

The Charter weave reads a material’s **manifestation unfolded to basics** — one rune per column — not a cramped join name. Timber (Plant) is Water | Earth | Salt. Ash is Fire | Water | Earth | Salt.

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

Odd rows (1, 3, 5…) travel right. Even rows (2, 4, 6…) travel left. A join unfolds to the full recipe and reads as one coloured chunk — Plant is a green bar of Water, Earth, Salt; Ash is a grey bar of Fire, Water, Earth, Salt. Each ingredient still has its own column. The gold ring and the join’s colour are the combined form. **Only tiles on the screen speak.** A rune that is off-camera cannot be drawn, even if it lives elsewhere in the room.

**Air is ambient.** Anything that still has a floor or a wall is a place that can be breathed. The weave carries Air in almost every view. A screen that is only void — a tear, no room left — has no breath.

**The adept is always in the weave** as mind · body · soul (Sulphur · Salt · Mercury). Soulless life does not carry Mercury. A living creature writes its own recipe as written — the ash mite is Fire · Salt · Life, and Life is a mark, not a join to unfold.

---

## In the sanctum now

| Material | Signature | Manifestation | Where |
| --- | --- | --- | --- |
| **Stone** | Earth · Salt · Stone | Stone | Walls, The Drop floor, halls |
| **Ash** | Fire · Plant · Ash | Ash | Ash Court floor |
| **Ember** | Fire · Ash · Ember | Ember | Ash Court bed |
| **Salt crust** | Salt · Earth | Salt | Ash Court / The Drop patches |
| **Timber** | Water · Earth · Salt · Plant | Plant | Wick Chapel, chapel hall |
| **Hearthstone** | Fire · Salt · Earth | Fire | Chapel / Storm Cell hearths |
| **Moss** | Water · Earth · Salt · Plant · Life · Grove | Grove | Chapel corners |
| **Void** | — (tear) | — | The Drop pits |
| **Vein** | Fire · Air · Spark · Earth | Spark | Storm Cell floor, storm hall |
| **Scoured** | Air · Earth · Dust | Dust | Storm Cell wind-cut stone |
| **Metal** | Fire · Earth · Lava · Metal | Metal | Storm Cell plate |

---

## Ready for later maps

These already have tiles and a class. Floor 1 uses Ice and Water as hazards (the fire-room ice cage, the cistern channel). The rest wait on later maps.

| Material | Signature | Manifestation | Note |
| --- | --- | --- | --- |
| **Ice** | Water · Salt · Earth · Ice | Ice | Hard water. Thaws. Not Death. Freeze a pool and you can walk it. |
| **Water** | Water · Salt | Water | A pool — yield holding a vessel. **It drowns.** Water work fills a connected pit smaller than 4×4 with this water. Ice asks it to stand. |
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
| **Blizzard** | Air · Water · Cloud · Ice · Snow · Wind · Blizzard | Blizzard | Wind driving Snow |
| **Glacier** | Water · Salt · Earth · Ice · Stone · Glacier | Glacier | Ice given Stone. Ordinary fire cannot take it; witchfire can |
| **Acid** | Fire · Water · Steam · Metal · Acid | Acid | Steam forced through Metal |
| **Damp stone** | Water · Earth | Water | Wet rest, not yet mud |

---

## Not listed yet (open)

Oil, gas, bone, flesh, blood, cloth, paper, gold, silver, mercury-as-metal, grave-ice (Water · Salt · Death), shade-stuff. Add a `MaterialId`, a `WorldMaterial` row, a `MaterialPaint`, and a line here. Joins that birth them still live in `MaterialTree` / [`SPELLS.md`](SPELLS.md).

---

## Painting a map

`WorldMaterial` is the hook: name, note, manifestation, signature, floor/wall tones, paint style, plus two tweakable numbers set in `MaterialCatalog.Flag`.

| Flag | Meaning |
| --- | --- |
| **Flammability** | Negative = fire-retardant (puts nearby fire out). Zero = will not burn. Positive = how readily it catches and how far the burn runs. |
| **Conductivity** | Zero = insulator. Positive = how freely a spark travels the body. |

Tiles keep live **Fire / Wet / Charge / Growth**. Water a plant and it climbs toward Grove, then **across an adjacent pit**. Fire spreads onto flammable neighbors and burns vegetable bodies to Ash. Charge walks metal, water, and vein. `WorldSim` ticks the neighbors.

| Material | Flam | Cond | Note |
| --- | --- | --- | --- |
| Plant | 1.5 | 0.05 | Catches fast. Burns to Ash. |
| Grove | 1.35 | 0.1 | Living mass. |
| Timber | 1.2 | 0 | Wood. |
| Moss | 1.05 | 0.1 | Soft green. |
| Ember | 0.35 | 0.15 | Already hot. |
| Dust | 0.55 | 0 | Rest that lost its weight. |
| Water | −1.6 | 1.25 | Puts fire out. Carries charge. |
| Rain | −1.1 | 0.7 | The veil drawn down. |
| Ice | −0.85 | 0.15 | Hard water. |
| Damp | −0.7 | 0.35 | Wet rest. |
| Metal | 0 | 1.6 | The spark’s favourite road. |
| Vein | 0 | 0.85 | Spark in the stone. |

The Grimoire and pause ledger list this catalog next to the written spells, and list every wrought birth (Acid is Steam · Metal; Ice is Water · Salt · Earth — Body, not Death).
