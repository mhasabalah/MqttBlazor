namespace MqttBlazor.Server;

public record InfluxDbConfig(string Url, string Token, string Org, string Bucket);
public static class InfluxDbServiceExtensions
{
    private const string ConfigSectionName = "InfluxDbConfig";
    public static string GetInfluxDbConfig(this IConfiguration configuration, string name) => configuration.GetSection(ConfigSectionName)[name];
    public static string GetInfluxDbUrl(this IConfiguration configuration) => configuration.GetSection(ConfigSectionName)["Url"];
    public static string GetInfluxDbBucket(this IConfiguration configuration) => configuration.GetSection(ConfigSectionName)["Bucket"];
    public static string GetInfluxDbOrg(this IConfiguration configuration) => configuration.GetSection(ConfigSectionName)["Org"];
    public static string GetInfluxDbToken(this IConfiguration configuration) => configuration.GetSection(ConfigSectionName)["Token"];

    public static InfluxDbConfig GetInfluxDbConfig(this IConfiguration configuration) => configuration.GetSection(ConfigSectionName).Get<InfluxDbConfig>();
}
