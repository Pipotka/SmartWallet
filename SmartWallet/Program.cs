using Nasurino.SmartWallet.Services.Validators;
using Nasurino.SmartWallet.Infrastructure;
using Nasurino.SmartWallet.Options;
using Nasurino.SmartWallet.Service.Models.Mappings;
using Nasurino.SmartWallet.Context.Repository;
using Nasurino.SmartWallet.Context;
using Microsoft.EntityFrameworkCore;
using Nasurino.SmartWallet.Service.Infrastructure;
using Nasurino.SmartWallet.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<SmartWalletContext>(options => options
    .UseNpgsql(builder.Configuration.GetValue<string>("ApiSettings:ConnectionString")));

builder.Services.AddControllers(x =>
{
    x.Filters.Add(typeof(SmartWalletExceptionFilter));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region Регистрация классов конфигурации
builder.Services.Configure<JwtOptions>(builder.Configuration
    .GetSection("ApiSettings:JwtSettings"));
#endregion

#region Регистрация сервисов в контейнере
builder.Services.AddAutoMapper(typeof(ServiceModelMapper));

builder.Services.AddTransient<UnitOfWork>();

builder.Services.AddScoped<CashVaultRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<SpendingAreaRepository>();
builder.Services.AddScoped<TransactionRepository>();
builder.Services.AddScoped<CashVaultService>();
builder.Services.AddScoped<SpendingAreaService>();
builder.Services.AddScoped<TransactionService>();
builder.Services.AddScoped<UserService>();

builder.Services.AddSingleton<SmartWalletValidateService>();
builder.Services.AddSingleton<JwtProvider>();
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();