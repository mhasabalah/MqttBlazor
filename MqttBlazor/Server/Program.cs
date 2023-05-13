using MqttBlazor.Server;

//const string token = "LEK-xrIL06ywC1brF0IeJq-G5nhEkBF7Wk7YrkkhS41Qw01G9BC4xVVOeymAMJoktA9jnyUw-WtnfMl9EOzXtg==";
//const string bucket = "SobaTest";
//const string org = "Smart Farm";

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseKestrel(options =>
{
    options.ListenAnyIP(1883, listenOptions => listenOptions.UseMqtt());

    options.ListenAnyIP(7079, listenOptions => listenOptions.UseHttps());
});

// Add services to the container.


//mqtt
builder.Services.AddSingleton<InfluxDbService>();
builder.Services.AddSingleton<MqttController>();
var mqttController = builder.Services.BuildServiceProvider().GetRequiredService<MqttController>();
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
    server.ClientConnectedAsync += mqttController.OnClientConnected;
    server.InterceptingPublishAsync += mqttController.OnInterceptingPublishAsync;
    server.ValidatingConnectionAsync += e => mqttController.ValidateConnection(e, "SobaTest", "SobaTest");
    server.ClientDisconnectedAsync += mqttController.OnClientDisconnected;
});

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();


app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");


app.MapMqtt("/mqtt");


app.Run();
