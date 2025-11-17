using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using Leatha.WarOfTheElements.Godot.framework.Controls.SpellBook;
using Leatha.WarOfTheElements.Godot.framework.Extensions;

namespace Leatha.WarOfTheElements.Godot.framework.Controls.Talents
{
    public sealed partial class TalentsControl : Control
    {
        [Export] public TalentTooltipControl TalentTooltipControl { get; set; }

        [Export] public Control SpellBookOption { get; set; }

        [Export] public Control EnhancementsOption { get; set; }

        [Export] public SpellBookControl SpellBookControl { get; set; }

        [Export] public EnhancementControl EnhancementControl { get; set; }

        private Control _selectedOptionControl;
        private Control _selectedControl;

        public override void _Ready()
        {
            base._Ready();

            void SetOptionButtonBorder(Control control, bool isActive)
            {
                control.SelfModulate = isActive ? Color.FromHtml("#ffff7a") : Colors.White;

                var title = control.GetNode<Label>("MarginContainer/Title");
                title.LabelSettings.FontColor = isActive ? Color.FromHtml("#ffd35b") : Colors.Gray;

                //title.Visible = isActive;
            }

            var spellBookOptionButton = SpellBookOption.GetNode<TextureButton>("PanelContainer/Control/TextureButton");
            var enhancementsOptionButton = EnhancementsOption.GetNode<TextureButton>("PanelContainer/Control/TextureButton");

            spellBookOptionButton.Pressed += () =>
            {
                SetOptionButtonBorder(_selectedOptionControl, false);

                _selectedOptionControl = SpellBookOption;

                SetOptionButtonBorder(_selectedOptionControl, true);

                if (_selectedControl != null)
                    _selectedControl.Visible = false;

                _selectedControl = SpellBookControl;
                _selectedControl.Visible = true;
            };

            spellBookOptionButton.MouseEntered += () => SetOptionButtonBorder(SpellBookOption, true);
            spellBookOptionButton.MouseExited += () =>
            {
                if (_selectedOptionControl != SpellBookOption)
                    SetOptionButtonBorder(SpellBookOption, false);
            };

            enhancementsOptionButton.Pressed += () =>
            {
                SetOptionButtonBorder(_selectedOptionControl, false);

                _selectedOptionControl = EnhancementsOption;

                SetOptionButtonBorder(_selectedOptionControl, true);

                if (_selectedControl != null)
                    _selectedControl.Visible = false;

                _selectedControl = EnhancementControl;
                _selectedControl.Visible = true;
            };

            enhancementsOptionButton.MouseEntered += () => SetOptionButtonBorder(EnhancementsOption, true);
            enhancementsOptionButton.MouseExited += () =>
            {
                if (_selectedOptionControl != EnhancementsOption)
                    SetOptionButtonBorder(EnhancementsOption, false);
            };

            _selectedOptionControl = SpellBookOption;
            SetOptionButtonBorder(_selectedOptionControl, true);
            SetOptionButtonBorder(EnhancementsOption, false);

            SpellBookControl.Visible = true;
            EnhancementControl.Visible = false;

            _selectedControl = SpellBookControl;

            // #TODO: Test only, generate it.
            //{
            //    var talentsContainer =
            //        GetNode<Control>(
            //            "PanelContainer/MarginContainer/HorizontalContainer/NinePatchRect/MarginContainer/VerticalContainer/ContentContainer/TalentsContainer");

            //    var itemControls = talentsContainer.GetChildren().OfType<TalentItemControl>().ToList();

            //    foreach (var item in itemControls)
            //        item.TalentsControl = this;
            //}

            //CallDeferred(nameof(Test2));
            CallDeferred(nameof(LoadPlayerSpellBook));
            CallDeferred(nameof(LoadPlayerEnhancements));
        }


        private async void LoadPlayerSpellBook()
        {
            var spellsResponse = await ObjectAccessor.GameHubService.GetClientHandler()
                .GetPlayerSpellBook(Guid.Parse("878d108f-7dfe-4309-9aab-c91e2bd927cb"));

            if (spellsResponse.IsError || spellsResponse.Data == null)
            {
                GD.PrintErr(spellsResponse.ErrorMessage);
                return;
            }

            //SpellBookControl.

            if (spellsResponse.Data.Count == 0)
                return;

            foreach (var spellInfo in spellsResponse.Data)
                SpellBookControl.AddSpellControl(spellInfo);

            SpellBookControl.SelectSpell(spellsResponse.Data.First().SpellId);
        }

        private async void LoadPlayerEnhancements()
        {
            var spellsResponse = await ObjectAccessor.GameHubService.GetClientHandler()
                .GetPlayerEnhancements(Guid.Parse("878d108f-7dfe-4309-9aab-c91e2bd927cb"));

            if (spellsResponse.IsError || spellsResponse.Data == null)
            {
                GD.PrintErr(spellsResponse.ErrorMessage);
                return;
            }

            //SpellBookControl.

            if (spellsResponse.Data.Count == 0)
                return;

            foreach (var spellInfo in spellsResponse.Data)
                EnhancementControl.AddSpellControl(spellInfo);

            EnhancementControl.SelectEnhancement(spellsResponse.Data.First().SpellId);
        }







        [Export]
        public PackedScene ItemScene { get; set; }

        [Export]
        public PackedScene LineScene { get; set; }

        private void Test2()
        {
            var primary =
                GetNode<Control>(
                    "PanelContainer/MarginContainer/HorizontalContainer/NinePatchRect/MarginContainer/VerticalContainer/ContentContainer/HBoxContainer/PrimaryElementTalents/VBoxContainer/TalentContainer");
            var secondary =
                GetNode<Control>(
                    "PanelContainer/MarginContainer/HorizontalContainer/NinePatchRect/MarginContainer/VerticalContainer/ContentContainer/HBoxContainer/SecondaryElementTalents/VBoxContainer/TalentContainer");
            var tertiary =
                GetNode<Control>(
                    "PanelContainer/MarginContainer/HorizontalContainer/NinePatchRect/MarginContainer/VerticalContainer/ContentContainer/HBoxContainer/TertiaryElementTalents/VBoxContainer/TalentContainer");

            Test(primary);
            Test(secondary);
            Test(tertiary);
        }

        private int _spellIdCounter = 1;

        private void Test(Control talentsContainer)
        {
            //var talentsContainer =
            //    GetNode<Control>(
            //        "PanelContainer/MarginContainer/NinePatchRect/MarginContainer/VerticalContainer/ContentContainer/TalentsContainer");
            //var talentsContainer =
            //    GetNode<Control>(
            //        "PanelContainer/MarginContainer/NinePatchRect/MarginContainer/VerticalContainer/ContentContainer/HBoxContainer/PrimaryElementTalents/VBoxContainer/TalentContainer");
            GD.Print(talentsContainer.Size);

            var spellsImages = new List<string>
            {
                "res://resources/textures/spell_frost_arcticwinds.jpg",
                "res://resources/textures/spell_frost_chillingblast.jpg",
                "res://resources/textures/spell_frost_frost.jpg",
                "res://resources/textures/spell_frost_frostblast.jpg",
                "res://resources/textures/spell_frost_frostbolt.jpg",
                "res://resources/textures/spell_frost_frozencore.jpg",
                "res://resources/textures/spell_frost_icefloes.jpg",
                "res://resources/textures/spell_frost_wisp.jpg",
            };

            var halfWidth = talentsContainer.Size.X / 2;
            var quarterWidth = talentsContainer.Size.X / 4;

            var points = new List<Point>
            {
                new () { Id = _spellIdCounter++, X = halfWidth, Y = talentsContainer.Size.Y * 0.15f, LinkIds = [ 2, 3 ]},
                new () { Id = _spellIdCounter++, X = quarterWidth, Y = talentsContainer.Size.Y * 0.3f, LinkIds = [ 4 ] },
                new () { Id = _spellIdCounter++, X = quarterWidth * 3, Y = talentsContainer.Size.Y * 0.3f, LinkIds = [ 4 ] },
                new () { Id = _spellIdCounter++, X = halfWidth, Y = talentsContainer.Size.Y * 0.45f, LinkIds = [ 5, 8 ] },
                new () { Id = _spellIdCounter++, X = halfWidth, Y = talentsContainer.Size.Y * 0.65f, LinkIds = [ 6, 7 ] },
                new () { Id = _spellIdCounter++, X = quarterWidth, Y = talentsContainer.Size.Y * 0.85f },
                new () { Id = _spellIdCounter++, X = quarterWidth * 3, Y = talentsContainer.Size.Y * 0.85f },
                new () { Id = _spellIdCounter++, X = talentsContainer.Size.X * 0.9f, Y = talentsContainer.Size.Y * 0.45f },
            };

            var learnTalents = new List<int>
            {
                1, 2, 4, 5
            };

            var rand = new RandomNumberGenerator();
            //for (var n = 0; n <= 5; ++n)
            for (var n = 0; n < points.Count; ++n)
            {
                var scene = ItemScene.Instantiate<TalentItemControl>();
                scene.TalentsControl = this;

                scene.SpellId = points[n].Id; // #TODO: Test only
                scene.LinkIds = points[n].LinkIds; // #TODO: Test only
                scene.TalentImageTexture = GD.Load<Texture2D>(spellsImages[n]);
                scene.Tooltip = "Increase the damage of [color=goldenrod]Fireball[/color] by [color=green]5%[/color].";

                if (learnTalents.Contains(scene.SpellId))
                {
                    scene.SetSpellLearnt(true);
                }

                talentsContainer.AddChild(scene);

                //scene.Position = new Vector2(100 + (50 * n), talentsContainer.Size.Y * (0.2f * n));
                //scene.Position = new Vector2(rand.RandiRange(100, (int)talentsContainer.Size.X - 200), talentsContainer.Size.Y * (0.2f * n));
                scene.Position = new Vector2(points[n].X, points[n].Y);
                GD.Print($"{n}: {scene.GlobalPosition}");
            }

            var lines = GetNode<Control>(
                "PanelContainer/MarginContainer/HorizontalContainer/NinePatchRect/MarginContainer/VerticalContainer/ContentContainer/Lines");
            //for (var n = 0; n < talentsContainer.GetChildCount(); ++n)
            //{
            //    var first = talentsContainer.GetChildOrNull<TalentItemControl>(n);
            //    var second = talentsContainer.GetChildOrNull<TalentItemControl>(n + 1);

            //    if (first != null && second != null)
            //    {
            //        var line = LineScene.Instantiate<Line2D>();
            //        lines.AddChild(line);
            //        LinkNodes(first, second, line);
            //    }
            //}

            var items = talentsContainer.GetChildren().OfType<TalentItemControl>().ToList();
            foreach (var item in items)
            {
                var links = item.LinkIds ?? [];
                if (!links.Any())
                    continue;

                foreach (var link in links)
                {
                    var line = LineScene.Instantiate<Line2D>();
                    lines.AddChild(line);

                    var second = items.SingleOrDefault(i => i.SpellId == link);
                    if (second != null)
                    {
                        var isLearnt = learnTalents.Contains(second.SpellId) && learnTalents.Contains(item.SpellId);
                        LinkNodes(item, second, line, isLearnt);
                    }
                }

                //var first = talentsContainer.GetChildOrNull<TalentItemControl>(n);
                //var second = talentsContainer.GetChildOrNull<TalentItemControl>(n + 1);

                //if (first != null && second != null)
                //{
                //    var line = LineScene.Instantiate<Line2D>();
                //    lines.AddChild(line);
                //    LinkNodes(first, second, line);
                //}
            }
        }

        private class Point
        {
            public int Id { get; set; }

            public float X { get; set; }

            public float Y { get; set; }

            public List<int> LinkIds { get; set; }
        }

        private void LinkNodes(TalentItemControl a, TalentItemControl b, Line2D line, bool isLearnt)
        {
            const float OFFSET = 30f;

            // True screen centers
            var p1 = a.GetGlobalRect().GetCenter();
            var p2 = b.GetGlobalRect().GetCenter();

            // Direction
            var dir = (p2 - p1).Normalized();

            // Offset towards each other
            var globalStart = p1 + dir * OFFSET;
            var globalEnd = p2 - dir * OFFSET;

            // Convert global → line local
            line.Points = new Vector2[]
            {
                line.ToLocal(globalStart),
                line.ToLocal(globalEnd)
            };

            if (!isLearnt)
                line.DefaultColor = Color.FromHtml("#3c3c3c");
        }
    }
}
