using devalut.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace devalut.Entities.Configuration;

public class SecretConfiguration : IEntityTypeConfiguration<Secret>
{
    public void Configure(EntityTypeBuilder<Secret> builder)
    {
        builder.ToTable("secrets");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(s => s.EncryptedValue)
               .IsRequired();

        builder.Property(s => s.CreatedAt)
               .IsRequired();

        builder.Property(s => s.UpdatedAt);

        builder.HasIndex(s => s.UserId);

        builder.HasIndex(s => new { s.UserId, s.Name })
               .IsUnique();

        builder.HasOne(s => s.User)
               .WithMany(u => u.Secrets)
               .HasForeignKey(s => s.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}