using LoanAI.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using UglyToad.PdfPig;
using System.Text;
using System.Text.RegularExpressions;

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

        var textBuilder = new StringBuilder();

        using (var document = PdfDocument.Open(filePath))
        {
            foreach (var page in document.GetPages())
            {
                var pageText = page.Text;

                pageText = Regex.Replace(pageText, @"\s+", " ");

                textBuilder.AppendLine(pageText);
            }
        }

        var fullText = textBuilder.ToString();

        int chunkSize = 500;   
        int overlap = 100;    

        var chunks = new List<string>();

        for (int i = 0; i < fullText.Length; i += chunkSize - overlap)
        {
            var length = Math.Min(chunkSize, fullText.Length - i);
            var chunk = fullText.Substring(i, length);

            if (!string.IsNullOrWhiteSpace(chunk))
            {
                chunks.Add(chunk);
            }
        }

        Console.WriteLine($"Total Chunks Created: {chunks.Count}");

        int chunkIndex = 0;

        foreach (var chunk in chunks)
        {
            Console.WriteLine($"--- Chunk {chunkIndex} ---");
            Console.WriteLine(chunk);

            var embedding = await _embeddingService.GenerateEmbeddingAsync(chunk);

            var id = Random.Shared.Next(1, int.MaxValue);

            await _vectorStore.StoreVectorAsync(
                id,
                embedding,
                chunk);

            chunkIndex++;
        }

        Console.WriteLine("Document processed and stored successfully.");
    }
}