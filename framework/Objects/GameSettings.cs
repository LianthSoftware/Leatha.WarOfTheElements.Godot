using Godot;

namespace Leatha.WarOfTheElements.Godot.framework.Objects
{
    public sealed class GameSettings
    {
        public bool IsMusicEnabled { get; set; }

        public int MusicVolume { get; set; }

        public bool IsSoundEnabled { get; set; }

        public int SoundVolume { get; set; }

        public bool IsBackgroundMusicEnabled { get; set; }

        public bool IsBackgroundSoundEnabled { get; set; }

        public string Language { get; set; }

        public string ScreenshotPath { get; set; }

        public float Brightness { get; set; }

        public float Contrast { get; set; }

        public float Saturation { get; set; }

        public Window.ModeEnum WindowMode { get; set; }

        public static GameSettings Default =>
            new()
            {
                IsMusicEnabled = true,
                IsSoundEnabled = true,
                MusicVolume = 50,
                SoundVolume = 50,
                IsBackgroundMusicEnabled = true,
                IsBackgroundSoundEnabled = true,
                WindowMode = Window.ModeEnum.Fullscreen,
                Brightness = 1.0f,
                Contrast = 1.0f,
                Saturation = 1.0f,
                //ScreenshotPath = ScreenService.DefaultScreenshotGlobalPath,
                Language = "en",
            };
    }
}
