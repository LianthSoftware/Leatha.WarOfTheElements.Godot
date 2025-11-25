using Godot;
using Leatha.WarOfTheElements.Common.Communication.Transfer;
using Leatha.WarOfTheElements.Common.Communication.Transfer.Enums;
using Leatha.WarOfTheElements.Godot.framework.Controls.Entities;
using Leatha.WarOfTheElements.Godot.framework.Extensions;
using Leatha.WarOfTheElements.Godot.framework.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Godot.Projection;

namespace Leatha.WarOfTheElements.Godot.framework.Controls.UserInterface
{
    public sealed partial class CharacterStatusBarControl : Control
    {
        [Export]
        public Label CharacterNameLabel { get; set; }

        [Export]
        public Label CharacterLevelLabel { get; set; }

        [ExportGroup("Cast Bar")]
        [Export]
        public CastBarControl CastBarControl { get; set; }



        [ExportGroup("Health")]
        [Export]
        public TextureProgressBar HealthBar { get; set; }

        [Export]
        public RichTextLabel HealthValueLabel { get; set; }

        [Export]
        public RichTextLabel HealthPercentLabel { get; set; }



        [ExportGroup("Primary Element")]
        [Export]
        public Control PrimaryElement { get; set; }

        [Export]
        public TextureProgressBar PrimaryLoadingBar { get; set; }

        [Export]
        public TextureRect PrimaryElementIcon { get; set; }

        [Export]
        public RichTextLabel PrimaryElementPercent { get; set; }




        [ExportGroup("Secondary Element")]
        [Export]
        public Control SecondaryElement { get; set; }

        [Export]
        public TextureProgressBar SecondaryLoadingBar { get; set; }

        [Export]
        public TextureRect SecondaryElementIcon { get; set; }

        [Export]
        public RichTextLabel SecondaryElementPercent { get; set; }




        [ExportGroup("Tertiary Element")]
        [Export]
        public Control TertiaryElement { get; set; }

        [Export]
        public TextureProgressBar TertiaryLoadingBar { get; set; }

        [Export]
        public TextureRect TertiaryElementIcon { get; set; }

        [Export]
        public RichTextLabel TertiaryElementPercent { get; set; }



        [ExportGroup("Auras")]
        [Export]
        public Control BuffContainer { get; set; }

        [Export]
        public Control DebuffContainer { get; set; }

        [Export]
        public PackedScene AuraControlScene { get; set; }

        public CharacterControl CharacterControl { get; set; }


        //public PlayerStateObject PlayerState { get; set; } // #TODO: General CharacterState


        private const string ElementChakraPercentTemplate =
            "[p][font_size=10]{0} [font_size=7][color={1}]%[/color][/font_size][/font_size][/p]";

        private const string HealthValueTemplate =
            "[p align=left]{0}[font_size=7][color=27ff7b] / [/color][/font_size]{1}[/p]";

        private const string HealthPercentPercentTemplate =
            "[p align=right]{0}[font_size=7][color=27ff7b] %[/color][/font_size][/p]";

        public override void _Ready()
        {
            base._Ready();

            GetGridControl(BuffContainer).ClearChildren(true);
            GetGridControl(DebuffContainer).ClearChildren(true);

            //GD.Print("Ready?");

            SetAuraPanelVisibility(BuffContainer);
            SetAuraPanelVisibility(DebuffContainer);
        }

        public void UpdateState(ICharacterStateObject characterState)
        {
            CharacterNameLabel.Text = characterState.CharacterName;
            CharacterLevelLabel.Text = characterState.CharacterLevel.ToString();

            //if (Name == "TargetStatusBarControl")
            //    GD.Print($"Wtf: { GetPath() } -> { CharacterNameLabel.Text }");

            UpdateResources(characterState);
        }

        public void UpdateResources(ICharacterStateObject characterState)
        {
            var healthPct = characterState.Resources.GetHealthPercent();

            //CharacterNameLabel.Text = ObjectAccessor.SessionService.CurrentCharacter.PlayerName;
            //CharacterLevelLabel.Text = ObjectAccessor.SessionService.CurrentCharacter.Level.ToString();

            HealthBar.Value = healthPct;

            HealthValueLabel.Text = string.Format(
                HealthValueTemplate,
                characterState.Resources.Health,
                characterState.Resources.MaxHealth);

            HealthPercentLabel.Text = string.Format(
                HealthPercentPercentTemplate,
                healthPct.ToString("F0"));

            // Primary Element.
            if (characterState.Resources.PrimaryChakra.Element != ElementTypes.None)
            {
                var color = GameConstants.GetColorForElement(characterState.Resources.PrimaryChakra.Element);
                var iconPath = GameConstants.GetElementIconPath(characterState.Resources.PrimaryChakra.Element);

                if (!string.IsNullOrWhiteSpace(iconPath))
                    PrimaryElementIcon.Texture = GD.Load<Texture2D>(iconPath);

                if (PrimaryElementIcon.Material is ShaderMaterial sm)
                    sm.SetShaderParameter("tint_color", color);

                var chakraPct = characterState.Resources.GetPrimaryChakraPercent();

                PrimaryElementPercent.Text = string.Format(
                    ElementChakraPercentTemplate,
                    chakraPct.ToString("F0"),
                    color.ToHtml());

                PrimaryLoadingBar.Value = chakraPct;
                PrimaryLoadingBar.TintProgress = color;
            }
            else
                PrimaryElement.Visible = false;

            // Secondary Element.
            if (characterState.Resources.SecondaryChakra.Element != ElementTypes.None)
            {
                var color = GameConstants.GetColorForElement(characterState.Resources.SecondaryChakra.Element);
                var iconPath = GameConstants.GetElementIconPath(characterState.Resources.SecondaryChakra.Element);

                if (!string.IsNullOrWhiteSpace(iconPath))
                    SecondaryElementIcon.Texture = GD.Load<Texture2D>(iconPath);

                if (SecondaryElementIcon.Material is ShaderMaterial sm)
                    sm.SetShaderParameter("tint_color", color);

                var chakraPct = characterState.Resources.GetSecondaryChakraPercent();

                SecondaryElementPercent.Text = string.Format(
                    ElementChakraPercentTemplate,
                    chakraPct.ToString("F0"),
                    color.ToHtml());

                SecondaryLoadingBar.Value = chakraPct;
                SecondaryLoadingBar.TintProgress = color;
            }
            else
                SecondaryElement.Visible = false;

            // Tertiary Element.
            if (characterState.Resources.TertiaryChakra.Element != ElementTypes.None)
            {
                var color = GameConstants.GetColorForElement(characterState.Resources.TertiaryChakra.Element);
                var iconPath = GameConstants.GetElementIconPath(characterState.Resources.TertiaryChakra.Element);

                if (!string.IsNullOrWhiteSpace(iconPath))
                    TertiaryElementIcon.Texture = GD.Load<Texture2D>(iconPath);

                if (TertiaryElementIcon.Material is ShaderMaterial sm)
                    sm.SetShaderParameter("tint_color", color);

                var chakraPct = characterState.Resources.GetTertiaryChakraPercent();

                TertiaryElementPercent.Text = string.Format(
                    ElementChakraPercentTemplate,
                    chakraPct.ToString("F0"),
                    color.ToHtml());

                TertiaryLoadingBar.Value = chakraPct;
                TertiaryLoadingBar.TintProgress = color;
            }
            else
                TertiaryElement.Visible = false;
        }

        public void SetSpellCasting(bool isCasting, SpellObject spellObject) // #TODO
        {
            if (isCasting)
            {
                CastBarControl.OnSpellCastStarted(spellObject);
            }
            else
            {
                CastBarControl.ResetCastBar();
            }
        }

        public void AddAura(AuraObject auraObject)
        {
            var control = AuraControlScene.Instantiate<AuraControl>();

            control.Initialize(auraObject);

            var container = auraObject.AuraInfo.AuraFlags.HasFlag(AuraFlags.IsPositive)
                ? GetGridControl(BuffContainer)
                : GetGridControl(DebuffContainer);

            container.AddChild(control);

            SetAuraPanelVisibility(BuffContainer);
            SetAuraPanelVisibility(DebuffContainer);
        }

        public void RemoveAura(AuraObject auraObject)
        {
            var auraControl = GetAuraControl(auraObject.AuraGuid);

            auraControl?.QueueFree();
            auraControl?.GetParent().RemoveChild(auraControl);

            SetAuraPanelVisibility(BuffContainer);
            SetAuraPanelVisibility(DebuffContainer);
        }

        public AuraControl GetAuraControl(Guid auraGuid)
        {
            return GetAllAuras().SingleOrDefault(i => i.AuraObject.AuraGuid == auraGuid);
        }

        private List<AuraControl> GetAllAuras()
        {
            var auraList = new List<AuraControl>();
            auraList.AddRange(GetBuffAuras());
            auraList.AddRange(GetDebuffAuras());

            return auraList;
        }

        public List<AuraControl> GetBuffAuras()
        {
            return GetGridControl(BuffContainer).GetChildren<AuraControl>();
        }

        public List<AuraControl> GetDebuffAuras()
        {
            return GetGridControl(DebuffContainer).GetChildren<AuraControl>();
        }

        private Control GetGridControl(Control parent)
        {
            return parent.GetNode<Control>("GridContainer");
        }

        private void SetAuraPanelVisibility(Control parent)
        {
            var gridControl = GetGridControl(parent);
            parent.Visible = gridControl != null && gridControl.GetChildCount() > 0;

            GD.Print($"{parent.GetPath()}: IsVisible = { parent.Visible } | Children = { gridControl?.GetChildCount() ?? -1}");
        }

        //public override void _Ready()
        //{
        //    base._Ready();

        //    var healthPct = PlayerState.Resources.GetHealthPercent();

        //    HealthBar.Value = healthPct;

        //    HealthValueLabel.Text = string.Format(
        //        HealthValueTemplate,
        //        PlayerState.Resources.Health,
        //        PlayerState.Resources.MaxHealth);

        //    HealthPercentLabel.Text = string.Format(
        //        HealthPercentPercentTemplate,
        //        healthPct);

        //    // Primary Element.
        //    if (PlayerState.Resources.PrimaryElement != ElementTypes.None)
        //    {
        //        var color = GameConstants.GetColorForElement(PlayerState.Resources.PrimaryElement);

        //        PrimaryLoadingBar.TintProgress = color;

        //        if (PrimaryElementIcon.Material is ShaderMaterial sm)
        //            sm.SetShaderParameter("tint_color", color);

        //        var chakraPct = PlayerState.Resources.GetPrimaryChakraPercent();

        //        PrimaryElementPercent.Text = string.Format(
        //            ElementChakraPercentTemplate,
        //            chakraPct);
        //    }
        //    else
        //        PrimaryElement.Visible = false;

        //    // Secondary Element.
        //    if (PlayerState.Resources.SecondaryElement != ElementTypes.None)
        //    {
        //        var color = GameConstants.GetColorForElement(PlayerState.Resources.SecondaryElement);

        //        SecondaryLoadingBar.TintProgress = color;

        //        if (SecondaryElementIcon.Material is ShaderMaterial sm)
        //            sm.SetShaderParameter("tint_color", color);

        //        var chakraPct = PlayerState.Resources.GetSecondaryChakraPercent();

        //        SecondaryElementPercent.Text = string.Format(
        //            ElementChakraPercentTemplate,
        //            chakraPct);
        //    }
        //    else
        //        SecondaryElement.Visible = false;

        //    // Tertiary Element.
        //    if (PlayerState.Resources.TertiaryElement != ElementTypes.None)
        //    {
        //        var color = GameConstants.GetColorForElement(PlayerState.Resources.TertiaryElement);

        //        TertiaryLoadingBar.TintProgress = color;

        //        if (TertiaryElementIcon.Material is ShaderMaterial sm)
        //            sm.SetShaderParameter("tint_color", color);

        //        var chakraPct = PlayerState.Resources.GetTertiaryChakraPercent();

        //        TertiaryElementPercent.Text = string.Format(
        //            ElementChakraPercentTemplate,
        //            chakraPct);
        //    }
        //    else
        //        TertiaryElement.Visible = false;
        //}
    }
}
