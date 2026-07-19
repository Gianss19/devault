using devault.Entities.Persistance;
using devault.Models;
using devault.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace devault.Entities.Configuration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Name)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(u => u.Email)
               .IsRequired()
               .HasMaxLength(100);

        builder.HasIndex(u => u.Email)
               .IsUnique();

        builder.Property(u => u.PasswordHash)
               .IsRequired();

        builder.Property(u => u.Rol)
               .HasConversion(new ValueConverter<Roles, string>(
                   v => v.ToString(),
                   v => ParseRole(v)
               ))
               .HasMaxLength(20)
               .IsRequired();

        builder.Property(u => u.CreatedAt)
               .IsRequired();

        builder.Property(u => u.UpdatedAt);

        builder.HasMany(u => u.Secrets)
               .WithOne(s => s.User)
               .HasForeignKey(s => s.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.RefreshTokens)
               .WithOne(r => r.User)
               .HasForeignKey(r => r.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }

    private static Roles ParseRole(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Roles.User;

        if (Enum.TryParse<Roles>(value, ignoreCase: true, out var result))
            return result;

        return Roles.User;
    }
}