using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Livraria.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateViewRelatorioLivrosPorAutor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE OR ALTER VIEW vw_Relatorio_Livros_Por_Autor AS
                SELECT
                    a.CodAu,
                    a.Nome AS Autor,
                    l.CodL,
                    l.Titulo,
                    l.Editora,
                    l.Edicao,
                    l.AnoPublicacao,
                    STRING_AGG(ass.Descricao, ', ') WITHIN GROUP (ORDER BY ass.Descricao) AS Assuntos
                FROM Autor a
                INNER JOIN Livro_Autor la ON a.CodAu = la.Autor_CodAu
                INNER JOIN Livro l ON la.Livro_CodL = l.CodL
                LEFT JOIN Livro_Assunto las ON l.CodL = las.Livro_CodL
                LEFT JOIN Assunto ass ON las.Assunto_CodAs = ass.CodAs
                GROUP BY a.CodAu, a.Nome, l.CodL, l.Titulo, l.Editora, l.Edicao, l.AnoPublicacao
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS vw_Relatorio_Livros_Por_Autor");
        }
    }
}