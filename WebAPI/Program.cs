using BusinessLogic;
using BusinessLogic.DTOs.Email;
using BusinessLogic.Interfaces;
using BusinessLogic.Services;
using DataAccess.DataContext;
using DataAccess.Interfaces;
using DataAccess.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // =========================
            // DB CONTEXT
            // =========================
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(
                    builder.Configuration.GetConnectionString("DefaultConnection")
                )
            );

            // =========================
            // AUTO MAPPER
            // =========================
            builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

            // =========================
            // DEPENDENCY INJECTION
            // =========================
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            builder.Services.AddScoped<IUserService, UserService>();

            builder.Services.AddScoped<IRoleService, RoleService>();

            builder.Services.AddScoped<IPlanService, PlanService>();

            builder.Services.AddScoped<IEmailVerificationRepository,
                EmailVerificationRepository>();

            builder.Services.AddScoped<IAuthService, AuthService>();

            builder.Services.Configure<EmailSettings>(
                builder.Configuration.GetSection("EmailSettings")
            );

            builder.Services.AddScoped<IEmailService, EmailService>();

            builder.Services.AddScoped<IEmailTemplateService,
                EmailTemplateService>();

            // =========================
            // CORS
            // =========================
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend",
                    policy =>
                    {
                        policy
                            .WithOrigins(
                                "http://localhost:5173",
                                "http://localhost:5174",
                                "https://4s-company.vercel.app"
                            )
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                    });
            });

            builder.Services.AddControllers();

            // =========================
            // JWT AUTHENTICATION
            // =========================
            builder.Services
                .AddAuthentication(
                    JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuer = true,

                            ValidateAudience = true,

                            ValidateLifetime = true,

                            ValidateIssuerSigningKey = true,

                            ValidIssuer =
                                builder.Configuration["Jwt:Issuer"],

                            ValidAudience =
                                builder.Configuration["Jwt:Audience"],

                            IssuerSigningKey =
                                new SymmetricSecurityKey(
                                    Encoding.UTF8.GetBytes(
                                        builder.Configuration["Jwt:Key"]!
                                    )
                                ),

                            ClockSkew = TimeSpan.Zero
                        };
                });

            // =========================
            // SWAGGER + JWT SUPPORT
            // =========================
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "4S_BE API",
                    Version = "v1"
                });

                c.AddSecurityDefinition("Bearer",
                    new OpenApiSecurityScheme
                    {
                        Name = "Authorization",

                        Type = SecuritySchemeType.Http,

                        Scheme = "bearer",

                        BearerFormat = "JWT",

                        In = ParameterLocation.Header,

                        Description =
                            "Enter JWT token like this: Bearer {token}"
                    });

                c.AddSecurityRequirement(
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference =
                                    new OpenApiReference
                                    {
                                        Type =
                                            ReferenceType.SecurityScheme,

                                        Id = "Bearer"
                                    }
                            },
                            Array.Empty<string>()
                        }
                    });
            });

            var app = builder.Build();

            // =========================
            // MIDDLEWARE PIPELINE
            // =========================
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();

                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint(
                        "/swagger/v1/swagger.json",
                        "4S_BE API V1"
                    );
                });
            }

            app.UseHttpsRedirection();

            // =========================
            // CORS
            // =========================
            app.UseCors("AllowFrontend");

            // =========================
            // AUTH
            // =========================
            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}