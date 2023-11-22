var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddSanGuoSha();
builder.Services.AddHostedServices();
builder.Logging.ClearProviders();
builder.Logging.AddColorConsole();

var app = builder.Build();
app.UseFileServer(false);

app.Run();

return 0;