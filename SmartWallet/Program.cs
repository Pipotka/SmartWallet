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
using Nasurino.SmartWallet.Options;
using Nasurino.SmartWallet.Context.Repository;
using Nasurino.SmartWallet.Service.Infrastructure;
using Services.Contracts;
using Nasurino.SmartWallet.Context.Repository.Contracts;
using Service.Infrastructure.Contracts;
using Nasurino.SmartWallet.Context.Contracts;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<SmartWalletContext>(options => options
    .UseNpgsql(builder.Configuration.GetConnectionString("SmartWalletConnectionString")));

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

#region Регистрация классов конфигурации
builder.Services.Configure<JwtOptions>(builder.Configuration
    .GetSection("ApiSettings:JwtSettings"));
#endregion

#region Регистрация сервисов в контейнере
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

builder.Services.AddScoped<IDataStorageContext, SmartWalletContext>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ICashVaultRepository, CashVaultRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISpendingAreaRepository, SpendingAreaRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

builder.Services.AddScoped<ICashVaultService, CashVaultService>();
builder.Services.AddScoped<ISpendingAreaService, SpendingAreaService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IFinancialAnalyticsService, FinancialAnalyticsService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IJwtProvider, JwtProvider>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

builder.Services.AddScoped<ISmartWalletValidateService, SmartWalletValidateService>();
#endregion

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("SmartWalletCorsPolicy");
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();