using System.Threading.Tasks;
using Godot;
using Leatha.WarOfTheElements.Godot.framework.Controls.Maps;
using Leatha.WarOfTheElements.Godot.framework.Extensions;

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
            var selectedCharacter = ObjectAccessor.SessionService.CurrentCharacter;
            if (selectedCharacter == null)
            {
                GD.PrintErr("Selected character is null.");
                return;
            }

            var result = await ObjectAccessor.GameHubService
                .GetClientHandler()
                .EnterWorld(selectedCharacter.PlayerId);

            if (result.IsError || result.Data == null)
            {
                GD.PrintErr("PlayerEnterWorld encountered an error: " + result.ErrorMessage);
                return;
            }

            SetLoadingBarValue(25.0f); // #TODO: TEST ONLY

            var snapshot = result.Data;
            var mapInfo = ObjectAccessor.TemplateService.GetMapInfo(snapshot.MapId);
            if (mapInfo == null)
            {
                GD.PrintErr("Map was not found.");
                return;
            }

            var scene = GD.Load<PackedScene>(mapInfo.MapPath);
            if (scene == null)
            {
                GD.PrintErr("Scene could not be loaded.");
                return;
            }

            SetLoadingBarValue(30.0f); // #TODO: TEST ONLY

            var mapControl = scene.Instantiate<MapControl>();
            mapControl.SetMapInfo(mapInfo);

            SetLoadingBarValue(50.0f); // #TODO: TEST ONLY

            // #TODO: Load map.

            GetTree().Root.AddChild(mapControl);
            mapControl.Visible = false;

            SetLoadingBarValue(100.0f); // #TODO: TEST ONLY

            await this.WaitForSeconds(0.75f);

            await this.RunOnMainThreadAsync(() =>
            {
                GetTree().CurrentScene.QueueFree(); // Remove loading scene
                GetTree().CurrentScene = mapControl;
                mapControl.Visible = true;

                GD.Print($"*** Switched to game map \"{mapControl.MapInfo.MapName}\" ***");

                ObjectAccessor.SessionService.IsWorldLoaded = true;
                ObjectAccessor.CharacterService.ApplySnapshot(result.Data);

                GD.Print($"Player \"{result.Data.WorldObjectId.ObjectId}\" entered the world!");

                return Task.CompletedTask;
            });
        }
    }
}
