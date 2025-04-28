using Nasurino.SmartWallet.Services.Validators;
using Nasurino.SmartWallet.Infrastructure;
using Nasurino.SmartWallet.Options;
using Nasurino.SmartWallet.Service.Models.Mappings;
using Nasurino.SmartWallet.Context.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// [TODO] �������� ������������ ��� ��������� ��

builder.Services.AddControllers(x =>
{
    x.Filters.Add(typeof(SmartWalletExceptionFilter));
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region ����������� ������� ������������
builder.Services.Configure<JwtOptions>(builder.Configuration
    .GetSection("ApiSettings")
    .GetSection("JwtSettings"));
#endregion

#region ����������� �������� � ����������
builder.Services.AddAutoMapper(typeof(ServiceModelMapper));

builder.Services.AddTransient<UnitOfWork>();

builder.Services.AddScoped<CashVaultRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<SpendingAreaRepository>();
builder.Services.AddScoped<TransactionRepository>();

builder.Services.AddSingleton<SmartWalletValidateService>();
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