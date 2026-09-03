using System.Collections.Generic;

namespace RuneMagic
{
    /// <summary>
    /// What the adept sees when they look. Short, and a hint at the
    /// rune — lore can grow later without changing the voice.
    /// </summary>
    public static class Sight
    {
        public static string YouSee(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                return "You see the room.";
            }

            description = description.Trim();
            if (description.StartsWith("You see ", System.StringComparison.OrdinalIgnoreCase))
            {
                return description;
            }

            return "You see " + LowerStart(description);
        }

        public static string OfItem(CatalogItem item)
        {
            if (item == null)
            {
                return "a thing without a name.";
            }

            if (!string.IsNullOrEmpty(item.look))
            {
                return item.look;
            }

            if (!string.IsNullOrEmpty(item.note))
            {
                return item.note;
            }

            var name = string.IsNullOrEmpty(item.name) ? item.id : item.name;
            return name + ".";
        }

        public static string OfLock(ISpellLock encounter)
        {
            if (encounter == null)
            {
                return "a lock.";
            }

            var look = OfFormula(encounter.FormulaId);
            if (!string.IsNullOrEmpty(look))
            {
                return look;
            }

            var name = encounter.DisplayName;
            return string.IsNullOrEmpty(name) ? "a lock." : name + ".";
        }

        public static string OfTile(WorldTile tile)
        {
            if (tile == null)
            {
                return "empty air.";
            }

            if (tile.Kind == TileKind.Pit || tile.Material == MaterialId.Void)
            {
                return GlyphView.Speak(
                    "a tear where rest should stand. Dark — withheld.",
                    "a tear where rest should stand. The veil is drawn.");
            }

            if (tile.Kindled && tile.Fire > 0.1f)
            {
                return GlyphView.Speak(
                    "hunger holding the walk. Flame ward is Fire · Salt · Sulphur. Yield thrown also puts it out.",
                    "hunger holding the walk. Hunger given a body, then the mind holds it on you. Yield thrown also forgets the flame.");
            }

            if (tile.HasMiasma)
            {
                return "foul breath hanging on this tile. A cloud that holds the step. Wind must take it.";
            }

            if (tile.HasVine)
            {
                return tile.IsBurning
                    ? "a climbing vine taking hunger. The wick is running."
                    : "a climbing vine. Hunger can run it as a wick.";
            }

            if (tile.IsBurning)
            {
                return "hunger standing on the floor.";
            }

            if (tile.HasFog)
            {
                return "a hanging veil on this tile.";
            }

            if (tile.IsPoisonWater)
            {
                return "poison water. Yield will wash it. Hunger lifts it as foul breath.";
            }

            var material = tile.Def.WorldMaterial;
            if (material != null && !string.IsNullOrEmpty(material.Note))
            {
                return material.Note;
            }

            return tile.Def.DisplayName + ".";
        }

        public static string OfRune(RuneId rune)
        {
            if (rune == RuneId.None)
            {
                return "a mark that will not speak.";
            }

            return RuneCatalog.TryGet(rune, out var def)
                ? def.Meaning
                : "a floating mark.";
        }

        public static string OfPlaque(string text)
        {
            return string.IsNullOrWhiteSpace(text) ? "carved words." : text.Trim();
        }

        public static string OfSpeech(string look, string title, string verb)
        {
            if (!string.IsNullOrWhiteSpace(look))
            {
                return look.Trim();
            }

            if (!string.IsNullOrWhiteSpace(title))
            {
                return title.Trim();
            }

            var use = string.IsNullOrWhiteSpace(verb) ? "read" : verb.Trim().ToLowerInvariant();
            return "words you may " + use + ".";
        }

        public static string OfBirth(IReadOnlyList<RuneId> sources, RuneId result)
        {
            if (result == RuneId.None)
            {
                return "a stone that has not yet learned a mark.";
            }

            if (sources == null || sources.Count == 0)
            {
                return OfRune(result);
            }

            return GlyphView.Speak(
                WorkingNames.RunePhrase(sources) + " become " + RuneCatalog.NameOf(result) + ".",
                "marks that join into another mark.");
        }

        public static string OfInteract(string look, string verb)
        {
            if (!string.IsNullOrWhiteSpace(look))
            {
                return look.Trim();
            }

            var use = string.IsNullOrWhiteSpace(verb) ? "Interact" : verb.Trim();
            return "a place you may " + use.ToLowerInvariant() + ".";
        }

        public static string InteractPrompt(string verb)
        {
            var use = string.IsNullOrWhiteSpace(verb) ? "interact" : verb.Trim();
            return "Press E to " + use.ToLowerInvariant();
        }

        public static string OfCrystal() =>
            "the first standing body. Death sends you back here.";

        public static string OfDoor(WorldDoor door)
        {
            if (door == null)
            {
                return "a door.";
            }

            var name = door.DisplayName;
            if (door.IsOpen)
            {
                return string.IsNullOrEmpty(name) ? "an open way." : "an open " + LowerStart(name) + ".";
            }

            return string.IsNullOrEmpty(name)
                ? "a shut door."
                : name + ", shut.";
        }

        public static string OfFormula(string formulaId)
        {
            switch ((formulaId ?? string.Empty).ToLowerInvariant())
            {
                case "ash-mite":
                    return "a small burning body, still hungry.";
                case "ice-thing":
                    return "cold given a walking form.";
                case "ice-cage":
                    return "yield asked to stand in the way.";
                case "flame-curtain":
                    return "hunger drawn as a veil.";
                case "fire-golem":
                    return "hunger stood up and given weight.";
                case "golem":
                case "stone-man":
                    return "rest given a body that walks.";
                case "warden":
                case "spirit-warden":
                    return "a watcher whose spirit is motion.";
                case "ember-adept":
                    return "a figure writing hunger.";
                case "bolt-adept":
                    return "a figure writing fire from the sky.";
                case "arrow-adept":
                    return "a figure loosing rest.";
                case "arrow-volley":
                    return "rest cut to fly.";
                case "poison-fog":
                case "miasma":
                    return "foul breath hanging still.";
                case "body-gap":
                case "chasm":
                    return "a tear where rest should stand.";
                case "cold-torch":
                    return "wood that remembers hunger.";
                case "storm-rod":
                case "spark-rod":
                    return "metal waiting for fire from the sky.";
                case "door-i":
                case "door-ii":
                case "door-iii":
                case "grove-door":
                case "cistern-door":
                case "spark-door":
                    return "a door that asks for seated stones.";
                default:
                    return string.Empty;
            }
        }

        static string LowerStart(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            if (text.Length == 1)
            {
                return text.ToLowerInvariant();
            }

            return char.ToLowerInvariant(text[0]) + text.Substring(1);
        }
    }
}
