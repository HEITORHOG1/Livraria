using Livraria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Livraria.Infrastructure.Data.Configurations;

/// <summary>
/// Configuração do Entity Framework para a entidade de junção LivroAssunto.
/// </summary>
public class LivroAssuntoConfiguration : IEntityTypeConfiguration<LivroAssunto>
{
    public void Configure(EntityTypeBuilder<LivroAssunto> builder)
    {
        builder.ToTable("Livro_Assunto");
        builder.HasKey(la => new { la.Livro_CodL, la.Assunto_CodAs });

        builder.Property(la => la.Livro_CodL).HasColumnName("Livro_CodL");
        builder.Property(la => la.Assunto_CodAs).HasColumnName("Assunto_CodAs");

        builder.HasOne(la => la.Livro)
            .WithMany(l => l.LivroAssuntos)
            .HasForeignKey(la => la.Livro_CodL)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(la => la.Assunto)
            .WithMany(a => a.LivroAssuntos)
            .HasForeignKey(la => la.Assunto_CodAs)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(la => la.Livro_CodL).HasDatabaseName("Livro_Assunto_FKIndex1");
        builder.HasIndex(la => la.Assunto_CodAs).HasDatabaseName("Livro_Assunto_FKIndex2");
    }
}