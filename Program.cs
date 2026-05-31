using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Google.Cloud.Firestore;
using IptasPeyzajApi.Backend.BakimPlanlari.Helpers;
using IptasPeyzajApi.Backend.Musteriler.Helpers;
using IptasPeyzajApi.Backend.Personeller.Helpers;
using IptasPeyzajApi.Backend.Teklifler.Helpers;



var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<MusteriHelper>();
builder.Services.AddScoped<BakimPlaniHelper>();
builder.Services.AddScoped<PersonelHelper>();
builder.Services.AddScoped<TeklifHelper>();
builder.Services.AddControllers();
var jwtKey = "iptas-peyzaj-cok-gizli-anahtar-2026";

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

string projectId = builder.Configuration["Firebase:ProjectId"] ?? "iptaspeyzaj";

// GOOGLE_APPLICATION_CREDENTIALS ortam değişkeni ile service account json dosyasını göstereceksin.
// Örnek: setx GOOGLE_APPLICATION_CREDENTIALS "C:\\firebase\\serviceAccountKey.json"
var firebaseKeyPath = Path.Combine(
    builder.Environment.ContentRootPath,
    "firebase-key.json"
);

Environment.SetEnvironmentVariable(
    "GOOGLE_APPLICATION_CREDENTIALS",
    firebaseKeyPath
);
builder.Services.AddSingleton(_ => FirestoreDb.Create("iptaspeyzaj"));


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
