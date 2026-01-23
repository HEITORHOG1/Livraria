using Livraria.Domain.Entities;
using Livraria.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Livraria.Infrastructure.Data;

/// <summary>
/// Contexto do Entity Framework Core para a aplicação.
/// </summary>
public class ApplicationDbContext : DbContext, IUnitOfWork
{
    public DbSet<Livro> Livros => Set<Livro>();
    public DbSet<Autor> Autores => Set<Autor>();
    public DbSet<Assunto> Assuntos => Set<Assunto>();
    public DbSet<FormaCompra> FormasCompra => Set<FormaCompra>();
    public DbSet<LivroAutor> LivroAutores => Set<LivroAutor>();
    public DbSet<LivroAssunto> LivroAssuntos => Set<LivroAssunto>();
    public DbSet<LivroPreco> LivroPrecos => Set<LivroPreco>();

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}