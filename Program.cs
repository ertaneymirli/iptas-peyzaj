using IptasPeyzajApi.Backend.BakimPlanlari.Helpers;
using IptasPeyzajApi.Backend.Kullanicilar.Helpers;
using IptasPeyzajApi.Backend.Musteriler.Helpers;
using IptasPeyzajApi.Backend.Personeller.Helpers;
using IptasPeyzajApi.Backend.Teklifler.Helpers;
using IptasPeyzajApi.Backend.MusteriKullanicilari.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using IptasPeyzajApi.Backend.Data;
using Microsoft.EntityFrameworkCore;



var builder = WebApplication.CreateBuilder(args);
string azureSqlConnection =
    builder.Configuration.GetConnectionString("AzureSql")
    ?? throw new InvalidOperationException(
        "ConnectionStrings:AzureSql ayarı bulunamadı.");

builder.Services.AddDbContext<IptasPeyzajDbContext>(options =>
    options.UseSqlServer(
        azureSqlConnection,
        sql => sql.EnableRetryOnFailure()));

builder.Services.AddScoped<MusteriHelper>();
builder.Services.AddScoped<BakimPlaniHelper>();
builder.Services.AddScoped<PersonelHelper>();
builder.Services.AddScoped<TeklifHelper>();
builder.Services.AddScoped<KullaniciHelper>();
builder.Services.AddScoped<MusteriKullanicisiHelper>();
builder.Services.AddSingleton<GoogleDriveStorage>();
builder.Services.AddControllers();
var jwtKey = builder.Configuration["JWT_KEY"]
    ?? Environment.GetEnvironmentVariable("JWT_KEY")
    ?? throw new InvalidOperationException("JWT_KEY ayarı bulunamadı.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
            )
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors("FrontendPolicy");
app.UseDefaultFiles();
app.UseStaticFiles();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
