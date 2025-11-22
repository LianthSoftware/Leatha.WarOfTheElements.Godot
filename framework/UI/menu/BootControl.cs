using Godot;
using Leatha.WarOfTheElements.Godot.framework.Extensions;

namespace Leatha.WarOfTheElements.Godot.framework.UI.menu
{
    public sealed partial class BootControl : Node
    {
        [Export]
        public PackedScene LoginScene { get; set; }

        [Export]
        public PackedScene LoadingScene { get; set; }

        public override void _Ready()
        {
            ControlExtensions.SetGauntletCursor();
            Initialize();
        }

        private async void Initialize()
        {
            LoadSettings();
            //SoundService.GetInstance().PlayMainMusic(true); // #TODO

            //await LoadServersAsync();

            var accessTokenValidationResponse = await ObjectAccessor.ApiService.ValidateTokenAsync();
            var scene = LoginScene;
            if (accessTokenValidationResponse is { IsTokenValid: true })
            {
                //ObjectAccessor.SessionService.SetCurrentServer(1, "TEST Server"); // #TODO
                ObjectAccessor.SessionService.AccountId = accessTokenValidationResponse.AccountId;
                scene = LoadingScene;
            }

            var sceneName = scene.ResourcePath;
            GD.Print("Moving to ..  " + sceneName);

            CallDeferred(nameof(ChangeSceneDeferred), scene);
        }

        private void LoadSettings() // #TODO
        {
            //GD.PrintErr("Loading settings ...");

            //var settings = FileExtensions.GetSettings();

            //// Sound settings.
            //var soundService = SoundService.GetInstance();
            //soundService.SetMusicEnabled(settings.IsMusicEnabled);
            //soundService.SetSoundEnabled(settings.IsSoundEnabled);
            //soundService.OnMusicVolumeChanged(settings.MusicVolume);
            //soundService.OnSoundVolumeChanged(settings.SoundVolume);
            //soundService.SetBackgroundMusicEnabled(settings.IsBackgroundMusicEnabled);
            //soundService.SetBackgroundSoundEnabled(settings.IsBackgroundSoundEnabled);

            //// Screen settings.
            //var screenService = ScreenService.GetInstance();
            //screenService.SetWindowMode(settings.WindowMode);

            //screenService.SetScreenshotPath(settings.ScreenshotPath);

            //// Display settings.
            //var worldEnvironmentService = WorldEnvironmentService.GetInstance();

            //worldEnvironmentService.SetBrightness(settings.Brightness);
            //worldEnvironmentService.SetContrast(settings.Contrast);
            //worldEnvironmentService.SetSaturation(settings.Saturation);
        }

        //private async Task LoadServersAsync()
        //{
        //    var servers = await ObjectAccessor.ApiService.GetServersAsync();
        //    ObjectAccessor.SessionService.Servers = servers;
        //}

        private void ChangeSceneDeferred(PackedScene scene)
        {
            GetTree().ChangeSceneToPacked(scene);
        }
    }
}
