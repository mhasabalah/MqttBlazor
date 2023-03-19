const string token = "Your_Token";
const string bucket = "Your_Bucket";
const string org = "Your_Org";

var _influxDbClient = new InfluxDBClient("http://localhost:8086", token);
var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseKestrel(options =>
{
    options.ListenAnyIP(1883, listenOptions => listenOptions.UseMqtt());

    options.ListenAnyIP(7079, listenOptions => listenOptions.UseHttps());
});

// Add services to the container.




builder.Services.AddMqttServer(optionsBuilder =>
{
    optionsBuilder.WithDefaultEndpointPort(1883)
                  .WithDefaultEndpoint();
})
.AddConnections();

builder.Services.AddMqttTcpServerAdapter();
builder.Services.AddMqttWebSocketServerAdapter();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseMqttServer(server =>
{
    server.StartedAsync += async e =>
    {
        Console.WriteLine("MQTT Server started.");
        await Task.CompletedTask;
    };
    server.ClientConnectedAsync += async e =>
    {
        Console.WriteLine($"Client '{e.ClientId}' connected.");
        await Task.CompletedTask;
    };
    server.InterceptingPublishAsync += async e =>
    {
        var topic = e.ApplicationMessage.Topic;
        var payload = e.ApplicationMessage?.Payload == null ? null : Encoding.UTF8.GetString(e.ApplicationMessage?.Payload);

        Console.WriteLine(
            " TimeStamp: {0} -- Message: ClientId = {1}, Topic = {2}, Payload = {3}, QoS = {4}, Retain-Flag = {5}",
            DateTime.Now,
            e.ClientId,
            e.ApplicationMessage?.Topic,
            payload,
            e.ApplicationMessage?.QualityOfServiceLevel,
            e.ApplicationMessage?.Retain);

        if (topic.StartsWith("sensors/"))
        {
            var sensorId = topic.Substring("sensors/".Length);
            var value = double.Parse(payload);

            var point = PointData.Measurement("sensors")
                .Tag("sensorId", sensorId)
                .Field("value", value)
                .Timestamp(DateTime.UtcNow, WritePrecision.Ms);

            using (var writeApi = _influxDbClient.GetWriteApi())
            {
                writeApi.WritePoint(point, bucket, org);
            }
        }


        await Task.CompletedTask;
    };
});

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();


app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");


app.MapMqtt("/mqtt");


app.Run();
