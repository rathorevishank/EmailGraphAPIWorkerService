using EmailGraphAPIWorkerService;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton<HttpClient>(); // Register HttpClient as a singleton service
builder.Services.AddSingleton<APIHandler>(); // Register APIHandler as a singleton service
builder.Services.AddHostedService<Worker>();

// Ensure the Microsoft.Extensions.Hosting.WindowsServices package is installed
builder.Services.AddWindowsService(); // Register to run as a Windows Service

var host = builder.Build();
host.Run();
