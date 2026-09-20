using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using backend.Models;

namespace backend.Data.Configurations
{
    public class WorkMemberConfiguration : IEntityTypeConfiguration<WorkMember>
    {
        public void Configure(EntityTypeBuilder<WorkMember> builder)
        {
            builder.HasKey(m => m.Id);

            builder.HasOne(m => m.Work)
                .WithMany(w => w.Members)
                .HasForeignKey(m => m.WorkId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(m => new { m.WorkId, m.UserId }).IsUnique();
        }
    }
}
