using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using backend.Models;

namespace backend.Data.Configurations
{
    public class YarnColorConfiguration : IEntityTypeConfiguration<YarnColor>
    {
        public void Configure(EntityTypeBuilder<YarnColor> builder)
        {
            builder.HasKey(y => y.Id);
            builder.Property(y => y.Name).IsRequired().HasMaxLength(100);
            builder.Property(y => y.HexValue).IsRequired().HasMaxLength(7);

            builder.HasOne(y => y.Owner)
                .WithMany(u => u.YarnColors)
                .HasForeignKey(y => y.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(y => y.OwnerId);
        }
    }
}
