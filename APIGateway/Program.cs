using APIGateway.config;
using APIGateway.Singleton;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using System.IO;

    var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
    .WithMetrics(opt =>

        opt
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("APIGateway"))
            .AddMeter(builder.Configuration.GetValue<string>("OpenRemoteManageMeterName"))
            .AddAspNetCoreInstrumentation()
            .AddRuntimeInstrumentation()
            .AddProcessInstrumentation()
            .AddOtlpExporter(opts =>
            {
                opts.Endpoint = new Uri(builder.Configuration["Otel:Endpoint"]);
            })
    );

// Checking environment variables to handle development or production mode.
bool isDevelopment = builder.Environment.IsDevelopment();
    string pwd = Directory.GetCurrentDirectory();
    string noAuthFile = Path.Combine(pwd, "RouterConfig", "noAuth.json");
    string routesPath = Path.Combine(pwd, isDevelopment ? "development" : "production");
    Console.WriteLine(isDevelopment ? "/* Using development mode" : "/* Running in Production mode.");

// Configuring the application builder.
builder.Configuration
    .SetBasePath(pwd)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile(Path.Combine(routesPath, "ocelot.json"), optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

Console.WriteLine("The current ocelot configuration directory is: " + routesPath);

Console.WriteLine("The current noAuth path configuration directory is: " + noAuthFile);

Console.WriteLine("The current Indentity service URL is: " + builder.Configuration["IdentityServiceUrl"]);

// Register IdentityServiceUrl from configuration
builder.Services.Configure<IdentityServiceOptions>(options =>
    options.IdentityServiceUrl = builder.Configuration["IdentityServiceUrl"]);

string jsoncontent = File.ReadAllText(noAuthFile).Trim();
JObject data = JObject.Parse(jsoncontent);
JToken token = data["Routes"];
List<string> routes = new List<string>();

if (token.Type == JTokenType.Array)
    foreach (JToken child in token.Children())
        routes.Add(child.ToString());


builder.Services.AddSingleton<Store>(new Store
{
    IsDevelopment = isDevelopment,
    noAuthroutes = routes
});


// Registering services for Ocelot and other dependencies.
builder.Services.AddHttpClient(); // Register IHttpClientFactory
builder.Services.AddOcelot(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Cors", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://127.0.0.1:5500", "http://127.0.0.1:5501","https://convos-psi.vercel.app")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseRouting();
app.UseCors("Cors");

// Adding custom middleware directly, without DI registration
app.UseMiddleware<TokenValidationMiddleware>();

app.UseOcelot().Wait();
app.Run();
