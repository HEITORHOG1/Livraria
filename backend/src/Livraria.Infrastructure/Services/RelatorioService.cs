using Livraria.Application.Common.Interfaces;
using Livraria.Application.DTOs;
using Livraria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Livraria.Infrastructure.Services;

/// <summary>
/// Serviço para geração de relatórios.
/// Consulta a VIEW vw_Relatorio_Livros_Por_Autor e gera PDF com QuestPDF.
/// </summary>
public class RelatorioService : IRelatorioService
{
    private readonly ApplicationDbContext _context;

    public RelatorioService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RelatorioLivroDto>> GetDadosRelatorioAsync(CancellationToken ct = default)
    {
        // Consulta a VIEW ordenando por Autor e Titulo
        var dados = await _context.Database
            .SqlQuery<RelatorioLivroDto>($"SELECT CodAu, Autor, CodL, Titulo, Editora, Edicao, AnoPublicacao, Assuntos FROM vw_Relatorio_Livros_Por_Autor ORDER BY Autor, Titulo")
            .ToListAsync(ct);

        return dados;
    }

    /// <inheritdoc />
    public async Task<byte[]> GerarPdfAsync(int[]? autorIds = null, CancellationToken ct = default)
    {
        var dados = await GetDadosRelatorioAsync(ct);

        // Filtrar por autores se especificado
        if (autorIds != null && autorIds.Length > 0)
        {
            dados = dados.Where(d => autorIds.Contains(d.CodAu)).ToList();
        }

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));

                // Header
                page.Header().Element(c => ComposeHeader(c, autorIds != null && autorIds.Length > 0));

                // Content
                page.Content().Element(c => ComposeContent(c, dados));

                // Footer
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container, bool isFiltered)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item()
                    .Text("Relatório de Livros por Autor")
                    .SemiBold()
                    .FontSize(20)
                    .FontColor(Colors.Blue.Medium);

                var subtitle = $"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}";
                if (isFiltered)
                {
                    subtitle += " (Filtrado)";
                }

                column.Item()
                    .Text(subtitle)
                    .FontSize(9)
                    .FontColor(Colors.Grey.Medium);
            });
        });
    }

    private void ComposeContent(IContainer container, IEnumerable<RelatorioLivroDto> dados)
    {
        container.PaddingVertical(1, Unit.Centimetre).Column(column =>
        {
            var gruposPorAutor = dados.GroupBy(d => new { d.CodAu, d.Autor });

            foreach (var grupo in gruposPorAutor)
            {
                // Nome do autor como cabeçalho de seção
                column.Item()
                    .PaddingTop(10)
                    .Text(grupo.Key.Autor)
                    .Bold()
                    .FontSize(14)
                    .FontColor(Colors.Blue.Darken2);

                // Tabela de livros do autor
                column.Item().Table(table =>
                {
                    // Definição das colunas
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3); // Título
                        columns.RelativeColumn(2); // Editora
                        columns.ConstantColumn(50); // Edição
                        columns.ConstantColumn(50); // Ano
                        columns.RelativeColumn(2); // Assuntos
                    });

                    // Cabeçalho da tabela
                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("Título").Bold();
                        header.Cell().Element(CellStyle).Text("Editora").Bold();
                        header.Cell().Element(CellStyle).Text("Edição").Bold();
                        header.Cell().Element(CellStyle).Text("Ano").Bold();
                        header.Cell().Element(CellStyle).Text("Assuntos").Bold();

                        static IContainer CellStyle(IContainer container)
                        {
                            return container
                                .Background(Colors.Grey.Lighten2)
                                .Padding(5)
                                .DefaultTextStyle(x => x.SemiBold());
                        }
                    });

                    // Linhas da tabela
                    foreach (var livro in grupo)
                    {
                        table.Cell().Element(DataCellStyle).Text(livro.Titulo);
                        table.Cell().Element(DataCellStyle).Text(livro.Editora);
                        table.Cell().Element(DataCellStyle).Text(livro.Edicao.ToString());
                        table.Cell().Element(DataCellStyle).Text(livro.AnoPublicacao);
                        table.Cell().Element(DataCellStyle).Text(livro.Assuntos ?? "-");

                        static IContainer DataCellStyle(IContainer container)
                        {
                            return container
                                .BorderBottom(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .Padding(5);
                        }
                    }
                });

                column.Item().PaddingVertical(10);
            }

            // Mensagem se não houver dados
            if (!dados.Any())
            {
                column.Item()
                    .PaddingVertical(20)
                    .AlignCenter()
                    .Text("Nenhum livro cadastrado.")
                    .FontSize(12)
                    .FontColor(Colors.Grey.Medium);
            }
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(x =>
        {
            x.Span("Página ");
            x.CurrentPageNumber();
            x.Span(" de ");
            x.TotalPages();
        });
    }
}