using Livraria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Livraria.Infrastructure.Data.Configurations;

/// <summary>
/// Configuração do Entity Framework para a entidade FormaCompra.
/// </summary>
public class FormaCompraConfiguration : IEntityTypeConfiguration<FormaCompra>
{
    public void Configure(EntityTypeBuilder<FormaCompra> builder)
    {
        builder.ToTable("FormaCompra");
        builder.HasKey(f => f.CodFc);

        builder.Property(f => f.CodFc)
            .HasColumnName("CodFc")
            .UseIdentityColumn();

        builder.Property(f => f.Descricao)
            .HasColumnName("Descricao")
            .HasMaxLength(20)
            .IsUnicode(false)
            .IsRequired();
    }
}