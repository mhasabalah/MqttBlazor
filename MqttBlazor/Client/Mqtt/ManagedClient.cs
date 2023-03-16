namespace MqttBlazor.Client;

public class ManagedClient
{
    public async Task ConnectClient()
    {
        /*
         * This sample creates a simple managed MQTT client and connects to a public broker.
         *
         * The managed client extends the existing _MqttClient_. It adds the following features.
         * - Reconnecting when connection is lost.
         * - Storing pending messages in an internal queue so that an enqueue is possible while the client remains not connected.
         */

        MqttFactory? mqttFactory = new MqttFactory();

        using var managedMqttClient = mqttFactory.CreateManagedMqttClient();

        MqttClientOptions? mqttClientOptions = new MqttClientOptionsBuilder()
            //.WithTcpServer("broker.hivemq.com")
            .WithWebSocketServer("broker.hivemq.com:8000/mqtt")
            .Build();

        ManagedMqttClientOptions? managedMqttClientOptions = new ManagedMqttClientOptionsBuilder()
            .WithClientOptions(mqttClientOptions)
            .Build();

        await managedMqttClient.StartAsync(managedMqttClientOptions);

        // The application message is not sent. It is stored in an internal queue and
        // will be sent when the client is connected.
        await managedMqttClient.EnqueueAsync("Topic", "Payload");

        Console.WriteLine("The managed MQTT client is connected.");

        // Wait until the queue is fully processed.
        SpinWait.SpinUntil(() => managedMqttClient.PendingApplicationMessagesCount == 0, 10000);

        Console.WriteLine($"Pending messages = {managedMqttClient.PendingApplicationMessagesCount}");
    }



    //private static async Task SubscribeToTopic(IManagedMqttClient client, string topic)
    //{
    //    await client.SubscribeAsync(new TopicFilterBuilder().WithTopic(topic).Build());
    //    Console.WriteLine($"Subscribed to topic: {topic}");

    //    client.UseApplicationMessageReceivedHandler(HandleReceivedMessage);
    //}
}