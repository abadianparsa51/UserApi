using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using UserApi.Configurations;
using UserApi.Data;
using UserApi.Models;
namespace YourNamespace.Configuration
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<JwtConfig>(Configuration.GetSection("JwtConfig"));

            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API Title", Version = "v1" });
                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "JWT Authorization header using the Bearer scheme.",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT"
                };
                c.AddSecurityDefinition("Bearer", securityScheme);
                var securityRequirement = new OpenApiSecurityRequirement
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
            new string[] { }
        }
    };
                c.AddSecurityRequirement(securityRequirement);
            });

            services.AddDefaultIdentity<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddEntityFrameworkStores<ApiDbContext>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(jwt =>
                {
                    var secretValue = Configuration["JwtConfig:Secret"];
                    if (string.IsNullOrEmpty(secretValue))
                    {
                        throw new InvalidOperationException("JwtConfig:Secret is missing or empty in the configuration.");
                    }
                    var key = Encoding.ASCII.GetBytes(secretValue);

                    jwt.RequireHttpsMetadata = false;
                    jwt.SaveToken = true;
                    jwt.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateAudience = false,
                        ValidateIssuer = false, // Set to true if you have a specific issuer to validate
                        RequireExpirationTime = true, // Set to true to validate token expiration
                        ValidateLifetime = true // Set to true to validate token lifetime
                    };
                });

            services.AddDbContext<ApiDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DevConnection")));

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder => builder
                        .WithOrigins("http://localhost:4200")
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API Title v1");
                });
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCors();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

    }
}
