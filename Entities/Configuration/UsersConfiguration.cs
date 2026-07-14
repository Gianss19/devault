using devalut.Entities.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace devalut.Entities.Configuration;

public class UsersConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        
        builder.HasKey(u => u.Id);

        builder.Property(u=>u.Name)
               .IsRequired()
               .HasMaxLength(100);
    
        builder.Property(u=> u.Email)
               .IsRequired()
               .HasMaxLength(100);

        builder.HasIndex(u=>u.Email).IsUnique();

        builder.Property(u=> u.PasswordHash)
               .IsRequired()
               .HasMaxLength(255);

        builder.Property(u=>u.CreatedAt)
               .IsRequired();

        builder.Property(u=>u.UpdatedAt);


    }

}
