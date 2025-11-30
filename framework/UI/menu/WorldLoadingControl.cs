using Godot;
using Leatha.WarOfTheElements.Godot.framework.Controls;
using Leatha.WarOfTheElements.Godot.framework.Controls.Maps;
using Leatha.WarOfTheElements.Godot.framework.Extensions;
using System.Threading.Tasks;
using static Godot.HttpRequest;

namespace Leatha.WarOfTheElements.Godot.framework.UI.menu
{
    public sealed partial class WorldLoadingControl : Node3D
    {
        [Export]
        public TextureProgressBar LoadingBar { get; set; }

        [Export]
        public Label LoadingText { get; set; }

        private Tween _loadingTween;

        public override void _Ready()
        {
            base._Ready();

            LoadingBar.Value = 0.0f;
            CallDeferred(nameof(LoadData));
        }

        private void SetLoadingBarValue(float value)
        {
            _loadingTween?.Kill();

            _loadingTween = CreateTween();
            _loadingTween.TweenProperty(LoadingBar, Range.PropertyName.Value.ToString(), value, 0.75f);
        }

        private async void LoadData()
        {
            var loadDataResult = await LoadDataInternal();
            if (!string.IsNullOrWhiteSpace(loadDataResult))
            {
                GD.PrintErr(loadDataResult);

                GetTree().ChangeSceneToFile(NodePathHelper.MainMenu_CharacterSelection_Path);

                // #TODO: Let the character selection control know about the error.
            }
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

            SetLoadingBarValue(25.0f); // #TODO: TEST ONLY

            var snapshot = result.Data;

            // Create game control.
            var gameControl = CreateGameControl();

            // Load particular map info.
            var mapInfo = ObjectAccessor.TemplateService.GetMapInfo(snapshot.MapId);
            if (mapInfo == null)
                return $"Map (Id = \"{ snapshot.MapId }\") was not found."; // #TODO: better way.

            // Load map scene.
            var mapScene = GD.Load<PackedScene>(mapInfo.MapPath);
            if (mapScene == null)
                return $"Map Scene (\"{ mapInfo.MapPath }\") could not be loaded."; // #TODO: better way.

            SetLoadingBarValue(30.0f); // #TODO: TEST ONLY

            // Initialize map control.
            var mapControl = gameControl.MapControl;
            mapControl.SetMapInfo(mapInfo);

            // Load the particular map.
            var loadedMap = mapScene.Instantiate<MapScene>(); // #TODO: Is "Node3D" correct? Add some control.
            mapControl.AddChild(loadedMap);

            SetLoadingBarValue(50.0f); // #TODO: TEST ONLY

            // #TODO: Load map.

            GetTree().Root.AddChild(gameControl);
            gameControl.Visible = false;

            SetLoadingBarValue(100.0f); // #TODO: TEST ONLY

            await this.WaitForSeconds(0.75f);

            await this.RunOnMainThreadAsync(() =>
            {
                GetTree().CurrentScene.QueueFree(); // Remove loading scene
                GetTree().CurrentScene = gameControl;
                gameControl.Visible = true;

                GD.Print($"*** Switched to game map \"{mapControl.MapInfo.MapName}\" ***");

                ObjectAccessor.SessionService.IsWorldLoaded = true;
                ObjectAccessor.CharacterService.ApplySnapshot(result.Data);

                loadedMap.OnPlayerEnteredMap(result.Data);

                GD.Print($"Player \"{result.Data.WorldObjectId.ObjectId}\" entered the world!");

                return Task.CompletedTask;
            });

            return string.Empty;
        }

        private static GameControl CreateGameControl()
        {
            var gameScene = GD.Load<PackedScene>(NodePathHelper.Game_GameControlScene_Path);
            if (gameScene == null)
            {
                GD.PrintErr($"Game scene (\"{ NodePathHelper.Game_GameControlScene_Path }\") could not be loaded.");
                return null;
            }

            var gameControl = gameScene.Instantiate<GameControl>();
            return gameControl;
        }
    }
}
