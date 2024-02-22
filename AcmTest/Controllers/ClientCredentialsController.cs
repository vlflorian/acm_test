using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace AcmTest.Controllers;

public class ClientCredentialsController : Controller
{
    private readonly ILogger<ClientCredentialsController> _logger;
    private readonly HttpClient _httpClient;
    private readonly IOptions<AcmSettings> _acmSettings;

    public ClientCredentialsController(ILogger<ClientCredentialsController> logger,
        HttpClient httpClient,
        IOptions<AcmSettings> acmSettings)
    {
        _logger = logger;
        _httpClient = httpClient;
        _acmSettings = acmSettings;
    }
    
    public async Task<IActionResult> Index()
    {
        _logger.LogInformation($"{nameof(ClientCredentialsController)}.{nameof(Index)} start.");
        
        var request = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "audience", _acmSettings.Value.ClientCredentialsSettings.Audience },
            { "client_id", _acmSettings.Value.ClientCredentialsSettings.ClientId },
            { "client_secret", _acmSettings.Value.ClientCredentialsSettings.ClientSecret },
            { "requested_token_type", "urn:ietf:params:oauth:token-type:jwt" },
            { "scope", _acmSettings.Value.ClientCredentialsSettings.Scopes },
        };
        var response = await _httpClient.PostAsync(_acmSettings.Value.AcmTokenEndpoint, new FormUrlEncodedContent(request));
        var responseBody = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new Exception($"Could not get token with client credentials flow. Got {response.StatusCode} with msg {responseBody} from endpoint {_acmSettings.Value.AcmTokenEndpoint}");
        
        var tokenResponse = JsonConvert.DeserializeObject<CallbackController.AcmTokenResponse>(responseBody);
        Guard.Against.Null(tokenResponse);
        _logger.LogInformation($"Successfully got token with client credentials flow: {tokenResponse.AccessToken} and id token {tokenResponse.IdToken}");
        ViewData["accessToken"] = tokenResponse.AccessToken;
        return View();
    }
}