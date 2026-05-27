using Nasurino.SmartWallet.Services.Validators;
using Nasurino.SmartWallet.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Nasurino.SmartWallet.Services;
using Nasurino.SmartWallet.Services.AutoMappers;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Nasurino.SmartWallet.Common.Infrastructure.Contracts;
using Nasurino.SmartWallet.Common.Infrastructure;
using Nasurino.SmartWallet.AutoMappers;
using Nasurino.SmartWallet.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Nasurino.SmartWallet.Options;
using Nasurino.SmartWallet.Context.Repository;
using Nasurino.SmartWallet.Service.Infrastructure;
using Services.Contracts;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Service.Infrastructure.Contracts;
using Nasurino.SmartWallet.Context.Contracts;
using Nasurino.SmartWallet.Services.Contracts;
using Hangfire;
using Hangfire.PostgreSql;
using Nasurino.SmartWallet.Services.Contracts.BackgroundService;
using Nasurino.SmartWallet.Services.BackgroundJobs;

var builder = WebApplication.CreateBuilder(args);

if (args.Length > 0)
{
    var migrationIndex = Array.IndexOf(args, "-m");
    if (migrationIndex != -1)
    {
        var connectionIndex = migrationIndex + 1;
        if (connectionIndex < args.Length && !string.IsNullOrEmpty(args[connectionIndex]))
        {
            var options = new DbContextOptionsBuilder<SmartWalletContext>()
                .UseNpgsql(args[connectionIndex])
                .Options;
            await SmartWalletMigrator.MigrateAsync(options);
            return;
        }
        else
        {
            throw new ArgumentException("Ожидалась строка подключения после ключа -m, но ничего не найдено");
        }
    }
}

// Add services to the container.

builder.Services.AddDbContext<SmartWalletContext>(options => options
    .UseNpgsql(builder.Configuration.GetConnectionString("SmartWalletConnectionString")));

builder.Services.AddHangfire(conf => conf
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(c =>
        c.UseNpgsqlConnection(builder.Configuration.GetConnectionString("HangfireConnection"))));
builder.Services.AddHangfireServer();

builder.Services.AddControllers(x =>
{
    x.Filters.Add(typeof(SmartWalletExceptionFilter));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme,
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
			Type = SecuritySchemeType.Http,
            Scheme = JwtBearerDefaults.AuthenticationScheme,
			BearerFormat = "JWT",
			In = ParameterLocation.Header,
			Description = 
            "JWT Authorization header using the Bearer scheme. \r\n\r\n" +
	        "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
	        "Example: \"Bearer 12345abcdef\""
        });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
			new OpenApiSecurityScheme()
		    {
                Reference = new OpenApiReference()
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
				},
                Scheme = "oauth2",
                Name =  JwtBearerDefaults.AuthenticationScheme,
                In = ParameterLocation.Header
            },
            new List<string>()
		}
    });
});

var allowedOrigins = builder.Configuration.GetSection("ApiSettings:CqrsSettings:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins);
        policy.AllowAnyHeader();
        policy.AllowAnyMethod();
        policy.AllowCredentials();
    });
});

#region Регистрация классов конфигурации
builder.Services.Configure<JwtOptions>(builder.Configuration
    .GetSection("ApiSettings:JwtSettings"));
builder.Services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<JwtOptions>>().Value);
builder.Services.Configure<BCryptOptions>(builder.Configuration
    .GetSection("ApiSettings:BCryptSettings"));
#endregion

#region Регистрация сервисов
builder.Services.AddAutoMapper(typeof(ServiceModelMapper));
builder.Services.AddAutoMapper(typeof(ApiModelMapper));
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
	var jwtOptions = new JwtOptions();
	builder.Configuration.GetSection("ApiSettings:JwtSettings").Bind(jwtOptions);
    x.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
        ValidateIssuer = false,
        ValidateAudience = false,
    };
});

builder.Services.AddScoped<IIdentityProvider, ApiIdentityProvider>();
builder.Services.AddScoped<IFinancialCalculator, FinancialCalculator>();

builder.Services.AddScoped<IDataStorageContext, SmartWalletContext>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ITransactionEndpointRepository, TransactionEndpointRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

builder.Services.AddScoped<ITransactionEndpointService, TransactionEndpointService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IFinancialAnalyticsService, FinancialAnalyticsService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IJwtProvider, JwtProvider>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

builder.Services.AddScoped<ISmartWalletValidateService, SmartWalletValidateService>();
builder.Services.AddScoped<IClearCategoryCacheService, ClearCategoryCacheService>();
#endregion

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHangfireDashboard();
}
else
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseHttpsRedirection();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

#region Регистрация cron задач
using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider
        .GetRequiredService<IRecurringJobManager>();

    recurringJobManager.AddOrUpdate<IClearCategoryCacheService>(
        "clear-category-cache",
        service => service.ClearCategoryCacheAsync(),
        Cron.Monthly);
}
#endregion

app.Run();