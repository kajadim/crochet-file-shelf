using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using backend.Models;

namespace backend.Data.Configurations
{
    public class WorkInvitationConfiguration : IEntityTypeConfiguration<WorkInvitation>
    {
        public void Configure(EntityTypeBuilder<WorkInvitation> builder)
        {
            builder.HasKey(i => i.Id);
            builder.Property(i => i.Code).IsRequired().HasMaxLength(20);

            builder.HasOne(i => i.Work)
                .WithOne(w => w.Invitation)
                .HasForeignKey<WorkInvitation>(i => i.WorkId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(i => i.WorkId).IsUnique();
            builder.HasIndex(i => i.Code).IsUnique();
        }
    }
}
