using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Leatha.WarOfTheElements.Godot.framework.Extensions;

namespace Leatha.WarOfTheElements.Godot.framework.communication
{
    public sealed class AuthTokenHandler : DelegatingHandler
    {
        public AuthTokenHandler(Func<string> getToken)
        {
            _getToken = getToken;
        }

        public const int MaxRetries = 3;
        private readonly Func<string> _getToken;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = null;
            for (var i = 0; i < MaxRetries; ++i)
            {
                try
                {
                    // Load the token dynamically.
                    var token = _getToken();

                    if (!string.IsNullOrEmpty(token))
                        request.Headers.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    var requestId = Guid.NewGuid();

                    GD.Print(
                        $"[{DateTime.Now:dd.MM.yyyy HH:mm:ss}][{requestId}]: Sending request to \"{request.RequestUri}\" with RetryCount = \"{i}\".");

                    response = await base.SendAsync(request, cancellationToken);

                    GD.Print(
                        $"[{DateTime.Now:dd.MM.yyyy HH:mm:ss}][{requestId}]: Request to \"{request.RequestUri}\" finished with StatusCode = \"{(int)response.StatusCode}\" ({response.StatusCode}).");

                    var requestUri = request.RequestUri?.ToString();
                    if (requestUri == null)
                        return response;

                    // Do NOT retry refresh token.
                    if (requestUri.Contains("api/auth/refresh-token"))
                    {
                        if (!response.IsSuccessStatusCode)
                            return response;
                    }

                    // We handle HTTP 401 - Unauthorized until we get other response or max tries is reached.
                    if (response.StatusCode != HttpStatusCode.Unauthorized || i == MaxRetries - 1)
                        return response;

                    await ObjectAccessor.ApiService.RegenerateTokenAsync();
                }
                catch (Exception ex)
                {
                    GD.PrintErr(ex.Message);
                }
                finally
                {
                    // Wait 1 second before retrying.
                    await Task.Delay(1000, cancellationToken);
                }
            }

            return response;
        }
    }
}
