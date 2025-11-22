using System;
using Godot;
using Leatha.WarOfTheElements.Common.Communication.Transfer.Enums;

namespace Leatha.WarOfTheElements.Godot.framework.Objects
{
    public static class GameConstants
    {
        public static readonly Color FireElementColor = Color.FromHtml("#ff4f32");
        public static readonly Color WaterElementColor = Color.FromHtml("#4ac4ff");
        public static readonly Color WindElementColor = Color.FromHtml("#c5c5c5");
        public static readonly Color EarthElementColor = Color.FromHtml("#46ff7b");
        public static readonly Color LightningElementColor = Color.FromHtml("#ffff7c");
        //public static readonly Color ShadowElementColor = Color.FromHtml("#5a34d0");
        //public static readonly Color HolyElementColor = Color.FromHtml("#ffc107");

        //public static readonly Color IncreasedStatsColor = Color.FromHtml("#6fe77a");
        //public static readonly Color ReducedStatsColor = Color.FromHtml("#ff4c4b");
        //public static readonly Color NormalStatsColor = Colors.White;

        public static Color GetColorForElement(ElementTypes elementType)
        {
            switch (elementType)
            {
                case ElementTypes.Fire:
                    return FireElementColor;
                case ElementTypes.Water:
                    return WaterElementColor;
                case ElementTypes.Air:
                    return WindElementColor;
                case ElementTypes.Nature:
                    return EarthElementColor;
                case ElementTypes.Lightning:
                    return LightningElementColor;
                default:
                    return Colors.White;
            }
        }

        public static string GetElementIconPath(ElementTypes elementType)
        {
            switch (elementType)
            {
                case ElementTypes.Fire:
                    return "res://resources/textures/icon_fire.png";
                case ElementTypes.Water:
                    return "res://resources/textures/icon_water.png";
                case ElementTypes.Air:
                    return "res://resources/textures/icon_wind.png";
                case ElementTypes.Nature:
                    return "res://resources/textures/icon_ground.png";
                case ElementTypes.Lightning:
                    return "res://resources/textures/icon_lightning.png";
                default:
                    return String.Empty;
            }
        }
    }
}
