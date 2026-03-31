using LoanAI.Application.Interfaces;
using LoanAI.Infrastructure.AI;
using LoanAI.Infrastructure.Documents;
using LoanAI.Infrastructure.VectorStore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http; // Add this using directive
using System; // Add this using directive for Uri

namespace LoanAI.Infrastructure.DependencyInjection;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        var ollamaUrl = configuration["Ollama:BaseUrl"];
        var qdrantUrl = configuration["Qdrant:BaseUrl"];
        services.AddHttpClient<IAIServices, OllamaService>(client =>
        {
            client.BaseAddress = new Uri("http://localhost:11434");
            client.Timeout= TimeSpan.FromSeconds(500);
        });

        services.AddHttpClient<IEmbeddingService, OllamaEmbeddingService>(client =>
        {
            client.BaseAddress = new Uri(ollamaUrl);
            client.Timeout= TimeSpan.FromSeconds(500);
        });

        services.AddHttpClient<IVectorService, QdrantService>(client =>
        {
            client.BaseAddress = new Uri(qdrantUrl);
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        services.AddScoped<IDocumentService, DocumentService>();

        return services;
    }
}