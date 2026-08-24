#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Window &gt; Rune Magic &gt; Map Painter.
    /// Paints the same JSON the runtime loads from Resources/Maps.
    /// </summary>
    public sealed class MapPainterWindow : EditorWindow
    {
        MapFile _map;
        string _path;
        int _room;
        TileKind _kind = TileKind.Floor;
        MaterialId _material = MaterialId.Stone;
        Vector2 _scroll;
        string _propType = "plaque";
        string _propText = "Read the weave.";
        string _propRunes = "Fire, Salt";

        [MenuItem("Window/Rune Magic/Map Painter")]
        public static void Open()
        {
            GetWindow<MapPainterWindow>("Map Painter");
        }

        void OnEnable()
        {
            if (_map == null)
            {
                TryLoadDefault();
            }
        }

        void TryLoadDefault()
        {
            var fallback = Path.Combine(Application.dataPath, "Resources/Maps/sanctum.json");
            if (File.Exists(fallback))
            {
                LoadFrom(fallback);
            }
            else
            {
                _map = NewMap();
            }
        }

        static MapFile NewMap()
        {
            return new MapFile
            {
                id = "untitled",
                name = "Untitled",
                spawn = new MapCoord(2, 5),
                rooms = new[]
                {
                    new MapRoom
                    {
                        id = "room-1",
                        name = "First Room",
                        origin = new MapCoord(0, 0),
                        width = 13,
                        height = 11,
                        wall = "Stone",
                        floor = "Stone",
                        exit = "none",
                        stamps = System.Array.Empty<MapStamp>(),
                        props = System.Array.Empty<MapProp>()
                    }
                },
                halls = System.Array.Empty<MapHall>()
            };
        }

        void LoadFrom(string path)
        {
            _path = path;
            _map = MapFile.FromJson(File.ReadAllText(path)) ?? NewMap();
            _room = 0;
        }

        void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "Leftover JSON painter. Play loads the scene Tilemap, not these files. Use Window → 2D → Tile Palette.",
                MessageType.Warning);
            if (_map == null)
            {
                TryLoadDefault();
            }

            DrawToolbar();
            if (_map.rooms == null || _map.rooms.Length == 0)
            {
                EditorGUILayout.HelpBox("This map has no rooms.", MessageType.Info);
                return;
            }

            _room = Mathf.Clamp(_room, 0, _map.rooms.Length - 1);
            var room = _map.rooms[_room];
            DrawRoomInspector(room);
            DrawPaintTools();
            DrawGrid(room);
        }

        void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("New", GUILayout.Width(70)))
            {
                _map = NewMap();
                _path = null;
            }

            if (GUILayout.Button("Open…", GUILayout.Width(70)))
            {
                var pick = EditorUtility.OpenFilePanel("Open map", Application.dataPath + "/Resources/Maps", "json");
                if (!string.IsNullOrEmpty(pick))
                {
                    LoadFrom(pick);
                }
            }

            if (GUILayout.Button("Save", GUILayout.Width(70)))
            {
                Save(false);
            }

            if (GUILayout.Button("Save As…", GUILayout.Width(80)))
            {
                Save(true);
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(string.IsNullOrEmpty(_path) ? "(unsaved)" : _path.Replace(Application.dataPath, "Assets"), EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();

            _map.name = EditorGUILayout.TextField("Map name", _map.name);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Rooms", GUILayout.Width(48));
            for (var i = 0; i < _map.rooms.Length; i++)
            {
                if (GUILayout.Toggle(_room == i, _map.rooms[i].name, "Button"))
                {
                    _room = i;
                }
            }

            if (GUILayout.Button("+", GUILayout.Width(24)))
            {
                AddRoom();
            }

            EditorGUILayout.EndHorizontal();
        }

        void DrawRoomInspector(MapRoom room)
        {
            room.id = EditorGUILayout.TextField("Id", room.id);
            room.name = EditorGUILayout.TextField("Name", room.name);
            var origin = room.origin ?? new MapCoord();
            EditorGUILayout.BeginHorizontal();
            origin.x = EditorGUILayout.IntField("Origin X", origin.x);
            origin.y = EditorGUILayout.IntField("Y", origin.y);
            room.origin = origin;
            EditorGUILayout.EndHorizontal();
            room.width = Mathf.Max(3, EditorGUILayout.IntField("Width", room.width));
            room.height = Mathf.Max(3, EditorGUILayout.IntField("Height", room.height));
            room.wall = EditorGUILayout.EnumPopup("Wall", MapFile.ParseMaterial(room.wall)).ToString();
            room.floor = EditorGUILayout.EnumPopup("Floor", MapFile.ParseMaterial(room.floor)).ToString();
            room.exit = EditorGUILayout.TextField("Exit (east/west/north/south/none)", room.exit);
        }

        void DrawPaintTools()
        {
            EditorGUILayout.BeginHorizontal();
            _kind = (TileKind)EditorGUILayout.EnumPopup("Stamp", _kind);
            _material = (MaterialId)EditorGUILayout.EnumPopup(_material);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            _propType = EditorGUILayout.TextField("Prop", _propType);
            _propText = EditorGUILayout.TextField(_propText);
            _propRunes = EditorGUILayout.TextField(_propRunes);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox("Left click paints a tile. Shift-click places the prop (plaque, runes, mite, arrows, fog, crystal, gate…). Alt-click clears a stamp. The browser editor in Tools/map-editor.html is the fuller painter — attack, heading, and cast seconds live there.", MessageType.None);
        }

        void DrawGrid(MapRoom room)
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            var size = 18f;
            var rect = GUILayoutUtility.GetRect(room.width * size, room.height * size);
            for (var y = 0; y < room.height; y++)
            {
                for (var x = 0; x < room.width; x++)
                {
                    var cell = CellAt(room, x, y);
                    var drawY = room.height - 1 - y;
                    var tile = new Rect(rect.x + x * size, rect.y + drawY * size, size - 1, size - 1);
                    EditorGUI.DrawRect(tile, MaterialCatalog.Of(MapFile.ParseMaterial(cell.material)).FloorTone);
                    if (cell.kind == TileKind.Wall)
                    {
                        EditorGUI.DrawRect(tile, Color.Lerp(Color.black, MaterialCatalog.Of(MapFile.ParseMaterial(cell.material)).WallTone, 0.45f));
                    }

                    if (cell.kind == TileKind.Pit)
                    {
                        EditorGUI.DrawRect(tile, Color.black);
                    }

                    if (Event.current.type == EventType.MouseDown && tile.Contains(Event.current.mousePosition))
                    {
                        if (Event.current.shift)
                        {
                            PlaceProp(room, x, y);
                        }
                        else if (Event.current.alt)
                        {
                            ClearCell(room, x, y);
                        }
                        else
                        {
                            Stamp(room, x, y, _kind, _material);
                        }

                        Event.current.Use();
                        Repaint();
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }

        static Cell CellAt(MapRoom room, int x, int y)
        {
            var border = x == 0 || y == 0 || x == room.width - 1 || y == room.height - 1;
            var cell = new Cell
            {
                kind = border ? TileKind.Wall : TileKind.Floor,
                material = border ? room.wall : room.floor
            };
            if (room.stamps == null)
            {
                return cell;
            }

            for (var s = 0; s < room.stamps.Length; s++)
            {
                var stamp = room.stamps[s];
                if (stamp?.cells == null)
                {
                    continue;
                }

                for (var i = 0; i + 1 < stamp.cells.Length; i += 2)
                {
                    if (stamp.cells[i] == x && stamp.cells[i + 1] == y)
                    {
                        cell.kind = MapFile.ParseKind(stamp.kind);
                        cell.material = stamp.material;
                    }
                }
            }

            return cell;
        }

        void Stamp(MapRoom room, int x, int y, TileKind kind, MaterialId material)
        {
            ClearCell(room, x, y);
            var border = x == 0 || y == 0 || x == room.width - 1 || y == room.height - 1;
            var defKind = border ? TileKind.Wall : TileKind.Floor;
            var defMat = MapFile.ParseMaterial(border ? room.wall : room.floor);
            if (kind == defKind && material == defMat)
            {
                return;
            }

            var list = new System.Collections.Generic.List<MapStamp>(room.stamps ?? System.Array.Empty<MapStamp>());
            MapStamp found = null;
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i].kind == kind.ToString() && list[i].material == material.ToString())
                {
                    found = list[i];
                    break;
                }
            }

            if (found == null)
            {
                found = new MapStamp { kind = kind.ToString(), material = material.ToString(), cells = System.Array.Empty<int>() };
                list.Add(found);
            }

            var cells = new System.Collections.Generic.List<int>(found.cells ?? System.Array.Empty<int>()) { x, y };
            found.cells = cells.ToArray();
            room.stamps = list.ToArray();
        }

        static void ClearCell(MapRoom room, int x, int y)
        {
            if (room.stamps == null)
            {
                return;
            }

            var kept = new System.Collections.Generic.List<MapStamp>();
            for (var s = 0; s < room.stamps.Length; s++)
            {
                var stamp = room.stamps[s];
                if (stamp?.cells == null)
                {
                    continue;
                }

                var cells = new System.Collections.Generic.List<int>();
                for (var i = 0; i + 1 < stamp.cells.Length; i += 2)
                {
                    if (stamp.cells[i] == x && stamp.cells[i + 1] == y)
                    {
                        continue;
                    }

                    cells.Add(stamp.cells[i]);
                    cells.Add(stamp.cells[i + 1]);
                }

                if (cells.Count > 0)
                {
                    stamp.cells = cells.ToArray();
                    kept.Add(stamp);
                }
            }

            room.stamps = kept.ToArray();
        }

        void PlaceProp(MapRoom room, int x, int y)
        {
            var list = new System.Collections.Generic.List<MapProp>(room.props ?? System.Array.Empty<MapProp>());
            list.RemoveAll(prop => prop != null && prop.x == x && prop.y == y);
            var prop = new MapProp { type = _propType, x = x, y = y, text = _propText };
            if (_propType == "runes")
            {
                prop.runes = _propRunes.Split(',');
                for (var i = 0; i < prop.runes.Length; i++)
                {
                    prop.runes[i] = prop.runes[i].Trim();
                }
            }

            list.Add(prop);
            room.props = list.ToArray();
        }

        void AddRoom()
        {
            var last = _map.rooms[_map.rooms.Length - 1];
            var next = new MapRoom
            {
                id = "room-" + (_map.rooms.Length + 1),
                name = "New Room",
                origin = new MapCoord(last.origin.x + last.width + 4, last.origin.y),
                width = 13,
                height = 11,
                wall = "Stone",
                floor = "Stone",
                exit = "none",
                stamps = System.Array.Empty<MapStamp>(),
                props = System.Array.Empty<MapProp>()
            };
            var rooms = new MapRoom[_map.rooms.Length + 1];
            System.Array.Copy(_map.rooms, rooms, _map.rooms.Length);
            rooms[rooms.Length - 1] = next;
            _map.rooms = rooms;
            _room = rooms.Length - 1;
        }

        void Save(bool forcePanel)
        {
            var path = _path;
            if (forcePanel || string.IsNullOrEmpty(path))
            {
                path = EditorUtility.SaveFilePanel("Save map", Application.dataPath + "/Resources/Maps",
                    (_map.id ?? "map") + ".json", "json");
            }

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            _map.id = Path.GetFileNameWithoutExtension(path);
            File.WriteAllText(path, _map.ToJson());
            _path = path;
            AssetDatabase.Refresh();
        }

        struct Cell
        {
            public TileKind kind;
            public string material;
        }
    }
}
#endif
