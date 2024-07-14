using Assignment4_WindowsService;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHttpClient();
        services.AddHostedService<Worker>();
    }).UseWindowsService();

var host = builder.Build();
host.Run();
