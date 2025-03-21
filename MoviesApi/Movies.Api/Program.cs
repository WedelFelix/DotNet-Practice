using System.Text;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Movies.Api.Auth;
using Movies.Api.Endpoints;
using Movies.Api.Health;
using Movies.Api.Mapping;
using Movies.Application;
using Movies.Application.Database;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
    x.TokenValidationParameters = new TokenValidationParameters
    {
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:key"]!)),
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = config["Jwt:Issuer"],
        ValidAudience = config["Jwt:Audience"],
        ValidateIssuer = true,
        ValidateAudience = true
    });

builder.Services.AddAuthorization(x =>
{
    // x.AddPolicy(AuthConstants.AdminUserPolicyName, policy =>
    //     policy.RequireClaim(AuthConstants.AdminUserClaimName, "true"));

    x.AddPolicy(AuthConstants.AdminUserPolicyName,
        policy => policy.AddRequirements(new AdminAuthRequirement(config["ApiKey"]!)));
    x.AddPolicy(AuthConstants.TrustedMemberPolicyName,
        policy => policy.RequireAssertion(c =>
            c.User.HasClaim(m => m is { Type: AuthConstants.AdminUserClaimName, Value: "true" }) ||
            c.User.HasClaim(m => m is { Type: AuthConstants.TrustedMemberClaimName, Value: "true" })
        ));
});

builder.Services.AddScoped<ApiKeyAuthFilter>();

builder.Services.AddApiVersioning(x =>
{
    x.DefaultApiVersion = new ApiVersion(1.0);
    x.AssumeDefaultVersionWhenUnspecified = true;
    x.ReportApiVersions = true;
    x.ApiVersionReader = new MediaTypeApiVersionReader("api-version");
}).AddMvc().AddApiExplorer();

builder.Services.AddResponseCaching();
builder.Services.AddOutputCache(x =>
{
    x.AddBasePolicy(c => c.Cache());
    x.AddBasePolicy(c => c.Cache()
        .Expire(TimeSpan.FromMinutes(1))
        .SetVaryByQuery(["title", "year", "sortBy", "page", "pageSize"])
        .Tag("Movies"));
});
builder.Services.AddControllers();
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>(DatabaseHealthCheck.Name);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddDatabase(config["Database:ConnectionString"]!);

var app = builder.Build();

app.CreateApiVersionSet();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTheme(ScalarTheme.DeepSpace).Servers = [];
        options.WithPreferredScheme("Bearer")
            .WithHttpBearerAuthentication(bearer => { bearer.Token = "your-bearer-token"; });
    });
}

app.MapHealthChecks("_health");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
//app.UseCors(); // This needs to come before caching 
app.UseResponseCaching();
app.UseOutputCache();
app.UseMiddleware<ValidationMappingMiddleware>();
// app.MapControllers();
app.MapApiEndpoints();

var dbInitializer = app.Services.GetRequiredService<DbInitializer>();
await dbInitializer.InitializeAsync();

app.Run();