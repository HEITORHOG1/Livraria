using Livraria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Livraria.Infrastructure.Data.Configurations;

/// <summary>
/// Configuração do Entity Framework para a entidade Autor.
/// </summary>
public class AutorConfiguration : IEntityTypeConfiguration<Autor>
{
    public void Configure(EntityTypeBuilder<Autor> builder)
    {
        builder.ToTable("Autor");
        builder.HasKey(a => a.CodAu);

        builder.Property(a => a.CodAu)
            .HasColumnName("CodAu")
            .UseIdentityColumn();

        builder.Property(a => a.Nome)
            .HasColumnName("Nome")
            .HasMaxLength(40)
            .IsUnicode(false)
            .IsRequired();
    }
}