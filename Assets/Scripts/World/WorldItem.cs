using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Something a charmed body can pick up and bring to the adept.
    /// </summary>
    public interface ICarryable
    {
        bool Collected { get; }
        bool CanFetch { get; }
        Vector3 WorldPosition { get; }
        string CarryName { get; }
        bool TryCarry(Transform carrier);
        bool DeliverTo(AdeptAvatar adept);
        void Drop();
    }

    /// <summary>
    /// A pickup from the item catalog. Walking into it teaches a recipe.
    /// Charmed bodies can carry a nearby prize to the adept.
    /// Fragile props do not pick up — they yield to opposed work.
    /// Drop a prefab (Fire Stone, Water Stone, …) from any folder under
    /// Assets/Prefabs rather than a blank Item and a typed id.
    /// Description is the pack / You see text. Pickup line is the take log.
    /// </summary>
    [ExecuteAlways]
    [SelectionBase]
    public sealed class WorldItem : MonoBehaviour, ILookable, IWorldMatter, ICarryable
    {
        public bool Collected { get; private set; }
        public bool Available => !Collected && _carrier == null;
        public bool CanFetch => Available && !Fragile && AdeptPack.CanCarry(_item);
        public string CarryName => _item != null && !string.IsNullOrEmpty(_item.name) ? _item.name : "the prize";
        public CatalogItem Item => _item;
        public Vector3 WorldPosition => transform.position;
        public float LookRadius => 0.55f;
        public bool CanLook => !Collected && _item != null;
        public string LookText => Sight.OfItem(_item);
        public Essence Matter => WorldMatter.Parse(AuthoredMatter());
        public MaterialId BoundMaterial =>
            MatterLaw.TryParse(AuthoredMatter(), out var id) ? id : MaterialId.None;
        public bool Fragile =>
            fragile
            || (_item != null && (_item.fragile || string.Equals(_item.kind, "prop", System.StringComparison.OrdinalIgnoreCase)));

        [Header("Words")]
        [SerializeField] string displayName;
        [Tooltip("Pack inspect and You see. Leave empty to use the catalog row for this Catalog Id.")]
        [TextArea(3, 8)]
        [SerializeField] string look;
        [Tooltip("Spoken when it goes into the pack. Leave empty to use the catalog note.")]
        [TextArea(2, 5)]
        [SerializeField] string note;

        [Header("Authoring")]
        [SerializeField] string catalogId;
        [SerializeField] string spriteId;
        [SerializeField] Sprite portrait;
        [SerializeField] Sprite[] idleFrames;
        [SerializeField] Sprite[] changeFrames;
        [SerializeField] string changeClip;
        [SerializeField] float changeFps = 10f;
        [Tooltip("What this object is made of. Used when the catalog row has no matter.")]
        [SerializeField] MaterialId material;
        [SerializeField] string matter;
        [SerializeField] bool fragile;
        [SerializeField] string[] keys;
        [SerializeField] string teachesSpell;

        CatalogItem _item;
        Grimoire _grimoire;
        AdeptPack _pack;
        System.Action<string> _log;
        Transform _carrier;
        CircleCollider2D _hit;
        SpriteRenderer _renderer;
        bool _wired;

        public static GameObject Spawn(Vector3 position, CatalogItem item)
        {
            var host = new GameObject(item != null ? item.name : "Item");
            host.transform.position = position;
            var view = host.AddComponent<WorldItem>();
            view._item = item;
            return host;
        }

        public void Bind(Grimoire grimoire, System.Action<string> log, AdeptPack pack = null)
        {
            _grimoire = grimoire;
            _log = log;
            _pack = pack;
            if (_wired)
            {
                return;
            }

            _wired = true;
            if (_item == null)
            {
                _item = AuthoringUtil.ResolveItem(catalogId, displayName, spriteId, AuthoredMatter(), fragile, keys, teachesSpell, note, look);
            }

            var art = !string.IsNullOrEmpty(spriteId)
                ? spriteId
                : _item != null && !string.IsNullOrEmpty(_item.sprite) ? _item.sprite : "charm";
            _renderer = AuthoringUtil.ApplyLook(gameObject, 5, art, portrait, idleFrames, 4f);
            if (GetComponent<PropBob>() == null)
            {
                PropBob.Attach(transform, 0.08f, 1.4f);
            }

            if (GetComponentInChildren<FixtureGlow>() == null)
            {
                FixtureGlow.Attach(transform, new Color(1f, 0.5f, 0.12f, 0.65f), 1.5f, 0.14f);
            }

            _hit = AuthoringUtil.GetOrAdd<CircleCollider2D>(gameObject);
            _hit.isTrigger = true;
            _hit.radius = 0.4f;
            WorldLabel.Attach(transform, _item != null ? _item.name : "Item", new Vector3(0f, 0.7f, 0f),
                new Color(1f, 0.72f, 0.3f));
            Lookables.Register(this);
            WorldMatter.Register(this);
            BindBurnable();
        }

        void BindBurnable()
        {
            if (!MatterLaw.TryParse(AuthoredMatter(), out var material) || !VitalLaw.CanBurn(material))
            {
                return;
            }

            var burnable = AuthoringUtil.GetOrAdd<Burnable>(gameObject);
            burnable.Bind(VitalLaw.ItemBurnSeconds(material), note =>
            {
                if (Collected)
                {
                    return;
                }

                Collected = true;
                _carrier = null;
                CoverCatalog.AshAt(transform.position);
                _log?.Invoke(string.IsNullOrEmpty(note)
                    ? "Hunger finishes the timber. Ash is what remains."
                    : note);
                Destroy(gameObject);
            });
        }

        void OnDisable()
        {
            Lookables.Unregister(this);
            WorldMatter.Unregister(this);
        }

        public bool YieldsTo(SpellId spell)
        {
            if (!Fragile || Collected)
            {
                return false;
            }

            var keyList = keys != null && keys.Length > 0 ? keys : _item != null ? _item.keys : null;
            if (keyList != null && keyList.Length > 0)
            {
                for (var i = 0; i < keyList.Length; i++)
                {
                    if (SpellRegistry.Parse(keyList[i]) == spell)
                    {
                        return true;
                    }
                }

                return false;
            }

            if (MatterLaw.TryParse(AuthoredMatter(), out var authoredMaterial))
            {
                if (MatterLaw.ResistsMagic(authoredMaterial))
                {
                    return false;
                }

                if (MatterLaw.Melts(spell, authoredMaterial))
                {
                    return true;
                }
            }

            var essence = Matter != Essence.None ? Matter : Essence.Physical;
            return WorldPhysics.UnmakesMatter(spell, essence);
        }

        public string Unmake(SpellId spell)
        {
            if (Collected || !Fragile)
            {
                return string.Empty;
            }

            Collected = true;
            _carrier = null;
            var name = _item != null && !string.IsNullOrEmpty(_item.name) ? _item.name : "The stood thing";
            var hasChange = (changeFrames != null && changeFrames.Length > 0) || !string.IsNullOrEmpty(changeClip);
            if (hasChange)
            {
                AuthoringUtil.PlayChange(gameObject, _renderer, changeFrames, changeClip, changeFps, () => Destroy(gameObject));
            }
            else
            {
                Destroy(gameObject);
            }

            if (MatterLaw.TryParse(AuthoredMatter(), out var authoredMaterial))
            {
                return MatterLaw.MeltNote(authoredMaterial);
            }

            return Matter == Essence.Water
                ? $"{name} remembers yield. Hunger takes it."
                : Matter == Essence.Air || Matter == Essence.Poison
                    ? $"{name} is breath. The wind unmakes it."
                    : $"{name} comes apart.";
        }

        public bool TryCarry(Transform carrier)
        {
            if (!CanFetch || carrier == null)
            {
                return false;
            }

            _carrier = carrier;
            if (_hit != null)
            {
                _hit.enabled = false;
            }

            return true;
        }

        public bool DeliverTo(AdeptAvatar adept)
        {
            if (Collected || adept == null)
            {
                return false;
            }

            CollectInto();
            return true;
        }

        public void Drop()
        {
            if (Collected)
            {
                return;
            }

            if (_carrier != null)
            {
                transform.position = _carrier.position;
            }

            _carrier = null;
            if (_hit != null)
            {
                _hit.enabled = true;
            }
        }

        void Update()
        {
            if (_carrier == null || Collected)
            {
                return;
            }

            transform.position = _carrier.position + Vector3.up * 0.55f;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!Available || Fragile || !AdeptAvatar.IsAdept(other) || _item == null)
            {
                return;
            }

            CollectInto();
        }

        string AuthoredMatter()
        {
            if (!string.IsNullOrEmpty(matter))
            {
                return matter;
            }

            if (material != MaterialId.None)
            {
                return material.ToString();
            }

            return _item != null ? _item.matter : null;
        }

        void CollectInto()
        {
            if (Collected || _item == null)
            {
                return;
            }

            Collected = true;
            _carrier = null;
            var carried = AdeptPack.CanCarry(_item) && _pack != null && _pack.Take(_item);

            if (!string.IsNullOrEmpty(_item.teachesSpell) && SpellCodex.TryGet(SpellRegistry.Parse(_item.teachesSpell), out var entry))
            {
                _grimoire?.LearnRecipe(entry.RecipeRunes.Count > 0 ? entry.RecipeRunes[0] : RuneId.Fire, RuneId.Mercury, entry.Shape);
            }

            if (!string.IsNullOrEmpty(_item.teachesFormula))
            {
                _grimoire?.LearnInterpretation(_item.teachesFormula);
            }

            if (!string.IsNullOrEmpty(_item.note))
            {
                _log?.Invoke(_item.note);
            }
            else if (!string.IsNullOrEmpty(_item.look))
            {
                _log?.Invoke(_item.look);
            }
            else if (carried)
            {
                _log?.Invoke($"{_item.name} goes into the pack. I to look.");
            }
            else
            {
                _log?.Invoke($"{_item.name} unspools. A recipe is borrowed.");
            }

            Destroy(gameObject);
        }

        void OnEnable()
        {
            if (!Application.isPlaying)
            {
                PreviewLook();
            }
        }

        void OnValidate()
        {
            if (Application.isPlaying)
            {
                return;
            }

#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += EditorRefresh;
#endif
        }

#if UNITY_EDITOR
        void EditorRefresh()
        {
            if (this == null || Application.isPlaying)
            {
                return;
            }

            PreviewLook();
        }
#endif

        void PreviewLook()
        {
            var renderer = AuthoringUtil.GetOrAdd<SpriteRenderer>(gameObject);
            renderer.sortingOrder = 5;
            if (portrait != null)
            {
                renderer.sprite = portrait;
                return;
            }

            var art = spriteId;
            if (string.IsNullOrEmpty(art) && CatalogBook.TryItem(catalogId, out var item) && item != null)
            {
                art = item.sprite;
            }

            renderer.sprite = SpriteFactory.Named(string.IsNullOrEmpty(art) ? "charm" : art);
        }
    }
}
