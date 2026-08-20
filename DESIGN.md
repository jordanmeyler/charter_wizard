# Rune Magic — Design Reference

*A 2D puzzle-RPG where the player perceives the runic substrate of reality and composes spells from it. The correct spell (or combination) instantly resolves an encounter — combat is a lock-and-key puzzle, not a damage race. Living source of truth. Version 0.8. Spell catalog: [`SPELLS.md`](SPELLS.md). Material tree: Liber Materia.*

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
- **Free = attunement.** Intuitive, emotional casting. Spells get more effective the more they're cast — but as **specialization, not a power bar**: attunement deepens one focus at the **direct expense of the others**, and "more effective" = reaching further up that rune's branch (enough Fire → chain lightning, call lava, while water-rooted casting withers).
- Consequences: Free casters **specialize hard**, **patch weak elements with items and unique free spells**, and must **understand a branch to exploit it** — so even the intuitive path forces learning the tree.

### Free as the default onboarding path
Players distrust orthodoxy by default, so most reach for Free first — the design leans in. The intuitive, item-friendly, rune-revealing path is the on-ramp; later the story **uses the player's anti-authority expectations against them** (the orthodoxy is more tangled than it looked; Free's costs and low ceiling are real; the "villains" are sincere believers; the union truth). An expectation-subversion engine that **encourages replays**.

*The three systems (Charter, Free, Primordial) are kept mechanically separate for now; their "union" is a narrative endgame idea, not a gameplay blend.*

---

## 2. The rune families

### Materials — *what the spell is made of*
The elements and everything they blend into (section 3 + `material-codex.html`).

### Aspects — *what the spell is doing* (required; never implied by an element)
An elemental string is **not a spell**. Charter refuses it. The aspect is a non-elemental rune. Soft tendencies remain: **Body = me, Spirit = them, Mind = the world** — but the aspect no longer picks the formation. That is chosen at cast (section 4).

| Rune | Family | Principle |
|---|---|---|
| **Salt** | Tria prima | Body / matter. Lasting, still. Asks for a standing shape. |
| **Mercury** | Tria prima | Motion / spirit. Flow and flight. Asks to travel. |
| **Sulphur** | Tria prima | Mind / soul-mood. Eruption, fear, sleep, command. |
| **Life** (Vita) | Existential | The animating pole. Growth, wells, living stone. |
| **Death** (Mors) | Existential | The stilling pole. Snuffs, stills, ends. |
| **Light** (Lumen) | Divine | Sol, projective light. |
| **Dark** (Umbra) | Divine | Luna, receptive dark. |
| **Animus** | Divine | Projective soul. Drive, the going-out. |
| **Anima** | Divine | Receptive soul. Draw, the taking-in. |

### Catalyst — Aether (section 6)
Inert alone. The prima materia / philosopher's stone. Present in the field; never the required aspect.

### Existential & polarity
- **Life ⟷ Death** (Vita ⟷ Mors) — animating axis. These **are** form-aspects.
- **Animus ⟷ Anima** — projective ⟷ receptive. Shown on the wall. The old Male/Female labels remain in the catalog as aliases and are not offered.

### Primordials — *the deep layer masters can work with*
- **Mundane:** **Hot/Cold**, **Wet/Dry**. Synthesizable; **knowledge-gated only.**
- **Divine:** components of the Aether aspects (section 6). **Not craftable** later. This slice shows Light, Dark, Animus, and Anima on the Charter wall so the grammar can be played; the item/deed gate returns when we ratify it.

---

## 3. The material tree

### The grammar of a spell
Every written spell has **three** parts. The first two are runes; the third is chosen when you cast.
- **Elements = substance.** Blend for richer matter: Fire+Air=**Spark**, Air+Water=**Cloud**, Water+Earth=**Mud**, Fire+Earth=**Lava** (shared quality → stable); Fire+Water=**Steam**, Air+Earth=**Dust** (opposed → violent, area-filling).
- **Aspects = nature** (required, non-elemental). They colour what the substance is *doing*. They do **not** pick Shot vs Pillar vs Spread vs Remote.
- **Formation = placement** (section 4). Chosen at cast, and only if that material × aspect can naturally take that form.
- Complex substances take a blend **and** an aspect: **Ice = Water · Salt · Death** (water given a body, then stilled). Cold is a quality of the square, not a rune you string for frost. Anima is draw and receive, not ice.
Masters may also **quality-shift** (add a mundane primordial) to nudge a substance. Each combination law is also an **environmental reaction** (section 10) — terrain is made of the same materials.

### The quality square
| | **Dry** | **Wet** |
|---|---|---|
| **Hot** | Fire | Air |
| **Cold** | Earth | Water |

Shared edge → stable; diagonal (Fire/Water, Air/Earth) → violent.

### The tree
Full branch-by-branch tree and the long spell list live in **[`SPELLS.md`](SPELLS.md)**. Families mix runes in different ways — there is no single Material × Aspect law.

**Plant = Water · Earth · Salt** (wet earth given a body). Add Life to grow. **Ice = Water · Salt · Death** (water given a body, then stilled). Water · Death with no Salt is Still — the pool stops and stays water. Codex “Mud · Life · Aether” is creature-grade soulless plant-life. The starting sixty live in [`SPELLS.md`](SPELLS.md).

---

## 4. The spell grammar

There is **no damage**. A spell kills, restrains, or does neither (traverse, heal, hide, lift, summon, transform).

Families mix differently (Energy travels or erupts; Life is wet earth given a body; Mind is Sulphur coloured by a substance; Manifestation needs class runes *and* a creature formula). Formation is how you place a written spell, not a third rune.

A combo that *looks* as if it should work can still fizzle if it is not written. The draft catalog is [`SPELLS.md`](SPELLS.md).

Casting is no longer automatic. **Cast** (or the held slot / F) opens aim:
1. Pick a formation the string can naturally take.
2. Click the world — aim a line, raise a column, release at your feet, or place at a distance.

| Formation | What you do | Typical use |
|---|---|---|
| **Shot** | Click through a point. The spell flies that way. | Bolts, jets, hurled stone |
| **Pillar** | Click the ground. A column rises there. | Flame-pillar, ice, stone, vine |
| **Spread** | Click to confirm. It wells from where you stand. | Frenzy, live-floor, fog, spring |
| **Remote** | Click a distant point. The spell forms there. | Snuff, lull, dread, raised earth |

Formations are **not** generic. Mercury does not always shoot; Salt does not always pillar. Fire × Salt will not offer Shot. Fire × Life offers nothing — fire does not animate. Water × Mercury will not stand as a pillar. The holes are the point.

**Charter:** an unwritten combo **fizzles**, even when the formation made sense. **Free:** an unwritten or blank string **borrows a random written spell of that type** (same material, then same aspect, then same formation if any exist). Free is still never the required key.

Written slice (not exhaustive; the Grimoire lists the live set):

| Material | Aspect | Formation | Spell |
|---|---|---|---|
| Fire | Mercury | Shot | Fireball |
| Fire | Salt | Pillar | Flame-pillar |
| Fire | Sulphur | Spread | Frenzy |
| Fire | Death | Remote | Snuff |
| Fire | Light | Shot | Sun-lance |
| Fire | Animus | Shot | Drive |
| Water | Mercury | Shot | Water-jet |
| Water | Death | Pillar | Ice-pillar (string Salt too — the body) |
| Water | Sulphur | Remote | Lull |
| Water | Life | Spread | Spring |
| Spark | Mercury | Shot | Lightning bolt |
| Spark | Salt | Spread | Live-floor |
| Earth | Mercury | Shot | Hurled stone |
| Earth | Salt | Pillar | Stone pillar |
| Earth | Salt | Remote | Raised earth |
| Air | Mercury | Shot | Gale |

The *same* material still solves an obstacle differently by form: a fire bolt is doused behind a waterfall; a flame-pillar rises behind it. Form is now a choice you make, not a property of Mercury.

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

## 8. Casting stances — Charter vs Free

A **stance chosen per cast**, not two spell lists.

| | **Bound (Charter)** | **Unbound (Free)** |
|---|---|---|
| Access | open to all; rejects no one | open to all |
| Reliability | reliable | unreliable, variance-loaded |
| Power in a vacuum | tamer | higher magnitude |
| Clashes | **wins** (coherence beats magnitude) | loses; even with an edge may sputter |
| Forbidden | can't touch souls / living runes / re-animation | all available |
| Cost | feeds the pantheon | feeds nothing; blasphemous |
| Ceiling | high (climbs to primordial) | low (easy mode) |

Charter **overpowers**, it does not dispel. **Free-fill:** leave runes blank, let them fill at random, overcharged; the specified:auto-filled ratio is a **risk dial**. Backfire has teeth (inverted targets, divine attention, hubris/taint). **Rule: free magic is never the required key — only the tempting shortcut.**

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

Marquee reactions (each a puzzle key):
- **Lightning + Water(floor) → conduction** — strikes everything in the pool.
- **Fire + Plant/Wood → spreading burn** — runs along connected material, clears cover, leaves Ash.
- **Water · Salt · Death → Ice** — bridge, root-trap, or shatterable surface.
- **Water + Lava → Obsidian** — instant bridge over a hazard.
- **Fire + Gas/Oil → explosion**; **Wind + Fire → firestorm** (can blow back); **Earth + Water → Mud** (bogs movement).

Reactions **cascade** (fire spreads, water flows, gas chains). **Charter** reactions are controlled; **Free** reactions are bigger but can spread to terrain you needed — the free-magic tax made physical.

---

## 11. Gameplay loop

The verb is **casting**, not puzzle-solving. You craft a spell from the runes available and cast it to overcome the obstacle — rarely one "correct" answer, only spells you can build.

The player **moves and casts**. Perception is a stance, not a tile overlay: **Space** stills the field into a **Charter wall** (runes over the world, veil behind them). The constant stream is the four primaries, the tria prima, Life, Death, Light, Dark, Animus, Anima, and Aether, plus nearby tiles and creature formulas. You string runes — up to eight — then **Cast** (aim a formation) or **Store** one string to carry. Esc lists every *written* recipe (a developer ledger, not a knowledge gate).

1. **Assess** — the obstacle (an enemy's nature/weakness, or terrain in the way), the environment's materials, and the runes flowing in your field.
2. **Craft** — assemble a spell from the Charter wall: element(s) for substance, a **non-elemental** aspect for nature, a stance (Charter/Free). An element alone is not a spell.
3. **Aim** — Cast from the wall, or later from the held slot. Choose Shot, Pillar, Spread, or Remote if that string can take it, then click where it goes. Unwritten Charter strings fizzle. Free borrows a random written spell of that type.
4. **Overcome** — the right spell at the right place resolves it at once. No HP bar.

Knowing an enemy's composition tells you *what spell it's vulnerable to* — you then **cast that spell**; it is not an abstract rune-puzzle. **Many solutions per obstacle** (torch behind a waterfall: freeze the fall, grow a plant, or raise a flame pillar). **Difficulty scales without stats:** the substance/form you need may be hard to build from what's flowing (decompose a primordial, use an item, reposition), the enemy's nature may demand a specific spell, or the environment may fight your casting. The same system runs **traversal**.

---

## 12. Open threads

- [~] **Spell catalog** — starting sixty in `SPELLS.md`. Ice uses Death, not Cold. Grow from here.
- [x] **Free-mage reliability** — attunement (focus) + items/mediums (off-focus). *(Resolved.)*
- [ ] **Male/Female role** — projective/receptive utility, sacred generative pair, or both.
- [ ] **Death rune** — sanctioned uses vs off-Charter (lean: policed by soul-involvement / primordial reach, not morality).
- [x] **Formation vs aspect** — aspect is nature; formation is a cast-time choice (Shot / Pillar / Spread / Remote). Sparse recipes. Charter fizzles; Free borrows.
- [~] **Field economy** — Space stills the field into a Charter wall (primaries, tria prima, Life/Death, Light/Dark, Animus/Anima, Aether, plus nearby tiles and creature formulas). Depletion and Aether scarcity still open.
- [ ] **Free attunement tuning** — build/decay rates, how many focus runes, off-focus penalty steepness.
- [ ] **Path model** — hard class, taint accumulation, or fully fluid with consequences only.
- [ ] **Learning surface** — how known/unknown knowledge is tracked and *felt* (a grimoire?); recording rune-reveals.
- [ ] **"Reading" creatures** — formula visible by default, or interpreting it requires learned knowledge?
- [ ] **Item catalogue** — concrete mythic items and the gates they touch.
- [ ] **Magnum Opus color meta** — Nigredo → Albedo → Citrinitas → Rubedo as chapter/tier/world-tint.

---

*Provisional names throughout (Spark, Lava, Vita, Mors, Animus, Anima, Lumen, Umbra, branch materials) pending ratification.*
