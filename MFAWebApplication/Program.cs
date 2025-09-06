using AuthenticationWebApplication.Context;
using AuthenticationWebApplication.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Filters;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder
    .Services
    .AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition(
            "oauth2",
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Description =
                    "Standard Authorization Header Using The Bearer Scheme (\"bearer {token}\")",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Name = "Authorization",
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey
            }
        );
        ;
        options.OperationFilter<SecurityRequirementsOperationFilter>();
    });

// Endpoint Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("registerLimiter", opt =>
    {
        opt.PermitLimit = 5; // Max 5 registrations per minute
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 2; // Allows 2 extra attempts to queue
    });

    options.AddTokenBucketLimiter("loginLimiter", opt =>
    {
        opt.TokenLimit = 10;  // Max 10 login attempts at any time
        opt.TokensPerPeriod = 2; // Refill 2 tokens every 10 seconds
        opt.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
        opt.AutoReplenishment = true;
        opt.QueueLimit = 3;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    options.AddSlidingWindowLimiter("mfaLimiter", opt =>
    {
        opt.PermitLimit = 3; // Max 3 MFA attempts within 30 seconds
        opt.Window = TimeSpan.FromSeconds(30);
        opt.SegmentsPerWindow = 3; // MFA attempts spread over 3 segments (10 sec each)
        opt.QueueLimit = 1; // Only 1 additional attempt in queue
    });

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter("globalLimiter", _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 100, // Max 100 requests per minute per user
            Window = TimeSpan.FromMinutes(1)
        })
    );
});

// Chaching for Challange Codes
builder.Services.AddMemoryCache();


// JWT Token
builder
    .Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value)
            ),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

// Cross-Origin Resource Sharing
var allowSpecificOrigin = "_myAllowSpecificOrigins";

builder
    .Services
    .AddCors(options =>
    {
        options.AddPolicy(
            allowSpecificOrigin,
            builder =>
            {
                builder
                    .WithOrigins("http://localhost:4200")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            }
        );
    });




// PostGresDB
builder
    .Services
    .AddDbContext<ApplicationDbContext>(
        options =>
            options.UseNpgsql(
                builder.Configuration.GetConnectionString("PostgreSQL_Connection_String")
            )
    );


// Depedency Injections
builder.Services.AddScoped<IUserRepository, UserRepository>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseCors(allowSpecificOrigin);
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
