using Microsoft.EntityFrameworkCore;
using nera_cji.Models;

namespace App.Core
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> users { get; set; }
        public DbSet<Event> events { get; set; }
        public DbSet<Department> departments { get; set; }
        public DbSet<Feedback> feedback { get; set; }
        public DbSet<EventParticipant> event_participants { get; set; }
        public DbSet<Notification> notifications { get; set; }
    }
}
