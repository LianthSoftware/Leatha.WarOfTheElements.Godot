using Godot;
using Leatha.WarOfTheElements.Common.Environment.Collisions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Leatha.WarOfTheElements.Godot.framework.Controls.Maps;
using Leatha.WarOfTheElements.Godot.framework.Extensions;
using FileAccess = Godot.FileAccess;

namespace Leatha.WarOfTheElements.Godot.Tools
{
	/// <summary>
	/// Editor script that exports collider archetypes + environment instances
	/// for the currently edited scene, purely based on colliders:
	/// any StaticBody3D that has a collider child is exported.
	/// </summary>
	[Tool]
	public partial class MapColliderExporter : EditorScript
	{
		private int _mapId = 1; // #TODO

		public override void _Run()
		{
			if (EditorInterface.Singleton.GetEditedSceneRoot() is not Node3D root)
			{
				GD.PrintErr("Exporter: No scene is opened.");
				return;
			}

			// Prefer scene file name (without extension) over root.Name
			// If the scene is unsaved or no path, fall back to root.Name
			var sceneFilePath = root.SceneFilePath;
			string sceneName;
			if (!string.IsNullOrEmpty(sceneFilePath))
			{
				sceneName = Path.GetFileNameWithoutExtension(sceneFilePath);
			}
			else
			{
				sceneName = root.Name;
			}

			var relativeDir = $"exports/maps/{sceneName}";  // relative to res://
			var exportDirResPath = $"res://{relativeDir}";

			// --- Create directory using instance DirAccess (NOT Absolute static) ---
			var resDir = DirAccess.Open("res://");
			if (resDir == null)
			{
				GD.PrintErr("MapColliderExporter: Failed to open res:// for writing.");
				return;
			}

			var mkErr = resDir.MakeDirRecursive(relativeDir);
			if (mkErr != Error.Ok && mkErr != Error.AlreadyInUse)
			{
				GD.PrintErr($"MapColliderExporter: MakeDirRecursive('{relativeDir}') failed: {mkErr}");
				return;
			}

			// 1) Collect all StaticBody3D instances that have a collider
			var usedShapeTypes = new HashSet<ColliderArchetypeType>();
			var instances = new List<EnvironmentInstanceObject>();
			CollectStaticBodiesWithColliders(root, instances, usedShapeTypes);
			var archetypes = BuildArchetypes(usedShapeTypes);

			// Serialize
			var jsonOptions = new JsonSerializerOptions
			{
				WriteIndented = true,
				IncludeFields = true,
				Converters =
				{
					new JsonStringEnumConverter()
				}
			};
			var envJson = JsonSerializer.Serialize(new EnvironmentExport { Instances = instances }, jsonOptions);
			var archJson = JsonSerializer.Serialize(new ColliderArchetypeExport { Archetypes = archetypes }, jsonOptions);

			// Save files
			var envPath = $"{exportDirResPath}/{sceneName}_environment.json";
			var archPath = $"{exportDirResPath}/{sceneName}_archetypes.json";

			using (var f = FileAccess.Open(envPath, FileAccess.ModeFlags.Write))
				f.StoreString(envJson);

			using (var f = FileAccess.Open(archPath, FileAccess.ModeFlags.Write))
				f.StoreString(archJson);

			GD.Print($"Exporter done:\n  {envPath}\n  {archPath}");
		}

		/// <summary>
		/// Recursively walks the scene tree, finds StaticBody3D nodes that have
		/// a CollisionShape3D child with a Shape3D, and exports them as environment instances.
		/// </summary>
		private void CollectStaticBodiesWithColliders(
			Node node,
			List<EnvironmentInstanceObject> instances,
			HashSet<ColliderArchetypeType> usedShapeTypes)
		{
			if (node is StaticBody3D body)
			{
				var collisionShape = FindChildOfType<CollisionShape3D>(body);
				if (collisionShape is { Shape: { } shape3D })
				{
					if (TryGetShapeInfo(shape3D, collisionShape, out var shapeType, out var colliderSize, out var convexHullPoints))
					{
						if (shapeType != ColliderArchetypeType.None && colliderSize != Vector3.Zero)
						{
							usedShapeTypes.Add(shapeType);

							var pos = body.GlobalPosition;
							var rotDeg = body.GlobalRotationDegrees;

							var archetypeName = GetArchetypeNameForShapeType(shapeType);

							var dto = new EnvironmentInstanceObject
							{
								ArchetypeName = archetypeName,
								Name = body.Name,
								MapId = 1, // #TODO
								Position = pos.FromGodotVector3(),
								RotationDegrees = rotDeg.FromGodotVector3(),
								ColliderSize = colliderSize.FromGodotVector3(),
								ShapeType = shapeType,
								ConvexHullPoints = convexHullPoints?.Select(i => i.FromGodotVector3()).ToList(),
								IsStatic = true
							};

							instances.Add(dto);
						}
					}
				}
			}

			foreach (var child in node.GetChildren())
			{
				CollectStaticBodiesWithColliders(child, instances, usedShapeTypes);
			}
		}

		/// <summary>
		/// Try to extract a useful collider size & type from a Shape3D.
		/// We keep it generic but simple: one size vector per shape.
		/// </summary>
		private static bool TryGetShapeInfo(
			Shape3D shape,
			Node3D shapeNode,
			out ColliderArchetypeType type,
			out Vector3 colliderSize,
			out List<Vector3> convexHullPoints)
		{
			type = ColliderArchetypeType.Box;
			colliderSize = Vector3.One;
			convexHullPoints = null;

			switch (shape)
			{
				case BoxShape3D box:
					{
						type = ColliderArchetypeType.Box;
						var size = box.Size;
						var scale = shapeNode.Scale;
						colliderSize = new Vector3(
							size.X * scale.X,
							size.Y * scale.Y,
							size.Z * scale.Z);
						return true;
					}

				case CapsuleShape3D capsule:
					{
						type = ColliderArchetypeType.Capsule;
						var scale = shapeNode.Scale;
						var radius = capsule.Radius * scale.X;
						var height = capsule.Height * scale.Y;
						colliderSize = new Vector3(radius, height, 0f); // (radius, height, 0)
						return true;
					}

				case CylinderShape3D cylinder:
					{
						type = ColliderArchetypeType.Cylinder;
						var scale = shapeNode.Scale;
						var radius = cylinder.Radius * scale.X;
						var height = cylinder.Height * scale.Y;
						colliderSize = new Vector3(radius, height, 0f); // (radius, height, 0)
						return true;
					}

				case SphereShape3D sphere:
					{
						type = ColliderArchetypeType.Box; // still approximated as box
						var scale = shapeNode.Scale;
						colliderSize = new Vector3(
							sphere.Radius * 2f * scale.X,
							sphere.Radius * 2f * scale.Y,
							sphere.Radius * 2f * scale.Z);
						return true;
					}

				case ConvexPolygonShape3D convex:
					{
						type = ColliderArchetypeType.ConvexHull;
						return TryComputeHull(shapeNode, convex.Points, out colliderSize, out convexHullPoints);
					}

				default:
					GD.PrintErr($"MapColliderExporter: Shape type '{shape.GetType().Name}' not yet supported.");
					return false;
			}
		}


		private static bool TryComputeHull(
			Node3D shapeNode,
			Vector3[]? points,
			out Vector3 colliderSize,
			out List<Vector3> convexHullPoints)
		{
			colliderSize = Vector3.Zero;
			convexHullPoints = null;

			if (points == null || points.Length == 0)
			{
				GD.PrintErr("MapColliderExporter: Convex hull shape has no points.");
				return false;
			}

			var scale = shapeNode.Scale;

			var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

			convexHullPoints = new List<Vector3>(points.Length);

			foreach (var p in points)
			{
				// apply node scale so the server gets the final hull
				var scaled = new Vector3(
					p.X * scale.X,
					p.Y * scale.Y,
					p.Z * scale.Z);

				// track AABB for convenience (ColliderSize)
				min.X = Math.Min(min.X, scaled.X);
				min.Y = Math.Min(min.Y, scaled.Y);
				min.Z = Math.Min(min.Z, scaled.Z);

				max.X = Math.Max(max.X, scaled.X);
				max.Y = Math.Max(max.Y, scaled.Y);
				max.Z = Math.Max(max.Z, scaled.Z);

				convexHullPoints.Add(scaled);
			}

			colliderSize = max - min;
			return true;
		}

		/// <summary>
		/// Computes AABB size from a set of local points, applying the node's scale.
		/// Used for Convex hull-style shapes (ConvexPolygon, SimpleConvex, etc.).
		/// </summary>
		private static bool TryComputeHullAabbSize(
			Vector3[]? points,
			Node3D shapeNode,
			out Vector3 colliderSize)
		{
			colliderSize = Vector3.Zero;

			if (points == null || points.Length == 0)
			{
				GD.PrintErr("MapColliderExporter: Convex hull shape has no points.");
				return false;
			}

			var scale = shapeNode.Scale;

			var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

			foreach (var p in points)
			{
				var scaled = new Vector3(
					p.X * scale.X,
					p.Y * scale.Y,
					p.Z * scale.Z);

				min.X = Math.Min(min.X, scaled.X);
				min.Y = Math.Min(min.Y, scaled.Y);
				min.Z = Math.Min(min.Z, scaled.Z);

				max.X = Math.Max(max.X, scaled.X);
				max.Y = Math.Max(max.Y, scaled.Y);
				max.Z = Math.Max(max.Z, scaled.Z);
			}

			colliderSize = max - min;
			return true;
		}

		private static string GetArchetypeNameForShapeType(ColliderArchetypeType type)
		{
			return type switch
			{
				ColliderArchetypeType.Box => "box_unit",
				ColliderArchetypeType.Capsule => "capsule_unit",
				ColliderArchetypeType.ConvexHull => "convexhull_unit",
				ColliderArchetypeType.Compound => "compound_unit",
				ColliderArchetypeType.Cylinder => "cylinder",
				_ => "unknown_unit"
			};
		}

		/// <summary>
		/// Build ColliderArchetype docs for all used shape types.
		/// Base size is (1,1,1) for all: instances carry the real size.
		/// </summary>
		private static List<ColliderArchetypeObject> BuildArchetypes(HashSet<ColliderArchetypeType> usedShapeTypes)
		{
			var result = new List<ColliderArchetypeObject>();

			foreach (var shapeType in usedShapeTypes)
			{
				var archetypeName = GetArchetypeNameForShapeType(shapeType);
				var displayName = shapeType switch
				{
					ColliderArchetypeType.Box => "Box Unit",
					ColliderArchetypeType.Capsule => "Capsule Unit",
					ColliderArchetypeType.ConvexHull => "Convex Hull Unit",
					ColliderArchetypeType.Compound => "Compound Unit",
					ColliderArchetypeType.Cylinder => "Cylinder",
					_ => "Unknown Unit"
				};

				result.Add(new ColliderArchetypeObject
				{
					ArchetypeName = archetypeName,
					Name = displayName,
					ShapeType = shapeType,
					Size = Vector3.One.FromGodotVector3(),
					IsStaticDefault = true
				});
			}

			return result;
		}

		/// <summary>
		/// Find first descendant of type T under given node (including grandchildren).
		/// </summary>
		private static T? FindChildOfType<T>(Node node) where T : class
		{
			foreach (var child in node.GetChildren())
			{
				if (child is T t)
					return t;

				var nested = FindChildOfType<T>(child);
				if (nested != null)
					return nested;
			}

			return null;
		}
	}

	/// <summary>
	/// Your server enum: Box, Capsule, ConvexHull, Compound.
	/// </summary>
	//public enum ColliderArchetypeType
	//{
	//	None = 0,
	//	Box = 1,
	//	Capsule = 2,
	//	ConvexHull = 3,
	//	Compound = 4,
	//	Cylinder = 5
	//}

	/// <summary>
	/// Mongo ColliderArchetype DTO (minus MongoEntity fields).
	/// </summary>
	//public sealed class ColliderArchetypeDto
	//{
	//	public string ArchetypeName { get; set; } = string.Empty;

	//	public string Name { get; set; } = string.Empty;

	//	public ColliderArchetypeType ShapeType { get; set; }

	//	/// <summary>
	//	/// Base size for this archetype.
	//	/// We use (1,1,1) and keep real dims per instance.
	//	/// </summary>
	//	public Vector3Dto Size { get; set; } = new();

	//	public bool IsStaticDefault { get; set; }
	//}

	/// <summary>
	/// Wrapper to export { "archetypes": [ ... ] }.
	/// </summary>
	//public sealed class ColliderArchetypeExportWrapper
	//{
	//	public List<ColliderArchetypeObject> Archetypes { get; set; } = new();
	//}

	/// <summary>
	/// One collider instance in the environment file.
	/// </summary>
	//public sealed class EnvironmentInstanceDto
	//{
	//	public string ArchetypeName { get; set; } = string.Empty;
	//	public string Name { get; set; } = string.Empty;

	//	public Vector3Dto Position { get; set; } = new();
	//	public Vector3Dto RotationDegrees { get; set; } = new();

	//	public Vector3Dto ColliderSize { get; set; } = new();

	//	public ColliderArchetypeType ShapeType { get; set; }

	//	public bool IsStatic { get; set; }

	//	/// <summary>
	//	/// Optional: local-space convex hull points (already scaled by the node scale).
	//	/// Used when ShapeType == ConvexHull.
	//	/// </summary>
	//	public List<Vector3Dto>? ConvexHullPoints { get; set; }
	//}


	/// <summary>
	/// Wrapper to export { "instances": [ ... ] }.
	/// </summary>
	//public sealed class EnvironmentExportWrapper
	//{
	//	public List<EnvironmentInstanceObject> Instances { get; set; } = new();
	//}

	/// <summary>
	/// Simple serializable Vector3 for JSON.
	/// </summary>
	//public sealed class Vector3Dto
	//{
	//	public float X { get; set; }
	//	public float Y { get; set; }
	//	public float Z { get; set; }

	//	public static Vector3Dto FromGodot(Vector3 v) => new Vector3Dto { X = v.X, Y = v.Y, Z = v.Z };
	//}
}
