using LogCompilerBeta.Infrastructure;
using LogCompilerBeta.Interfaces;
using LogCompilerBeta.Models;
using LogCompilerBeta.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Globalization;

var cultureInfo = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<Settings>(builder.Configuration.GetSection("Settings"));

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddContentReaders();
builder.Services.AddScoped<IFileAnalyzer, FileAnalyzer>();

var repositorySettings = builder.Configuration.GetSection("RepositorySettings");
bool useInMemory = repositorySettings.GetValue<bool>("UseInMemoryRepository");
if (useInMemory)
{
    builder.Services.AddSingleton<IDataRepository, InMemoryRepository>();
}
else
{
    builder.Services.AddScoped<IDataRepository, SQLRepository>();
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}


builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 1024 * 1024 * 1024; // 1GB
});

builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 1024 * 1024 * 1024;
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseCors("AllowReactApp");
app.MapControllers();

try
{
    Log.Information("Starting web application");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
