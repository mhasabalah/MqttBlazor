namespace MqttBlazor.Client;

public class MqttService : IMqttService
{
    private readonly IMqttClient _mqttClient;

    public MqttService()
    {
        MqttFactory? mqttFactory = new MqttFactory();
        _mqttClient = mqttFactory.CreateMqttClient();
    }

    public async Task ConnectMqttTcp(string uri) => await _mqttClient.ConnectAsync(MqttWebTcp(uri));
    public async Task ConnectMqttWebSocet(string uri) => await _mqttClient.ConnectAsync(MqttWebSocet(uri));
    public async Task ConnectClientTimeout(string uri, bool isWebSocet = true)
    {
        try
        {
            using (var timeoutToken = new CancellationTokenSource(TimeSpan.FromSeconds(1)))
            {
                MqttClientOptions mqttClientOptions = isWebSocet ? MqttWebSocet(uri) : MqttWebTcp(uri);
                await _mqttClient.ConnectAsync(mqttClientOptions, timeoutToken.Token);

            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Timeout while connecting.");
        }
    }
    public async Task ConnectClientUsingTLS(string uri, bool isWebSocket = true)
    {
        var mqttClientOptionsBuilder = new MqttClientOptionsBuilder();
        mqttClientOptionsBuilder = isWebSocket
            ? mqttClientOptionsBuilder.WithWebSocketServer(uri)
            : mqttClientOptionsBuilder.WithTcpServer(uri);

        mqttClientOptionsBuilder.WithTls(o =>
        {
            o.AllowUntrustedCertificates = true;
            o.SslProtocol = SslProtocols.Tls12;
        });

        MqttClientOptions? mqttClientOptions = mqttClientOptionsBuilder.Build();

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await _mqttClient.ConnectAsync(mqttClientOptions, timeout.Token);

        Console.WriteLine("The MQTT client is connected.");
    }
    public async Task ConnectClientWithTLSEncryption(string uri, bool isWebSocket = true)
    {
        var mqttClientOptionsBuilder = new MqttClientOptionsBuilder();
        mqttClientOptionsBuilder = isWebSocket
            ? mqttClientOptionsBuilder.WithWebSocketServer(uri)
            : mqttClientOptionsBuilder.WithTcpServer(uri);

        MqttClientOptions? mqttClientOptions = mqttClientOptionsBuilder
                                                     .WithTls(o =>
                                                     o.CertificateValidationHandler = _ => true).Build();

        using var timeout = new CancellationTokenSource(5000);
        var response = await _mqttClient.ConnectAsync(mqttClientOptions, timeout.Token);

        Console.WriteLine("The MQTT client is connected.");

        response.DumpToConsole();
    }

    public async Task PublishMqtt(string topic, string payload)
    {
        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload)
            .Build();

        await _mqttClient.PublishAsync(message);
    }

    public async Task SubscribeMqtt(string topic, Action<string> messageHandler)
    {
        MqttClientSubscribeOptions mqttSubscribeOptions = new MqttClientSubscribeOptionsBuilder().WithTopicFilter(topic).Build();

        await _mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

        Console.WriteLine($"MQTT client subscribed to {topic}.");
        _mqttClient.ApplicationMessageReceivedAsync += e =>
        {
            if (e.ApplicationMessage.Topic == topic)
            {
                var message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                messageHandler.Invoke(message);
            }
            return Task.CompletedTask;
        };
    }
    public async Task SubscribeMqtt(List<string> topics, Action<string> messageHandler)
    {
        foreach (var topic in topics)
        {
            MqttClientSubscribeOptions topicFilter = new MqttClientSubscribeOptionsBuilder().WithTopicFilter(topic).Build();
            await _mqttClient.SubscribeAsync(topicFilter, CancellationToken.None);
            Console.WriteLine($"MQTT client subscribed to {topic}.");

            _mqttClient.ApplicationMessageReceivedAsync += e =>
            {
                if (e.ApplicationMessage.Topic == topic)
                {
                    var message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                    messageHandler.Invoke(message);
                }
                return Task.CompletedTask;
            };
        }
    }

    public async Task PingServer(string uri, bool isWebSocet = true)
    {
        MqttClientOptions mqttClientOptions = isWebSocet ? MqttWebSocet(uri) : MqttWebTcp(uri);
        await _mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

        await _mqttClient.PingAsync(CancellationToken.None);

        Console.WriteLine("The MQTT server replied to the ping request.");
    }

    public async Task ReconnectUsingEvent(string uri, bool isWebSocet = true)
    {
        /*
         * This sample shows how to reconnect when the connection was dropped.
         * This approach uses one of the events from the client.
         * This approach has a risk of dead locks! Consider using the timer approach (see sample).
         */
        MqttClientOptions mqttClientOptions = isWebSocet ? MqttWebSocet(uri) : MqttWebTcp(uri);

        _mqttClient.DisconnectedAsync += async e =>
        {
            if (e.ClientWasConnected)
            {
                // Use the current options as the new options.
                await _mqttClient.ConnectAsync(_mqttClient.Options);
            }
        };
        await _mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

    }
    public void ReconnectUsingTimer(string uri, bool isWebSocet = true)
    {
        /*
         * This sample shows how to reconnect when the connection was dropped.
         * This approach uses a custom Task/Thread which will monitor the connection status.
         * This is the recommended way but requires more custom code!
         */
        MqttClientOptions mqttClientOptions = isWebSocet ? MqttWebSocet(uri) : MqttWebTcp(uri);

        _ = Task.Run(
            async () =>
            {
                // User proper cancellation and no while(true).
                while (true)
                {
                    try
                    {
                        // This code will also do the very first connect! So no call to _ConnectAsync_ is required in the first place.
                        if (!await _mqttClient.TryPingAsync())
                        {
                            await _mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                            // Subscribe to topics when session is clean etc.
                            Console.WriteLine("The MQTT client is connected.");
                        }
                    }
                    catch
                    {
                        // Handle the exception properly (logging etc.).
                    }
                    finally
                    {
                        // Check the connection state every 5 seconds and perform a reconnect if required.
                        await Task.Delay(TimeSpan.FromSeconds(5));
                    }
                }
            });
    }

    public async Task UnsubscribeMqtt(string topic)
    {
        await _mqttClient.UnsubscribeAsync(topic);
        Console.WriteLine("Unsub Async");
    }

    public async Task CleanDisconnectMqtt() => await _mqttClient.DisconnectAsync(new MqttClientDisconnectOptionsBuilder().WithReason(MqttClientDisconnectReason.NormalDisconnection).Build());
    public void Dispose() => _mqttClient?.Dispose();

    private static MqttClientOptions MqttWebSocet(string uri) => new MqttClientOptionsBuilder()
            .WithWebSocketServer(uri)
            .Build();
    private static MqttClientOptions MqttWebTcp(string uri) => new MqttClientOptionsBuilder()
            .WithTcpServer(uri)
            .Build();
}
