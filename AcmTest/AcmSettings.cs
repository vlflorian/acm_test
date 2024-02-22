namespace AcmTest;

public class AcmSettings
{
    public string AcmLoginUrl { get; set; }
    public string AcmTokenEndpoint { get; set; }
    public string FrontendClientId { get; set; }
    public string FrontendClientSecret { get; set; }
    public string FrontendRedirectUri { get; set; }
    public string FrontendScopes { get; set; }
    public string ApiClientId { get; set; }

    public ClientCredentialsSettings ClientCredentialsSettings { get; set; }
}

public class ClientCredentialsSettings
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string Scopes { get; set; }
    public string Audience { get; set; }
}