using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using backend.Models;

namespace backend.Data.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.HasKey(n => n.Id);
            builder.Property(n => n.Message).IsRequired();

            builder.HasOne(n => n.Recipient)
                .WithMany()
                .HasForeignKey(n => n.RecipientId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(n => n.Work)
                .WithMany()
                .HasForeignKey(n => n.WorkId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(n => new { n.RecipientId, n.IsRead });
        }
    }
}
