using CondoNet.Engagement.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CondoNet.Engagement.Infrastructure.Persistence
{
    public class EngagementDbContext(DbContextOptions<EngagementDbContext> options) : DbContext(options)
    {
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Survey> Surveys { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<Quorum> Quorums { get; set; }
        public DbSet<DigitalSignature> DigitalSignatures { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Vote: relación con VotingSession (a futuro, aquí solo VotingSessionId)
            modelBuilder.Entity<Vote>()
                .HasIndex(v => new { v.VotingSessionId, v.VoterId })
                .IsUnique(); // Un voto por usuario por sesión

            // DigitalSignature: relación con Documento (a futuro, aquí solo DocumentId)
            modelBuilder.Entity<DigitalSignature>()
                .HasIndex(ds => new { ds.DocumentId, ds.UserId })
                .IsUnique(); // Una firma por usuario por documento

            // Quorum: relación con Meeting (a futuro, aquí solo MeetingId)
            modelBuilder.Entity<Quorum>()
                .HasIndex(q => new { q.MeetingId, q.RecordedAt });

            // Notification: relación con Recipient (a futuro, aquí solo RecipientId)
            modelBuilder.Entity<Notification>()
                .HasIndex(n => n.RecipientId);
        }
    }
}