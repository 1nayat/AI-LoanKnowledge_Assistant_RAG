using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanAI.Application.Interfaces
{
    public interface IEmbeddingService
    {
            Task<float[]> GenerateEmbeddingAsync(string input);
    }
}
