namespace MqttBlazor.Client;

public class Sensor
{
    public SensorType SensorType { get; set; }
    public decimal Value { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
}