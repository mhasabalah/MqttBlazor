using System.Security.Policy;

namespace MqttBlazor.Server;

public class InfluxDbService
{
    private readonly InfluxDBClient _influxDbClient;
    public InfluxDbService(IConfiguration configuration) => _influxDbClient = new InfluxDBClient(configuration.GetInfluxDbUrl(), configuration.GetInfluxDbToken());

    public InfluxDbConfig GetInfluxDbConfig(IConfiguration configuration)
    {
        var influxDbConfig = configuration.GetInfluxDbConfig();
        return influxDbConfig;
    }

    public IWriteApiAsync GetWriteApiAsync() => _influxDbClient.GetWriteApiAsync();
    public IWriteApi GetWriteApi() => _influxDbClient.GetWriteApi();





    public void Dispose() => _influxDbClient.Dispose();

}
