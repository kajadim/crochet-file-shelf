using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using backend.Models;

namespace backend.Data.Configurations
{
    public class SiteReferenceConfiguration : IEntityTypeConfiguration<SiteReference>
    {
        public void Configure(EntityTypeBuilder<SiteReference> builder)
        {
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Url).IsRequired().HasMaxLength(2048);

            builder.HasOne(s => s.Work)
                .WithOne(w => w.SiteReference)
                .HasForeignKey<SiteReference>(s => s.WorkId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(s => s.WorkId).IsUnique();
        }
    }
}
