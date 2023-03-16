using MqttBlazor.Client;
using MQTTnet.Client;
using MQTTnet.Packets;
using MQTTnet.Protocol;

namespace MqttBlazor.Client;

public static class ClientSubscribe
{
    public static async Task HandleReceivedApplicationMessage()
    {
        /*
         * This sample subscribes to a topic and processes the received message.
         */

        var mqttFactory = new MqttFactory();

        using (var mqttClient = mqttFactory.CreateMqttClient())
        {
            var mqttClientOptions = new MqttClientOptionsBuilder().WithWebSocketServer("localhost:5109/mqtt").Build();


            // Setup message handling before connecting so that queued messages
            // are also handled properly. When there is no event handler attached all
            // received messages get lost.
            mqttClient.ApplicationMessageReceivedAsync += e =>
            {
                Console.WriteLine("Received application message.");
                e.DumpToConsole();

                return Task.CompletedTask;
            };

            await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

            var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(
                    f =>
                    {
                        f.WithTopic("humidity/bme680");
                    })
                .Build();

            await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

            Console.WriteLine("MQTT client subscribed to topic.");


        }
    }

    public static async Task SendResponses()
    {
        /*
         * This sample subscribes to a topic and sends a response to the broker. This requires at least QoS level 1 to work!
         */

        var mqttFactory = new MqttFactory();

        using (var mqttClient = mqttFactory.CreateMqttClient())
        {
            mqttClient.ApplicationMessageReceivedAsync += delegate (MqttApplicationMessageReceivedEventArgs args)
            {
                // Do some work with the message...

                // Now respond to the broker with a reason code other than success.
                args.ReasonCode = MqttApplicationMessageReceivedReasonCode.ImplementationSpecificError;
                args.ResponseReasonString = "That did not work!";

                // User properties require MQTT v5!
                args.ResponseUserProperties.Add(new MqttUserProperty("My", "Data"));

                // Now the broker will resend the message again.
                return Task.CompletedTask;
            };

            var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer("broker.hivemq.com").Build();

            await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

            var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(
                    f =>
                    {
                        f.WithTopic("mqttnet/samples/topic/1");
                    })
                .Build();

            var response = await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

            Console.WriteLine("MQTT client subscribed to topic.");

            // The response contains additional data sent by the server after subscribing.
            response.DumpToConsole();
        }
    }

    public static async Task SubscribeMultipleTopics()
    {
        /*
         * This sample subscribes to several topics in a single request.
         */

        var mqttFactory = new MqttFactory();

        using (var mqttClient = mqttFactory.CreateMqttClient())
        {
            var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer("broker.hivemq.com").Build();

            await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

            // Create the subscribe options including several topics with different options.
            // It is also possible to all of these topics using a dedicated call of _SubscribeAsync_ per topic.
            var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(
                    f =>
                    {
                        f.WithTopic("mqttnet/samples/topic/1");
                    })
                .WithTopicFilter(
                    f =>
                    {
                        f.WithTopic("mqttnet/samples/topic/2").WithNoLocal();
                    })
                .WithTopicFilter(
                    f =>
                    {
                        f.WithTopic("mqttnet/samples/topic/3").WithRetainHandling(MqttRetainHandling.SendAtSubscribe);
                    })
                .Build();

            var response = await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

            Console.WriteLine("MQTT client subscribed to topics.");

            // The response contains additional data sent by the server after subscribing.
            response.DumpToConsole();
        }
    }

    public static async Task SubscribeTopic()
    {
        /*
         * This sample subscribes to a topic.
         */

        var mqttFactory = new MqttFactory();

        using (var mqttClient = mqttFactory.CreateMqttClient())
        {
            var mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer("broker.hivemq.com").Build();

            await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

            var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(
                    f =>
                    {
                        f.WithTopic("mqttnet/samples/topic/1");
                    })
                .Build();

            var response = await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

            Console.WriteLine("MQTT client subscribed to topic.");

            // The response contains additional data sent by the server after subscribing.
            response.DumpToConsole();
        }
    }
}