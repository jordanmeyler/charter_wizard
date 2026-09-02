using UnityEngine;

namespace RuneMagic
{
    public enum ElementFamily
    {
        Aether,
        Fire,
        Flame,
        Water,
        Ice,
        Earth,
        Air,
        Lightning,
        Spark,
        Fog,
        Poison,
        Plant,
        Dark,
        Light,
        Steam,
        Lava
    }

    public enum VeilKind
    {
        None,
        Fog,
        Poison,
        Darkness
    }

    /// <summary>
    /// How a work looks as particles and light. Families follow the
    /// element, not the formation — fire is embers whether it flies or stands.
    /// </summary>
    public readonly struct ElementLook
    {
        public ElementLook(
            ElementFamily family,
            Color core,
            Color glow,
            Color mist,
            bool additive,
            bool lit,
            float lightStrength)
        {
            Family = family;
            Core = core;
            Glow = glow;
            Mist = mist;
            Additive = additive;
            Lit = lit;
            LightStrength = lightStrength;
        }

        public ElementFamily Family { get; }
        public Color Core { get; }
        public Color Glow { get; }
        public Color Mist { get; }
        public bool Additive { get; }
        public bool Lit { get; }
        public float LightStrength { get; }

        public static ElementLook For(RuneId material, SpellId spell = SpellId.None)
        {
            if (WorldWork.IsPoisonVeil(spell))
            {
                return Of(ElementFamily.Poison);
            }

            if (spell == SpellId.Darkness)
            {
                return Of(ElementFamily.Dark);
            }

            if (WorldWork.IsSightVeil(spell) || spell == SpellId.Fog)
            {
                return Of(ElementFamily.Fog);
            }

            if (spell == SpellId.LightningBolt || spell == SpellId.LightningStrike
                || spell == SpellId.BrilliantArc || spell == SpellId.ChainLightning)
            {
                return Of(ElementFamily.Lightning);
            }

            return Of(FamilyOf(material, spell));
        }

        public static ElementFamily FamilyOf(RuneId material, SpellId spell = SpellId.None)
        {
            if (WorldWork.IsPoisonVeil(spell))
            {
                return ElementFamily.Poison;
            }

            if (spell == SpellId.Darkness)
            {
                return ElementFamily.Dark;
            }

            if (WorldWork.IsSightVeil(spell))
            {
                return ElementFamily.Fog;
            }

            if (spell == SpellId.Witchfire)
            {
                return ElementFamily.Flame;
            }

            if (WorldWork.IsOilWork(spell))
            {
                return ElementFamily.Lava;
            }

            if (spell == SpellId.FirePillar || spell == SpellId.FlamePillar)
            {
                return ElementFamily.Fire;
            }

            if (spell == SpellId.LavaPillar)
            {
                return ElementFamily.Lava;
            }

            if (spell == SpellId.Grove
                || spell == SpellId.Tree || spell == SpellId.WoodWall || spell == SpellId.Grow
                || spell == SpellId.Forest || spell == SpellId.Wither
                || spell == SpellId.Plantward || spell == SpellId.GroveForm || spell == SpellId.TaintedTree)
            {
                return ElementFamily.Plant;
            }

            if (spell == SpellId.CloudForm)
            {
                return ElementFamily.Fog;
            }

            switch (material)
            {
                case RuneId.Flame:
                    return ElementFamily.Flame;
                case RuneId.Fire:
                case RuneId.Ember:
                case RuneId.Inferno:
                    return ElementFamily.Fire;
                case RuneId.Water:
                case RuneId.Current:
                    return ElementFamily.Water;
                case RuneId.Ice:
                case RuneId.Glacier:
                    return ElementFamily.Ice;
                case RuneId.Earth:
                case RuneId.Stone:
                case RuneId.Dust:
                case RuneId.Mud:
                case RuneId.Sand:
                    return ElementFamily.Earth;
                case RuneId.Air:
                case RuneId.Wind:
                    return ElementFamily.Air;
                case RuneId.Lightning:
                case RuneId.Plasma:
                    return ElementFamily.Lightning;
                case RuneId.Spark:
                    return ElementFamily.Spark;
                case RuneId.Cloud:
                    return ElementFamily.Fog;
                case RuneId.Poison:
                case RuneId.Acid:
                case RuneId.Miasma:
                    return ElementFamily.Poison;
                case RuneId.Plant:
                case RuneId.Vine:
                case RuneId.Oil:
                case RuneId.Vita:
                    return ElementFamily.Plant;
                case RuneId.Umbra:
                case RuneId.Mors:
                case RuneId.Shade:
                    return ElementFamily.Dark;
                case RuneId.Lumen:
                    return ElementFamily.Light;
                case RuneId.Steam:
                    return ElementFamily.Steam;
                case RuneId.Lava:
                    return ElementFamily.Lava;
                default:
                    return ElementFamily.Aether;
            }
        }

        public static ElementLook Of(ElementFamily family)
        {
            switch (family)
            {
                case ElementFamily.Fire:
                    return new ElementLook(family,
                        new Color(1f, 0.55f, 0.18f),
                        new Color(1f, 0.42f, 0.08f, 0.85f),
                        new Color(0.28f, 0.12f, 0.08f, 0.45f),
                        true, true, 1.35f);
                case ElementFamily.Flame:
                    return new ElementLook(family,
                        new Color(0.78f, 0.42f, 1f),
                        new Color(0.55f, 0.22f, 0.95f, 0.88f),
                        new Color(0.28f, 0.1f, 0.42f, 0.5f),
                        true, true, 1.25f);
                case ElementFamily.Water:
                    return new ElementLook(family,
                        new Color(0.35f, 0.7f, 1f),
                        new Color(0.18f, 0.48f, 0.95f, 0.7f),
                        new Color(0.2f, 0.4f, 0.7f, 0.35f),
                        false, true, 0.7f);
                case ElementFamily.Ice:
                    return new ElementLook(family,
                        new Color(0.82f, 0.94f, 1f),
                        new Color(0.55f, 0.82f, 1f, 0.75f),
                        new Color(0.7f, 0.85f, 0.95f, 0.4f),
                        true, true, 0.85f);
                case ElementFamily.Earth:
                    return new ElementLook(family,
                        new Color(0.62f, 0.44f, 0.26f),
                        new Color(0.42f, 0.3f, 0.18f, 0.35f),
                        new Color(0.45f, 0.36f, 0.24f, 0.5f),
                        false, false, 0.15f);
                case ElementFamily.Air:
                    return new ElementLook(family,
                        new Color(0.86f, 0.94f, 1f),
                        new Color(0.75f, 0.88f, 1f, 0.4f),
                        new Color(0.8f, 0.88f, 0.95f, 0.28f),
                        false, false, 0.2f);
                case ElementFamily.Lightning:
                    return new ElementLook(family,
                        new Color(0.92f, 0.96f, 1f),
                        new Color(0.7f, 0.85f, 1f, 0.95f),
                        new Color(0.45f, 0.55f, 0.85f, 0.3f),
                        true, true, 1.8f);
                case ElementFamily.Spark:
                    return new ElementLook(family,
                        new Color(1f, 0.9f, 0.35f),
                        new Color(1f, 0.82f, 0.2f, 0.9f),
                        new Color(0.55f, 0.4f, 0.1f, 0.25f),
                        true, true, 1.2f);
                case ElementFamily.Fog:
                    return new ElementLook(family,
                        new Color(0.78f, 0.82f, 0.88f),
                        new Color(0.55f, 0.58f, 0.65f, 0.2f),
                        new Color(0.7f, 0.74f, 0.8f, 0.55f),
                        false, false, 0.05f);
                case ElementFamily.Poison:
                    return new ElementLook(family,
                        new Color(0.48f, 0.78f, 0.18f),
                        new Color(0.32f, 0.55f, 0.1f, 0.45f),
                        new Color(0.28f, 0.42f, 0.08f, 0.55f),
                        false, true, 0.35f);
                case ElementFamily.Plant:
                    return new ElementLook(family,
                        new Color(0.42f, 0.78f, 0.28f),
                        new Color(0.22f, 0.5f, 0.16f, 0.4f),
                        new Color(0.18f, 0.36f, 0.12f, 0.3f),
                        false, false, 0.25f);
                case ElementFamily.Dark:
                    return new ElementLook(family,
                        new Color(0.28f, 0.16f, 0.42f),
                        new Color(0.12f, 0.08f, 0.22f, 0.55f),
                        new Color(0.08f, 0.06f, 0.14f, 0.5f),
                        false, false, 0.1f);
                case ElementFamily.Light:
                    return new ElementLook(family,
                        new Color(1f, 0.96f, 0.72f),
                        new Color(1f, 0.92f, 0.55f, 0.85f),
                        new Color(0.95f, 0.9f, 0.7f, 0.3f),
                        true, true, 1.6f);
                case ElementFamily.Steam:
                    return new ElementLook(family,
                        new Color(0.92f, 0.94f, 0.96f),
                        new Color(0.85f, 0.82f, 0.78f, 0.35f),
                        new Color(0.8f, 0.8f, 0.82f, 0.5f),
                        false, false, 0.15f);
                case ElementFamily.Lava:
                    return new ElementLook(family,
                        new Color(1f, 0.32f, 0.08f),
                        new Color(0.95f, 0.22f, 0.04f, 0.8f),
                        new Color(0.2f, 0.08f, 0.06f, 0.4f),
                        true, true, 1.25f);
                default:
                    return new ElementLook(family,
                        new Color(0.82f, 0.7f, 1f),
                        new Color(0.7f, 0.5f, 0.95f, 0.6f),
                        new Color(0.45f, 0.35f, 0.6f, 0.3f),
                        true, true, 0.7f);
            }
        }
    }

    /// <summary>
    /// Runtime particles and a soft light. Built in code so a cast never
    /// depends on a prefab. Failures stay visual — the room still resolves.
    /// </summary>
    public static class ElementFx
    {
        static Material _alpha;
        static Material _additive;
        static Texture2D _soft;
        static Texture2D _shard;
        static bool _ready;

        public static GameObject Burst(
            Vector3 position,
            ElementLook look,
            SpellShape shape,
            float potency,
            Transform follow = null)
        {
            try
            {
                var authored = SpawnAuthored("Burst", look, position, follow);
                if (authored != null)
                {
                    Object.Destroy(authored, 1.4f);
                    return authored;
                }

                Ensure();
                var host = new GameObject("ElementBurst");
                host.transform.position = position;
                if (follow != null)
                {
                    host.transform.SetParent(follow, true);
                }

                AttachParticles(host, look, shape, potency, loop: false);
                if (look.Lit)
                {
                    AttachLight(host, look, potency, 0.55f);
                }

                Object.Destroy(host, 1.4f);
                return host;
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning("Element burst failed: " + exception.Message);
                return null;
            }
        }

        public static GameObject Stream(Transform follow, ElementLook look, SpellShape shape, float potency)
        {
            try
            {
                var authored = SpawnAuthored("Stream", look, follow != null ? follow.position : Vector3.zero, follow);
                if (authored != null)
                {
                    return authored;
                }

                Ensure();
                var host = new GameObject("ElementStream");
                host.transform.SetParent(follow, false);
                host.transform.localPosition = Vector3.zero;
                AttachParticles(host, look, shape, potency, loop: true);
                if (look.Lit)
                {
                    AttachLight(host, look, potency, 0f);
                }

                return host;
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning("Element stream failed: " + exception.Message);
                return null;
            }
        }

        public static GameObject Linger(Transform parent, ElementLook look, float potency, Vector3 localOffset)
        {
            try
            {
                var authored = SpawnAuthored("Linger", look, parent != null ? parent.TransformPoint(localOffset) : localOffset, parent);
                if (authored != null)
                {
                    authored.transform.localPosition = localOffset;
                    return authored;
                }

                Ensure();
                var host = new GameObject("ElementLinger");
                host.transform.SetParent(parent, false);
                host.transform.localPosition = localOffset;
                AttachParticles(host, look, SpellShape.Pillar, potency, loop: true);
                if (look.Lit)
                {
                    AttachLight(host, look, potency * 0.65f, 0f);
                }

                return host;
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning("Element linger failed: " + exception.Message);
                return null;
            }
        }

        static GameObject SpawnAuthored(string kind, ElementLook look, Vector3 position, Transform parent)
        {
            var family = look.Family.ToString();
            var prefab = Resources.Load<GameObject>("Fx/" + family + kind)
                ?? Resources.Load<GameObject>("Fx/" + family);
            if (prefab == null)
            {
                return null;
            }

            var spawned = Object.Instantiate(prefab, position, Quaternion.identity);
            spawned.name = prefab.name;
            if (parent != null)
            {
                spawned.transform.SetParent(parent, true);
            }

            return spawned;
        }

        public static GameObject VeilCloud(Transform parent, ElementLook look, float radius)
        {
            try
            {
                Ensure();
                var host = new GameObject("VeilCloud");
                host.transform.SetParent(parent, false);
                host.transform.localPosition = new Vector3(0f, 0.2f, 0f);
                var system = CreateStoppedSystem(host);
                TuneVeil(system, look, radius);
                StyleRenderer(host, look, 14);
                system.Play(true);
                return host;
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning("Veil cloud failed: " + exception.Message);
                return null;
            }
        }

        public static void AttachLight(GameObject host, ElementLook look, float potency, float lifetime)
        {
            var glow = new GameObject("Light");
            glow.transform.SetParent(host.transform, false);
            glow.transform.localPosition = new Vector3(0f, 0.1f, 0f);
            var scale = Mathf.Lerp(1.6f, 3.2f, look.LightStrength * 0.35f) * Mathf.Max(0.4f, potency);
            glow.transform.localScale = Vector3.one * scale;
            var renderer = glow.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.Glow(look.Glow);
            renderer.sortingOrder = 17;
            renderer.color = look.Glow;
            var pulse = glow.AddComponent<SpellLight>();
            pulse.Bind(look.Glow, look.LightStrength, lifetime);

            try
            {
                var lamp = glow.AddComponent<Light>();
                lamp.type = LightType.Point;
                lamp.color = look.Core;
                lamp.intensity = look.LightStrength * 1.1f * Mathf.Max(0.4f, potency);
                lamp.range = 2.4f + look.LightStrength;
                lamp.shadows = LightShadows.None;
            }
            catch (System.Exception)
            {
            }
        }

        static void AttachParticles(GameObject host, ElementLook look, SpellShape shape, float potency, bool loop)
        {
            var system = CreateStoppedSystem(host);
            TuneMain(system, look, shape, potency, loop);
            TuneEmission(system, look, shape, potency, loop);
            TuneShape(system, look, shape);
            TuneMotion(system, look, shape);
            TuneLifetime(system, look);
            StyleRenderer(host, look, loop ? 8 : 22);
            if (!loop)
            {
                system.Emit(BurstCount(look, shape, potency));
            }

            system.Play(true);
        }

        static ParticleSystem CreateStoppedSystem(GameObject host)
        {
            var system = host.AddComponent<ParticleSystem>();
            // Duration cannot be set while Unity is already playing the new system.
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            return system;
        }

        static void TuneMain(ParticleSystem system, ElementLook look, SpellShape shape, float potency, bool loop)
        {
            if (system.isPlaying || system.particleCount > 0)
            {
                system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            var main = system.main;
            main.loop = loop;
            main.playOnAwake = true;
            main.duration = loop ? 1f : 0.55f;
            main.startLifetime = Lifetime(look, shape, loop);
            main.startSpeed = Speed(look, shape);
            var scale = Mathf.Lerp(0.85f, 1.25f, Mathf.Clamp01(potency - 0.2f));
            var size = Size(look, shape);
            main.startSize = new ParticleSystem.MinMaxCurve(size.constantMin * scale, size.constantMax * scale);
            main.startColor = look.Core;
            main.gravityModifier = Gravity(look);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = loop ? 80 : 140;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 6.28f);
        }

        static void TuneEmission(ParticleSystem system, ElementLook look, SpellShape shape, float potency, bool loop)
        {
            var emission = system.emission;
            emission.enabled = true;
            var rate = Rate(look, shape) * Mathf.Max(0.6f, potency);
            emission.rateOverTime = loop ? rate : rate * 0.35f;
            if (!loop)
            {
                emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)BurstCount(look, shape, potency)) });
            }
        }

        static void TuneShape(ParticleSystem system, ElementLook look, SpellShape shape)
        {
            var module = system.shape;
            module.enabled = true;
            module.rotation = new Vector3(-90f, 0f, 0f);
            switch (look.Family)
            {
                case ElementFamily.Fire:
                case ElementFamily.Lava:
                    module.shapeType = ParticleSystemShapeType.Cone;
                    module.angle = shape == SpellShape.Shot ? 8f : 18f;
                    module.radius = shape == SpellShape.Spread ? 0.55f : 0.12f;
                    break;
                case ElementFamily.Lightning:
                    module.shapeType = ParticleSystemShapeType.Box;
                    module.scale = new Vector3(0.08f, 0.08f, shape == SpellShape.Shot ? 0.7f : 0.25f);
                    break;
                case ElementFamily.Fog:
                case ElementFamily.Poison:
                case ElementFamily.Steam:
                    module.shapeType = ParticleSystemShapeType.Circle;
                    module.radius = shape == SpellShape.Spread ? 1.1f : 0.45f;
                    break;
                case ElementFamily.Earth:
                    module.shapeType = ParticleSystemShapeType.Hemisphere;
                    module.radius = 0.22f;
                    break;
                default:
                    module.shapeType = ParticleSystemShapeType.Circle;
                    module.radius = shape == SpellShape.Spread ? 0.7f : 0.16f;
                    break;
            }
        }

        static void TuneMotion(ParticleSystem system, ElementLook look, SpellShape shape)
        {
            var velocity = system.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            switch (look.Family)
            {
                case ElementFamily.Fire:
                case ElementFamily.Lava:
                    SetVelocity(velocity, 0f, 0f, 0.6f, 1.6f);
                    break;
                case ElementFamily.Water:
                    SetVelocity(velocity, -0.25f, 0.25f, -1.4f, -0.2f);
                    break;
                case ElementFamily.Ice:
                    SetVelocity(velocity, 0f, 0f, -0.15f, 0.35f);
                    break;
                case ElementFamily.Earth:
                    SetVelocity(velocity, 0f, 0f, 0.4f, 1.1f);
                    break;
                case ElementFamily.Air:
                    SetVelocity(velocity, -1.6f, 1.6f, -0.2f, 0.4f);
                    break;
                case ElementFamily.Fog:
                case ElementFamily.Poison:
                case ElementFamily.Steam:
                    SetVelocity(velocity, -0.25f, 0.25f, 0.05f, 0.28f);
                    break;
                case ElementFamily.Plant:
                    SetVelocity(velocity, 0f, 0f, 0.2f, 0.7f);
                    break;
                default:
                    SetVelocity(velocity, 0f, 0f, -0.1f, 0.4f);
                    break;
            }

            var noise = system.noise;
            noise.enabled = look.Family != ElementFamily.Lightning;
            noise.strength = look.Family == ElementFamily.Fog || look.Family == ElementFamily.Poison ? 0.55f : 0.28f;
            noise.frequency = 0.45f;
            noise.scrollSpeed = 0.2f;
            noise.damping = true;

            if (look.Family == ElementFamily.Lightning || look.Family == ElementFamily.Spark)
            {
                var trails = system.trails;
                trails.enabled = true;
                trails.lifetime = 0.12f;
                trails.ratio = 0.7f;
                trails.dieWithParticles = true;
                trails.widthOverTrail = 0.12f;
            }
        }

        static void TuneLifetime(ParticleSystem system, ElementLook look)
        {
            var color = system.colorOverLifetime;
            color.enabled = true;
            var gradient = new Gradient();
            var mid = Color.Lerp(look.Core, Color.white, look.Family == ElementFamily.Lightning ? 0.55f : 0.15f);
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(look.Core, 0f),
                    new GradientColorKey(mid, 0.4f),
                    new GradientColorKey(look.Mist, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(look.Mist.a > 0.4f ? 0.9f : 0.75f, 0.18f),
                    new GradientAlphaKey(0f, 1f)
                });
            color.color = gradient;

            var size = system.sizeOverLifetime;
            size.enabled = true;
            var grow = look.Family == ElementFamily.Fog || look.Family == ElementFamily.Poison || look.Family == ElementFamily.Steam;
            size.size = new ParticleSystem.MinMaxCurve(1f, grow
                ? AnimationCurve.EaseInOut(0f, 0.55f, 1f, 1.6f)
                : AnimationCurve.EaseInOut(0f, 1f, 1f, 0.15f));
        }

        static void TuneVeil(ParticleSystem system, ElementLook look, float radius)
        {
            var main = system.main;
            main.loop = true;
            main.playOnAwake = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(2.2f, 4.2f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.02f, 0.12f);
            main.startSize = new ParticleSystem.MinMaxCurve(1.1f, 2.1f);
            main.startColor = look.Mist;
            main.gravityModifier = 0f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 48;
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 6.28f);

            var emission = system.emission;
            emission.rateOverTime = 8f;

            var shape = system.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = Mathf.Max(0.8f, radius);
            shape.rotation = new Vector3(-90f, 0f, 0f);

            var velocity = system.velocityOverLifetime;
            velocity.enabled = true;
            SetVelocity(velocity, -0.18f, 0.18f, 0.02f, 0.16f);

            var noise = system.noise;
            noise.enabled = true;
            noise.strength = 0.4f;
            noise.frequency = 0.22f;

            var color = system.colorOverLifetime;
            color.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(look.Mist, 0f),
                    new GradientColorKey(look.Core, 0.5f),
                    new GradientColorKey(look.Mist, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.55f, 0.3f),
                    new GradientAlphaKey(0f, 1f)
                });
            color.color = gradient;

            var size = system.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 0.7f, 1f, 1.5f));
        }

        static void StyleRenderer(GameObject host, ElementLook look, int order)
        {
            var renderer = host.GetComponent<ParticleSystemRenderer>();
            if (renderer == null)
            {
                return;
            }

            renderer.material = look.Additive ? AdditiveMaterial() : AlphaMaterial();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.alignment = ParticleSystemRenderSpace.View;
            renderer.sortingOrder = order;
            renderer.trailMaterial = look.Additive ? AdditiveMaterial() : AlphaMaterial();
            var texture = look.Family == ElementFamily.Ice || look.Family == ElementFamily.Plant
                ? ShardTexture()
                : SoftTexture();
            if (renderer.material != null && texture != null)
            {
                renderer.material.mainTexture = texture;
            }
        }

        static void SetVelocity(
            ParticleSystem.VelocityOverLifetimeModule velocity,
            float xMin,
            float xMax,
            float yMin,
            float yMax)
        {
            // Unity requires every velocity axis to use the same MinMaxCurve mode.
            velocity.x = new ParticleSystem.MinMaxCurve(xMin, xMax);
            velocity.y = new ParticleSystem.MinMaxCurve(yMin, yMax);
            velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);
        }

        static ParticleSystem.MinMaxCurve Lifetime(ElementLook look, SpellShape shape, bool loop)
        {
            switch (look.Family)
            {
                case ElementFamily.Fog:
                case ElementFamily.Poison:
                case ElementFamily.Steam:
                    return new ParticleSystem.MinMaxCurve(loop ? 2.4f : 1.1f, loop ? 3.8f : 1.8f);
                case ElementFamily.Lightning:
                    return new ParticleSystem.MinMaxCurve(0.08f, 0.22f);
                case ElementFamily.Fire:
                    return new ParticleSystem.MinMaxCurve(0.28f, 0.7f);
                case ElementFamily.Earth:
                    return new ParticleSystem.MinMaxCurve(0.35f, 0.75f);
                default:
                    return new ParticleSystem.MinMaxCurve(0.3f, 0.85f);
            }
        }

        static ParticleSystem.MinMaxCurve Speed(ElementLook look, SpellShape shape)
        {
            switch (look.Family)
            {
                case ElementFamily.Lightning:
                    return new ParticleSystem.MinMaxCurve(4f, 8f);
                case ElementFamily.Fire:
                case ElementFamily.Spark:
                    return new ParticleSystem.MinMaxCurve(0.6f, 2.2f);
                case ElementFamily.Water:
                    return new ParticleSystem.MinMaxCurve(1.2f, 2.8f);
                case ElementFamily.Earth:
                    return new ParticleSystem.MinMaxCurve(0.8f, 2.4f);
                case ElementFamily.Air:
                    return new ParticleSystem.MinMaxCurve(1.6f, 3.4f);
                case ElementFamily.Fog:
                case ElementFamily.Poison:
                    return new ParticleSystem.MinMaxCurve(0.05f, 0.22f);
                default:
                    return new ParticleSystem.MinMaxCurve(0.4f, 1.6f);
            }
        }

        static ParticleSystem.MinMaxCurve Size(ElementLook look, SpellShape shape)
        {
            switch (look.Family)
            {
                case ElementFamily.Fog:
                case ElementFamily.Poison:
                case ElementFamily.Steam:
                    return new ParticleSystem.MinMaxCurve(0.45f, 1.1f);
                case ElementFamily.Lightning:
                    return new ParticleSystem.MinMaxCurve(0.06f, 0.16f);
                case ElementFamily.Earth:
                    return new ParticleSystem.MinMaxCurve(0.1f, 0.22f);
                case ElementFamily.Ice:
                    return new ParticleSystem.MinMaxCurve(0.08f, 0.18f);
                default:
                    return new ParticleSystem.MinMaxCurve(0.08f, 0.2f);
            }
        }

        static float Gravity(ElementLook look)
        {
            switch (look.Family)
            {
                case ElementFamily.Earth:
                    return 1.1f;
                case ElementFamily.Water:
                    return 0.45f;
                case ElementFamily.Ice:
                    return 0.2f;
                case ElementFamily.Fire:
                case ElementFamily.Lava:
                    return -0.25f;
                default:
                    return 0f;
            }
        }

        static float Rate(ElementLook look, SpellShape shape)
        {
            var spread = shape == SpellShape.Spread ? 1.6f : 1f;
            switch (look.Family)
            {
                case ElementFamily.Fog:
                case ElementFamily.Poison:
                    return 14f * spread;
                case ElementFamily.Fire:
                case ElementFamily.Spark:
                    return 28f * spread;
                case ElementFamily.Lightning:
                    return 40f;
                case ElementFamily.Water:
                    return 32f * spread;
                case ElementFamily.Earth:
                    return 18f;
                default:
                    return 22f * spread;
            }
        }

        static int BurstCount(ElementLook look, SpellShape shape, float potency)
        {
            var n = look.Family == ElementFamily.Lightning ? 28
                : look.Family == ElementFamily.Earth ? 16
                : look.Family == ElementFamily.Fog || look.Family == ElementFamily.Poison ? 10
                : 18;
            if (shape == SpellShape.Spread)
            {
                n += 8;
            }

            return Mathf.RoundToInt(n * Mathf.Max(0.7f, potency));
        }

        static void Ensure()
        {
            if (_ready && _soft != null)
            {
                return;
            }

            _soft = StampSoft();
            _shard = StampShard();
            _ready = true;
        }

        static Material AlphaMaterial()
        {
            if (_alpha != null)
            {
                return _alpha;
            }

            _alpha = MakeMaterial(additive: false);
            return _alpha;
        }

        static Material AdditiveMaterial()
        {
            if (_additive != null)
            {
                return _additive;
            }

            _additive = MakeMaterial(additive: true);
            return _additive;
        }

        static Material MakeMaterial(bool additive)
        {
            var shader = Shader.Find("Particles/Standard Unlit")
                ?? Shader.Find(additive ? "Legacy Shaders/Particles/Additive" : "Legacy Shaders/Particles/Alpha Blended")
                ?? Shader.Find("Sprites/Default")
                ?? Shader.Find("Unlit/Transparent");
            var material = new Material(shader != null ? shader : Shader.Find("Sprites/Default"));
            if (material.HasProperty("_Mode"))
            {
                material.SetFloat("_Mode", additive ? 4f : 2f);
            }

            if (material.HasProperty("_SrcBlend"))
            {
                material.SetInt("_SrcBlend", additive
                    ? (int)UnityEngine.Rendering.BlendMode.SrcAlpha
                    : (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", additive
                    ? (int)UnityEngine.Rendering.BlendMode.One
                    : (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            }

            if (_soft != null)
            {
                material.mainTexture = _soft;
            }

            return material;
        }

        static Texture2D SoftTexture()
        {
            Ensure();
            return _soft;
        }

        static Texture2D ShardTexture()
        {
            Ensure();
            return _shard;
        }

        static Texture2D StampSoft()
        {
            const int size = 32;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            var mid = (size - 1) * 0.5f;
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var dx = (x - mid) / mid;
                    var dy = (y - mid) / mid;
                    var d = Mathf.Sqrt(dx * dx + dy * dy);
                    var alpha = Mathf.Clamp01(1.05f - d);
                    alpha *= alpha;
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply();
            return texture;
        }

        static Texture2D StampShard()
        {
            const int size = 24;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            var clear = new Color(1f, 1f, 1f, 0f);
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    texture.SetPixel(x, y, clear);
                }
            }

            for (var i = 0; i < size; i++)
            {
                var t = i / (float)(size - 1);
                var half = Mathf.Lerp(1f, 7f, 1f - Mathf.Abs(t * 2f - 1f));
                for (var x = 0; x < size; x++)
                {
                    var dx = Mathf.Abs(x - (size - 1) * 0.5f);
                    if (dx <= half)
                    {
                        var a = Mathf.Clamp01(1f - dx / Mathf.Max(1f, half));
                        texture.SetPixel(x, i, new Color(1f, 1f, 1f, a));
                    }
                }
            }

            texture.Apply();
            return texture;
        }
    }

    /// <summary>
    /// Soft pulse on a glow sprite so a cast reads as light, not a flat disc.
    /// </summary>
    public sealed class SpellLight : MonoBehaviour
    {
        Color _color;
        float _strength;
        float _lifetime;
        float _age;
        SpriteRenderer _renderer;
        Light _lamp;

        public void Bind(Color color, float strength, float lifetime)
        {
            _color = color;
            _strength = strength;
            _lifetime = lifetime;
            _renderer = GetComponent<SpriteRenderer>();
            _lamp = GetComponent<Light>();
        }

        void Update()
        {
            _age += Time.unscaledDeltaTime;
            var pulse = 0.72f + Mathf.Sin((_age + _strength) * (7f + _strength * 4f)) * 0.22f;
            var fade = 1f;
            if (_lifetime > 0f)
            {
                fade = Mathf.Clamp01(1f - _age / _lifetime);
                fade *= fade;
            }

            if (_renderer != null)
            {
                var color = _color;
                color.a = _color.a * pulse * fade;
                _renderer.color = color;
            }

            if (_lamp != null)
            {
                _lamp.intensity = _strength * pulse * fade * 1.2f;
            }

            if (_lifetime > 0f && _age >= _lifetime && transform.parent == null)
            {
                Destroy(gameObject);
            }
        }
    }
}
