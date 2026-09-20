using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using backend.Models;

namespace backend.Data.Configurations
{
    public class PatternCellConfiguration : IEntityTypeConfiguration<PatternCell>
    {
        public void Configure(EntityTypeBuilder<PatternCell> builder)
        {
            builder.HasKey(c => c.Id);

            builder.HasOne(c => c.Pattern)
                .WithMany(p => p.Cells)
                .HasForeignKey(c => c.PatternId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(c => c.YarnColor)
                .WithMany(y => y.PatternCells)
                .HasForeignKey(c => c.YarnColorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(c => new { c.PatternId, c.RowIndex, c.ColumnIndex }).IsUnique();
        }
    }
}
