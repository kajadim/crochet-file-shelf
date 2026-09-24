using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using backend.Models;

namespace backend.Data.Configurations
{
    public class WorkCommentConfiguration : IEntityTypeConfiguration<WorkComment>
    {
        public void Configure(EntityTypeBuilder<WorkComment> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Text).IsRequired();
            builder.Property(c => c.PlainText).IsRequired();

            builder.HasOne(c => c.Work)
                .WithMany(w => w.Comments)
                .HasForeignKey(c => c.WorkId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(c => c.Author)
                .WithMany()
                .HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(c => c.WorkId);
        }
    }
}
