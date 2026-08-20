# Rune Magic — Design Reference

*A 2D puzzle-RPG where the player perceives the runic substrate of reality and composes spells from it. The correct spell (or combination) instantly resolves an encounter — so combat is a lock-and-key puzzle, not a damage race. This document is the living source of truth for the magic system. Version 0.3.*

---

## 1. Core premise, casting & progression

- Casting is **perception, not position.** Runes stream and orbit around the player through the fabric of reality (the "matrix"). You cast by *reading the field and composing from what flows through it* — not by standing on a particular tile.
- **The field is constant.** A novice and a master perceive exactly the same runes. There is no sight-leveling and no "seeing deeper." What changes is *understanding*, never perception.
- **Power is gated by knowledge and items — never by experience.** No XP, no levels, no stat growth. You get stronger by *understanding more* and occasionally by *acquiring something*. The vast majority of the game opens through knowledge alone.
- Every enemy is a **lock**; every spell you can assemble is a **key**. The right key resolves the encounter instantly.
- **The three gates:** *knowledge* (the main one — recipes and rune-meanings you've learned), *items* (shortcuts, wards, and the hard gate to Primordial — section 7), and *narrative grant* (the divine top tier). Most locks accept more than one solution, so sequence-breaking is a supported playstyle.
- **Learning loop:** you learn by *observation* — seeing runes in the world, in creature formulas, and especially in **free magic and free-magic items, which reveal their rune composition.** Borrowing an effect you don't understand teaches you the recipe; mastery is graduating from *borrowing* to *composing*. Free magic is, among other things, the game's tutorial substrate.

*The three systems (Charter, Free, Primordial) are kept mechanically separate for now; their "union" is a narrative endgame idea, not a gameplay blend.*

---

## 2. The rune families

Four families, plus a primordial layer beneath them.

### Materials — *what the spell is made of*
The elements and everything they blend into (section 3).

### Aspects — *what the spell does* (the tria prima)
Reshape a material into a spell. Soft tendencies, not laws: **Body = me, Spirit = them, Mind = the world.**

| Rune | Principle | Reshapes into… | Default target |
|---|---|---|---|
| **Salt** | Body / Matter | solid, lasting terrain; self-directed effects | oneself |
| **Mercury** | Motion / Spirit | projectiles, jets, flow; weakening | the enemy |
| **Sulphur** | Mind / Soul | mental effects: fear, sleep, command | something else |

### Catalyst — *the divine spark* (see section 6)
- **Aether** — inert alone. Reframed as the **prima materia / philosopher's stone**: the union of a Light and a Dark aspect. Permits animation and (with soul-primordials) soul-work.

### Existential — *the state and nature of a being*
- **Life ⟷ Death** — the animating axis.
- **Male ⟷ Female** — the polarity axis (projective ⟷ receptive; united, generative).

### Primordials — *the deep layer masters can work with*
- **Mundane:** the quality axes **Hot/Cold** and **Wet/Dry**. Synthesizable; **knowledge-gated only.** (Cold + Wet = Water from first principles.)
- **Divine:** the components of the two Aether aspects (section 6). **Not craftable from the field.** Access to Primordial magic is **hard-gated by an item or deed** (an artifact, a divine grant, an act) — knowledge alone never opens it.

> **Power gates:** physical mastery ← knowledge; item shortcuts ← acquisition; divine mastery ← narrative grant.

---

## 3. The material tree

### The quality square
| | **Dry** | **Wet** |
|---|---|---|
| **Hot** | Fire | Air |
| **Cold** | Earth | Water |

**Rule:** elements sharing an edge (a quality) blend **stably**; elements on a diagonal share nothing and blend **violently**.

### Blends (secondary materials)
- **Stable:** Fire+Air → **Spark** · Air+Water → **Cloud** · Water+Earth → **Mud** · Fire+Earth → **Lava**
- **Violent (opposed):** Fire+Water → **Steam** · Air+Earth → **Dust**

Violent blends are the unstable reactions free magic tends to produce messily.

### Deeper nodes
- Spark + Air/Water → **Storm** · Water toward Cold → **Ice** · Lava cooled → **Stone / Glass** · Mud + Air → **Sand**
- **Mud + Vita + Aether → Plant** (soulless life — the handoff from Materials into the Existential/Catalyst layer)

*(Full second/third-tier list is an open thread — section 8.)*

---

## 4. The spell grammar

**Material (noun) × Aspect (verb) = spell.** Orthogonal axes: one material cast three ways gives three unrelated spells.

| Material | + Mercury (motion) | + Salt (body) | + Sulphur (mind) |
|---|---|---|---|
| **Fire** | Fireball | Flame-wall | Frenzy |
| **Water** | Water-jet / wave | Ice-wall | Lull (sleep) |
| **Spark** | Lightning bolt | Live-floor | Jolt (stun) |
| **Earth** | Hurled stone | Stone wall | Dread |

Grow either axis to multiply the matrix.

---

## 5. Life, souls, and creatures

### Two kinds of life
- **Soulless life** — beasts, plants, bugs. Animate matter, **no soul, cannot use magic.** Creating this is sanctioned (Vita + matter).
- **Ensouled life** — carries a soul. **A soul grants access to magic, across all races.** The player is ensouled.

### The soul rule
**Charter magic cannot touch souls.** Making soulless life or terrain-golems is Charter-legal; *anything involving a soul* — inserting, binding, truly commanding, or extracting one — is beyond the Charter and therefore **Free or divine-tier only.** This re-derives the forbidden acts: true mind-control (reaching the soul, not the mood), necromancy (binding a soul into dead matter), and use of the **Anima/Animus** soul-primordials are all off-limits to Charter.

### Creatures as rune-formulas
Every creature is a formula doing three jobs:
1. **States the weakness** — overload with an opposed element, or **strip a load-bearing rune** (pull Aether → life goes out; dissolve Salt → body loses cohesion). Rewards reading over brute force.
2. **Tells the history** — `{Earth · Salt · Aether · Vita}` was *born*; the same with a graveyard/corruption rune was *raised* → a necromancer is near. Place-runes are fingerprints.
3. **Sets difficulty** — by composition complexity (2-rune tutorial beast vs 5-rune chimera holding an opposed pair in tension).

Reads straight off the formula: **living** carries Vita; **undead** reads `{… Mors · Aether}` (unmade by severing the Mors-binding); **ensouled** carries a soul-rune → *can cast back at you*; **soulless** cannot.

---

## 6. Aether as prima materia

Aether is the **philosopher's stone / prima materia** — the reconciliation of opposites (the *rebis*). It is the union of two aspects:

- **Light aspect (sol · projective)** — the bright poles: **Vita** (life), **Animus** (male soul), **Lumen** (light).
- **Dark aspect (luna · receptive)** — the dark poles: **Mors** (death), **Anima** (female soul), **Umbra** (dark).

The opposed primordial pairs: **Vita ⟷ Mors**, **Animus ⟷ Anima**, **Lumen ⟷ Umbra**. (Expandable — a fourth pair can be added.)

*Note: the male=light / female=dark split is the traditional alchemical Sol/Luna coding, not a value claim, and is invertible.*

---

## 7. Items & progression

Mythic items are a **parallel progression axis to knowledge**, letting the non-studious progress and letting masters skip grinding. Three roles:

- **Wards / passives** — e.g. a *fire cloak* removes the standing cost of recasting water/ice to survive heat.
- **Rune-mediums** — e.g. a *lamp* puts a Fire rune in your field where none flows; or (Free side) a *medium* that makes one specific spell reliable.
- **Keys** — open a specific gate. Most gates have another solution, so keys usually *ease* rather than hard-lock; a few are true walls.

- **The Primordial gate** — access to Primordial magic is *only* opened by acquiring a specific item or performing a deed. This is the one place items gate power that knowledge cannot reach.

**Free magic is item-intertwined.** Because Free casters lack fundamental understanding, they rely on mediums and foci far more than Charter mages — and those items are the likely source of a Free spell's reliability (the answer to the free-mage coherence problem: reliability comes from the *item*, not from comprehension).

**Free magic and free-magic items reveal their runes** — this is a core *learning* mechanic, not a side effect. Borrowing an effect you don't understand exposes its recipe, so items are a teaching tool that carries the player from *borrowing* effects to *composing* them.

**Acquisition is non-linear:** items sit at varied difficulty; some are main-quest (but skippable via sequence-breaking); some are *needed regardless* of route. Multiple solution paths per lock make sequence-breaking supported.

---

## 8. Casting stances — Charter vs Free

A **stance chosen per cast**, not two spell lists. Runes and knowledge are universal.

| | **Bound (Charter)** | **Unbound (Free)** |
|---|---|---|
| Access | open to anyone; the Charter rejects no one | open to anyone |
| Reliability | reliable, coherent | unreliable, variance-loaded |
| Power in a vacuum | tamer | higher magnitude (fireball → fire-vortex) |
| Direct clashes | **wins** (coherence beats magnitude) | loses; even with an edge it may sputter |
| Forbidden | can't touch souls; can't pull runes from the living or re-animate the dead | all available — the appeal |
| Cost | feeds the pantheon (light siphon) | feeds nothing; blasphemous |
| Ceiling | high — keeps climbing to primordial | low — self-imposed easy mode |

**Clash resolution:** no elemental edge → Charter wins by default. Charter *with* an edge → over-performs (water engulfs the fireball *and* counterattacks). Free *with* an edge → still unreliable. Charter **overpowers**, it does not dispel.

**Free-fill mechanic:** leave runes unspecified and let the blanks fill at random, overcharging the result. "How much you sculpt it" is a **risk dial** (specified : auto-filled ratio) trading control for power. Backfire has teeth: inverted targets, drawn divine attention, persistent **hubris/taint.** **Rule: free magic is never the required key to a lock — only the tempting shortcut.**

---

## 9. Cosmology (at the depth the game needs)

- **Primordial magic** is the god tier — it reconstructs runes, and therefore reality. Charter and Free are two **applications** of it, seen by nearly everyone as opposed.
- **Primordial is outside the Charter** — reaching for it *is*, by definition, free magic. Ascension = transgression.
- **The union is metaphorical:** order + freedom. Primordial needs both **reach** (Free) and **coherence** (Charter). The Free sorcerer must acquire discipline (a submission); the Charter mage must transgress (a fall). Both become partly what they were taught to despise; the player is rare in holding both.
- **The pantheon:** beings who use primordial magic and govern the realm. They authored the Charter and, *according to them*, the world. It empowers them (siphon) and **indirectly** prevents rivals by fencing mortals below the ceiling. Motive is **self-preservation, not malice**; a newly-ascended mortal is a rival *in potential*, never their equal (they hold millennia of power beyond mere access).
- **Enforcement is outsourced to belief.** No divine hunters. Charter mages, indoctrinated that Free magic is evil, persecute Free sorcerers *themselves*. The gods act on the world only through the Charter and the faith of believers — until the very top.
- **Deliberately mysterious:** whether the gods truly made the world; where they came from. *(Author's private north star, never canon: they created the world but came from elsewhere — not the same order of place. Felt, never confirmed.)*

---

## 10. Open threads

- [ ] **Full material tree** — every second/third-tier material and what it needs.
- [x] **Free-mage coherence** — reliability comes from **items/mediums** (section 7), not comprehension. *(Resolved.)*
- [ ] **Male/Female role** — projective/receptive utility axis, sacred generative pair, or both.
- [ ] **Death rune** — sanctioned uses (mercy, banishing undead) vs off-Charter? (Lean: policed by *primordial reach* / soul-involvement, not morality.)
- [ ] **Field economy** — how runes manifest and get consumed in the constant field; how scarce Aether is. (No sight tiers — the field is the same for everyone.)
- [ ] **Fourth primordial pair?** — whether to expand beyond Vita/Mors, Animus/Anima, Lumen/Umbra.
- [ ] **Path model** — hard class, taint accumulation, or fully fluid with consequences only.
- [ ] **"Reading" creatures** — is a creature's formula visible by default, or does interpreting it require learned knowledge?
- [ ] **Item catalogue** — concrete mythic items: wards, rune-mediums, keys, and the Primordial-gate item(s).
- [ ] **The learning UI** — how known vs unknown recipes are tracked/shown (a grimoire?), and how rune-reveals from free magic get recorded.
- [ ] **Magnum Opus color meta** — Nigredo → Albedo → Citrinitas → Rubedo as chapter/tier/world-tint.

---

*Provisional names (Spark, Lava, Vita, Mors, Animus, Anima, Lumen, Umbra) are placeholders pending ratification.*
