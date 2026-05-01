using CondoNet.Engagement.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Engagement.Infrastructure.Persistence
{
    public class EngagementDbContext : DbContext
    {
        public EngagementDbContext(DbContextOptions<EngagementDbContext> options) : base(options) { }

        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Survey> Surveys { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<Quorum> Quorums { get; set; }
        public DbSet<DigitalSignature> DigitalSignatures { get; set; }
    }
}