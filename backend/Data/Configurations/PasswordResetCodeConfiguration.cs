using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using backend.Models;

namespace backend.Data.Configurations
{
    public class PasswordResetCodeConfiguration : IEntityTypeConfiguration<PasswordResetCode>
    {
        public void Configure(EntityTypeBuilder<PasswordResetCode> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.CodeHash).IsRequired();

            builder.HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(p => p.UserId);
        }
    }
}
