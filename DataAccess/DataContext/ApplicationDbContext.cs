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
        public DbSet<AiEvaluation> AiEvaluations { get; set; }
        public DbSet<UserAiSummary> UserAiSummaries { get; set; }
        public DbSet<ChatAiSession> ChatAiSessions { get; set; }
        public DbSet<ChatAiAnswer> ChatAiAnswers { get; set; }
        public DbSet<ChatAiSummary> ChatAiSummaries { get; set; }

        public DbSet<Skill> Skills { get; set; }
        public DbSet<MajorSkill> MajorSkills { get; set; }

        public DbSet<UniversityMajor> UniversityMajors { get; set; }

        public DbSet<AdmissionMethod> AdmissionMethods { get; set; }
        public DbSet<UniversityMajorMethod> UniversityMajorMethods { get; set; }

        public DbSet<EduRegistration> EduRegistrations { get; set; }
        public DbSet<EduActivationKey> EduActivationKeys { get; set; }
        public DbSet<OperationalExpense> OperationalExpenses { get; set; }
        public DbSet<DailyWebVisit> DailyWebVisits { get; set; }
        public DbSet<DailyUserVisit> DailyUserVisits { get; set; }

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

            modelBuilder.Entity<AiEvaluation>()
                .HasOne(ae => ae.Category)
                .WithMany()
                .HasForeignKey(ae => ae.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChatAiAnswer>()
                .HasOne(a => a.Session)
                .WithMany()
                .HasForeignKey(a => a.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChatAiAnswer>()
                .HasOne(a => a.Question)
                .WithMany()
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChatAiSummary>()
                .HasOne(s => s.Session)
                .WithMany()
                .HasForeignKey(s => s.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            // =========================
            // EduRegistration & EduActivationKey
            // =========================
            modelBuilder.Entity<EduRegistration>()
                .HasOne(er => er.Plan)
                .WithMany()
                .HasForeignKey(er => er.PlanId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EduActivationKey>()
                .HasOne(eak => eak.Registration)
                .WithMany()
                .HasForeignKey(eak => eak.RegistrationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EduActivationKey>()
                .HasOne(eak => eak.UsedByUser)
                .WithMany()
                .HasForeignKey(eak => eak.UsedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // DailyUserVisit
            // =========================
            modelBuilder.Entity<DailyUserVisit>()
                .HasOne(duv => duv.User)
                .WithMany()
                .HasForeignKey(duv => duv.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DailyUserVisit>()
                .HasIndex(duv => new { duv.Date, duv.UserId })
                .IsUnique();
        }
    }
}