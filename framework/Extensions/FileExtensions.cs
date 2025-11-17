using System;
using System.Text.Json;
using Godot;
using Leatha.WarOfTheElements.Godot.framework.Objects;

namespace Leatha.WarOfTheElements.Godot.framework.Extensions
{
    internal sealed class FileExtensions
    {
        private const string UserFolder = "user://";
        private const string EncryptionKeyPath = "encryption.key";
        private const string PlayerPath = "player.woe";
        private const string AccessTokenPath = "access.token";
        private const string RefreshTokenPath = "refresh.token";
        private const string RealmListPath = "realmlist.woe";
        private const string SettingPath = "settings.woe";

        static FileExtensions()
        {
            GD.Print(GetRealUserPath());
        }

        public static GameSettings GetSettings()
        {
            using var file = FileAccess.Open(UserFolder + SettingPath, FileAccess.ModeFlags.Read);
            if (file == null)
            {
                var defaultSettings = GameSettings.Default;
                SetSettings(defaultSettings);
                return defaultSettings;
            }

            var text = file.GetAsText();

            return JsonSerializer.Deserialize<GameSettings>(text);
        }

        public static RealmList GetRealmList()
        {
            using var file = FileAccess.Open(UserFolder + RealmListPath, FileAccess.ModeFlags.Read);
            if (file == null)
                return null;

            var text = file.GetAsText();

            return JsonSerializer.Deserialize<RealmList>(text);
        }

        public static Guid GetPlayerId()
        {
            using var file = FileAccess.Open(UserFolder + PlayerPath, FileAccess.ModeFlags.Read);
            if (file == null)
                return Guid.Empty;

            var playerId = file.GetLine();
            file.Close();

            return Guid.Parse(playerId);
        }

        public static string GetAccessToken()
        {
            using var file = FileAccess.Open(UserFolder + AccessTokenPath, FileAccess.ModeFlags.Read);
            if (file == null)
                return null;

            var token = file.GetLine();
            file.Close();

            return token;
        }

        public static string GetRefreshToken()
        {
            using var file = FileAccess.Open(UserFolder + RefreshTokenPath, FileAccess.ModeFlags.Read);
            if (file == null)
                return null;

            var token = file.GetLine();
            file.Close();

            return token;
        }

        public static void SetSettings(GameSettings settings)
        {
            var data = settings ?? GameSettings.Default;

            using var file = FileAccess.Open(UserFolder + SettingPath, FileAccess.ModeFlags.Write);
            if (file == null)
                return;

            var text = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            file.StoreString(text);
            file.Close();
        }

        public static void SetPlayerId(string playerId)
        {
            if (playerId == null)
            {
                DirAccess.RemoveAbsolute(UserFolder + PlayerPath);
                return;
            }

            using var file = FileAccess.Open(UserFolder + PlayerPath, FileAccess.ModeFlags.Write);
            if (file == null)
                return;

            file.StoreLine(playerId);
            file.Close();
        }

        public static void SetAccessToken(string token)
        {
            if (token == null)
            {
                DirAccess.RemoveAbsolute(UserFolder + AccessTokenPath);
                return;
            }

            using var file = FileAccess.Open(UserFolder + AccessTokenPath, FileAccess.ModeFlags.Write);
            if (file == null)
                return;

            file.StoreLine(token);
            file.Close();
        }

        public static void SetRefreshToken(string token)
        {
            if (token == null)
            {
                DirAccess.RemoveAbsolute(UserFolder + RefreshTokenPath);
                return;
            }

            using var file = FileAccess.Open(UserFolder + RefreshTokenPath, FileAccess.ModeFlags.Write);
            if (file == null)
                return;

            file.StoreLine(token);
            file.Close();
        }

        public static void ClearOnLogout()
        {
            var dir = DirAccess.Open(UserFolder);

            // Player.
            if (FileAccess.FileExists(UserFolder + PlayerPath))
                dir?.Remove(PlayerPath);

            // Access Token.
            if (FileAccess.FileExists(UserFolder + AccessTokenPath))
                dir?.Remove(AccessTokenPath);

            // Refresh Token.
            if (FileAccess.FileExists(UserFolder + RefreshTokenPath))
                dir?.Remove(RefreshTokenPath);
        }




        public static bool FileExists(string path)
        {
            return FileAccess.FileExists(path);
        }


        public static void GenerateEncryptKey()
        {
            var key = "8D04A0FAFBCD47F398590919122E9E00";

            using var file = FileAccess.Open(UserFolder + EncryptionKeyPath, FileAccess.ModeFlags.Write);
            file.StoreLine(key);
            file.Close();
            GD.Print("Encryption key saved.");
        }

        public static string LoadEncryptionKey()
        {
            using var file = FileAccess.Open(UserFolder + EncryptionKeyPath, FileAccess.ModeFlags.Read);
            var key = file.GetLine();
            file.Close();

            GD.Print($"Encryption key loaded = {key}.");

            return key;
        }

        public static string GetRealUserPath()
        {
            var realPath = ProjectSettings.GlobalizePath(UserFolder);
            return realPath;
        }
    }
}
