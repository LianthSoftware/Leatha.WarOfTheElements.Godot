using Godot;
using Leatha.WarOfTheElements.Godot.framework.Controls;
using Leatha.WarOfTheElements.Godot.framework.Controls.Maps;
using Leatha.WarOfTheElements.Godot.framework.Extensions;
using Leatha.WarOfTheElements.Godot.framework.UI.menu;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace Leatha.WarOfTheElements.Godot.framework.Scripts.Maps.Map001
{
    public sealed partial class CharacterCreationEventControl : Node3D
    {
        [Export]
        public Label TextLabel { get; set; }

        private readonly List<(string Text, float Delay)> _texts =
        [
            ("(Woman's voice):  Don't worry, my child. Step into the portal, everything will be alright.", 3.0f),
            ("(Woman's voice):  You will learn all you need on the other side", 3.0f),
            ("(Woman's voice):  Step into the portal.", 3.0f),
        ];

        public override void _Ready()
        {
            base._Ready();

            StartEventAsync();
        }

        private void StartEventAsync()
        {
            // Delay event a bit.
            //await this.WaitForSeconds(3.0f);

            var tween = CreateTween();
            foreach (var text in _texts)
            {
                tween
                    .TweenProperty(TextLabel, Label.PropertyName.Text.ToString(), text.Text, 0.0f)
                    .SetDelay(text.Delay);
            }

            tween.TweenCallback(Callable.From(() =>
            {
                var textContainer = GetNode<Control>("CanvasLayer/TextContainer");
                var size = GetTree().Root.GetVisibleRect().Size;
                ObjectAccessor.ShadowCircleWrapper.GetShadowCircleControl().SetShaderParameters(size.X * 1.5f);
                ObjectAccessor.ShadowCircleWrapper.GetShadowCircleControl().Visible = true;
                textContainer.Visible = false;

                _ = RunAnimation();
            }));
        }

        private async Task RunAnimation()
        {
            // Wait so everything can load.
            await this.WaitForSeconds(0.1f);

            //Visible = false;

            ObjectAccessor.ShadowCircleWrapper.GetShadowCircleControl().RunAnimation(false, async () =>
            {
                GD.PrintErr("CALLBACK run");
                // #TODO: Load the player like exactly in EnterWorld

                var loadDataResult = await LoadDataInternal();
                if (!string.IsNullOrWhiteSpace(loadDataResult))
                {
                    GD.PrintErr(loadDataResult);

                    GetTree().ChangeSceneToFile(NodePathHelper.MainMenu_CharacterSelection_Path);

                    // #TODO: Let the character selection control know about the error.
                }
            });

            //this.SwitchSceneRoot(scene);
            //GetTree().CurrentScene = scene;
        }

        private async Task<string> LoadDataInternal()
        {
            var selectedCharacter = ObjectAccessor.SessionService.CurrentCharacter;
            if (selectedCharacter == null)
                return "Selected character is null."; // #TODO: better way.

            var result = await ObjectAccessor.GameHubService
                .GetClientHandler()
                .EnterWorld(selectedCharacter.PlayerId);

            if (result.IsError || result.Data == null)
                return "PlayerEnterWorld encountered an error: " + result.ErrorMessage; // #TODO: better way.

            var snapshot = result.Data;

            // Create game control.
            var gameControl = CreateGameControl();

            // Load particular map info.
            var mapInfo = ObjectAccessor.TemplateService.GetMapInfo(snapshot.MapId);
            if (mapInfo == null)
                return $"Map (Id = \"{snapshot.MapId}\") was not found."; // #TODO: better way.

            // Load map scene.
            var mapScene = GD.Load<PackedScene>(mapInfo.MapPath);
            if (mapScene == null)
                return $"Map Scene (\"{mapInfo.MapPath}\") could not be loaded."; // #TODO: better way.

            // Initialize map control.
            var mapControl = gameControl.MapControl;
            mapControl.SetMapInfo(mapInfo);

            // Load the particular map.
            var loadedMap = mapScene.Instantiate<MapScene>(); // #TODO: Is "Node3D" correct? Add some control.
            mapControl.AddChild(loadedMap);

            // #TODO: Load map.

            GetTree().Root.AddChild(gameControl);
            gameControl.Visible = false;

            await this.WaitForSeconds(3f); // #TODO

            ObjectAccessor.ShadowCircleWrapper.GetShadowCircleControl().SetShaderParameters(0.0f);
            ObjectAccessor.ShadowCircleWrapper.GetShadowCircleControl().RunAnimation(true, async () =>
            {
                await this.WaitForSeconds(1f); // #TODO

                GD.PrintErr("CALLBACK 2");
                // #TODO: Load the player like exactly in EnterWorld

                ObjectAccessor.ShadowCircleWrapper.GetShadowCircleControl().Visible = false;

                GetTree().CurrentScene.QueueFree(); // Remove loading scene
                //QueueFree();
                GetTree().CurrentScene = gameControl;
                gameControl.Visible = true;

                GD.Print($"*** Switched to game map \"{mapControl.MapInfo.MapName}\" ***");

                ObjectAccessor.SessionService.IsWorldLoaded = true;
                ObjectAccessor.CharacterService.ApplySnapshot(result.Data);

                loadedMap.OnPlayerEnteredMap(result.Data);

                GD.Print($"Player \"{result.Data.WorldObjectId.ObjectId}\" entered the world!");
            });

            return string.Empty;
        }

        private static GameControl CreateGameControl()
        {
            var gameScene = GD.Load<PackedScene>(NodePathHelper.Game_GameControlScene_Path);
            if (gameScene == null)
            {
                GD.PrintErr($"Game scene (\"{NodePathHelper.Game_GameControlScene_Path}\") could not be loaded.");
                return null;
            }

            var gameControl = gameScene.Instantiate<GameControl>();
            return gameControl;
        }
    }
}
