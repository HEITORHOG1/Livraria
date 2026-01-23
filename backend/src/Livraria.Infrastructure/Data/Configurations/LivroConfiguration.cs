using Livraria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Livraria.Infrastructure.Data.Configurations;

/// <summary>
/// Configuração do Entity Framework para a entidade Livro.
/// </summary>
public class LivroConfiguration : IEntityTypeConfiguration<Livro>
{
    public void Configure(EntityTypeBuilder<Livro> builder)
    {
        builder.ToTable("Livro");
        builder.HasKey(l => l.CodL);

        builder.Property(l => l.CodL)
            .HasColumnName("CodL")
            .UseIdentityColumn();

        builder.Property(l => l.Titulo)
            .HasColumnName("Titulo")
            .HasMaxLength(40)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(l => l.Editora)
            .HasColumnName("Editora")
            .HasMaxLength(40)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(l => l.Edicao)
            .HasColumnName("Edicao")
            .IsRequired();

        builder.Property(l => l.AnoPublicacao)
            .HasColumnName("AnoPublicacao")
            .HasMaxLength(4)
            .IsUnicode(false)
            .IsRequired();
    }
}