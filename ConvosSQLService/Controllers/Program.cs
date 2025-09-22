using controller.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Services;
using Services.impl;
using Services.Interfaces;
using Services.SignalR;
using Services.Tools;
using Services.Tool;
using Swashbuckle.AspNetCore.Filters;
using System.Text;
using BusinessObjects;
using System.Text.Json.Serialization;
using StackExchange.Redis;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Repositories.Interfaces;
using Repositories.impl;
using BusinessObjects.Models;
using BusinessObjects.Settings;




var builder = WebApplication.CreateBuilder(args);

// Confugration
var _configuration = builder.Configuration;

// Getting environment variable
var isdev = builder.Environment.IsDevelopment();

// Applying connection string
string conn;
string _redis;

if (isdev)
{
    Console.WriteLine("[INFO]: Running in development mode.");
    Console.WriteLine("Using Connection String: " + builder.Configuration.GetConnectionString("DefaultConnection"));
    _redis = builder.Configuration.GetConnectionString("Redis");
    Console.WriteLine("Using Redis Connection String: " + _redis);
    builder.Services.AddDbContext<ConvosDbContext>(opt =>
    {
        opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
            sql => sql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
    });

}

else
{
    conn = Environment.GetEnvironmentVariable("ConnectionString");

    Console.WriteLine("[INFO]: Running in production mode.");

    Console.WriteLine("Using Connection String: " + conn);

    _redis = Environment.GetEnvironmentVariable("Redis");
    Console.WriteLine("Using Redis Connection String: " + _redis);
    builder.Services.AddDbContext<ConvosDbContext>(options =>
     options.UseSqlServer(conn, sql => sql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

}



builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
    builder.Services.Configure<KeySetting>(builder.Configuration.GetSection("KeySetting"));


    // Add services to the container.
    builder.Services.AddControllers().AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = new HyphenNamingPolicy();
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
    });
    var firebaseConfig = builder.Configuration.GetSection("Firebase");

    FirebaseApp.Create(new AppOptions
    {
        Credential = GoogleCredential.FromFile(firebaseConfig["ServiceAccountKeyPath"])
    });


    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policyBuilder =>
        {
            policyBuilder

                .WithOrigins("http://localhost:3000", "http://127.0.0.1:5500", "http://127.0.0.1:5501", "http://192.168.100.203:3000", "https://convos-psi.vercel.app", "http://192.168.238.49:3000", "https://convos-psi.vercel.app")
                //.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });



    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = _redis;
        options.InstanceName = "OTPInstance:";
    });

    builder.Services.AddScoped<UserPasswordHasher>();
    builder.Services.AddScoped<TokenTool>();
    builder.Services.AddScoped<SmsService>();
    builder.Services.AddScoped<TokenTools>();
    builder.Services.AddScoped<IEmailService, EmailService>();
    builder.Services.AddScoped<IFriendshipService, FriendshipService>();
    builder.Services.AddScoped<IOTPService, OTPService>();
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
    builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
    builder.Services.AddScoped<IServerRepository, ServerRepository>();
    builder.Services.AddScoped<IChannelRepository, ChannelRepository>();
    builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
    builder.Services.AddScoped<ICategoryService, CategoryService>();
    builder.Services.AddScoped<IServerService, ServerService>();
    builder.Services.AddScoped<IChannelService, ChannelService>();
    builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
    builder.Services.AddScoped<IRoleRepository, RoleRepository>();
    builder.Services.AddScoped<IRoleService, RoleService>();
    builder.Services.AddScoped<IServerMemberRepository, ServerMemberRepository>();
    builder.Services.AddScoped<IServerMemberService, ServerMemberService>();
    builder.Services.AddScoped<IInviteRepository, InviteRepository>();
    builder.Services.AddScoped<IInviteService, InviteService>();
    builder.Services.AddScoped<IFriendShipRepository, FriendShipRepository>();
    builder.Services.AddScoped<IEmojiRepository, EmojiRepository>();
    builder.Services.AddScoped<IEmojiService, EmojiService>();
    builder.Services.AddScoped<IPermissionService, PermissionService>();
    builder.Services.AddScoped<IChannelRolePermissionService, ChannelRolePermissionService>();
    builder.Services.AddScoped<ISoundBoardService, SoundBoardService>();
    builder.Services.AddScoped<IChannelRolePermissionRepository, ChannelRolePermissionRepository>();
    builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();
    builder.Services.AddScoped<IFriendShipRepository, FriendShipRepository>();
    builder.Services.AddScoped<IInviteUsageService, InviteUsageService>();
    builder.Services.AddScoped<IInviteUsageRepository, InviteUsageRepository>();
    builder.Services.AddScoped<IMemberRoleRepository, MemberRoleRepository>();
    builder.Services.AddScoped<ISoundBoardRepository, SoundBoardRepository>();
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddScoped<IEventRepository, EventRepository>();
    builder.Services.AddScoped<IEventService, EventService>();
    builder.Services.AddScoped<IQuizRepository, QuizRepository>();
    builder.Services.AddScoped<IQuizService, QuizService>();


builder.Services.AddSingleton<FirebaseService>();

    // new feat: Quiz


    builder.Services.AddControllers().AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
    });

    builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    {
        var configuration = ConfigurationOptions.Parse(_redis, true);
        return ConnectionMultiplexer.Connect(configuration);
    });

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSignalR();
    builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        {
            Description = "Standard Authorization header using the Bearer scheme (\"{token}\")",
            In = ParameterLocation.Header,
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });
        options.SwaggerDoc("v1", new OpenApiInfo { Title = "ConvosSQLServiceAPI", Version = "v1" });
        options.OperationFilter<SecurityRequirementsOperationFilter>();
    });

    var secretKey = builder.Configuration["KeySetting:SecretKey"];
    var secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            // Validate token
            ValidateIssuer = false,
            ValidateAudience = false,
            // Validate token signing key
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(secretKeyBytes),
            ClockSkew = TimeSpan.Zero
        };
    });


    var app = builder.Build();


    app.UseSwagger();
    app.UseSwaggerUI();


    app.UseDeveloperExceptionPage();
    app.MigrationDB();
    app.UseHttpsRedirection();

    app.UseRouting();
    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHub<ServerHub>("/server-hub");
    app.MapHub<CategoryHub>("/category-hub");
    app.MapHub<InviteHub>("/invite-hub");
    app.MapHub<UserHub>("/user-hub");
    app.MapHub<RoleHubs>("/rolehubs");
    app.MapHub<VoiceChannelHubs>("/voicechannelhubs");

    app.Run();
