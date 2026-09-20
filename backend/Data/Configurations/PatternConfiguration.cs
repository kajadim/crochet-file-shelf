using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using backend.Models;

namespace backend.Data.Configurations
{
    public class PatternConfiguration : IEntityTypeConfiguration<Pattern>
    {
        public void Configure(EntityTypeBuilder<Pattern> builder)
        {
            builder.HasKey(p => p.Id);

            builder.HasOne(p => p.Work)
                .WithOne(w => w.Pattern)
                .HasForeignKey<Pattern>(p => p.WorkId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(p => p.WorkId).IsUnique();
        }
    }
}
