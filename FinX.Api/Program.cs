using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using FinX.Api.Services;
using MongoDB.Driver;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Insira 'Bearer {token}'"
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] { }
        }
    });
});

var mongoConn = builder.Configuration.GetSection("Mongo:ConnectionString").Value ?? "mongodb://localhost:27017";
var mongoDbName = builder.Configuration.GetSection("Mongo:Database").Value ?? "finxdb";
var client = new MongoClient(mongoConn);
var database = client.GetDatabase(mongoDbName);

builder.Services.AddSingleton<IMongoClient>(client);
builder.Services.AddSingleton(database);
builder.Services.AddScoped<IPatientService, MongoPatientService>();
builder.Services.AddScoped<IHospitalService, MongoHospitalService>();
builder.Services.AddScoped<IAgendamentoService, MongoAgendamentoService>();
builder.Services.AddScoped<IGrupoPacienteHospitalService, MongoGrupoPacienteHospitalService>();
builder.Services.AddScoped<IPatientUnificationService, PatientUnificationService>();
builder.Services.AddSingleton<IAuthService, AuthService>();
builder.Services.AddHttpClient();

builder.Services.AddSingleton<IKeyService, KeyService>();

var issuer = builder.Configuration["Jwt:Issuer"] ?? "FinXApi";
var urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "http://localhost:5006";
builder.WebHost.UseUrls(urls);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.Events = new JwtBearerEvents();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true
        };
    });

var app = builder.Build();

var jwtOptions = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<JwtBearerOptions>>();
var keys = app.Services.GetRequiredService<IKeyService>();
var pubRsa = keys.GetPublicKey();
var rsaKey = new Microsoft.IdentityModel.Tokens.RsaSecurityKey(pubRsa) { KeyId = keys.GetKeyId() };
jwtOptions.CurrentValue.TokenValidationParameters.IssuerSigningKey = rsaKey;

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();



app.Run();
