using LoanAI.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LoanAI.Infrastructure.VectorStore
{
    public class QdrantService : IVectorService
    {
        private readonly HttpClient _httpClient;
        private const string CollectionName = "loan_docs";
        public QdrantService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task StoreVectorAsync(int id, float[] vector, string text)
        {
            var request = new
            {
                points = new[]
                {
                    new
                    {
                        id = id,
                        vector = vector,
                        payload =new {
                            text = text
                        }
                    }
                }
            };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(
                $"/collections/{CollectionName}/points?wait=true",
                content);

            response.EnsureSuccessStatusCode();
        }

        public async Task<List<string>> SearchVectorAsync(float[] queryVector, int topK = 3)
        {
            var request = new
            {
                vector = queryVector,
                top = topK,
                with_payload = true
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                $"/collections/{CollectionName}/points/search",
                content);

            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseJson);

            var results = doc.RootElement
                .GetProperty("result")
                .EnumerateArray()
                .Select(r => r
                    .GetProperty("payload")
                    .GetProperty("text")
                    .GetString() ?? "")
                .ToList();

            return results;
        }
    }
}
