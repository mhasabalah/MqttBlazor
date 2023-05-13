using MQTTnet.Protocol;
using MQTTnet.Server;

namespace MqttBlazor.Server;
public class MqttController
{
    private readonly string org;
    private readonly string bucket;
    private readonly InfluxDbService _influxDbService;

    public MqttController(IConfiguration configuration, InfluxDbService influxDbService)
    {
        _influxDbService = influxDbService;
        bucket = configuration.GetInfluxDbBucket();
        org = configuration.GetInfluxDbOrg();
    }

    public Task OnClientConnected(ClientConnectedEventArgs eventArgs)
    {
        Console.WriteLine($"Client '{eventArgs.ClientId}' connected.");
        return Task.CompletedTask;
    }

    public Task OnClientDisconnected(ClientDisconnectedEventArgs eventArgs)
    {
        Console.WriteLine($"Client '{eventArgs.ClientId}' disconnected.");
        return Task.CompletedTask;
    }

    public Task ValidateConnection(ValidatingConnectionEventArgs eventArgs, string username, string passward)
    {
        if (eventArgs.UserName != username)
            eventArgs.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;

        if (eventArgs.Password != passward)
            eventArgs.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;

        return Task.CompletedTask;
    }

    public async Task OnInterceptingPublishAsync(InterceptingPublishEventArgs eventArgs)
    {
        string? topic = eventArgs.ApplicationMessage.Topic;
        string? payload = eventArgs.ApplicationMessage?.Payload == null ? null : Encoding.UTF8.GetString(eventArgs.ApplicationMessage?.Payload!);

        Console.WriteLine(
            $" TimeStamp: {DateTime.Now} -- Message: ClientId = {eventArgs.ClientId}, Topic = {topic}," +
            $" Payload = {payload}, QoS = {eventArgs.ApplicationMessage?.QualityOfServiceLevel}," +
            $" Retain-Flag = {eventArgs.ApplicationMessage?.Retain}+");
        /*
         * modem.sensors
         * foreach sensors in sensor
         * if sensor.name == "temp"
         * 
         * 
         * 
     {
        // topic : sensor/temp/123
    "location": {
        "lat": "43.56704 N",
        "lon": "116.24053 W",
        "elev": 2822
    },
    "name": "KBOI",
    "sensors": [{
        "name": "temp",
        "date": 48,
        }
        "dew_point": 34,
        "humidity": 61,
        "wind": 9,
        "direction": "NW",
        "pressure": 30.23
    }],
    "updated": "25 Oct 12:05 PM MDT"
}kd
         */

        if (topic.StartsWith("sensor/"))
        {
            var sensorId = topic.Substring("sensor/".Length);
            var value = double.Parse(payload);

            var point = PointData.Measurement("sensors")
                .Tag("sensorId", sensorId)
                .Field("value", value)
                .Timestamp(DateTime.UtcNow, WritePrecision.Ns);

            using var writeApi = _influxDbService.GetWriteApi();

            writeApi.WritePoint(point, bucket, org);
        }

        await Task.CompletedTask;
    }
}
