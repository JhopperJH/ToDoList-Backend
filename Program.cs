using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ToDo.Models;
using DotNetEnv;

Env.Load(); // Load .env file

var builder = WebApplication.CreateBuilder(args);

// Add user secrets for secure configuration
// builder.Configuration.AddUserSecrets<Program>();

// Load database connection from .env
var connectionString = $"server={Env.GetString("DB_SERVER")};port={Env.GetString("DB_PORT")};user={Env.GetString("DB_USER")};password={Env.GetString("DB_PASSWORD")};database={Env.GetString("DB_DATABASE")};";

// Load JWT settings from .env
var jwtKey = Encoding.UTF8.GetBytes(Env.GetString("JWT_KEY") ?? throw new ArgumentNullException("JWT_KEY is missing."));
var jwtIssuer = Env.GetString("JWT_ISSUER");
var jwtAudience = Env.GetString("JWT_AUDIENCE");

builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;
// Configure Entity Framework with MySQL
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        IssuerSigningKey = new SymmetricSecurityKey(jwtKey),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Enable CORS (Adjust as needed)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// Register Controllers
builder.Services.AddControllers();

// Configure Swagger to support JWT Authentication
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ToDo API",
        Version = "v1"
    });

    // Add JWT Authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter 'Bearer {your JWT token}'",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Configure Middleware
// if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");           // Enable CORS
app.UseAuthentication();           // Enable JWT Authentication
app.UseAuthorization();            // Enable Authorization
app.MapControllers();              // Map API Controllers

app.Run();
