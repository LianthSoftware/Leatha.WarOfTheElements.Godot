using System;
using Godot;
using Leatha.WarOfTheElements.Godot.framework.Extensions;

namespace Leatha.WarOfTheElements.Godot.framework.UI.menu
{
    public sealed partial class LoadingControl : Node
    {
        [Export]
        public PackedScene MainMenuScene { get; set; }

        [Export]
        public PackedScene GameScene { get; set; }

        [Export]
        public Label LoadingLabel { get; set; }

        [Export]
        public double LoadingTimerTick { get; set; } = 0.5D;

        private const string ServerTemplate = "Server  {0}";
        private const string LoadingTemplate = "Loading {0}";
        private const int PadWidth = 3;

        private double _loadingTimer;
        private int _dotsCount = 0;

        public override void _Ready()
        {
            base._Ready();

            LoadData();
        }

        public override void _Process(double delta)
        {
            base._Process(delta);

            if (_loadingTimer <= 0.0D)
            {
                _loadingTimer = LoadingTimerTick;
                OnLoadingTimerTick();
            }
            else
                _loadingTimer -= delta;
        }

        private void OnLoadingTimerTick()
        {
            // Create a string with _dotsCount dots
            var value = new string('.', _dotsCount);

            // Pad it with spaces to make the total length PadWidth
            value = value.PadRight(PadWidth, ' ');

            LoadingLabel.Text = String.Format(LoadingTemplate, value);

            ++_dotsCount;
            if (_dotsCount > PadWidth)
                _dotsCount = 0;
        }

        private async void LoadData()
        {
            try
            {
                var sessionService = ObjectAccessor.SessionService;

                var instance = ObjectAccessor.GameHubService;

                // Connect to SignalR server.
                await instance.CreateConnectionAsync();
                await instance.ConnectToServerAsync();

                //await Task.Delay(10000); // #TODO

                var playerResponse = await instance
                    .GetClientHandler()
                    .GetPlayer(sessionService.PlayerId);
                if (playerResponse.IsError)
                    throw new InvalidOperationException(playerResponse.ErrorMessage);

                sessionService.Player = playerResponse.Data;

                await sessionService.LoadTemplatesAsync();

                //var gameResponse = await instance
                //    .GetClientHandler()
                //    .GetCurrentGame(sessionService.PlayerId);
                //var game = gameResponse.Data;
                //if (game == null)
                //{
                //    GetTree().ChangeSceneToPacked(MainMenuScene);
                //    return;
                //}

                //var gamePlayer = game.Players.SingleOrDefault(i => i.PlayerId == sessionService.PlayerId);
                //if (gamePlayer == null)
                //    throw new InvalidOperationException("Game player is null.");

                //if (game.GameState != GameState.Finished && gamePlayer.Health > 0)
                //{
                //    var gameControl = GameScene.Instantiate<GameControl>();

                //    GetTree().Root.AddChild(gameControl);
                //    gameControl.Visible = false;

                //    await this.RunOnMainThreadAsync(async () =>
                //    {
                //        GetTree().CurrentScene.QueueFree(); // Remove loading scene
                //        GetTree().CurrentScene = gameControl;
                //        gameControl.Visible = true;

                //        GD.Print($"*** Reconnect to the game: { game.GameId } ***");

                //        await gameControl.ReconnectToTheGameAsync(game, gamePlayer);
                //    });
                //}
                //else
                    GetTree().ChangeSceneToPacked(MainMenuScene);
            }
            catch (Exception ex)
            {
                GD.PrintErr(ex);
                GetTree().Quit((int)Error.CantResolve);
            }
        }
    }
}
