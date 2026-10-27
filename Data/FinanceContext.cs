using FinanceAgent.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FinanceAgent.Data
{
    public class FinanceContext : IdentityDbContext<ApplicationUser>
    {
        public FinanceContext(DbContextOptions<FinanceContext> options) : base(options) { }

        // Set by UserDbContextFactory. Every user-owned query is filtered to this user, and with no
        // user set those queries return nothing, so one user's data can't leak to another.
        public string? CurrentUserId { get; set; }

        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<AgentAction> AgentActions => Set<AgentAction>();
        public DbSet<CategoryRule> CategoryRules => Set<CategoryRule>();
        public DbSet<ImportBatch> ImportBatches => Set<ImportBatch>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Identity tables

            // Matches the decimal(18,2) columns the first migration created
            modelBuilder.Entity<Transaction>().Property(t => t.Amount).HasPrecision(18, 2);
            modelBuilder.Entity<Category>().Property(c => c.MonthlyBudget).HasPrecision(18, 2);
            modelBuilder.Entity<Account>().Property(a => a.Balance).HasPrecision(18, 2);

            modelBuilder.Entity<Transaction>().HasIndex(t => new { t.AccountId, t.Date });
            modelBuilder.Entity<Category>().Property(c => c.Name).HasMaxLength(100);
            modelBuilder.Entity<Category>().HasIndex(c => new { c.UserId, c.Name }).IsUnique();
            modelBuilder.Entity<CategoryRule>().Property(r => r.Keyword).HasMaxLength(200);
            modelBuilder.Entity<AgentAction>().HasIndex(a => a.Timestamp);
            modelBuilder.Entity<ImportBatch>().Property(b => b.FileName).HasMaxLength(260);

            // Deleting a file's transactions is done explicitly by the service; NoAction avoids
            // a second cascade path into Transactions alongside the Account cascade
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.ImportBatch)
                .WithMany(b => b.Transactions)
                .HasForeignKey(t => t.ImportBatchId)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<ImportBatch>()
                .HasOne(b => b.Account)
                .WithMany()
                .HasForeignKey(b => b.AccountId)
                .OnDelete(DeleteBehavior.NoAction);

            ConfigureOwned<Account>(modelBuilder);
            ConfigureOwned<Category>(modelBuilder);
            ConfigureOwned<Transaction>(modelBuilder);
            ConfigureOwned<CategoryRule>(modelBuilder);
            ConfigureOwned<AgentAction>(modelBuilder);
            ConfigureOwned<ImportBatch>(modelBuilder);
        }

        private void ConfigureOwned<T>(ModelBuilder modelBuilder) where T : class, IUserOwned
        {
            // NoAction avoids SQL Server's "multiple cascade paths" error; users aren't deleted by the app
            modelBuilder.Entity<T>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<T>().HasQueryFilter(e => e.UserId != null && e.UserId == CurrentUserId);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            StampOwner();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            StampOwner();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void StampOwner()
        {
            foreach (var entry in ChangeTracker.Entries<IUserOwned>())
            {
                if (entry.State == EntityState.Added && string.IsNullOrEmpty(entry.Entity.UserId))
                {
                    entry.Entity.UserId = CurrentUserId
                        ?? throw new InvalidOperationException("Can't save user data without a signed-in user.");
                }
                else if (entry.State == EntityState.Modified && entry.Property(e => e.UserId).IsModified)
                {
                    throw new InvalidOperationException("Rows can't be moved between users.");
                }
            }
        }
    }
}
