#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace RuneMagic
{
    /// <summary>
    /// Paint looks first (any palette), then stamp Kind / Material / Cover /
    /// Blocks onto those cells without changing the sprite.
    /// </summary>
    public sealed class TilePropertyPaint : EditorWindow
    {
        const string Folder = "Assets/Tiles/Authored";

        static readonly Dictionary<string, WorldPaintTile> Cache = new();

        TileKind _kind = TileKind.Floor;
        MaterialId _material = MaterialId.Stone;
        TileCover _cover = TileCover.None;
        bool _blocks;
        bool _applyKind = true;
        bool _applyMaterial = true;
        bool _applyCover;
        bool _applyBlocks;
        bool _applyOpacity;
        float _opacity = 0.42f;
        bool _paint;
        bool _coverLayer;
        Tilemap _hoverMap;
        Vector3Int _hoverCell;
        bool _hasHover;

        [MenuItem("Window/Rune Magic/Tile Properties")]
        public static void Open()
        {
            GetWindow<TilePropertyPaint>("Tile Properties");
        }

        void OnEnable()
        {
            SceneView.duringSceneGui += OnScene;
            _ = RuneStampOverlay.Enabled;
            SceneView.RepaintAll();
        }

        void OnDisable()
        {
            SceneView.duringSceneGui -= OnScene;
            AssetDatabase.SaveAssets();
        }

        void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "Stamps show in the Scene view — not the Game tab. Nothing else to turn on. If you still see no colours: click the Scene tab, turn on Gizmos at the top-right of that view, and keep Play off. Window → Rune Magic → Show Stamps should be checked.",
                MessageType.Info);

            _paint = EditorGUILayout.Toggle("Paint in Scene view", _paint);
            _coverLayer = EditorGUILayout.Toggle("Write onto Cover layer", _coverLayer);
            var show = EditorGUILayout.Toggle("Show stamps in Scene view", RuneStampOverlay.Enabled);
            if (show != RuneStampOverlay.Enabled)
            {
                RuneStampOverlay.Enabled = show;
            }

            if (RuneStampOverlay.Enabled)
            {
                var lookOnly = EditorGUILayout.Toggle("Outline look-only cells", RuneStampOverlay.ShowLookOnly);
                if (lookOnly != RuneStampOverlay.ShowLookOnly)
                {
                    RuneStampOverlay.ShowLookOnly = lookOnly;
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Legend", EditorStyles.boldLabel);
                RuneStampOverlay.DrawLegendGui();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Stamp", EditorStyles.boldLabel);
            DrawField("Kind", ref _applyKind, () => _kind = (TileKind)EditorGUILayout.EnumPopup(_kind));
            DrawField("Material", ref _applyMaterial, () => _material = (MaterialId)EditorGUILayout.EnumPopup(_material));
            EditorGUILayout.BeginHorizontal();
            _applyCover = EditorGUILayout.Toggle(_applyCover, GUILayout.Width(18));
            EditorGUILayout.PrefixLabel("Cover");
            EditorGUILayout.LabelField(CoverLabel(_cover), EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
            using (new EditorGUI.DisabledScope(!_applyCover))
            {
                RunePicker.DrawCover(ref _cover);
            }

            DrawField("Blocks", ref _applyBlocks, () => _blocks = EditorGUILayout.Toggle(_blocks));
            DrawField("Opacity", ref _applyOpacity, () =>
            {
                _opacity = EditorGUILayout.Slider(_opacity, 0f, 1f);
            });

            EditorGUILayout.Space();
            if (_hasHover && _hoverMap != null)
            {
                var tile = _hoverMap.GetTile(_hoverCell);
                var paint = tile as WorldPaintTile;
                EditorGUILayout.LabelField("Under cursor", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Cell", _hoverCell.x + ", " + _hoverCell.y);
                EditorGUILayout.LabelField("Look", tile != null ? tile.name : "(empty)");
                if (paint != null)
                {
                    EditorGUILayout.LabelField("Kind", paint.kind.ToString());
                    EditorGUILayout.LabelField("Material", paint.material.ToString());
                    EditorGUILayout.LabelField("Cover", paint.ResolvedCover().ToString());
                    EditorGUILayout.LabelField("Blocks", paint.blocks ? "yes" : "no");
                    EditorGUILayout.LabelField("Opacity", Mathf.RoundToInt(paint.ResolvedOpacity() * 100f) + "%");
                }
                else if (tile != null)
                {
                    EditorGUILayout.LabelField("Properties", "not set — Play will guess from the name");
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Select the layer first. A cell is floor only if you stamp Kind = Floor or paint a Floor brush. Looks on any layer — including extra Floor / Tiles children — are not walkable until stamped. Environment Details is its own stamp — check only Blocks and drag across a cluster of tables or statues to give that group collision. Stamps sit over the tile you painted and do not start a reaction — only player and NPC spells do. Plant, timber, water, and fire stamps keep the tileset sprite; they do not swap in Floor-Plant / Floor-Timber / Floor-Fire pack art. Floor-Fire and Wall-Fire mark hunger on the walk or wall cell — they do not kindle a hall. A watered plant may spread onto one neighboring water floor or water covering. Hunger spent on timber or plant adds an ash covering; the walk tile stays. Cover is the overlay: look, work, and the same catalog mark as an inscription. Ice is Water · Earth. Fire cover only marks hunger — the weave speaks Fire, and you can click the mark. It does not kindle a hall. A later fireball, spreading burn, or oil a spell left will still find that cover. Ice cover melts when hunger crosses it. Oil or metal stamped on the Cover layer is fuel or a path for the spark — the stamp does not start the reaction. Vine is Plant · Mercury. Ash is Fire · Plant. Miasma is Cloud · Acid. Fog is Cloud. A Water material stamp keeps the tile you painted. Cover-Water may generate the Water mark. Write onto Cover layer (or select Cover) so a stamp does not change Kind. Play shows the generated mark; click it to draw that rune. Material = Miasma on Cover is the same as Cover = Miasma. Miasma and fog are see-through unless you stamp Opacity. Blank Tiles cells are pits at Play.",
                MessageType.None);
        }

        static string CoverLabel(TileCover cover)
        {
            if (cover == TileCover.None)
            {
                return "None";
            }

            var rune = CoverCatalog.RuneOf(cover);
            return rune != RuneId.None ? RuneCatalog.NameOf(rune) : cover.ToString();
        }

        static void DrawField(string label, ref bool apply, System.Action drawer)
        {
            EditorGUILayout.BeginHorizontal();
            apply = EditorGUILayout.Toggle(apply, GUILayout.Width(18));
            EditorGUILayout.PrefixLabel(label);
            using (new EditorGUI.DisabledScope(!apply))
            {
                drawer();
            }

            EditorGUILayout.EndHorizontal();
        }

        void OnScene(SceneView view)
        {
            if (Application.isPlaying)
            {
                return;
            }

            var map = ResolveMap();
            if (map == null)
            {
                if (_hasHover)
                {
                    _hasHover = false;
                    Repaint();
                }

                return;
            }

            var world = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;
            world.z = 0f;
            var cell = map.WorldToCell(world);
            if (!_hasHover || _hoverMap != map || _hoverCell != cell)
            {
                _hoverMap = map;
                _hoverCell = cell;
                _hasHover = true;
                Repaint();
            }

            var look = _coverLayer ? WalkMap() : map;
            var tile = look != null ? look.GetTile(cell) : map.GetTile(cell);
            if (tile != null)
            {
                var min = map.CellToWorld(cell);
                var max = map.CellToWorld(cell + Vector3Int.one);
                var fill = _paint ? new Color(0.72f, 0.55f, 1f, 0.22f) : new Color(1f, 1f, 1f, 0.06f);
                Handles.DrawSolidRectangleWithOutline(
                    new[]
                    {
                        new Vector3(min.x, min.y, 0f),
                        new Vector3(max.x, min.y, 0f),
                        new Vector3(max.x, max.y, 0f),
                        new Vector3(min.x, max.y, 0f)
                    },
                    fill,
                    new Color(0.72f, 0.55f, 1f, 0.9f));

                if (RuneStampOverlay.Enabled)
                {
                    Handles.Label(
                        new Vector3((min.x + max.x) * 0.5f, max.y + 0.08f, 0f),
                        RuneStampOverlay.Describe(tile));
                }
            }

            if (!_paint)
            {
                return;
            }

            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            var ev = Event.current;
            if (ev.button == 1 && (ev.type == EventType.MouseDown || ev.type == EventType.MouseDrag))
            {
                Sample(map, cell);
                ev.Use();
                return;
            }

            if (ev.button == 0 && (ev.type == EventType.MouseDown || ev.type == EventType.MouseDrag))
            {
                Stamp(map, cell);
                ev.Use();
            }

            if (ev.type == EventType.MouseUp && ev.button == 0)
            {
                AssetDatabase.SaveAssets();
            }
        }

        void Sample(Tilemap map, Vector3Int cell)
        {
            var paint = map.GetTile<WorldPaintTile>(cell);
            if (paint == null)
            {
                return;
            }

            _kind = paint.kind;
            _material = paint.material;
            _cover = paint.ResolvedCover();
            if (_cover == TileCover.None && (_coverLayer || map.gameObject.name.ToLowerInvariant().Contains("cover")))
            {
                _cover = WorldPaintTile.CoverFromMaterial(paint.material);
            }

            _blocks = paint.blocks;
            _opacity = paint.opacity > 0.001f ? paint.opacity : paint.ResolvedOpacity();
            Repaint();
        }

        void Stamp(Tilemap map, Vector3Int cell)
        {
            var raw = map.GetTile(cell);
            if (raw == null && _coverLayer)
            {
                var walk = WalkMap();
                raw = walk != null ? walk.GetTile(cell) : null;
            }

            if (raw == null)
            {
                return;
            }

            var current = raw as WorldPaintTile;
            var sprite = SpriteOf(map, cell, raw);
            if (sprite == null && current != null && !current.IsQualityStamp)
            {
                sprite = current.sprite;
            }

            var kind = _applyKind ? _kind : (current != null ? current.kind : GuessKind(raw));
            var material = _applyMaterial ? _material : (current != null ? current.material : GuessMaterial(raw));
            var cover = _applyCover ? _cover : (current != null ? current.ResolvedCover() : TileCover.None);
            var blocks = _applyBlocks ? _blocks : current != null && current.blocks;
            var opacity = _applyOpacity ? _opacity : (current != null ? current.opacity : 0f);
            if (_coverLayer)
            {
                kind = current != null ? current.kind : TileKind.None;
                if (!_applyMaterial)
                {
                    material = current != null ? current.material : MaterialId.Stone;
                }

                blocks = false;
                if (cover == TileCover.None)
                {
                    cover = WorldPaintTile.CoverFromMaterial(_applyMaterial ? _material : material);
                }

                if (sprite == null)
                {
                    var walk = WalkMap();
                    sprite = walk != null ? SpriteOf(walk, cell, walk.GetTile(cell)) : null;
                }
            }

            var authored = EnsureTile(sprite, kind, material, cover, blocks, opacity);
            if (authored == null || authored == raw)
            {
                return;
            }

            if (authored.sprite == null && raw is Tile previous && previous.sprite != null)
            {
                authored.sprite = previous.sprite;
                EditorUtility.SetDirty(authored);
            }

            if (authored.KeepsPaintedLook && authored.sprite == null && !_coverLayer)
            {
                return;
            }

            Undo.RecordObject(map, "Assign tile properties");
            map.SetTile(cell, authored);
            EditorUtility.SetDirty(map);
        }

        Tilemap ResolveMap()
        {
            var selected = Selection.activeGameObject != null
                ? Selection.activeGameObject.GetComponent<Tilemap>()
                : null;
            if (selected != null && !_coverLayer)
            {
                return selected;
            }

            var authoring = Object.FindFirstObjectByType<LevelAuthoring>();
            if (_coverLayer)
            {
                if (selected != null && selected.gameObject.name.ToLowerInvariant().Contains("cover"))
                {
                    return selected;
                }

                if (authoring != null && authoring.overlays != null)
                {
                    return authoring.overlays;
                }

                var tiles = authoring != null ? authoring.tilemap : TilemapLevel.FindPaintedMap();
                return NamedSibling(tiles, "cover") ?? WalkMap();
            }

            return WalkMap();
        }

        static Tilemap NamedSibling(Tilemap from, string name)
        {
            if (from == null || from.transform.parent == null)
            {
                return null;
            }

            var maps = from.transform.parent.GetComponentsInChildren<Tilemap>(true);
            for (var i = 0; i < maps.Length; i++)
            {
                if (maps[i].gameObject.name.ToLowerInvariant().Contains(name))
                {
                    return maps[i];
                }
            }

            return null;
        }

        static Tilemap WalkMap()
        {
            var selected = Selection.activeGameObject != null
                ? Selection.activeGameObject.GetComponent<Tilemap>()
                : null;
            if (selected != null)
            {
                return selected;
            }

            var authoring = Object.FindFirstObjectByType<LevelAuthoring>();
            if (authoring != null && authoring.tilemap != null)
            {
                return authoring.tilemap;
            }

            return TilemapLevel.FindPaintedMap();
        }

        public static WorldPaintTile KeepLook(
            Sprite sprite,
            WorldPaintTile stamp,
            TileCover? cover = null,
            TileKind? kind = null,
            MaterialId? material = null)
        {
            if (stamp == null)
            {
                return null;
            }

            return EnsureTile(
                sprite,
                kind ?? stamp.kind,
                material ?? stamp.material,
                cover ?? stamp.ResolvedCover(),
                stamp.blocks,
                stamp.opacity);
        }

        static Sprite SpriteOf(Tilemap map, Vector3Int cell, TileBase tile)
        {
            if (map != null)
            {
                var shown = map.GetSprite(cell);
                if (shown != null)
                {
                    return shown;
                }
            }

            if (tile is WorldPaintTile quality && quality.IsQualityStamp)
            {
                return null;
            }

            if (tile is Tile painted && painted.sprite != null)
            {
                return painted.sprite;
            }

            return null;
        }

        static TileKind GuessKind(TileBase tile)
        {
            return TilemapLevel.GuessKindForEditor(tile);
        }

        static MaterialId GuessMaterial(TileBase tile)
        {
            return TilemapLevel.GuessMaterialForEditor(tile);
        }

        static WorldPaintTile EnsureTile(
            Sprite sprite,
            TileKind kind,
            MaterialId material,
            TileCover cover,
            bool blocks,
            float opacity)
        {
            var aura = WorldPaintTile.AuraFromCover(cover);
            var key = Key(sprite, kind, material, cover, aura, blocks, opacity);
            if (Cache.TryGetValue(key, out var cached) && cached != null)
            {
                return cached;
            }

            if (!AssetDatabase.IsValidFolder("Assets/Tiles"))
            {
                AssetDatabase.CreateFolder("Assets", "Tiles");
            }

            if (!AssetDatabase.IsValidFolder(Folder))
            {
                AssetDatabase.CreateFolder("Assets/Tiles", "Authored");
            }

            var path = Folder + "/" + key + ".asset";
            var tile = AssetDatabase.LoadAssetAtPath<WorldPaintTile>(path);
            if (tile == null)
            {
                tile = CreateInstance<WorldPaintTile>();
                AssetDatabase.CreateAsset(tile, path);
            }

            tile.sprite = sprite;
            tile.kind = kind;
            tile.material = material;
            tile.cover = cover;
            tile.aura = aura;
            tile.blocks = blocks;
            tile.opacity = opacity;
            tile.color = new Color(1f, 1f, 1f, tile.ResolvedOpacity());
            EditorUtility.SetDirty(tile);
            Cache[key] = tile;
            return tile;
        }

        static string Key(
            Sprite sprite,
            TileKind kind,
            MaterialId material,
            TileCover cover,
            TileAura aura,
            bool blocks,
            float opacity)
        {
            var guid = "none";
            long local = 0;
            if (sprite != null)
            {
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sprite, out guid, out local);
            }

            var sign = local < 0 ? "n" : "p";
            var solid = blocks ? "block" : "open";
            var fade = Mathf.RoundToInt(Mathf.Clamp01(opacity) * 100f);
            return kind + "_" + material + "_" + cover + "_" + aura + "_" + solid + "_a" + fade + "_" + guid + "_" + sign + Mathf.Abs(local);
        }
    }
}
#endif
