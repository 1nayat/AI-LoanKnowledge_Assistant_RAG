using LoanAI.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using UglyToad.PdfPig;
using System.Text;

namespace LoanAI.Infrastructure.Documents;

public class DocumentService : IDocumentService
{
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorService _vectorStore;

    public DocumentService(
        IEmbeddingService embeddingService,
        IVectorService vectorStore)
    {
        _embeddingService = embeddingService;
        _vectorStore = vectorStore;
    }

    public async Task ProcessDocumentsAsync(IFormFile file)
    {
        // 1️⃣ Save file to disk
        var documentsPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "Documents");

        if (!Directory.Exists(documentsPath))
            Directory.CreateDirectory(documentsPath);

        var filePath = Path.Combine(documentsPath, file.FileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // 2️⃣ Extract text from PDF
        var textBuilder = new StringBuilder();

        using (var document = PdfDocument.Open(filePath))
        {
            foreach (var page in document.GetPages())
            {
                textBuilder.AppendLine(page.Text);
            }
        }

        var fullText = textBuilder.ToString();

        // 3️⃣ Chunk text (simple paragraph-based chunking)
        var chunks = fullText
            .Split("\n\n", StringSplitOptions.RemoveEmptyEntries)
            .Where(c => c.Length > 100)
            .ToList();

        // 4️⃣ For each chunk → embed + store
        int chunkIndex = 0;

        foreach (var chunk in chunks)
        {
            var embedding = await _embeddingService.GenerateEmbeddingAsync(chunk);

            var id = Random.Shared.Next(1, int.MaxValue);

            await _vectorStore.StoreVectorAsync(
                id,
                embedding,
                chunk);

            chunkIndex++;
        }
    }
}