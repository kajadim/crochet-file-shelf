using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using backend.Models;

namespace backend.Data.Configurations
{
    public class WorkConfiguration : IEntityTypeConfiguration<Work>
    {
        public void Configure(EntityTypeBuilder<Work> builder)
        {
            builder.HasKey(w => w.Id);
            builder.Property(w => w.Name).IsRequired().HasMaxLength(150);

            builder.HasOne(w => w.Owner)
                .WithMany(u => u.OwnedWorks)
                .HasForeignKey(w => w.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(w => w.Folder)
                .WithMany(f => f.Works)
                .HasForeignKey(w => w.FolderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(w => w.OwnerId);
            builder.HasIndex(w => w.FolderId);
        }
    }
}
