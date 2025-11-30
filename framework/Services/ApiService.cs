using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Godot;
using Leatha.WarOfTheElements.Common.Communication.Messages.Requests;
using Leatha.WarOfTheElements.Common.Communication.Messages.Responses;
using Leatha.WarOfTheElements.Common.Communication.Transfer;
using Leatha.WarOfTheElements.Godot.framework.communication;
using Leatha.WarOfTheElements.Godot.framework.Extensions;
using static Leatha.WarOfTheElements.Godot.framework.Extensions.FileExtensions;

namespace Leatha.WarOfTheElements.Godot.framework.Services
{
    public interface IApiService
    {
        Task<ValidateTokenResponse> ValidateTokenAsync();

        Task RegenerateTokenAsync();

        Task<Guid> AuthenticateUserAsync(string email, string password, bool rememberMe);

        Task LogoutAsync();



        Task<List<SpellInfoObject>> GetSpellTemplatesAsync();

        Task<List<MapInfoObject>> GetMapTemplatesAsync();

        Task<List<GameObjectInfoObject>> GetGameObjectTemplatesAsync();
    }

    public sealed partial class ApiService : Node, IApiService
    {
        private WarOfTheElementsServerClient _serverClient;

        public override void _Ready()
        {
            base._Ready();

            var serverUrl = GetRealmList().ServerUrl;

            var authTokenHandler = new AuthTokenHandler(GetAccessToken)
            {
                InnerHandler = new HttpClientHandler()
            };

            var httpClient = new System.Net.Http.HttpClient(authTokenHandler)
            {
                BaseAddress = new Uri(serverUrl)
            };

            _serverClient = new WarOfTheElementsServerClient(serverUrl, httpClient);

            ObjectAccessor.ApiService = this;
        }

        public async Task<ValidateTokenResponse> ValidateTokenAsync()
        {
            try
            {
                var accessToken = GetAccessToken();
                if (String.IsNullOrWhiteSpace(accessToken))
                    return null;

                var request = new ValidateTokenRequest
                {
                    AccessToken = accessToken
                };

                var response = await _serverClient.ApiAuthValidateTokenAsync(request);
                var json = System.Text.Json.JsonSerializer.Serialize(response);

                GD.Print(json);

                return response;
            }
            catch (Exception ex)
            {
                GD.PrintErr(ex);
                return null;
            }
        }

        public async Task RegenerateTokenAsync()
        {
            try
            {
                var request = new RefreshTokenRequest
                {
                    AccountId = GetAccountId(),
                    RefreshToken = GetRefreshToken(),
                };

                var response = await _serverClient.ApiAuthRefreshTokenAsync(request);
                var json = System.Text.Json.JsonSerializer.Serialize(response);

                // #TODO: Encrypt files.

                //SetAccountId(response.AccountId.ToString()); // #TODO
                SetAccessToken(response.AccessToken);
                SetRefreshToken(response.RefreshToken);

                GD.Print(json);
            }
            catch (Exception ex)
            {
                GD.PrintErr(ex);
                //return false;
            }
        }

        public async Task<Guid> AuthenticateUserAsync(string email, string password, bool rememberMe)
        {
            // #TODO: If successful call, get access and (if enabled) refresh token.

            //TravianServerClient client = new TravianServerClient(null, null);
            //client.ApiAuthLoginAsync(new LoginRequest
            //{
            //    Email = 
            //});

            GD.Print($"AuthenticateUserAsync - Email = {email}");

            //try
            //{
            var request = new LoginRequest
            {
                Email = email,
                Password = password,
                RememberMe = rememberMe,
            };

            var response = await _serverClient.ApiAuthLoginAsync(request);
            var json = System.Text.Json.JsonSerializer.Serialize(response);

            // #TODO: Encrypt files.

            GD.Print($"AuthenticateUserAsync - AccountId = {response.AccountId}");

            SetAccountId(response.AccountId.ToString());
            SetAccessToken(response.AccessToken);
            SetRefreshToken(response.RefreshToken);

            GD.Print(json);

            return response.AccountId;
            //}
            //catch (Exception ex)
            //{
            //    GD.PrintErr(ex);
            //}
        }

        public async Task LogoutAsync()
        {
            try
            {
                // Call API to logout.
                var request = new LogoutRequest
                {
                    AccountId = GetAccountId(),
                    RefreshToken = GetRefreshToken()
                };

                await _serverClient.ApiAuthLogoutAsync(request);

                // Clear all access info.
                ClearOnLogout();

                // Disconnect client.
                await ObjectAccessor.GameHubService.DisconnectAsync();

                // Change to LoginScene.
                GetTree().ChangeSceneToFile("res://scenes/auth/login_page.tscn");
            }
            catch (Exception ex)
            {
                GD.PrintErr(ex);
            }
        }

        public async Task<List<SpellInfoObject>> GetSpellTemplatesAsync()
        {
            try
            {
                var response = await _serverClient.ApiGameDataTemplatesSpellTemplateAsync();
                return response.ToList();
            }
            catch (Exception ex)
            {
                GD.PrintErr(ex);
                return [];
            }
        }

        public async Task<List<MapInfoObject>> GetMapTemplatesAsync()
        {
            try
            {
                var response = await _serverClient.ApiGameDataTemplatesMapTemplateAsync();
                return response.ToList();
            }
            catch (Exception ex)
            {
                GD.PrintErr(ex);
                return [];
            }
        }

        public async Task<List<GameObjectInfoObject>> GetGameObjectTemplatesAsync()
        {
            try
            {
                var response = await _serverClient.ApiGameDataTemplatesGameObjectTemplateAsync();
                return response.ToList();
            }
            catch (Exception ex)
            {
                GD.PrintErr(ex);
                return [];
            }
        }

        //public async Task<List<SpellTemplateObject>> GetSpellTemplatesAsync()
        //{
        //    try
        //    {
        //        var response = await _serverClient.ApiGameDataTemplatesCardsAsync();
        //        return response.ToList();
        //    }
        //    catch (Exception ex)
        //    {
        //        GD.PrintErr(ex);
        //        return [];
        //    }
        //}
    }
}
