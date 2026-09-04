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
4. **Void** / pits tear the weave. The gap is **Dark** (withheld). Only what the camera can see speaks. Hover the clipped belt to still it and see where a mark is from. Each available rune appears often enough to click; extra copies follow how often that material is on screen, and uncommon marks take a larger share.
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
| **Ember** | Fire | Fire | Ash Court bed. Coals. Speaks Fire. Provides fire and lets hunger sit and walk across. The tile underneath stays embered; it does not leftover. |
| **Salt crust** | Salt · Earth | Salt | Ash Court / The Drop patches |
| **Timber** | Water · Salt · Earth · Plant | Plant | Wick Chapel, chapel hall. Tree and Wood-wall stand this wood. |
| **Hearthstone** | Fire · Salt · Earth | Fire | Chapel / Storm Cell hearths |
| **Fire** | Fire | Fire | Rest stamp. Weak source, not fuel. Speaks Fire. At rest it lights adjacent covers, oil, and details (a bush or table); it does not light a plant or timber walk beside it, and it does not walk a field until a spell starts hunger. Cover-Fire is the live layer: standing on it burns, and it lights the same overlay fuel beside it. |
| **Moss** | Water · Salt · Earth · Plant · Life | Plant | Chapel corners |
| **Void** | Dark (tear) | — | The Drop pits. On camera they speak Dark. |
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
| **Grove** | Water · Salt · Earth · Plant · Life | Plant | Living plant as a mass. Tree is the stood spell, not a rune. Wither leaves Death-speaking cover; Wolfsbane, Grove-cure, and the light orbs remember the green. Yield walks a living plant one ring. Poison turns a plant; more poison walks like yield. A tainted tree or nightshade weeps until shown or living plant-work wakes it. |
| **Plant** | Water · Salt · Earth · Plant | Plant | Green cover, not yet Life |
| **Cloud** | Air · Water · Cloud | Cloud | A hanging veil |
| **Rain** | Air · Water · Cloud | Cloud | Weather left on the stone, not a rune |
| **Snow** | Air · Water · Cloud · Ice | Ice | Weather left on the stone, not a rune |
| **Oil** | Plant · Fire · Earth · Oil | Oil | Fuel. Surfaces hold flame. It floats: a film on water still burns. A lit slick flashes across connected oil. A geyser, once lit, keeps burning until water finds it. |
| **Miasma** | Poison · Fire · Miasma | Miasma | Airborne poison fog. Faster poison, holds the step, wind must take it. Will not catch. Hunger on a poison slick births it. |
| **Wardstone** | Earth · Salt · Sulphur · Stone | Stone | Mind-bound masonry. Mostly spell-proof. |
| **Aegis** | Metal · Light | Metal | Shown steel. Mostly spell-proof. |
| **Glacier** | Water · Earth · Ice · Animus · Glacier | Glacier | Ice given logos and ice again. Ordinary fire cannot take it; witchfire can |
| **Acid** | Fire · Water · Steam · Metal · Acid | Acid | Poison liquid on the walk. Contact only; yield washes it. Steam forced through Metal. |
| **Damp stone** | Water · Stone | Water | Wet rest, not ice |
| **Dirt** | Earth | Earth | Loose rest from Dirt toss. Smothers ground-fire. |

---

## Not listed yet (open)

Bone, flesh, blood, cloth, paper, gold, silver, mercury-as-metal, dark-crystal (Crystal · Dark · Death), shade-stuff. Add a `MaterialId`, a `WorldMaterial` row, a `MaterialPaint`, and a line here. Joins that birth them still live in `MaterialTree` / [`SPELLS.md`](SPELLS.md).

---

## Painting a map

`WorldMaterial` is the hook: name, note, manifestation, signature, floor/wall tones, paint style, plus flammability, leftover conductivity, a burn clock, a **0–10 Hunger grade**, a **0–10 Quench grade**, and a **0–10 Conduct grade**. Set them on `MaterialCatalog.Flag` when you add a body (`Flag(id, flam, cond, seconds, hunger, quench, conduct)`). Omit hunger and it stays **0** (neutral). Omit quench and it stays **0** (dry). Omit conduct and the leftover conductivity number is read into a grade. `VitalLaw.HungerOf` / `QuenchOf` / `ConductOf` read those catalog numbers. `BurnRate` is still `5 − seconds` (the clock leftover). **Fire spread uses Hunger, not BurnRate.** Wet work uses **Quench**. Charge hold and spread use **Conduct**. A spell volume can still light or stun whatever it hits.

| Flag | Meaning |
| --- | --- |
| **Hunger** | One 0–10 grade. Catch and spread. Room for later fuel in the open slots. |
| **Quench** | One 0–10 grade. The wet counterpart. Dry stone is 0. Mud suppresses. Water puts fire out. |
| **Flammability** | Positive = how readily hunger takes it once it is allowed to catch. Zero = will not catch. Negative tracks Quench (about `−grade × 0.16`; water 10 → −1.6). |
| **Conduct** | One 0–10 grade. Hold and spread for the spark. Room for later bodies in the open slots. |
| **Conductivity** | Leftover signed number. Negative = how hard an insulator breaks a neighbor's clock. Zero = poor hold. Positive tracks Conduct (metal 10 → 1.6). |
| **BurnSeconds** | How long a full fire lasts on this body. Fuel lives on a **1–5 second** clock (oil 5, wood 4, plant 3, grove 2). Independent of whether the body may *run*. Oil and wood last; plant and grove burn out sooner. Ember hosts fire and is a path, not fuel — the tile stays embered. |
| **BurnRate** | Clock leftover `5 − seconds`. Not what walks fire to a neighbor. |

**Hunger 0–10**

| Grade | Band | Catch from neighbors | Spreads | Typical seconds | Now / later |
| --- | --- | --- | --- | --- | --- |
| **0** | Neutral | No — only a spell that hits the cell | No | 0 | Stone, dirt, metal |
| **1** | Tinder | From a strong source, inside that source's reach, touching fuel toward it | No | — | Open — ember hosts fire without this grade |
| **2** | Tinder | Same | No | 2 | Dust, fire cover |
| **3** | Soft | Same | No | 3 | Moss |
| **4** | Soft | Same | No | 2 | Grove |
| **5** | Plant | Same | No | 3 | Open — thatch / young plant |
| **6** | Plant | Same | No — a plant field does not run | 3 | Living plant |
| **7** | Timber | Strong source — reach **1** | Yes — equal-or-weaker fuel, touching fuel | 4 | Open — brush / dry wood |
| **8** | Timber | Strong source — reach **2** | Yes | 4 | Timber |
| **9** | Oil | Strong source — reach **3** | Yes; oil also flashes a slick | 5 | Open — pitch / grease |
| **10** | Oil / hall | Strong source — reach **4** | Yes | 5 | Oil. Kindled hall counts as 10 |

**One spread rule:** a **strong source** is Hunger **7+**. Reach is the grade itself: **hunger − 6** (timber 2, oil / a hall 4). It may walk fire to equal-or-weaker fuel inside that reach if the cell **touches fuel toward the source**. A timber hall burns along the wood. Fire does not leap a stone or empty gap. Weaker fuel does not walk fire. A spell can still light whatever it hits.

A **kindled hall** is the **Aura-Fire** brush (or a Flame Hall plaque): painted walk that stays lit until yield is thrown. It is not a material — it is a source. Live Aura-Fire, a geyser, or lit Floor-Fire counts as Hunger **10**. Floor-Fire / Wall-Fire stay at rest until a spell or covering starts work — plant on that walk lights the vine, not the masonry. Vine is a wick: any adjacent live flame can run that line. Stamp **Cover-Vine** (or write Vine / Sprout) to let fire cross a stone gap; Floor-Plant and Floor-Grove catch and burn out, they do not run a field. **Ember** is coals: it provides fire, accepts a flame from another source, and lets hunger walk across. The tile underneath stays embered even after everything around it burns. Neutral stone / dirt never catch from a neighbor.

**Quench 0–10**

Dry stone next to timber leaves that fire alone — full clock, full vigor, it may spread. Mud smothers it (no spread, the clock runs down sooner). Water puts it out on the cell and on adjacent fuel. Oil and a plant standing on water still ignore yield.

| Grade | Band | Neighbor fire | Now / later |
| --- | --- | --- | --- |
| **0** | Dry | None | Stone, dirt, timber, oil |
| **1** | Trace | None (below suppress) | Salt crust |
| **2** | Trace | Open — moist dust | — |
| **3** | Mud | Suppress | Mud |
| **4** | Damp | Suppress | Damp stone |
| **5** | Ice | Suppress (then melt) | Ice, snow |
| **6** | Ice | Suppress | Glacier |
| **7** | Rain | Strong suppress | Rain |
| **8** | Rain | Open — shallow water | — |
| **9** | Water | Puts fire out | Open — flood edge |
| **10** | Water | Puts fire out on the cell and neighbors | Standing water |

**Conduct 0–10**

| Grade | Band | Direct charge (bolt / live-floor) | Holds | Spreads | Now / later |
| --- | --- | --- | --- | --- | --- |
| **0** | Insulator | No — the cell refuses. Occupants in the spell volume still stun. | No | No; breaks a neighbor's path | Timber, plant, grove, moss, oil |
| **1** | Poor | Yes | ~1s | No | Open — packed earth |
| **2** | Poor | Yes | 1s | No | Stone, dirt, sand, ash, ice |
| **3** | Poor | Yes | 1s | No | Open — baked clay |
| **4** | Weak | Yes | 1s | No | Salt crust, mud |
| **5** | Weak | Yes | 1s | No | Damp stone, crystal, lava |
| **6** | Weak | Yes | 1s | No | Acid |
| **7** | Conductor | Yes | 1.5s | Yes — onto other conductors | Rain. Wet stone counts as 7. |
| **8** | Conductor | Yes | 2s | Yes | Vein, aegis |
| **9** | Conductor | Yes | 2.5s | Yes | Water |
| **10** | Conductor | Yes | 3s | Yes | Metal |

**One charge rule:** a **conductor** is Conduct **7+**. Live-floor and a bolt still charge poor stone for **one second** so a gate turns and bodies stun, the way a fire sentence still burns who stands in the volume. Charge only **walks** onto other conductors. Wood and plants **refuse the cell** and break a metal path. Wet stone runs.

Tiles keep live **Fire / Wet / Charge / Growth**. A player or NPC spell that waters a land plant grows it toward Grove and may take a neighbouring water tile. Sprout lays plant cover three tiles from the caster, the way ice covers water — it does not walk the pool. Forest covers every water still on the screen. Stamps and covers sit on the tile you painted — plant growth does not start itself. Fire cover and rest fire at rest light overlay fuel on or beside them — covers, oil, a bush or table — not a plant or timber walk. Fire a spell starts still lights the cells it hits; neighbor hunger then follows the 0–10 rule (a 7+ source, equal-or-weaker fuel, out to that source's reach, touching fuel — plant does not run a field, and fire does not leap a gap). When the burn clock is spent, the vegetable body **gains an ash covering** and a plant or timber floor becomes **dirt** (look and Earth). Stone, dirt, Floor-Fire, ember, and fire cover stay. An embered tile keeps the walk that was already there even if everything around it burns. A burned crate or table ashes the cell under it the same way. **Vine cover** is a wick: hunger runs the climbing line into tiles that would not otherwise catch. Timber, plant, and oil props burn on a meter until they are ash. Charge follows the 0–10 Conduct grade. A bolt or live-floor charges stone for a second; metal and water walk it. Wood, plants, and vine cover **insulate**. `WorldSim` ticks the neighbors. `ChargeLaw` names the bands.

**Oil floats.** A film on water still catches, flashes, and runs at oil’s rate. Standing yield does not put that fire out; a water sentence still can. **A plant on water can light, but it does not run.** Land plants keep their three-second clock. A spell may still walk them onto neighboring water, weaker than wood.

Stood timber, plant, and oil props use the same 1–5 second clocks.

| Material | Hunger | Quench | Conduct | Flam | Cond | Sec | Note |
| --- | --- | --- | --- | --- | --- | --- |
| Oil | 10 | 0 | 0 | 2.2 | −0.25 | 5 | Strongest fuel. Floats. Flashes a slick. Lasts. Refuses the spark. |
| Timber | 8 | 0 | 0 | 1.6 | −0.9 | 4 | Wood. May run fire. Refuses the spark. |
| Plant | 6 | 0 | 0 | 1.1 | −1.1 | 3 | Catches from timber / oil. Does not run fire. Refuses the spark. |
| Grove | 4 | 0 | 0 | 0.85 | −1.2 | 2 | Living mass. Catch-only. Refuses the spark. |
| Moss | 3 | 0 | 0 | 1.05 | −0.7 | 3 | Soft green. Catch-only. Refuses the spark. |
| Dust | 2 | 0 | 2 | 0.55 | 0 | 2 | Loose grit. Tinder. Holds a spark one second. |
| Ember | 0 | 0 | 2 | 0 | 0 | 0 | Coals. Speaks Fire. Holds a spark one second. The tile stays embered. |
| Stone / Dirt | 0 | 0 | 2 | 0 | 0 | 0 | Neutral and dry. Holds a spark one second. Does not pass it. |
| Salt crust | 0 | 1 | 4 | −0.15 | 0.2 | 0 | Trace moisture. Weak hold. |
| Mud | 0 | 3 | 4 | −0.35 | 0.25 | 0 | Suppresses neighbor fire. Weak hold. |
| Damp | 0 | 4 | 5 | −0.7 | 0.35 | 0 | Wet rest. Weak hold. Standing yield on stone runs (7). |
| Ice / Snow | 0 | 5 | 2 | −0.85 / −0.65 | 0 | 0 | Hard water. Holds a spark one second. |
| Glacier | 0 | 6 | 2 | −0.9 | 0 | 0 | Ordinary fire cannot take it. |
| Rain | 0 | 7 | 7 | −1.1 | 0.7 | 0 | The veil drawn down. Conducts. |
| Water | 0 | 10 | 9 | −1.6 | 1.25 | 0 | Puts fire out. The spark’s wet road. |
| Metal | 0 | 0 | 10 | 0 | 1.6 | 0 | The spark’s favourite road. Holds and spreads. |
| Vein | 0 | 0 | 8 | 0 | 0.85 | 0 | Spark in the stone. Conducts. |

The Grimoire and pause ledger list this catalog next to the written spells, and list every wrought birth (Acid is Steam · Metal; Ice is Water · Earth; Mud is Earth · Water. Water · Earth · Salt is water-pillar. Water · Salt · Earth is Plant).
