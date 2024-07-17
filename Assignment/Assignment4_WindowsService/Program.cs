using Assignment4_WindowsService;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHttpClient();
        services.AddHostedService<Worker>();
    }).UseWindowsService();

var host = builder.Build();
host.Run();

/*
 dotnet publish -o .\publish -c Release -p:PublishSingleFile=true

sc create "AAAAAA" BinPath="Path\To\publish\Assignment4_WindowsService.exe"
 
 */