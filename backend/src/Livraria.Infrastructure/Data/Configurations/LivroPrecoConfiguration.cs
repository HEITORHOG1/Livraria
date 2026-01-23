using Livraria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Livraria.Infrastructure.Data.Configurations;

/// <summary>
/// Configuração do Entity Framework para a entidade de junção LivroPreco.
/// </summary>
public class LivroPrecoConfiguration : IEntityTypeConfiguration<LivroPreco>
{
    public void Configure(EntityTypeBuilder<LivroPreco> builder)
    {
        builder.ToTable("Livro_Preco");
        builder.HasKey(lp => new { lp.Livro_CodL, lp.FormaCompra_CodFc });

        builder.Property(lp => lp.Livro_CodL).HasColumnName("Livro_CodL");
        builder.Property(lp => lp.FormaCompra_CodFc).HasColumnName("FormaCompra_CodFc");
        builder.Property(lp => lp.Valor).HasColumnName("Valor").HasColumnType("decimal(18,2)");

        builder.HasOne(lp => lp.Livro)
            .WithMany(l => l.LivroPrecos)
            .HasForeignKey(lp => lp.Livro_CodL)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(lp => lp.FormaCompra)
            .WithMany(f => f.LivroPrecos)
            .HasForeignKey(lp => lp.FormaCompra_CodFc)
            .OnDelete(DeleteBehavior.Cascade);
    }
}