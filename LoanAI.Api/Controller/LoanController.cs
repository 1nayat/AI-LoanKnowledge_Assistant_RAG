using LoanAI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoanAI.Api.Controllers;

//[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LoanController : ControllerBase
{
    private readonly IAIServices _aiService;
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorService _vectorService;

    public LoanController(IAIServices aiService, IEmbeddingService embeddingService, IVectorService vectorService)
    {
        _aiService = aiService;
        _embeddingService = embeddingService;
        _vectorService = vectorService;
    }

    [HttpPost("ask")]
    public async Task<IActionResult> Ask([FromBody] AskRequest request)
    {
        var result = await _aiService.AskAsync(request.Question);
        return Ok(result);
    }

    [HttpPost("embed")]
    public async Task<IActionResult> Embed([FromBody] AskRequest request)
    {
        var vector = await _embeddingService.GenerateEmbeddingAsync(request.Question);
        return Ok(vector.Length);
    }

    [HttpPost("store")]
    public async Task<IActionResult> Store([FromBody] AskRequest request)
    {
        var embedding = await _embeddingService.GenerateEmbeddingAsync(request.Question);

        var id = Random.Shared.Next(1,1_00_00);

        await _vectorService.StoreVectorAsync(id, embedding, request.Question);

        return Ok("Stored successfully");
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] AskRequest request)
    {
        var embedding = await _embeddingService.GenerateEmbeddingAsync(request.Question);

        var results = await _vectorService.SearchVectorAsync(embedding);

        return Ok(results);
    }

    
    [HttpPost("ask-rag")]
    public async Task<IActionResult> AskRag([FromBody] AskRequest request)
    {
        // 1️⃣ Generate embedding for user question
        var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(request.Question);

        // 2️⃣ Search similar chunks
        var similarChunks = await _vectorService.SearchVectorAsync(queryEmbedding);

        // 3️⃣ Combine chunks into context
        var context = string.Join("\n\n", similarChunks);

        // 4️⃣ Create grounded prompt
        var ragPrompt = $@"
You are a loan assistant. Answer ONLY from the provided context.

Context:
{context}

Question:
{request.Question}
";

        // 5️⃣ Ask LLM
        var answer = await _aiService.AskAsync(ragPrompt);

        return Ok(answer);
    }
}

public class AskRequest
{
    public string Question { get; set; } = string.Empty;
}