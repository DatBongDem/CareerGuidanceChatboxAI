using DataAccess.Entities;
using DataAccess.Entities.ChatAI;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.DataContext
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Role> Roles { get; set; }

        public DbSet<Plan> Plans { get; set; }

        public DbSet<PlanHistory> PlanHistories { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public DbSet<EmailVerification> EmailVerifications { get; set; }


        public DbSet<University> Universities { get; set; }

        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }

        public DbSet<ChatHistory> ChatHistories { get; set; }

        public DbSet<QuestionCategory> QuestionCategories { get; set; }

        public DbSet<Question> Questions { get; set; }

        public DbSet<QuestionOption> QuestionOptions { get; set; }

        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Major> Majors { get; set; }
        public DbSet<Recommendation> Recommendations { get; set; }
        public DbSet<UserAnswer> UserAnswers { get; set; }

        public DbSet<Skill> Skills { get; set; }
        public DbSet<MajorSkill> MajorSkills { get; set; }





        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =========================
            //  Chat AI
            // =========================
            modelBuilder.Entity<QuestionCategory>()
                .HasMany(qc => qc.Questions)
                .WithOne(q => q.Category)
                .HasForeignKey(q => q.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Question>()
                .HasMany(q => q.Options)
                .WithOne(o => o.Question)
                .HasForeignKey(o => o.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            // =========================
            // User - Role
            // =========================
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // User - PlanHistory
            // =========================
            modelBuilder.Entity<PlanHistory>()
                .HasOne(ph => ph.User)
                .WithMany(u => u.PlanHistories)
                .HasForeignKey(ph => ph.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // =========================
            // Plan - PlanHistory
            // =========================
            modelBuilder.Entity<PlanHistory>()
                .HasOne(ph => ph.Plan)
                .WithMany() // Assuming Plan doesn't have a collection of PlanHistories
                .HasForeignKey(ph => ph.PlanId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // EmailVerification
            // =========================
            modelBuilder.Entity<EmailVerification>(entity =>
            {
                entity.ToTable("EmailVerifications");

                entity.Property(e => e.Id)
                    .HasColumnName("Id");

                entity.Property(e => e.Email)
                    .HasColumnName("Email");

                entity.Property(e => e.Otp)
                    .HasColumnName("Otp");

                entity.Property(e => e.VerifyToken)
                    .HasColumnName("VerifyToken");

                entity.Property(e => e.IsUsed)
                    .HasColumnName("IsUsed");

                entity.Property(e => e.ExpiredAt)
                    .HasColumnName("ExpiredAt");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("CreatedAt");

                entity.Property(e => e.TemporaryUserData)
                    .HasColumnName("TemporaryUserData");
            });
        }
    }
}