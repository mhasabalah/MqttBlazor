﻿namespace MqttBlazor.Client;

public interface IMqttService : IDisposable
{
    event Func<MqttApplicationMessageReceivedEventArgs, Task> MattApplicationMessageReceivedAsync;

    Task ConnectMqttTcp(string uri);
    Task ConnectMqttWebSocet(string uri);
    Task ConnectClientTimeout(string uri, bool isWebSocet = true);
    Task ConnectClientUsingTLS(string uri, bool isWebSocket = true);
    Task ConnectClientWithTLSEncryption(string uri, bool isWebSocket = true);

    Task PublishMqtt(string topic, string payload);

    Task SubscribeMqtt(List<string> topics, Action<string> messageHandler);
    Task SubscribeMqtt(string topic, Action<string> messageHandler);
    
    Task PingServer(string uri, bool isWebSocet = true);
    
    Task ReconnectUsingEvent(string uri, bool isWebSocet = true);
    void ReconnectUsingTimer(string uri, bool isWebSocet = true);
    
    Task UnsubscribeMqtt(string topic);
    
    Task CleanDisconnectMqtt();
    Task SubscribeMqtt(string topic, Func<string, Task> messageHandler);
}