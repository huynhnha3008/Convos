using System.Text.Json.Serialization;
using Amazon.S3;
using BusinessObject.SupportModels;
using Repository.DatabaseSettings;
using Repository.UnitOfWork;
using Service.AesEncryptionService;
using Service.ChannelMessageService;
using Service.EmojiService;
using Service.HubService.ChannelMessageHub;
using Service.HubService.PrivateMessageHub;
using Service.HubService.WebRTCHub;
using Service.MessageService;
using Service.NotificationService;
using Service.PrivateCallSessionService;
using Service.PrivateMessageService;
using Service.DocumentService;
using Service.WhiteboardService;
using Service.HubService.DocumentHub;
using Service.HubService.WhiteboardHub;

var builder = WebApplication.CreateBuilder(args);
DotNetEnv.Env.Load();
//Check current env
 bool isDevelopment = builder.Environment.IsDevelopment();
 string pwd = Directory.GetCurrentDirectory();
 
 if (isDevelopment)
 {
     Console.WriteLine("/* Using development mode");
     // Add configuration based on environment
     builder.Configuration
         .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
         .AddEnvironmentVariables();
     var DBConnectionString = builder.Configuration["ConnectionStrings:MongoDb"];
     Console.WriteLine("/* Currently using connection string:" + DBConnectionString);
     builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("ConnectionStrings"));
 
 }
 else
 {
     Console.WriteLine("/* Using production mode");
     // Add configuration based on environment
     builder.Configuration
         .AddJsonFile("appsettings.Production.json", optional: true, reloadOnChange: true)
         .AddEnvironmentVariables();
     var DBConnectionString = builder.Configuration["ConnectionStrings:MongoDb"];
     Console.WriteLine("/* Currently using connection string:" + DBConnectionString);
     builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection("ConnectionStrings"));
 }

builder.Services.AddSingleton<ConvosDbContext>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IChannelMessageService, ChannelMessageService>();
builder.Services.AddScoped<IEmojiService, EmojiService>();
builder.Services.AddScoped<IPrivateMessageService, PrivateMessageService>();
builder.Services.AddSingleton<IAesEncryptionService, AesEncryptionService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IPrivateCallSessionService, PrivateCallSessionService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IWhiteboardService, WhiteboardService>();
builder.Services.AddControllers();

builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonS3>();

builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = new HyphenNamingPolicy();
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
            });
builder.Services.AddCors(options =>
            {
                options.AddPolicy("NoSQLService", policyBuilder =>
                {
                    policyBuilder
                        .WithOrigins("http://localhost:3000", "http://127.0.0.1:5500", "http://127.0.0.1:5501", "http://192.168.100.203:3000", "https://convos-psi.vercel.app", "http://192.168.238.49:3000", "https://convos-psi.vercel.app")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });
builder.Services.AddCors(options =>
           {
               options.AddDefaultPolicy(builder =>
               {
                   builder.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost" || new Uri(origin).Host == "127.0.0.1" || new Uri(origin).Host == "192.168.100.203" || new Uri(origin).Host == "convos-psi.vercel.app" || new Uri(origin).Host == "192.168.238.49")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
               });
           });
builder.Services.AddSignalR();
builder.Services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.AddDebug();
});
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<PrivateMessageHub>("/private-message-hub");
app.MapHub<ChannelMessageHub>("/channel-message-hub");
app.MapHub<VoiceChatHub>("/voice-chat-hub");
app.MapHub<PrivateCallHub>("/webrtc-signal-hub");
app.MapHub<ChannelCallHub>("/channel-call-hub");
app.MapHub<DocumentHub>("/document-hub");
app.MapHub<WhiteboardHub>("/whiteboard-hub");
app.UseCors("NoSQLService");
app.MapControllers();
app.UseHttpsRedirection();

app.Run();

