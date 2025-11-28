using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leatha.WarOfTheElements.Godot.framework.Controls;
using SystemVector3 = System.Numerics.Vector3;
using GodotVector3 = Godot.Vector3;

namespace Leatha.WarOfTheElements.Godot.framework.Extensions
{
    public static class ControlExtensions
    {
        private static readonly RandomNumberGenerator RandomNumberGenerator = new RandomNumberGenerator();

        public static GameControl GetGameControl(this Node node)
        {
            return node.GetTree().CurrentScene as GameControl;
        }

        public static async Task RunOnMainThreadAsync(this Node node, Func<Task> action)
        {
            await node.WaitFrameAsync();

            await action();
        }

        public static async Task WaitFrameAsync(this Node node)
        {
            await node.ToSignal(node.GetTree(), "process_frame");
        }

        public static async Task WaitForSeconds(this Node node, float seconds)
        {
            await node.ToSignal(node.GetTree().CreateTimer(seconds), "timeout");
        }

        public static void SetGauntletCursor()
        {
            var cursor = GD.Load<Texture2D>("res://resources/textures/Pointer_gauntlet_on_32x32.png");
            Input.SetCustomMouseCursor(cursor);
        }

        public static void SetGlowingGauntletCursor()
        {
            var cursor = GD.Load<Texture2D>("res://resources/textures/Pointer_gauntlet_cast_on_32x32.png");
            Input.SetCustomMouseCursor(cursor);
        }

        public static int Random(int min, int max)
        {
            return RandomNumberGenerator.RandiRange(min, max);
        }



        public static void ClearChildren(this Node node, bool removeImmediately = false)
        {
            var children = node.GetChildren();
            foreach (var child in children)
            {
                node.RemoveChild(child);

                if (removeImmediately)
                    child.Free();
                else
                    child.QueueFree();
            }
        }

        public static void ClearChildren(this Node node, Type type, bool removeImmediately = false)
        {
            var children = node.GetChildren().Where(i => i.GetType() == type);
            foreach (var child in children)
            {
                node.RemoveChild(child);

                if (removeImmediately)
                    child.Free();
                else
                    child.QueueFree();
            }
        }

        public static List<TChildren> GetChildren<TChildren>(this Node node) where TChildren : Node
        {
            var children = node.GetChildren().OfType<TChildren>();
            return children.ToList();
        }




        public static SystemVector3 FromGodotVector3(this GodotVector3 vector)
        {
            return new SystemVector3(vector.X, vector.Y, vector.Z);
        }

        public static GodotVector3 ToGodotVector3(this SystemVector3 vector)
        {
            return new GodotVector3(vector.X, vector.Y, vector.Z);
        }
    }
}
