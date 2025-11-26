using Microsoft.EntityFrameworkCore;
using nera_cji.Models;

namespace App.Core {
    public class ApplicationDbContext : DbContext {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) {
        }

        public DbSet<User> users { get; set; }
        public DbSet<Event> events { get; set; }
        public DbSet<Department> departments { get; set; }
        public DbSet<Feedback> feedback { get; set; }
        public DbSet<EventParticipant> event_participants { get; set; }
        public DbSet<Notification> notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedNever();
                entity.Property(e => e.FullName)
                    .HasColumnName("full_name");
                entity.Property(e => e.email)
                    .HasColumnName("email");
                entity.Property(e => e.password_hash)
                    .HasColumnName("password_hash");
                entity.Property(e => e.is_active)
                    .HasColumnName("is_active");
                entity.Property(e => e.department_id)
                    .HasColumnName("department_id");
                entity.Property(e => e.is_admin)
                    .HasColumnName("is_admin");
                entity.Property(e => e.created_at)
                    .HasColumnName("created_at");
            });

            modelBuilder.Entity<Event>(entity =>
            {
                entity.ToTable("events");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();
                entity.Property(e => e.CreatedBy)
                    .HasColumnName("created_by")
                    .IsRequired();
            });
        }
    }
}
