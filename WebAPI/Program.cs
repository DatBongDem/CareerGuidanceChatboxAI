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
using CloudinaryDotNet;
using BusinessLogic.Configurations;
using WebAPI.Hubs;

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
            builder.Services.Configure<PayOSSettings>(builder.Configuration.GetSection("PayOS")
);

            // =========================
            // DEPENDENCY INJECTION
            // =========================
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IPayOSService, PayOSService>();

            builder.Services.AddScoped<IUniversityRepository, UniversityRepository>();
            builder.Services.AddScoped<IUniversityService, UniversityService>();

            builder.Services.AddScoped<IUserService, UserService>();

            builder.Services.AddScoped<IRoleService, RoleService>();

            builder.Services.AddScoped<IPlanService, PlanService>();

            builder.Services.AddScoped<IEmailVerificationRepository,
                EmailVerificationRepository>();

            builder.Services.AddScoped<IAuthService, AuthService>();

            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IRoleRepository, RoleRepository>();
            builder.Services.AddScoped<IPlanRepository, PlanRepository>();
            builder.Services.AddScoped<IPlanHistoryRepository, PlanHistoryRepository>();
            builder.Services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();
            builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            builder.Services.AddScoped<IEmailVerificationRepository, EmailVerificationRepository>();
            builder.Services.AddScoped<IChatHistoryRepository, ChatHistoryRepository>();
            builder.Services.AddScoped<IQuestionCategoryRepository, QuestionCategoryRepository>();
            builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
            builder.Services.AddScoped<IQuestionOptionRepository, QuestionOptionRepository>();

            builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();
            builder.Services.AddScoped<IMajorRepository, MajorRepository>();
            //builder.Services.AddScoped<IUserAnswerRepository, UserAnswerRepository>();
            builder.Services.AddScoped<IRecommendationRepository, RecommendationRepository>();



            builder.Services.Configure<EmailSettings>(
                builder.Configuration.GetSection("EmailSettings")
            );
            builder.Services.AddScoped<IPaymentService, PaymentService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IRoleService, RoleService>();
            builder.Services.AddScoped<IPlanService, PlanService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IAvatarService, AvatarService>();
            builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
            builder.Services.AddScoped<IChatService, ChatService>();
            builder.Services.AddScoped<IQuestionCategoryService, QuestionCategoryService>();
            builder.Services.AddScoped<IQuestionService, QuestionService>();
            builder.Services.AddScoped<IQuestionOptionService, QuestionOptionService>();

            builder.Services.AddScoped<IUserProfileService, UserProfileService>();
            builder.Services.AddScoped<IMajorService, MajorService>();
            //builder.Services.AddScoped<IUserAnswerService, UserAnswerService>();
            builder.Services.AddScoped<IRecommendationService, RecommendationService>();


            builder.Services.AddHttpClient();

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

            // =========================
            // SIGNALR
            // =========================
            builder.Services.AddSignalR();

            // =========================
            // CONTROLLERS
            // =========================
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
            // Cloudinary
            // =========================
            builder.Services.AddSingleton(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();

                var account = new Account(
                    config["Cloudinary:CloudName"],
                    config["Cloudinary:ApiKey"],
                    config["Cloudinary:ApiSecret"]
                );

                return new Cloudinary(account);
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
            // SWAGGER
            // =========================
            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint(
                    "/swagger/v1/swagger.json",
                    "4S_BE API V1"
                );
            });

            // =========================
            // HTTPS
            // =========================
            if (!app.Environment.IsProduction())
            {
                app.UseHttpsRedirection();
            }

            // =========================
            // CORS
            // =========================
            app.UseCors("AllowFrontend");

            // =========================
            // AUTH
            // =========================
            app.UseAuthentication();

            app.UseAuthorization();

            // =========================
            // ROUTES
            // =========================
            app.MapControllers();
            app.MapHub<PaymentHub>("/payment-hub");

            app.MapGet("/", () => "4S_BE API Running");

            app.Run();
        }
    }
}