WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton<IMqttService, MqttService>();

//builder.Services.AddSingleton<IInfluxDbClient, InfluxDbClientWrapper>();
//builder.Services.AddSingleton<MqttInfluxDbService>();


await builder.Build().RunAsync();