using LoanAI.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LoanAI.Infrastructure.AI
{
    public class OllamaEmbeddingService : IEmbeddingService
    {
        private readonly HttpClient _httpClient;
        public OllamaEmbeddingService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<float[]> GenerateEmbeddingAsync(string text)
        {
            var request = new
            {
                model = "nomic-embed-text",
                input = text
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/embed", content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseJson);

            var embeddings = doc.RootElement.GetProperty("embeddings");

            if (embeddings.GetArrayLength() == 0)
                throw new Exception("No embeddings returned from Ollama.");

            var vector = embeddings[0]
                .EnumerateArray()
                .Select(x => x.GetSingle())
                .ToArray();

            return vector;
        }
    }
}
