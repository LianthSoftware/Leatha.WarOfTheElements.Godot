#if TOOLS

using Godot;
using System;
using System.Text.Json;

namespace Leatha.WarOfTheElements.Godot.addons.Terrain3DExporter
{
    [Tool]
    public sealed partial class Terrain3DExporterPlugin : EditorPlugin
    {
        private const string MenuItemName = "Export Terrain3D Heightmap";

        public override void _EnterTree()
        {
            AddToolMenuItem(MenuItemName, new Callable(this, nameof(OnExportTerrain3DHeightmap)));
        }

        public override void _ExitTree()
        {
            RemoveToolMenuItem(MenuItemName);
        }

        private void OnExportTerrain3DHeightmap()
        {
            //var editorInterface = GetEditorInterface();
            var sceneRoot = EditorInterface.Singleton.GetEditedSceneRoot();

            if (sceneRoot == null)
            {
                GD.PrintErr("No scene loaded.");
                return;
            }

            var terrainNode = FindFirstTerrain3D(sceneRoot);
            if (terrainNode == null)
            {
                GD.PrintErr("No Terrain3D node found in the current scene.");
                return;
            }

            try
            {
                ExportTerrainData(terrainNode);
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Terrain export failed: {ex}");
            }
        }

        private Node FindFirstTerrain3D(Node root)
        {
            // Adjust GetClass() value according to your addon (inspect in Godot)
            if (root.GetClass() == "Terrain3D")
                return root;

            foreach (Node child in root.GetChildren())
            {
                var found = FindFirstTerrain3D(child);
                if (found != null)
                    return found;
            }

            return null;
        }

        private void ExportTerrainData(Node terrainNode)
        {
            GD.Print($"Exporting Terrain3D data from node: {terrainNode.Name}");

            Variant terrainDataVar = terrainNode.Get("data");

            // Case 1: property doesn't exist or is null
            if (terrainDataVar.VariantType == Variant.Type.Nil)
            {
                GD.PrintErr("Terrain3D node has no 'data' property or it's null.");
                return;
            }

            // Case 2: it's something, but not an object
            if (terrainDataVar.VariantType != Variant.Type.Object)
            {
                GD.PrintErr("'data' is not an object Variant.");
                return;
            }

            // Now this is guaranteed non-null for Variant.Type.Object
            var terrainData = terrainDataVar.AsGodotObject();

            var exportDir = "res://exports/terrain";
            EnsureDir(exportDir);

            var heightPngPath = $"{exportDir}/terrain_height.png";
            var metaJsonPath = $"{exportDir}/terrain_meta.json";

            // --- EXPORT HEIGHT MAP IMAGE ---
            // You must adjust method name & enum according to Terrain3D's API.
            // Typical pattern: terrainData.Call("export_image", path, map_type_enum);
            // Suppose height map type enum = 0 (example).
            terrainData.Call("export_image", heightPngPath, 0);

            // --- GET METADATA ---
            // cell size / scale (adjust property name):
            var scaleObj = terrainNode.Get("map_scale");

            //if (scaleObj.AsSingle())
            var cellSize = scaleObj.AsSingle();

            // Terrain origin from node’s global position:
            var node3D = terrainNode as Node3D;
            var origin = node3D?.GlobalPosition ?? Vector3.Zero;

            // Get heightmap width/height using the same API you used to export,
            // or via e.g. terrainData.Call("get_map_resolution") etc.
            // For simplicity, we read the exported image back:
            var img = Image.LoadFromFile(heightPngPath);
            var width = img.GetWidth();
            var height = img.GetHeight();

            var meta = new TerrainMeta
            {
                OriginX = origin.X,
                OriginY = origin.Y,
                OriginZ = origin.Z,
                CellSize = cellSize,
                Width = width,
                Height = height
            };

            var json = JsonSerializer.Serialize(meta, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            using var file = FileAccess.Open(metaJsonPath, FileAccess.ModeFlags.Write);
            file.StoreString(json);

            GD.Print($"Terrain heightmap exported to: {heightPngPath}");
            GD.Print($"Terrain metadata exported to: {metaJsonPath}");
        }

        private void EnsureDir(string path)
        {
            var dir = DirAccess.Open(path);
            if (dir == null)
            {
                var baseDir = DirAccess.Open("res://");
                baseDir.MakeDirRecursive(path.Substring("res://".Length));
            }
        }

        private sealed class TerrainMeta
        {
            public float OriginX { get; set; }

            public float OriginY { get; set; }

            public float OriginZ { get; set; }

            public float CellSize { get; set; }

            public int Width { get; set; }

            public int Height { get; set; }
        }
    }
}

#endif