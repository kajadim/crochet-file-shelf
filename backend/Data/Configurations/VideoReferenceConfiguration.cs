using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using backend.Models;

namespace backend.Data.Configurations
{
    public class VideoReferenceConfiguration : IEntityTypeConfiguration<VideoReference>
    {
        public void Configure(EntityTypeBuilder<VideoReference> builder)
        {
            builder.HasKey(v => v.Id);
            builder.Property(v => v.OriginalUrl).IsRequired();
            builder.Property(v => v.NormalizedUrl).IsRequired();

            builder.HasOne(v => v.Work)
                .WithOne(w => w.VideoReference)
                .HasForeignKey<VideoReference>(v => v.WorkId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(v => v.WorkId).IsUnique();
        }
    }
}
