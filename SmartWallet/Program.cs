using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Nasurino.SmartWallet.Context;
using Nasurino.SmartWallet.Extensions;
using Nasurino.SmartWallet.Infrastructure;
using Nasurino.SmartWallet.Options;
using System.Text;

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
                    Name = JwtBearerDefaults.AuthenticationScheme,
                    In = ParameterLocation.Header
                },
                new List<string>()
            }
        });
});

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

var allowedOrigins = builder.Configuration
            .GetSection("ApiSettings:CqrsSettings:AllowedOrigins")
            .Get<string[]>() ?? [];

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

builder.Services.AddControllers(x =>
{
    x.Filters.Add(typeof(SmartWalletExceptionFilter));
});

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSmartWalletConfiguration(builder.Configuration);
builder.Services.AddSmartWalletServices();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.UseForwardedHeaders();

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

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok("healthy"));

app.Services.RegisterSmartWalletCronJobs();

app.Run();
