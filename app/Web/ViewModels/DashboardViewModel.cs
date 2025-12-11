namespace nera_cji.ViewModels {
    using System;
    using System.Collections.Generic;
    using nera_cji.Models;

    public class DashboardViewModel {
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;

        public int TotalEvents { get; set; }
        public int UpcomingEvents { get; set; }
        public int MyRegistrations { get; set; }

        // Events the current user is registered for
        public List<Event> RegisteredEvents { get; set; } = new();

        // Recent Activity
        public Event? LastEventRegistered { get; set; }
        public Event? LastEventCreated { get; set; }
        public List<Notification> RecentNotifications { get; set; } = new();
    }
}
