using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using FootballSquad.Core.Domain.RepositoryContracts;
using FootballSquad.Core.ServiceContracts;
using FootballSquad.Core.Services;
using FootballSquad.Infrastructure.Repositories;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace FootballSquad;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        //// Add Services
        builder.Services.AddLogging();

        builder.Configuration.AddEnvironmentVariables("footballsquad_");

        builder.Services.AddHttpClient("MyHttpClient", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // add services for controllers
        builder.Services.AddControllers(options =>
        {
            options.Filters.Add(new ProducesAttribute("application/json"));
            options.Filters.Add(new ConsumesAttribute("application/json"));

            var globalAuthorizationPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser().Build();

            options.Filters.Add(new AuthorizeFilter(globalAuthorizationPolicy));
        });


        // add api versioning service and configure routing for versions
        builder.Services.AddApiVersioning(options =>
        {
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV"; // v1
            options.SubstituteApiVersionInUrl = true; // don't make me set version api in swagger ui
        });
        // Configure ApiBehaviorOptions if needed

        // Adds JWT service which will be used in Authentication
        builder.Services.AddTransient<IJWTService, JWTService>();
        // Token services for generating token when resetting password with email
        builder.Services.AddTransient<ITokenService, TokenService>();
        // Not so reliable email service which uses Gmail with SMTP
        builder.Services.AddScoped<IEmailService, GmailEmailService>();

        builder.Services.AddScoped<IFootballerRepository, FootballerRepository>();
        builder.Services.AddTransient<IFootballerService, FootballerService>();

        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddTransient<IUserService, UserService>();

        builder.Services.AddScoped<ISquadRepository, SquadRepository>();
        builder.Services.AddTransient<ISquadService, SquadService>();

        // add services for OpenAPI
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            // generated xml file from source code comments
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo()
                { Title = "FootballSquad Web API", Version = "1.0" });
        });

        builder.Services.AddEndpointsApiExplorer();

        // add CORS services and configure it
        // you can configure with allowed origins, enabled headers and http methods
        builder.Services.AddCors(c =>
        {
            c.AddDefaultPolicy(options =>
            {
                options.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
            });
        });


        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateAudience = true,
                ValidAudience = builder.Configuration["Auth:Jwt:Audience"],
                ValidateIssuer = true,
                ValidIssuer = builder.Configuration["Auth:Jwt:Issuer"],
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration["JwtSecretKey"])),
                ClockSkew = TimeSpan.Zero
            };
        });

        builder.Services.AddAuthorization();

        // Configure generated URLs for aesthetics
        builder.Services.Configure<RouteOptions>(options =>
        {
            options.LowercaseQueryStrings = true;
            options.LowercaseUrls = true;
        });

        var app = builder.Build();

        //// Use Middlewares

        app.UseHsts(); // only use HTTPS

        app.UseHttpsRedirection();

        // No need for now
        // app.UseStaticFiles();

        // Use OpenAPI if 'Development'
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "1.0");
            }); // configure endpoints
        }

        app.UseRouting();

        app.UseCors();

        app.UseAuthentication();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}