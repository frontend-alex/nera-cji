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
                    .ValueGeneratedOnAdd();
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
                entity.Property(e => e.Created_By)
                    .HasColumnName("created_by")
                    .IsRequired();
            });

            modelBuilder.Entity<EventParticipant>(entity =>
            {
                entity.ToTable("event_participants");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Event_Id)
                    .HasColumnName("event_id")
                    .IsRequired();

                entity.Property(e => e.User_Id)
                    .HasColumnName("user_id")
                    .IsRequired();

                entity.Property(e => e.Registered_At)
                    .HasColumnName("registered_at");

                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasMaxLength(50);

                entity.HasOne(e => e.Event)
                    .WithMany(ev => ev.Participants)
                    .HasForeignKey(e => e.Event_Id);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.EventRegistrations)
                    .HasForeignKey(e => e.User_Id);
            });

        }
    }
}
