using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace devalut.Entities.Configuration;

public class SecretsConfiguration : IEntityTypeConfiguration<Secret>
{
    public void Configure(EntityTypeBuilder<Secret> builder)
    {
        builder.ToTable("secrets");
        
        builder.HasKey(u=>u.Id);
        
        builder.Property(u=>u.Name)
               .IsRequired()
               .HasMaxLength(100);
        builder.Property(u=> u.EncryptedValue)
               .IsRequired()
               .HasMaxLength(255);
        builder.Property(u=>u.UserId)
               .IsRequired();
               
        builder.HasOne(u=>u.Usuario)
               .WithMany(u=>u.Secrets)
               .HasForeignKey(u=>u.UserId)
               .OnDelete(DeleteBehavior.Cascade);

    }

}
