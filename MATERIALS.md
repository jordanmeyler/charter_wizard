# World materials

A running development list, kept beside [`SPELLS.md`](SPELLS.md). Spells are sentences you write. **Materials are sentences the world has already become.** Stamp one on a tile with `MaterialId`; each has its own floor/wall paint and a `WorldMaterial` you can grow later (physics, reactions, map palettes).

The Charter weave reads a material’s **manifestation unfolded to basics** — one rune per column — not a cramped join name. Timber (Plant) is Water | Salt | Earth. Ash is Fire | Water | Salt | Earth.

Runtime catalog: `Assets/Scripts/World/MaterialCatalog.cs`. Apply with:

```csharp
grid.Set(x, y, TileKind.Floor, MaterialId.Ice);
grid.RoomShell(x0, y0, x1, y1, MaterialId.Stone, MaterialId.Ash);
```

`TileSubstance` still names the first sanctum slice (ash, timber, void…). New maps should use `MaterialId`.

Paint: the atlas draws **stone, dirt, or water** as the walk. Ice, fire, and lightning are coverings, props, or FX (`TILES.md`). A freeze, a burn, or a spark **swaps** the covering and adds the effect — they are not room floors.

---

## How a material speaks

1. **Signature** is the chain the room writes when you scan across that substance.
2. **Manifestation** is the wrought rune the mix has already become (Ash, Ice, Grove…). Layer runes alone are not enough — timber is Water · Salt · Earth · Plant, not “Earth.”
3. Contiguous runs of the same material collapse to one clause so a floor of ash is one Ash sentence, not eighty copies.
4. **Void** / pits tear the weave. They contribute a gap, not a rune.
5. Locks and world-strings enter the sentence when the scan reaches their tile.

Odd rows (1, 3, 5…) travel right. Even rows (2, 4, 6…) travel left. A join unfolds to the full recipe and reads as one coloured chunk — Plant is a green bar of Water, Salt, Earth; Ash is a grey bar of Fire, Water, Salt, Earth. Each ingredient still has its own column. The gold ring and the join’s colour are the combined form. **Only tiles on the screen speak.** A rune that is off-camera cannot be drawn, even if it lives elsewhere in the room.

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
| **Timber** | Water · Salt · Earth · Plant | Plant | Wick Chapel, chapel hall. Tree and Wood-wall stand this wood. |
| **Hearthstone** | Fire · Salt · Earth | Fire | Chapel / Storm Cell hearths |
| **Moss** | Water · Salt · Earth · Plant · Life | Plant | Chapel corners |
| **Void** | — (tear) | — | The Drop pits |
| **Vein** | Fire · Air · Spark · Earth | Spark | Storm Cell floor, storm hall |
| **Scoured** | Air · Earth · Dust | Dust | Storm Cell wind-cut stone |
| **Metal** | Fire · Earth · Lava · Spark · Metal | Metal | Storm Cell plate. Lava · Spark · Earth. Conducts heat and the spark. |

---

## Ready for later maps

These already have tiles and a class. Floor 1 uses Ice and Water as hazards (the fire-room ice cage, the cistern channel). The rest wait on later maps.

| Material | Signature | Manifestation | Note |
| --- | --- | --- | --- |
| **Ice** | Water · Earth · Ice | Ice | Hard water. Thaws. Not Death. Freeze a pool and you can walk it. |
| **Water** | Water · Salt | Water | A pool — yield holding a vessel. **It drowns.** Water work fills a connected pit smaller than 4×4 with this water. Ice asks it to stand. |
| **Mud** | Earth · Water · Mud | Mud | Soft ground. Rest meeting yield. Water · Earth is Ice. |
| **Sand** | Air · Earth · Dust | Dust | The same grit as dust. A paint, not a second rune. |
| **Dust** | Air · Earth · Dust | Dust | Rest that lost its weight. Sand is the same thing. |
| **Lava** | Fire · Earth · Lava | Lava | Earth that cannot stay earth |
| **Steam** | Fire · Water · Steam | Steam | Violent hot veil |
| **Glass** | Dust · Flame · Earth · Glass | Glass | Grains, witchfire, rest |
| **Crystal** | Earth · Salt · Stone · Water · Crystal | Crystal | Stone grown with yield |
| **Obsidian** | Fire · Earth · Lava · Salt · Water · Obsidian | Obsidian | Hungry earth quenched. Lava · Salt · Water. Melt, Shatter, and hunger's thaw will not take it |
| **Grove** | Water · Salt · Earth · Plant · Life | Plant | Living plant as a mass. Tree is the stood spell, not a rune. |
| **Plant** | Water · Salt · Earth · Plant | Plant | Green cover, not yet Life |
| **Cloud** | Air · Water · Cloud | Cloud | A hanging veil |
| **Rain** | Air · Water · Cloud | Cloud | Weather left on the stone, not a rune |
| **Snow** | Air · Water · Cloud · Ice | Ice | Weather left on the stone, not a rune |
| **Oil** | Plant · Fire · Earth · Oil | Oil | Fuel. Surfaces hold flame. It floats: a film on water still burns. A lit slick flashes across connected oil. A geyser, once lit, keeps burning until water finds it. |
| **Miasma** | Cloud · Acid · Miasma | Miasma | Foul breath on the floor |
| **Wardstone** | Earth · Salt · Sulphur · Stone | Stone | Mind-bound masonry. Mostly spell-proof. |
| **Aegis** | Metal · Light | Metal | Shown steel. Mostly spell-proof. |
| **Glacier** | Water · Earth · Ice · Animus · Glacier | Glacier | Ice given logos and ice again. Ordinary fire cannot take it; witchfire can |
| **Acid** | Fire · Water · Steam · Metal · Acid | Acid | Steam forced through Metal |
| **Damp stone** | Water · Stone | Water | Wet rest, not ice |
| **Dirt** | Earth | Earth | Loose rest from Dirt toss. Smothers ground-fire. |

---

## Not listed yet (open)

Bone, flesh, blood, cloth, paper, gold, silver, mercury-as-metal, grave-ice (Water · Salt · Death), shade-stuff. Add a `MaterialId`, a `WorldMaterial` row, a `MaterialPaint`, and a line here. Joins that birth them still live in `MaterialTree` / [`SPELLS.md`](SPELLS.md).

---

## Painting a map

`WorldMaterial` is the hook: name, note, manifestation, signature, floor/wall tones, paint style, plus flammability, conductivity, and a burn clock set in `MaterialCatalog.Flag`. `BurnRate` is derived from that clock.

| Flag | Meaning |
| --- | --- |
| **Flammability** | Negative = fire-retardant (puts nearby fire out). Zero = will not catch. Positive = how readily hunger takes it. |
| **Conductivity** | Negative = insulator (wood and plants break the path). Zero = neutral (may hold a spark but will not pass it). Positive = how freely a spark travels the body. |
| **BurnSeconds** | How long a full fire lasts on this body. Fuel lives on a **1–5 second** clock (oil 1, wood 2, plant 3, grove 4, ember 5). Wood burns better than plant. Zero is not fuel. |
| **BurnRate** | How fast a standing fire travels from this tile. `5 − seconds`: oil 4, wood 3, plant 2, grove 1, ember 0. Ember stays put. Flammability is the separate catch number. |

Tiles keep live **Fire / Wet / Charge / Growth**. A player or NPC spell that waters a plant grows it toward Grove. Reach is the sentence — a short douse stays put, Sprout takes one wet neighbor, Forest covers every water still on the screen. Stamps and covers sit on the tile you painted — they do not start a reaction. Fire a spell starts still spreads onto flammable neighbors. When the burn clock is spent, the vegetable body **gains an ash covering**; the walk tile stays. **Vine cover** is a wick: hunger runs the climbing line into tiles that would not otherwise catch. Timber, plant, and oil props burn on a meter until they are ash. Charge walks metal, water, wet stone, and vein. A bolt can land on neutral stone, but it will not spread unless a neighbor conducts. Wood, plants, and vine cover **insulate** — they disrupt the flow even on iron. `WorldSim` ticks the neighbors. `ChargeLaw` names the three bands.

**Oil floats.** A film on water still catches, flashes, and runs at oil’s rate. Standing yield does not put that fire out; a water sentence still can. **A plant on water can light, but it does not run.** Land plants keep their three-second clock. A spell may still walk them onto neighboring water, weaker than wood.

Stood timber, plant, and oil props use the same 1–5 second clocks.

| Material | Flam | Cond | Sec | Run | Note |
| --- | --- | --- | --- | --- | --- |
| Oil | 2.2 | −0.25 | 1 | 4 | Fuel. Floats. Fastest clock. Flashes. Insulates. |
| Timber | 1.6 | −0.9 | 2 | 3 | Wood. Burns better than plant. Blocks the bolt. |
| Plant | 1.1 | −1.1 | 3 | 2 | Green body. Slower than wood. On water it lights and does not run. |
| Moss | 1.05 | −0.7 | 3 | 2 | Soft green. Same clock as plant. |
| Grove | 0.85 | −1.2 | 4 | 1 | Living mass. Slow, still a weak run. |
| Dust | 0.55 | 0 | 4 | 1 | Loose grit. Slow, weak run. |
| Ember | 0.35 | 0 | 5 | 0 | Slow coals. Stay put. |
| Water | −1.6 | 1.25 | 0 | 0 | Puts fire out. Carries charge. Oil on it still burns. |
| Rain | −1.1 | 0.7 | 0 | 0 | The veil drawn down. |
| Ice | −0.85 | 0 | 0 | 0 | Hard water. Holds a spark, does not run it. |
| Damp | −0.7 | 0.35 | 0 | 0 | Wet rest. |
| Metal | 0 | 1.6 | 0 | 0 | The spark’s favourite road. |
| Vein | 0 | 0.85 | 0 | 0 | Spark in the stone. |

The Grimoire and pause ledger list this catalog next to the written spells, and list every wrought birth (Acid is Steam · Metal; Ice is Water · Earth; Mud is Earth · Water. Water · Earth · Salt is water-pillar. Water · Salt · Earth is Plant).
