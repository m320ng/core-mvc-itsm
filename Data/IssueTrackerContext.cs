using System.Linq;
using IssueTracker.Entities;
using IssueTracker.Entities.Components;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Data {
    public class IssueTrackerContext : DbContext {
        public IssueTrackerContext(DbContextOptions<IssueTrackerContext> options) : base(options) { }

        public override int SaveChanges() {
            ChangeTracker.DetectChanges();

            var modified = ChangeTracker.Entries().Where(
                x => x.State == EntityState.Added ||
                x.State == EntityState.Modified ||
                x.State == EntityState.Deleted);

            return base.SaveChanges();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Issue>()
                .HasOne(x => x.AcceptIssueThread)
                .WithMany();

            modelBuilder.Entity<Issue>()
                .HasOne(x => x.CompleteIssueThread)
                .WithMany();

            modelBuilder.Entity<IssueCategory>()
                .HasOne(x => x.Parent)
                .WithMany(x => x.Childs);
            modelBuilder.Entity<IssueCategory>()
                .HasOne(x => x.IssueCategory1)
                .WithMany();
            modelBuilder.Entity<IssueCategory>()
                .HasOne(x => x.IssueCategory2)
                .WithMany();
            modelBuilder.Entity<IssueCategory>()
                .HasOne(x => x.IssueCategory3)
                .WithMany();
            modelBuilder.Entity<IssueCategory>()
                .HasOne(x => x.IssueCategory4)
                .WithMany();

            modelBuilder.Entity<Issue>()
                .OwnsOne(c => c.UploadFile);

            modelBuilder.Entity<IssueEmployee>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey("User_id");

            modelBuilder.Entity<IssueEmployee>()
                .HasOne(p => p.CreateBy)
                .WithMany()
                .HasForeignKey("CreateBy_id");
            modelBuilder.Entity<IssueEmployee>()
                .HasOne(p => p.UpdateBy)
                .WithMany()
                .HasForeignKey("UpdateBy_id");
        }

        public DbSet<User> User { get; set; }
        public DbSet<IssueEmployee> IssueEmployee { get; set; }
        public DbSet<IssueCategory> IssueCategory { get; set; }
        public DbSet<Issue> Issue { get; set; }
        public DbSet<IssueThread> IssueThread { get; set; }
    }
}