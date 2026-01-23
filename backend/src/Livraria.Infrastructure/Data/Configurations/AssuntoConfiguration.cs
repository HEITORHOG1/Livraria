using Livraria.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Livraria.Infrastructure.Data.Configurations;

/// <summary>
/// Configuração do Entity Framework para a entidade Assunto.
/// </summary>
public class AssuntoConfiguration : IEntityTypeConfiguration<Assunto>
{
    public void Configure(EntityTypeBuilder<Assunto> builder)
    {
        builder.ToTable("Assunto");
        builder.HasKey(a => a.CodAs);

        builder.Property(a => a.CodAs)
            .HasColumnName("CodAs")
            .UseIdentityColumn();

        builder.Property(a => a.Descricao)
            .HasColumnName("Descricao")
            .HasMaxLength(20)
            .IsUnicode(false)
            .IsRequired();
    }
}