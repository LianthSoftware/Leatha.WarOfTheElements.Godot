using System;
using System.Threading.Tasks;
using Godot;
using Leatha.WarOfTheElements.Godot.framework.Extensions;
using Range = Godot.Range;

namespace Leatha.WarOfTheElements.Godot.framework.UI.menu
{
	public sealed partial class LoadingControl : Node
	{
		[Export]
		public PackedScene CharacterSelectionScene { get; set; }

		[Export]
		public TextureProgressBar LoadingBar { get; set; }

		private Tween _loadingTween;

		public override void _Ready()
		{
			base._Ready();

			LoadingBar.Value = 0.0f;
			LoadData();
		}

		private void SetLoadingBarValue(float value, float duration = 0.75f)
		{
			_loadingTween?.Kill();

			_loadingTween = CreateTween();
			_loadingTween.TweenProperty(LoadingBar, Range.PropertyName.Value.ToString(), value, duration);
		}

		private async void LoadData()
		{
			try
			{
				var sessionService = ObjectAccessor.SessionService;

				var instance = ObjectAccessor.GameHubService;

				// Connect to SignalR server.
				await instance.CreateConnectionAsync();

				SetLoadingBarValue(10.0f); // #TODO: TEST ONLY

				await instance.ConnectToServerAsync();

				//await Task.Delay(1000); // #TODO
				SetLoadingBarValue(20.0f); // #TODO: TEST ONLY

				//var playerResponse = await instance
				//    .GetClientHandler()
				//    .GetPlayer(sessionService.PlayerId);
				//if (playerResponse.IsError)
				//    throw new InvalidOperationException(playerResponse.ErrorMessage);

				//sessionService.Player = playerResponse.Data;

				var characterListResponse = await instance
					.GetClientHandler()
					.GetCharacterList(sessionService.AccountId);
				if (characterListResponse.IsError)
					throw new InvalidOperationException(characterListResponse.ErrorMessage);

				ObjectAccessor.SessionService.Characters = characterListResponse.Data;

				//sessionService.Player = playerResponse.Data;

				SetLoadingBarValue(66.0f); // #TODO: TEST ONLY

				await ObjectAccessor.TemplateService.LoadTemplatesAsync();

				//await Task.Delay(3000);

				SetLoadingBarValue(100.0f, 0.5f); // #TODO: TEST ONLY

				await this.WaitForSeconds(0.5f);

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
				GetTree().ChangeSceneToPacked(CharacterSelectionScene);
			}
			catch (Exception ex)
			{
				GD.PrintErr(ex);
				GetTree().Quit((int)Error.CantResolve);
			}
		}
	}
}
