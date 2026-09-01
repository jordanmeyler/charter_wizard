# Rune Magic — Design Reference

*A 2D puzzle-RPG where the player perceives the runic substrate of reality and composes spells from it. The correct spell (or combination) instantly resolves an encounter — combat is a lock-and-key puzzle, not a damage race. Living source of truth. Version 0.21. Spell catalog: [`SPELLS.md`](SPELLS.md). World materials: [`MATERIALS.md`](MATERIALS.md). First floor: [`FLOOR1.md`](FLOOR1.md). Eleven basic runes; joins are wrought runes. Primordials later.*

---

## 1. Core premise, casting & progression

- Casting is **perception, not position.** Runes stream around the player through the fabric of reality (the "matrix"); you cast by reading the field and composing from what flows through it.
- **The field is constant.** Novice and master perceive the same runes. No sight-leveling. What changes is *understanding*.
- **Power is gated by knowledge and items — never generic experience.** No character levels, no stat growth, no XP bar. The one practice-driven exception is Free attunement (a lopsided specialization, not a level — see below).
- Every enemy (and much terrain) is a **lock**; every spell you can assemble is a **key**. The right key resolves instantly.
- **Three gates:** *knowledge* (main — learned recipes/rune-meanings), *items* (shortcuts, wards, and the hard gate to Primordial), *narrative grant* (divine tier). Most locks accept more than one solution → sequence-breaking is supported.
- **Learning loop:** you learn by observation — runes seen in the world, in creature formulas, and especially in **free magic and free-magic items, which reveal their composition.** Borrowing an effect you don't understand teaches the recipe; mastery is graduating from *borrowing* to *composing*.

### Two progression models (one per path)
- **Charter = knowledge.** Formulas acquired by instruction (demonstration, reading, being told combos). Binary; new knowledge never costs old. Charter mages are **broad generalists** — no grind, no decay.
- **Free = attunement.** Intuitive, emotional casting. Using a spell or a type (Fire, Water, Spark, grave-work…) grows that focus: later **recipe clashes lean toward it**, and those spells become **larger / more effective**. Specialization, not a level bar. Withering unused types is still an open tuning question — this pass only grows what you use.
- Consequences: Free casters **specialize hard**, **patch weak elements with items and unique free spells**, and must **understand a branch to exploit it** — so even the intuitive path forces learning the tree.

### Free as the default onboarding path
Players distrust orthodoxy by default, so most reach for Free first — the design leans in. The intuitive, item-friendly, rune-revealing path is the on-ramp; later the story **uses the player's anti-authority expectations against them** (the orthodoxy is more tangled than it looked; Free's costs and low ceiling are real; the "villains" are sincere believers; the union truth). An expectation-subversion engine that **encourages replays**.

*The three systems (Charter, Free, Primordial) are kept mechanically separate for now; their "union" is a narrative endgame idea, not a gameplay blend.*

---

## 2. The eleven basic runes

Each rune is a **concept**, not a mechanic. Players will eventually see effects, not names, so the concept has to be what the effect shows. Full table: [`SPELLS.md`](SPELLS.md).

| Rune | Concept |
|---|---|
| **Fire** | Hunger. The will to consume so it can continue. |
| **Air** | Breath. The between. That which has no weight and will not stay. |
| **Earth** | Rest. Patience. That which remains. |
| **Water** | Yield. Mercy. That which becomes what holds it. |
| **Salt** | A standing body. Manifestation that stays — walls, pillars, a floor that holds. Not how a spark flies. |
| **Mercury** | Going. A path opening. Hunger sent is fire. Breath then Mercury is a bolt. A stood body then Mercury goes *into* a thing. |
| **Sulphur** | The wildcard. Add it and the work becomes something else, the way Life makes a plant living. |
| **Light** | Shown. The veil is lifted. |
| **Dark** | Withheld. The veil is drawn. |
| **Life** | Modifier. Marks a living recipe (plant, heal, hop, a called beast). Not a school. |
| **Death** | Modifier. The grave. **Reserved** — not in ordinary recipes. Load-bearing for Free. |

A join is not a modifier. **Fire · Air** becomes **Spark**, which is its own rune and combines again (**Spark · Air → Lightning**). Two runes birth a join or wait as a clause. Three or more is a spell. Longer chains are stories.

Hot, Cold, Wet, Dry, and Aether stay reserved. **Anima** (`Water · Sulphur · Earth`) and **Animus** (`Fire · Sulphur · Air`) are wrought — eros and logos, not a second pair of sexed runes. Male and Female are the old names for those two.

---

## 3. Joins and the tree

A chain is a sentence, left to right. Two roots meeting become a third:

| Join | Becomes | Concept |
|---|---|---|
| Fire · Air | **Spark** | Hunger given breath. A seed of charge. |
| Air · Water | **Cloud** | Breath holding yield. A hanging veil. |
| Water · Earth | **Ice** | Yield meeting rest. Hard water. |
| Fire · Earth | **Lava** | Hunger meeting rest. |
| Fire · Water | **Steam** | Hunger forced through yield. |
| Air · Earth | **Dust** | Breath forced through rest. Grit. The same thing as sand. |
| Earth · Water | **Mud** | Rest meeting yield. Soft ground. Order is the sentence — Water · Earth is Ice. |

Then the wrought rune combines again: **Spark · Air → Lightning**, **Steam · Metal → Acid**, **Cloud · Acid → Miasma**, **Plant · Fire · Earth → Oil**, **Flame · Lightning → Plasma**, **Lava · Salt · Water → Obsidian**, **Fire · Sulphur · Air → Animus**, **Water · Sulphur · Earth → Anima**, **Fire · Animus · Fire → Flame**, **Ice · Animus · Ice → Glacier**. Ice is **Water · Earth**. Mud is **Earth · Water**. **Water · Earth · Salt** is water-pillar. **Water · Salt · Earth** is Plant. Tree is Plant · Life · Salt. Wood-wall is Plant · Life · Salt · Plant · Life — a line of trees. **Vine** is a spell (`Plant · Mercury`), not a rune — vine cover speaks Plant. **Wind** is a spell (`Air · Mercury`), not a rune. **Grotto** is the cave-spell (`Plant · Dark`), not a rune. Weather is Cloud written as a sentence. The Grimoire lists every birth.

**Anima** (eros) opens a work to many and can make it healing. **Animus** (logos) asserts a work into a higher nature. **Flame** is fire given logos and fire again. **Glacier** is ice given logos and ice again. Drive is Animus sent. Balm and Chorus are Anima sent or stood. **Sulphur** stays on status — wards, Rage, Freeze, Jolt.

The quality square (Hot/Cold, Wet/Dry) belongs to the primordial pass and is not used to explain joins right now.

Full wrought list and the written story-chains: **[`SPELLS.md`](SPELLS.md)**. 1–40 ordinary (no Death). 41–50 Death / Free. 51 Time-stop (Life · Death · mind · Dark · Water · Earth, no Light, no Mercury). Each combination law is also an **environmental reaction** (section 10) — terrain is made of the same materials.

---

## 4. The spell grammar

There is **no generic hit-point bar**. A spell kills, restrains, or does neither (traverse, heal, hide, lift, summon, transform). The right key still unmakes a lock at once. **Burning and poison** are the exception: they run a named meter to ash or death. Bodies can now **strike back** — a slam or a flying shot will send the adept to the spawn crystal — and spells leave **visible statuses** (buffs and debuffs) that different natures take differently.

**Targeting is written with the spell.** Single-target work finds the nearest lock at the click. Area work (Rain, Live-floor, Thunderclap, Sprout…) offers the key to every lock in the radius and paints the tiles. Self work (Hop, Flight, Stoneskin, Veil) stays on the caster. Grow form widens a single sentence into an area around the feet. Runtime table: `SpellVerb`.

**Statuses share one host.** Burning, Frozen, Soaked, Stunned, Sleeping, Rooted, Frightened, Raging, Charmed, Confused, Poisoned are debuffs. Veiled is a hide buff. **Wards** are the four elements given a body and held on you (`Element · Salt · Sulphur`). Only one ward stands at a time.

**Two clocks.** **Meters** are burning and poison: they run only while you still stand in that kind of walk or covering, and empty is death or ash. Hunger ticks on a fire floor, fire cover, or a live flame. Poison ticks on a poison slick (liquid — yield washes it) or in a **miasma** cloud (airborne fog — the tile, a neighbour, or a hanging veil; wind must take it). Leave the tile and the clock waits. A later fire or blight does not refill it. Yield (douse, rain, soak) puts hunger out and resets the burn meter. A wind ward turns foul breath before it takes. Each nature and each burnable body has its own time: the adept burns in eight breaths, flesh in six, ice faster, earth slower; timber and plant become ash. Fire-nature will not burn; fire, ice, and earth shrug poison. **Focus** holds **mind spells** — ailments and wards. They all write Sulphur. Charm, Command, Lull, Rage, Terror, Confuse, and the four wards stay until you write another sentence that reuses a mark from the held working. A fizzle plays when the hold breaks. Wall drops stoneskin (Earth · Salt). Fireball drops a flame ward (Fire) but not stoneskin. Only one ward stands at a time. Frost, stun, and root lift on their own clocks. Ice-spear, ice-pillar, and ice-wall make hard water; they do not freeze a living body. **Freeze** (`Ice · Sulphur`) and weather like **Snowfall** / **Snowstorm** do.

The four roots are a square. Adjacent sides have a winner. Opposites do not touch.

| Wear | Sentence | Fends off | Broken through by |
| --- | --- | --- | --- |
| **Water ward** | Water · Salt · Sulphur | Fire — fireballs, burning floors | Earth / a physical blow |
| **Flame ward** | Fire · Salt · Sulphur | Earth — hurled rest, roots | Air |
| **Stoneskin** | Earth · Salt · Sulphur | Air, and **physical** blows (arrows, a golem slam) | Fire — hunger sent still finds you |
| **Wind ward** | Air · Salt · Sulphur | Water — ice, soak — and **miasma** | Fire |

Water douses Fire. Fire scorches Earth. Earth stands against Air. Air dries Water. A chip over the body names what holds; the HUD repeats it for the adept. Fire-nature will not burn; ice will not freeze; earth shrugs off elemental soak and heat; mind takes stun, sleep, and fear harder. Frozen / stunned / sleeping stop action and movement. Rooted stops movement.

A spell is a **chain that tells a story**. **Order is the sentence.** Fire is **Fire · Mercury**. Add breath and the same send is a bolt: **Fire · Air · Mercury** (or **Spark · Mercury** / **Lightning · Mercury** if that join already stands). Melt is the stood fire-body sent *into* a thing: **Fire · Salt · Mercury**. Salt is for work that *stands* — Fire-pillar is **Fire · Salt** (it goes out in a few seconds without a source). Flame-pillar is **Fire · Salt · Earth**, a lasting hearth. Lava-pillar is **Fire · Earth · Salt**. **Flame** is witchfire: **Fire · Animus · Fire** — fire given logos and its own perpetuity. Send it (`Flame · Mercury`) and it melts glacier and glass that ordinary hunger cannot. Heat is a property of the recipe (`MatterLaw`): any fire-bearing sentence melts ice it crosses. Logos on ice does not count as hunger. **Sulphur** turns a sentence into a status (Fire · Sulphur · Mercury is Rage; Lightning · Sulphur is Jolt; the four wards are Element · Salt · Sulphur), the way Life makes a plant living. Death is not in the ordinary book. Hop and Flight stay on the caster (Air · Salt · Air, Air · Mercury · Salt) — the same ideas, a different order. Chain is longer because more happened.

**Formation is part of the spell.** The chain writes how it lands. There is no Remote / Pillar fork at cast time.

| The sentence does this | Form | Example |
|---|---|---|
| Asked to stand (Salt, or Earth at the end of a standing body) | **Pillar** | Fire-pillar: Fire · **Salt** (temporary without a source). Flame-pillar: Fire · Salt · **Earth**. Lava-pillar: Fire · Earth · **Salt**. Ice-pillar. Wall. Ice-wall: Ice · Salt · Ice. Tree: Plant · Life · Salt. Wood-wall: Plant · Life · Salt · Plant · Life. |
| Sent *into* a thing, or placed away (a stood body, then Mercury) | **Remote** | Melt: Fire · Salt · **Mercury**. Pit. Rain. |
| Hunger sent, or breath already in the chain, then sent | **Shot** | Fire: Fire · Mercury. Lightning: Fire · Air · Mercury. Ice-spear. Vine: Plant · Mercury — a climbing wick. |
| A body around your feet | **Grow** | Live-floor: Fire · Air · Salt. Fog. Sprout (plant cover, three tiles). |
| Kept on the caster | **Self** | Hop. Flight. Wards. |

Cast opens aim for the form the sentence already wrote. Click the world — fly a line, raise a column, release at your feet, or place at a distance. You do not pick the form. **Hop** (Air · Salt · Air) is Self: click a landing and leap a few tiles, including over a pit. **Push** (Air · Salt · Mercury) is Shot: breath given a body and sent, so the wind moves them. **Lightning strike** (Fire · Air · Salt · Air · Mercury) falls from the sky. **A flying shot stops on a wall or a shut door** — fireball, ice-spear, hurled stone, a bolt, an arrow, a vine. Lightning strike ignores cover because it is not a line through the room. An opened door is a hole. Remote work still forms at the click. **Flight** (Air · Mercury · Salt) stays on you so pits will not take you for a short while. **Pillar** is one tile on the floor — a hollow fills and holds; a floor grows a column. **Wall** is the same rest, but you click a start and a stop: across a pit it is a two-tile span, on the floor it is a barrier. **Ice-wall** is that same start-to-stop for hard water: Ice · Salt · Ice. **Wood-wall** is that same start-to-stop for living plant: Plant · Life · Salt · Plant · Life — a line of trees. **Tree** is Plant · Life · Salt. Standard earth, ice, and wood spans must find a floor or a wall at each end, or they fall. A metal wall hangs without a far bank. Hunger eats the wood. Ice freezes water without needing banks; plant grows a walkable cover the same way; earth only muds it. Later work stands on water unless the square forbids it (hunger goes out). Columns use the same law.

A combo that *looks* as if it should work can still fizzle if it is not written. The catalog is [`SPELLS.md`](SPELLS.md).

**Charter:** an unwritten, unfinished, or **scrambled** combo **fizzles**. The written order is the sentence. No blanks are filled. **Free:** may **unscramble** the same runes into a valid recipe, and may fill missing runes up to a **fill budget** (1 now; the matcher is written so the budget can rise). A 2-of-3 recipe can still become a spell. Mercury · Fire becomes Fire; Charter would refuse it. If several written chains fit, Free **picks at random, weighted by attunement**. A finished sentence — even a scrambled one Free can read — is never “upgraded” by filling toward a longer one. Free is still never the required key.

The catalog chains now resolve in play. Fire is Fire · Mercury. Lightning is Fire · Air · Mercury. Joins fold (Fire · Air is Spark). In Develop the Grimoire lists the full book; in Play it holds only workings you Keep. Click a name to string it if those runes are in view. Short tutorial strings still work as a fallback. Charter fizzles Free-only Death-work.

---

## 5. Life, souls & creatures

- **Soulless life** — beasts, plants, bugs. Animate matter, **no soul, no magic.** Creating this is sanctioned.
- **Ensouled life** — carries a soul; **a soul grants magic across all races.** The player is ensouled. The adept’s recipe is **mind · body · soul** (Sulphur · Salt · Mercury). That sentence is always in the weave. Soulless life does not carry Mercury.
- **The soul rule: Charter magic cannot touch souls.** Soulless creation/terrain-golems are legal; inserting, binding, commanding, or extracting a soul is **Free or divine-tier only.** Anima and Animus are eros and logos — stances of mind — not a license to touch a soul.

**Creatures are rune-formulas** that (1) state the weakness (strip a load-bearing rune or hit an opposed element), (2) tell the history (born vs raised; place-fingerprints), (3) set difficulty by complexity. They speak in the Charter weave as a grouped recipe, **as written** — Life is a mark, not a join to unfold. The ash mite is **Fire · Salt · Life** (soulless living). The adept is **Sulphur · Salt · Mercury** (mind, body, soul). Undead reads `{… Mors · Aether}`; ensouled carries Mercury → **can cast back**; soulless cannot.

---

## 6. Aether as prima materia

Aether = the **philosopher's stone**, union of a **Light aspect** (sol · projective: **Vita, Lumen**) and a **Dark aspect** (luna · receptive: **Mors, Umbra**). Opposed pairs: Vita⟷Mors, Lumen⟷Umbra. **Animus** and **Anima** are now wrought logos/eros (`Fire · Sulphur · Air`, `Water · Sulphur · Earth`), not a second sexed pair on Aether. Male and Female parse as those two joins.

---

## 7. Items & progression

- **Wards / passives** — e.g. a *fire cloak* removes the standing cost of recasting water/ice.
- **Rune-mediums** — e.g. a *lamp* puts a Fire rune in your field where none flows; or (Free) a medium that makes one specific spell reliable.
- **Stones / artifacts** — seat a concept in a gate; most gates have another solution, so they usually *ease* rather than hard-lock. They also name the idea (hunger, yield, ice, spark).
- **The pack** — stones, artifacts, charms, wards, and mediums are carried. **I** opens the pack to look; click an item to read it. Each look is a short hint at the rune (the spirit is motion; salt of the earth; fire from the sky). Doors still gate on possession, not on using an item from a menu.
- **The info box** — the Rune Magic panel is a running look: mouse-over or the current target, then `You see` and the description. Workings are spoken as the runes that were written, or as a name the player saved for that exact composition. Spark and Fire · Air are different writings of the same join.
- **The Primordial gate** — access to Primordial is *only* opened by acquiring an item or performing a deed.

Free magic is **item-intertwined** (lacking understanding, it leans on mediums/foci — the source of off-focus reliability). Free magic and its items **reveal their runes** — a core learning mechanic. Acquisition is **non-linear**: varied difficulty, some main-quest (skippable), some needed regardless of route.

---

## 8. Charter Cast, Store, and Free Cast

Not a stance toggle. Three actions on the same string:

| Action | What it does |
|---|---|
| **Charter Cast** | The sentence must already be written. Wrong or unfinished recipes **fizzle**. Reliable. Overpowers; does not dispel. |
| **Store** | Holds one **Charter** sentence to aim later. **Free cannot be stored** — it is wild and untamable. An item may later hold a single Free working; that is a Charter-path benefit you do not get for free. |
| **Free Cast** | Completes the string. **Unscrambles** a valid bag of runes into a written sentence. Fill budget starts at **1** (2/3 of a recipe is enough). Several matches → **attunement-weighted random pick**. Using a spell or type grows that focus (clash weight + larger effect). Death-work the Charter will not write still needs Free. |

| | **Charter** | **Free** |
|---|---|---|
| Access | open to all; rejects no one | open to all |
| Reliability | the written sentence in order, or nothing | unscrambles; fills blanks; clashes are a roll |
| Power in a vacuum | tamer | wilder, and it grows with use |
| Clashes | there are none — exact or fizzle | attunement leans the roll |
| Forbidden | can't touch souls / the worst grave-work | all available |
| Cost | feeds the pantheon | feeds nothing; blasphemous |
| Store | yes — that is the point of coherence | no (unless a later item binds one) |
| Ceiling | high (climbs to primordial) | low (easy mode) |

**Rule: free magic is never the required key — only the tempting shortcut.** Backfire still has teeth later (inverted targets, divine attention, hubris/taint). Fill budget may rise with items; the matcher already takes a budget, not a hard-coded “one”.

---

## 9. Cosmology

- **Primordial magic** is the god tier — reconstructs runes and thus reality. Charter and Free are two **applications**, seen as opposed.
- **Primordial is outside the Charter** → reaching for it *is* free magic. Ascension = transgression. It needs both **reach** (Free) and **coherence** (Charter); each archetype starts with one and must become partly what it despises.
- **The pantheon:** beings who use primordial magic and govern the realm; authored the Charter and (per them) the world. It empowers them (siphon) and **indirectly** fences mortals below the ceiling. Motive is **self-preservation** — an ascendant is a rival *in potential*, never their equal.
- **Enforcement is outsourced to belief.** No divine hunters; indoctrinated Charter mages persecute Free sorcerers themselves. The gods act only through the Charter and the faithful — until the top.
- **Deliberately mysterious** origin. *(Private north star, never canon: they made the world but came from elsewhere.)*

---

## 10. Environmental interaction

**The world is made of the same materials as spells** (water, wood, oil, gas, stone, lava, ice, grass), so the four combination laws govern **spell-on-world**, not just spell-crafting.

Tiles are **materials** (`WorldMaterial` / `MaterialId`), each with its own paint and a **full signature** — roots plus the manifestation the mix has already become. Timber is Water · Earth · Salt · Plant, not a lone Earth. Those signatures do not hover on the floor while you walk. They unroll in the **Charter** as a sideways-scrolling grid: odd rows travel right, even rows travel left. If a wrought join already stands in the room — Spark, Plant, Ice, Ash — the weave shows **that rune as itself**, so you can click Spark and send it. The basics that compose it are still there, **strewn through the grid** (Fire and Air from Spark, not glued to it). Knowledge still works where the join is absent: Fire · Air writes Spark in a room that only has hunger and breath. Creature recipes stay **as written**. One continuous sentence of **what is on screen**. Off-screen tiles do not speak; you cannot string a rune that is not in the camera view. **Air is ambient** — breath is already in any room that still has a floor or a wall. A view that is only void has no Air. **The adept’s recipe is always in the weave**: Sulphur · Salt · Mercury (mind, body, soul). Creature formulas enter when the scan reaches their tile and stay **as written** — the ash mite is Fire · Salt · Life, and Life is not unfolded. The weave is **what the camera can see** — if a mark is on screen, it is valid to string. Generation puts **at least one of each available rune** in the grid; extra copies follow how often that material appears. Voids tear the weave and speak **Dark** (withheld). Hover a Charter mark to see where it is from. Locks and world-strings enter when they are in view. The player reads glyphs there, then weaves.

Each material now carries flammability, conductivity, a burn clock, a **0–10 Hunger** grade, and a **0–10 Quench** grade in `MaterialCatalog.Flag` (`BurnRate` is derived from the clock). Spread uses Hunger. Wet work uses Quench. Dry stone (quench 0) leaves a fire alone; mud (3) suppresses it; water (10) puts it out.

| Flag | Negative | Zero | Positive |
| --- | --- | --- | --- |
| **Hunger** | — | Neutral — spell volume only | 1–6 catch from a 7+ source (equal-or-weaker, that source's reach, touching fuel); 7–10 are strong sources (oil 10 / reach 4, timber 8 / reach 2, plant 6) |
| **Quench** | — | Dry — no neighbor effect | 3–8 suppress; 9–10 put fire out (mud 3, rain 7, water 10) |
| **Flammability** | Tracks Quench (`−grade × 0.16`; water −1.6) | Will not catch | How readily hunger takes it (oil 2.2, timber 1.6, plant 1.1, grove 0.85) |
| **Conductivity** | Insulator — wood and plants disrupt the path (timber −0.9, plant −1.1, grove −1.2) | Neutral — may hold a spark but will not pass it (stone, dirt, sand) | How freely a spark travels (metal 1.6, water 1.25, vein 0.85) |
| **BurnSeconds** | — | Not fuel | How long a full fire lasts (oil 5s, wood 4s, plant 3s, grove 2s). Ember is a Fire path, not fuel. |
| **BurnRate** | — | A 5s leftover stays put | Clock leftover (`5 − seconds`: oil 0, wood 1, plant 2, grove 3). Spread uses Hunger. |

Tiles keep live state: **Fire**, **Wet**, **Charge**, **Growth**. **Sprout** lays plant cover in a three-tile disk from your feet, the way ice covers water — it does not walk the pool. Water a land plant with a spell and it may take a neighbouring water tile. Plant cover already on water stays put. **Forest** (`Plant · Life · Anima · Plant · Life`) drinks every water still on the screen. A short douse only wets what it hits. Stamps and covers do not start that work. Fire a spell starts still spreads onto equal-or-weaker fuel from a strong source (7+), out to that source's reach (hunger − 6), if that cell touches fuel toward the flame — it does not leap a gap. When the clock is spent the plant gains an **ash covering** and the walk becomes **dirt** (look and Earth) if the floor itself was fuel; masonry stays stone under the ash. Fire cover wears off. Wet neighbors use Quench: mud smothers, water extinguishes, dry stone does nothing. Charge walks **conductive** tiles (metal, water, wet stone, vein). Neutral stone can take a bolt but will not pass it unless a neighbor conducts. Wood and plants **insulate** — they break the path, even on metal. Overlays (`tile-fire`, `tile-wet`, `tile-charge`, `tile-grow`) make the reaction visible.

Marquee reactions (each a puzzle key):
- **Lightning + Water(floor) → conduction** — charge runs the pool.
- **Fire + Plant/Wood → spreading burn** — wood lasts longer than plant. Wood is four seconds; plant lasts three; grove burns out in two. When the clock is spent the tile **gains an ash covering**; a plant or timber floor becomes dirt (look and Earth). Masonry stays. Fire cover is tinder and wears off.
- **Fire + Oil → flash** — oil spreads flame across connected fuel in one breath, much faster than timber. It floats: a film on water still burns and flashes. A lit geyser stays kindled until yield is thrown. A plant standing on water can light, but it does not carry the flame.
- **Water + Plant → growth** — a player or NPC plant or water spell grows a vegetable body toward Grove. **Sprout** lays plant cover three tiles from the caster. A watered land plant may take one neighbouring water tile (monsoon takes two). Plant cover on water does not walk further. **Forest** covers every water still on the screen. Dry pits, stamps, and covers do not start that work. Hunger spent on a plant **adds an ash covering** and turns a plant floor to dirt. Masonry under a burned table stays stone.
- **Water drowns** — yield holding a vessel has no floor. Walking a water tile (or a water-filled pit) sends you back. Flight and hop still clear it.
- **Water work fills a connected pit smaller than 4×4** with drowning water. Larger hollows stay open. Ice is how that water is asked to stand.
- **Blank floor is a pit.** Cells you never painted on Tiles, and a rim past the painted island, are Void. Walking off the ledge returns you to the last safe floor. Painted walls keep their cells. A pillar or wall drawn across the drop fills it as a two-tile span. Standard earth, ice, and wood must join two floors (or grab a wall) or the span falls. Metal needs no far rest. Ice over water freezes without banks; earth leaves a mud covering that will not hold you. Plant grows a walkable cover over water without banks. Hunger cannot stand on yield.
- **Water · Earth → Ice** — hard water that thaws, and **a walkable freeze over a pool**. **Water · Earth · Salt** is a water-pillar. **Water · Salt · Earth** is Plant. Ice-pillar, Ice-wall, Ice-spear, and Snowfall freeze water tiles. **Heat lives on the recipe** (`MatterLaw`): any fire-bearing sentence melts ice it crosses, including room ice, not only a conjured pillar. Glacier and glass need **witchfire** (`Flame` = Fire · Animus · Fire). Glacier is ice given logos (`Ice · Animus · Ice`); that Fire inside Animus is not hunger. **Melt** (`Fire · Salt · Mercury`) bores stone and steel masonry — a wall at the map edge opens, and you can tunnel if you know the sentence. **Plasma** eats ordinary matter. **Obsidian**, **wardstone**, and **aegis** refuse Melt, Shatter, plasma, and hunger's thaw. Grave-ice (Water · Salt · Death) is Free/arcane.
- **Earth · Water → Mud** — rest meeting yield. Soft ground. Water · Earth is Ice.
- **Lava · Salt · Water → Obsidian** — hungry earth quenched and given a body. Instant bridge over a hazard. The wall is Obsidian · Salt · Obsidian.
- **Oil shot** slicks a surface so it can hold flame, and grows a fire already standing (including a fire-golem). **Oil puddle** (`Oil · Salt`) stands a small pool at the click. **Oil geyser** (`Oil · Salt · Mercury`) is a fountain: once hunger finds it, the fire stays like a kindled hall until yield is thrown. **Oil slick** (`Oil · Salt · Oil`) runs outward from a point and covers a wide floor, including water. Oil lasts five seconds and spreads flame across connected fuel in a flash — much faster than timber or plant — and a film on water still runs. Stood oil, wood, and plant props use the same clocks (5s / 4s / 3s). **Oil-pillar** is a stood wick. A later fire sentence — a separate recipe — makes it a bomb. **Wind + Fire → firestorm** (can blow back) still later.
- **Conjured walls and pillars stay.** A stood body is masonry or a column, not a flash. It yields to an opposed element, or to matter-breaking work. **Water melts a basic earth wall and puts out a flame wall.** Water cools a lava wall into rock; a hurled stone or **Shatter** (`Earth · Salt · Earth · Air · Mercury`) then breaks that rock. Fire thaws ice wherever it stands. **Melt** bores stone and steel, including the room's own walls. Fire eats vine and wood. **Obsidian** will not take the work. Death (or yielding yourself with **K**) drops every pillar, wall, and hanging veil you stood in the room; stones and artifacts stay in the pack.
- **Standing in hunger.** A burning floor kills in **eight seconds** unless you wear a water ward or put the fire out. A **kindled hall** (the Aura-Fire brush — painted fire that stays until yield is thrown) kills at once without a water ward. Cover-Fire is only a mark so the weave speaks Fire. Fire-golems die to any water work and to earth work; they only shrug off base fire and wind.
- **Fog and miasma linger** as hanging veils. **Wind** (Gust / Gale / Push) takes miasma. Light lifts fog, not the foul cloud. **Poison** is a liquid slick on the walk — contact only, and yield washes it.

Reactions **cascade** (fire spreads, wet grows, charge runs). **Charter** reactions are controlled; **Free** reactions are bigger but can spread to terrain you needed — the free-magic tax made physical.

A beginner wizard writes a fireball for **two seconds** and commits the facing when the sentence starts — hop over it, raise a wall, or get behind them. Golems slam in reach. Arrow racks fire real shots down a heading; a stood body (wall / pillar) breaks them, and so does room masonry. Your own shots break the same way. Stoneskin breaks arrows and slams only. Death is temporary: the adept returns to the **spawn crystal**, which names what found them and, if a spell unmade them, shows the marks that wrote it. Pits still use the last safe floor.

---

## 11. Gameplay loop

The verb is **casting**, not puzzle-solving. You craft a spell from the runes available and cast it to overcome the obstacle — rarely one "correct" answer, only spells you can build.

The player **moves and casts**. Perception is a stance, not a tile overlay. The world has two pictures: the **tiles** (what you walk on) and, only in the Charter, a **woven grid** of runes spoken by those tiles, by creature formulas, and by world-strings **that are on screen**. Glyphs are not glued to a floor square and they do not follow you while you explore. **Space** opens the **Charter**. The weave is the room’s sentence. Play does **not** keep a wall of root marks at the top — you draw from the grid, or send a kept Grimoire working if those marks are around. Remembered marks will sit on that wall later. You cannot string a rune that is not in the camera, except that Air is already there wherever the room can be breathed, and **you** are already there as mind · body · soul. Those three are also **carved into the hub floor** and held on pillars around the crystal, so the adept’s recipe can be read in the room. You string runes — up to eight — then **Charter Cast**, **Store** (Charter only), or **Free Cast**. Click a cell in the weave to draw that rune. A Grimoire page that needs marks not in the weave says so, and shows the missing marks.

World altars do not teach by writing FIRE. They put the **mark beside a picture** of the thing — flame, water, rock, gale; a standing body, an opening path, a mind. Play hides the name. Develop still writes it.

A **recent-cast strip** (last twenty-five) sits on the Charter and the world, with a **Grimoire** tab beside it. ○ the sentence held; ✕ it fizzled. Charter leaves the marks. Free blocks them — wild work is not written down. Keep a working and it stays in the book so you can send it again after it leaves the strip.

**Two sights, toggled in play (F1 or the bar):**

| | **Play** | **Develop** |
|---|---|---|
| Marks | Abstract work-signs. No letters, names, or element colours. | Letters, names, birth recipes, and the palette. |
| Wall | Hidden. Draw from the weave. Remembered marks will sit here later. | The eleven plus the elemental joins (Spark, Lightning, Ice, Plant…), named. Off-screen marks stay grey. |
| Book | Kept workings only. Click a saved page to send it if those marks are in the weave. | The full written ledger (click a name to string it). |

Play is the game. Part of the fun is learning what a mark *does*. Simple sentences stay worthwhile because you have to find the marks and keep them. Develop is for authoring.

1. **Assess** — the obstacle (an enemy's nature/weakness, or terrain in the way), and the sentence the room is writing. Ash Court reads Ash as itself, then the mite as Fire · Salt · Life, and you as mind · body · soul. The Drop is a tear. Storm Cell writes **Spark** as itself; Fire and Air are strewn through the grid.
2. **Craft** — draw glyphs out of the room’s weave, or send a kept Grimoire working if those marks are around. Two runes birth a join or wait. A finished spell is a sentence.
3. **Aim** — Charter Cast or Free Cast from the wall, or later from the held Charter slot. The chain already chose Shot, Pillar, Grow, Remote, or Self. Click where that form goes. Unwritten or scrambled Charter strings fizzle. Free unscrambles a valid bag, fills up to the fill budget, and, on a clash, attunement picks the whole sentence — form included.
4. **Overcome** — the right spell at the right place resolves it at once. No HP bar. A missed key can still leave a status or start a tile reaction. A slam or a shot that lands sends you to the crystal.

Knowing an enemy's composition tells you *what spell it's vulnerable to* — you then **cast that spell**; it is not an abstract rune-puzzle. **Many solutions per obstacle** (torch behind a waterfall: freeze the fall, grow a plant, or raise a flame pillar). **Difficulty scales without stats:** the substance/form you need may be hard to build from what's flowing (decompose a primordial, use an item, reposition), the enemy's nature may demand a specific spell, or the environment may fight your casting. The same system runs **traversal**.

---

## 12. Open threads

- [~] **Spell catalog** — written story-chains in `SPELLS.md`. Weather is spells (Storm, Fog, Darkness, Thunder, Blizzard), not runes. **76** Water-pillar. **77–78** Oil shot / Oil-pillar. **79–80** Poison / Miasma. **81** Plasma (`Flame · Lightning`). **82** Fire-pillar (`Fire · Salt`) — temporary without a source. **71** Grotto (`Plant · Dark`). **83** Monsoon. **84** Dirt toss. **85–86** Metal-pillar / Metal-wall (`Lava · Spark · Earth`). **87** Obsidian-wall (`Lava · Salt · Water · Salt · Lava · Salt · Water`). **88–90** Balm, Chorus, Drive (`Anima · Mercury`, `Anima · Salt`, `Animus · Mercury`). **91–92** Tree / Wood-wall (`Plant · Life · Salt`, `Plant · Life · Salt · Plant · Life`). **93–95** Oil puddle / geyser / slick (`Oil · Salt`, `Oil · Salt · Mercury`, `Oil · Salt · Oil`). **96** Forest (`Plant · Life · Anima · Plant · Life`) covers visible water. Vine and Wind are spells, not runes. **Focus holds mind spells**, including wards. Elemental clocks stand on their own.
- [~] **First floor** — The Foundation. Hub, four element wings, Door I, three aspect sanctums, Door II, then the Wrought Courts (keys, inner doors, Door III). See [`FLOOR1.md`](FLOOR1.md). Connected water (vault basin → Door I moat) and steam-secrets on the Fire return-trip are still open.
- [x] **Free-mage reliability** — attunement (focus) + items/mediums (off-focus). *(Resolved.)*
- [x] **Anima / Animus** — eros and logos, born `Water · Sulphur · Earth` and `Fire · Sulphur · Air`. Male/Female are the old names. Anima opens (AoE, heal). Animus asserts a magical quality. Flame is `Fire · Animus · Fire`. Glacier is `Ice · Animus · Ice`. Sulphur stays on status.
- [~] **Death rune** — reserved for grave-work and Free. Not in ordinary ice/stone/pit recipes. Charter fizzles the worst of it.
- [x] **Formation vs aspect** — aspect is nature; formation is written in the chain (Earth stands, Mercury-into is Remote, breath+Mercury is Shot). No cast-time Remote / Pillar fork.
- [~] **Field economy** — tiles are materials with full signatures plus **flammability** and **conductivity** numbers. Fire, wet, charge, and growth now tick. A spell-watered land plant may take a neighboring water floor or water covering; plant cover on water stays put, like ice, unless Forest drinks the visible pool. Stamps do not start that work. Water fills small hollows and **drowns** until ice freezes it. The weave is Charter-only: **what the camera can see**. If a mark is on screen, it is valid. Generation puts at least one of each available rune in the grid; extra copies follow how often that material appears. The Charter shows that sentence as a **clipped belt** — hover stills it and names where a mark is from. A wrought join on screen appears as itself; its basics are still there when that material is in view. Creature recipes hang as written when the being is on screen. Casters show the marks they are writing over their heads. Depletion still open. Primordial runes later. Catalog: [`MATERIALS.md`](MATERIALS.md).
- [~] **Combat that can kill** — golems slam, beginner wizards spend two seconds on a fireball, arrow racks fire projectiles. A caster shows the marks they are writing over their head. Lock-and-key still unmakes. Death respawns at the crystal. A real death / last-rites pass is later.
- [~] **Free attunement** — use grows a type and a named spell (clash weight + potency). Fill budget is 1, stored as a number. Free also unscrambles a valid bag of runes. Decay of unused types, higher budgets, and a Free-store item are still open.
- [ ] **Free attunement tuning** — build/decay rates, how many focus runes, off-focus penalty steepness.
- [ ] **Path model** — hard class, taint accumulation, or fully fluid with consequences only.
- [~] **Learning surface** — Play sight hides names and the Charter wall. Draw from the weave, or send a kept Grimoire working if those marks are around. Remembered marks will sit on the wall later, with keep-conditions by rune depth. The Develop ledger remains the full book and still lists the eleven on the wall.
- [~] **"Reading" creatures** — Play hides formula text on the chrome. The weave still shows the marks. Whether the player *understands* a living recipe is still open.
- [ ] **Item catalogue** — concrete mythic items and the gates they touch.
- [ ] **Magnum Opus color meta** — Nigredo → Albedo → Citrinitas → Rubedo as chapter/tier/world-tint.
- [~] **Finished sprites** — generated painters are a fallback. Drop PNGs in `Assets/Resources/Sprites/` or point `art.json` at them. First replacements: the adept and the living locks. See [`ART.md`](ART.md).

---

*Provisional names throughout (Spark, Lava, Vita, Mors, Animus, Anima, Lumen, Umbra, branch materials) pending ratification.*
