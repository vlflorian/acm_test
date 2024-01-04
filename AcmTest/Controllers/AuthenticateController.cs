using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace AcmTest.Controllers;

public class AuthenticateController(
    ILogger<HomeController> logger,
    IHttpContextAccessor httpContextAccessor,
    HttpClient httpClient,
    IOptions<AcmSettings> acmSettings,
    IMemoryCache memoryCache) : Controller
{
    /// <summary>
    /// This method is called after the user logs in with ACM (itsme, eid) and is redirected back to this sample app with an access code.
    /// https://authenticatie.vlaanderen.be/docs/beveiligen-van-toepassingen/integratie-methoden/oidc/technische-info/flow/#authorization-code-flow
    /// https://authenticatie.vlaanderen.be/docs/beveiligen-van-toepassingen/integratie-methoden/oidc/technische-info/pkce/
    /// </summary>
    /// <returns></returns>
    public async Task<IActionResult> Index()
    {
        Guard.Against.Null(httpContextAccessor);
        var codeVerifier = memoryCache.Get<string>("codeVerifier");
        var accessCode = httpContextAccessor?.HttpContext?.Request.Query["code"];
        logger.LogInformation($"{nameof(AuthenticateController)}.{nameof(Index)} was called with code {accessCode}, codeVerifier {codeVerifier}");
        Guard.Against.NullOrEmpty(accessCode);
        
        // https://authenticatie.vlaanderen.be/docs/beveiligen-van-toepassingen/integratie-methoden/oidc/technische-info/aanmelden/#tokens-opvragen-bij-het-token-endpoint
        var tokenResponse = await ExchangeAccessCodeForIdToken(accessCode, codeVerifier);
        
        // https://authenticatie.vlaanderen.be/docs/beveiligen-van-api/oauth-rest/rest-namens-gebruiker/rest-token-exchange/
        var apiToken = await ExchangeIdTokenForApiToken(tokenResponse.AccessToken);
        
        ViewData["idToken"] = tokenResponse.IdToken;
        ViewData["accessToken"] = tokenResponse.AccessToken;
        ViewData["apiToken"] = apiToken;
        
        return View();
    }

    private async Task<AcmTokenResponse> ExchangeAccessCodeForIdToken(string accessCode, string codeVerifier)
    {
        logger.LogInformation($"{nameof(ExchangeAccessCodeForIdToken)} start.");
        
       var exchangeCodeRequest = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "code", accessCode },
            { "redirect_uri", acmSettings.Value.FrontendRedirectUri },
            { "client_id", acmSettings.Value.FrontendClientId },
            { "client_secret", acmSettings.Value.FrontendClientSecret },
            { "code_verifier", codeVerifier } // pkce
        };
        var response = await httpClient.PostAsync(acmSettings.Value.AcmTokenEndpoint, new FormUrlEncodedContent(exchangeCodeRequest));
        var responseBody = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new Exception($"Could not exchange accessCode for id/access tokens. Got {response.StatusCode} with msg {responseBody} from endpoint {acmSettings.Value.AcmTokenEndpoint}");
        
        var tokenResponse = JsonConvert.DeserializeObject<AcmTokenResponse>(responseBody);
        Guard.Against.Null(tokenResponse);
        logger.LogInformation($"Successfully exchanged accessCode for id/access tokens: {tokenResponse.AccessToken} and id token {tokenResponse.IdToken}");

        return tokenResponse;
    }

    /// <summary>
    /// Exchange id token for api access token
    /// </summary>
    /// <param name="accessCode"></param>
    private async Task<string> ExchangeIdTokenForApiToken(string accessToken)
    {
        logger.LogInformation($"{nameof(ExchangeIdTokenForApiToken)} start.");
        var exchangeCodeRequest = new Dictionary<string, string>
        {
            { "audience", acmSettings.Value.ApiClientId },
            { "client_id", acmSettings.Value.FrontendClientId },
            { "client_secret", acmSettings.Value.FrontendClientSecret },
            { "subject_token", accessToken },
            { "subject_token_type", "urn:ietf:params:oauth:token-type:access_token" },
            { "grant_type", "urn:ietf:params:oauth:grant-type:token-exchange" },
            { "requested_token_type", "urn:ietf:params:oauth:token-type:jwt" },
            { "scope", acmSettings.Value.FrontendScopes}
        };
        var response = await httpClient.PostAsync(acmSettings.Value.AcmTokenEndpoint, new FormUrlEncodedContent(exchangeCodeRequest));
        var responseBody = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new Exception($"Could not exchange access token for api access token. Got {response.StatusCode} with msg {responseBody} from endpoint {acmSettings.Value.AcmTokenEndpoint}");
        
        var tokenResponse = JsonConvert.DeserializeObject<AcmTokenResponse>(responseBody);
        Guard.Against.Null(tokenResponse);
        logger.LogInformation($"Successfully exchanged access token for api access token: {tokenResponse.AccessToken}");
        return tokenResponse.AccessToken;
    }

    public class AcmTokenResponse
    {
        [JsonPropertyName("access_token")]
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        
        [JsonPropertyName("token_type")]
        [JsonProperty("token_type")]
        public string TokenType { get; set; }
        
        [JsonPropertyName("expires_in")]
        [JsonProperty("expires_in")]
        public string ExpiresIn { get; set; }
        
        [JsonPropertyName("id_token")]
        [JsonProperty("id_token")]
        public string IdToken { get; set; }
    }
    
    
}