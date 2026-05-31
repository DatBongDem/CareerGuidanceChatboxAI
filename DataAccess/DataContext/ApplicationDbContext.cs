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

        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }

        public DbSet<ChatHistory> ChatHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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