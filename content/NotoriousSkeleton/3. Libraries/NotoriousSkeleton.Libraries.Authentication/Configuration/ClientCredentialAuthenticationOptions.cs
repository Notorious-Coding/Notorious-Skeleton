using NotoriousSkeleton.Libraries.Configuration;

namespace NotoriousSkeleton.Libraries.Authentication.Configuration;
public class ClientCredentialAuthenticationOptions : IConfigurationOptions
{
    public static string SECTION_KEY => "ClientCredentialAuthentication";

    public string Authority { get; set; }
    public string Audience { get; set; }
}
