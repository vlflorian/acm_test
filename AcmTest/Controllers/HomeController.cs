using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using AcmTest.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace AcmTest.Controllers;

public class HomeController(ILogger<HomeController> logger, IOptions<AcmSettings> acmSettings, IMemoryCache memoryCache) : Controller
{
    public IActionResult Index()
    {
        // https://authenticatie.vlaanderen.be/docs/beveiligen-van-toepassingen/integratie-methoden/oidc/technische-info/flow/#authorization-code-flow
        // https://authenticatie.vlaanderen.be/docs/beveiligen-van-toepassingen/integratie-methoden/oidc/technische-info/pkce/

        var codeVerifier = PkceHelper.GenerateCodeVerifier();
        var codeChallenge = PkceHelper.TransformCodeVerifier(codeVerifier);
        logger.LogInformation($"codeVerifier {codeVerifier}");
        logger.LogInformation($"codeChallenge {codeChallenge}");
        memoryCache.Set("codeVerifier", codeVerifier);
        var encodedClientId = WebUtility.UrlEncode(acmSettings.Value.FrontendClientId);
        var encodedRedirectUri = WebUtility.UrlEncode(acmSettings.Value.FrontendRedirectUri);
        var encodedScopes = WebUtility.UrlEncode(acmSettings.Value.FrontendScopes);
        var acmLoginUrl = $"{acmSettings.Value.AcmLoginUrl}?" +
                          $"response_type=code" +
                          $"&client_id={encodedClientId}" +
                          $"&scope={encodedScopes}" +
                          $"&redirect_uri={encodedRedirectUri}" +
                          $"&code_challenge_method={"S256"}" +
                          $"&code_challenge={codeChallenge}"
                          ;

        ViewData["acmLoginUrl"] = acmLoginUrl;
        
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
