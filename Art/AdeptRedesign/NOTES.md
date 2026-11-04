# Adept redesign: "Hollow Cloak" (inspired by Beksinski)
A shadowy, near-formless mage: a purple hooded cloak with a black void where the face should be and two faint bone/amber eye points. The tattered hem breaks into ragged strips and drips, and there are no legs. Walking is a sway/drift of the cloak: the hood leads and the hem lags. When casting, the cloak spreads, the eyes flare amber, and a rune spark grows into a ring beside the hand. Low detail, generated pixel by pixel in gen.py (re-run with `python3 gen.py`).

Palette (7): outline/void #12081A, dark violet #281438, purple #462460, light purple #684482, bone #A89876, rune amber #E8C86A, glow core #FFF0B0. Pixel art drawn as a 192x384 sheet.

Slice mapping (matches Hero_22.png.meta, each 16x16 slice at cell offset x+8, y+16 from the top-left):
- Slices 0-5 (row 0): Idle, 5fps, breathing bob plus one eye flicker
- Slices 6-11 (row 1): Walk, 8fps, 6-frame sway cycle
- Slices 30-35 (row 5): Cast, 4fps: spark, cross, rune ring, fade
- Slices 60-63 (row 10): Hop, 7fps: crouch, air (hem trailing), fall, land
- Other slices (rows 2-4, 6-9, 11): generic drift and spread poses; row 11 has the eyes closed
Pivot (0.5, 0.18) is unchanged. Drop-in replacement for Hero_22.png (keep the old .meta/GUID).
