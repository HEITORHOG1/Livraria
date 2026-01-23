using Livraria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Livraria.Infrastructure.Data.Configurations;

/// <summary>
/// Configuração do Entity Framework para a entidade de junção LivroAutor.
/// </summary>
public class LivroAutorConfiguration : IEntityTypeConfiguration<LivroAutor>
{
    public void Configure(EntityTypeBuilder<LivroAutor> builder)
    {
        builder.ToTable("Livro_Autor");
        builder.HasKey(la => new { la.Livro_CodL, la.Autor_CodAu });

        builder.Property(la => la.Livro_CodL).HasColumnName("Livro_CodL");
        builder.Property(la => la.Autor_CodAu).HasColumnName("Autor_CodAu");

        builder.HasOne(la => la.Livro)
            .WithMany(l => l.LivroAutores)
            .HasForeignKey(la => la.Livro_CodL)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(la => la.Autor)
            .WithMany(a => a.LivroAutores)
            .HasForeignKey(la => la.Autor_CodAu)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(la => la.Livro_CodL).HasDatabaseName("Livro_Autor_FKIndex1");
        builder.HasIndex(la => la.Autor_CodAu).HasDatabaseName("Livro_Autor_FKIndex2");
    }
}