# Rune Magic — Design Reference

*A 2D puzzle-RPG where the player perceives the runic substrate of reality and composes spells from it. The correct spell (or combination) instantly resolves an encounter — combat is a lock-and-key puzzle, not a damage race. Living source of truth. Version 0.11. Spell catalog: [`SPELLS.md`](SPELLS.md). World materials: [`MATERIALS.md`](MATERIALS.md). Eleven basic runes; joins are wrought runes. Primordials later.*

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
| **Salt** | A body. “This, here, is a thing.” |
| **Mercury** | Going. A path opening. Through space if breath is already in the chain; *into* a thing if not. |
| **Sulphur** | Passion. A mind that can be reached. |
| **Light** | Shown. The veil is lifted. |
| **Dark** | Withheld. The veil is drawn. |
| **Life** | Modifier. Marks a living recipe (plant, heal, hop, a called beast). Not a school. |
| **Death** | Modifier. The grave. **Reserved** — not in ordinary recipes. Load-bearing for Free. |

A join is not a modifier. **Fire · Air** becomes **Spark**, which is its own rune and combines again (**Spark · Air → Lightning**). Two runes birth a join or wait as a clause. Three or more is a spell. Longer chains are stories.

Primordial runes (Hot, Cold, Wet, Dry, Aether, Animus, Anima) are **out of this pass**. They stay in the catalog as reserved names and are not on the Charter wall.

---

## 3. Joins and the tree

A chain is a sentence, left to right. Two roots meeting become a third:

| Join | Becomes | Concept |
|---|---|---|
| Fire · Air | **Spark** | Hunger given breath. A seed of charge. |
| Air · Water | **Cloud** | Breath holding yield. A hanging veil. |
| Water · Earth | **Mud** | Yield meeting rest. |
| Fire · Earth | **Lava** | Hunger meeting rest. |
| Fire · Water | **Steam** | Hunger forced through yield. |
| Air · Earth | **Dust** | Breath forced through rest. |

Then the wrought rune combines again: **Spark · Air → Lightning**, **Spark · Cloud → Storm**, **Cloud · Earth → Rain**. Ice is **Water · Salt · Earth** (yield given a body and asked to rest). Death is not how you freeze water. Plant is **Water · Earth · Salt**; Grove is Plant · Life.

The quality square (Hot/Cold, Wet/Dry) belongs to the primordial pass and is not used to explain joins right now.

Full wrought list and fifty story-chains: **[`SPELLS.md`](SPELLS.md)**. 1–40 ordinary (no Death). 41–50 Death / Free. Each combination law is also an **environmental reaction** (section 10) — terrain is made of the same materials.

---

## 4. The spell grammar

There is **no damage**. A spell kills, restrains, or does neither (traverse, heal, hide, lift, summon, transform).

A spell is a **chain that tells a story**. Fireball is not Fire · Mercury — that is hunger going *into* a thing. Fireball is hunger that learned breath, was given a body, and was sent: **Fire · Air · Salt · Mercury** (or **Spark · Salt · Mercury** if Spark is already in the field). Life only marks a living recipe. Death is not in the ordinary book. Mind-work can stay short (**Fire · Sulphur · Mercury**). Flight and Chain are longer because more happened.

**Formation is part of the spell.** The chain writes how it lands. There is no Remote / Pillar fork at cast time.

| The sentence does this | Form | Example |
|---|---|---|
| Asked to rest (Earth at the end of a body) | **Pillar** | Flame-pillar: Fire · Salt · **Earth**. Ice-pillar. Wall. |
| Sent *into* a thing, or placed away (Mercury, no breath) | **Remote** | Melt: Fire · Salt · **Mercury**. Pit. Rain. |
| Breath already in the chain, then sent | **Shot** | Fireball: Fire · Air · Salt · Mercury. Lightning. Ice-spear. |
| A body around your feet | **Spread** | Live-floor: Fire · Air · Salt. Fog. Sprout. |
| Kept on the caster | **Self** | Flight |

Cast opens aim for the form the sentence already wrote. Click the world — fly a line, raise a column, release at your feet, or place at a distance. You do not pick the form.

A combo that *looks* as if it should work can still fizzle if it is not written. The catalog is [`SPELLS.md`](SPELLS.md).

**Charter:** an unwritten or unfinished combo **fizzles**. No blanks are filled. **Free:** fills missing runes up to a **fill budget** (1 now; the matcher is written so the budget can rise). A 2-of-3 recipe can still become a spell. If several written chains fit, Free **picks at random, weighted by attunement**. A finished sentence is never “upgraded” by filling toward a longer one. Free is still never the required key.

The fifty catalog chains now resolve in play. Joins fold (Fire · Air is Spark). The Grimoire lists the full book; click a name to string it for testing. Short tutorial strings still work as a fallback. Charter fizzles Free-only Death-work.

---

## 5. Life, souls & creatures

- **Soulless life** — beasts, plants, bugs. Animate matter, **no soul, no magic.** Creating this is sanctioned.
- **Ensouled life** — carries a soul; **a soul grants magic across all races.** The player is ensouled.
- **The soul rule: Charter magic cannot touch souls.** Soulless creation/terrain-golems are legal; inserting, binding, commanding, or extracting a soul is **Free or divine-tier only.** This re-derives the forbidden acts (true mind-control, necromancy, the Anima/Animus soul-primordials).

**Creatures are rune-formulas** that (1) state the weakness (strip a load-bearing rune or hit an opposed element), (2) tell the history (born vs raised; place-fingerprints), (3) set difficulty by complexity. Reads: living carries **Vita**; undead reads `{… Mors · Aether}`; ensouled carries a soul-rune → **can cast back**; soulless cannot.

---

## 6. Aether as prima materia

Aether = the **philosopher's stone**, union of a **Light aspect** (sol · projective: **Vita, Animus, Lumen**) and a **Dark aspect** (luna · receptive: **Mors, Anima, Umbra**). Opposed pairs: Vita⟷Mors, Animus⟷Anima, Lumen⟷Umbra (expandable). *Male=light/female=dark is the traditional Sol/Luna coding, invertible.*

---

## 7. Items & progression

- **Wards / passives** — e.g. a *fire cloak* removes the standing cost of recasting water/ice.
- **Rune-mediums** — e.g. a *lamp* puts a Fire rune in your field where none flows; or (Free) a medium that makes one specific spell reliable.
- **Keys** — open a specific gate; most gates have another solution, so keys usually *ease* rather than hard-lock.
- **The Primordial gate** — access to Primordial is *only* opened by acquiring an item or performing a deed.

Free magic is **item-intertwined** (lacking understanding, it leans on mediums/foci — the source of off-focus reliability). Free magic and its items **reveal their runes** — a core learning mechanic. Acquisition is **non-linear**: varied difficulty, some main-quest (skippable), some needed regardless of route.

---

## 8. Charter Cast, Store, and Free Cast

Not a stance toggle. Three actions on the same string:

| Action | What it does |
|---|---|
| **Charter Cast** | The sentence must already be written. Wrong or unfinished recipes **fizzle**. Reliable. Overpowers; does not dispel. |
| **Store** | Holds one **Charter** sentence to aim later. **Free cannot be stored** — it is wild and untamable. An item may later hold a single Free working; that is a Charter-path benefit you do not get for free. |
| **Free Cast** | Completes the string. Fill budget starts at **1** (2/3 of a recipe is enough). Several matches → **attunement-weighted random pick**. Using a spell or type grows that focus (clash weight + larger effect). Death-work the Charter will not write still needs Free. |

| | **Charter** | **Free** |
|---|---|---|
| Access | open to all; rejects no one | open to all |
| Reliability | the written sentence, or nothing | fills blanks; clashes are a roll |
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

Tiles are **materials** (`WorldMaterial` / `MaterialId`), each with its own paint and a **full signature** — roots plus the manifestation the mix has already become. Timber is Water · Earth · Salt · Plant, not a lone Earth. Those signatures do not hover on the floor while you walk. They unroll in the **Charter** as a sideways-scrolling grid: even rows left to right, odd rows right to left, one continuous sentence of the current room. Contiguous same-material runs collapse to one clause. Voids tear the weave. Locks and world-strings enter when the scan reaches their tile. The player reads glyphs there, then weaves.

Marquee reactions (each a puzzle key):
- **Lightning + Water(floor) → conduction** — strikes everything in the pool.
- **Fire + Plant/Wood → spreading burn** — runs along connected material, clears cover, leaves Ash.
- **Water · Salt · Earth → Ice** — hard water that thaws. Grave-ice (Water · Salt · Death) is Free/arcane.
- **Water + Lava → Obsidian** — instant bridge over a hazard.
- **Fire + Gas/Oil → explosion**; **Wind + Fire → firestorm** (can blow back); **Earth + Water → Mud** (bogs movement).

Reactions **cascade** (fire spreads, water flows, gas chains). **Charter** reactions are controlled; **Free** reactions are bigger but can spread to terrain you needed — the free-magic tax made physical.

---

## 11. Gameplay loop

The verb is **casting**, not puzzle-solving. You craft a spell from the runes available and cast it to overcome the obstacle — rarely one "correct" answer, only spells you can build.

The player **moves and casts**. Perception is a stance, not a tile overlay. The world has two pictures: the **tiles** (what you walk on) and, only in the Charter, a **woven grid** of runes spoken by those tiles, by creature formulas, and by world-strings. Glyphs are not glued to a floor square and they do not follow you while you explore. **Space** opens the **Charter**: the eleven writeable runes on the wall, and the room’s sentence in a scrolling alternating-row grid. You string runes — up to eight — then **Charter Cast**, **Store** (Charter only), or **Free Cast**. Click a cell in the weave to draw that rune. Esc lists every *written* recipe and the material catalog (a developer ledger, not a knowledge gate).

1. **Assess** — the obstacle (an enemy's nature/weakness, or terrain in the way), and the sentence the room is writing. Ash Court reads Fire · Plant · Ash, then ember and salt. The Drop is a tear. Storm Cell writes Spark into the vein.
2. **Craft** — assemble a chain from the Charter wall, or draw glyphs out of the room’s weave. Two runes birth a join or wait. A finished spell is a sentence.
3. **Aim** — Charter Cast or Free Cast from the wall, or later from the held Charter slot. The chain already chose Shot, Pillar, Spread, Remote, or Self. Click where that form goes. Unwritten Charter strings fizzle. Free fills up to the fill budget and, on a clash, attunement picks the whole sentence — form included.
4. **Overcome** — the right spell at the right place resolves it at once. No HP bar.

Knowing an enemy's composition tells you *what spell it's vulnerable to* — you then **cast that spell**; it is not an abstract rune-puzzle. **Many solutions per obstacle** (torch behind a waterfall: freeze the fall, grow a plant, or raise a flame pillar). **Difficulty scales without stats:** the substance/form you need may be hard to build from what's flowing (decompose a primordial, use an item, reposition), the enemy's nature may demand a specific spell, or the environment may fight your casting. The same system runs **traversal**.

---

## 12. Open threads

- [~] **Spell catalog** — fifty story-chains in `SPELLS.md`. 1–40 ordinary (no Death). 41–50 reserved for Death / Free. Composer still folds the short slice.
- [x] **Free-mage reliability** — attunement (focus) + items/mediums (off-focus). *(Resolved.)*
- [ ] **Male/Female role** — projective/receptive utility, sacred generative pair, or both.
- [~] **Death rune** — reserved for grave-work and Free. Not in ordinary ice/stone/pit recipes. Charter fizzles the worst of it.
- [x] **Formation vs aspect** — aspect is nature; formation is written in the chain (Earth stands, Mercury-into is Remote, breath+Mercury is Shot). No cast-time Remote / Pillar fork.
- [~] **Field economy** — tiles are materials with full signatures. The weave is Charter-only: a sideways-scrolling boustrophedon of the current room. World-strings and the adept's place can hang more sentences later. Depletion still open. Primordial runes later. Catalog: [`MATERIALS.md`](MATERIALS.md).
- [~] **Free attunement** — use grows a type and a named spell (clash weight + potency). Fill budget is 1, stored as a number. Decay of unused types, higher budgets, and a Free-store item are still open.
- [ ] **Free attunement tuning** — build/decay rates, how many focus runes, off-focus penalty steepness.
- [ ] **Path model** — hard class, taint accumulation, or fully fluid with consequences only.
- [ ] **Learning surface** — how known/unknown knowledge is tracked and *felt* (a grimoire?); recording rune-reveals.
- [ ] **"Reading" creatures** — formula visible by default, or interpreting it requires learned knowledge?
- [ ] **Item catalogue** — concrete mythic items and the gates they touch.
- [ ] **Magnum Opus color meta** — Nigredo → Albedo → Citrinitas → Rubedo as chapter/tier/world-tint.

---

*Provisional names throughout (Spark, Lava, Vita, Mors, Animus, Anima, Lumen, Umbra, branch materials) pending ratification.*
