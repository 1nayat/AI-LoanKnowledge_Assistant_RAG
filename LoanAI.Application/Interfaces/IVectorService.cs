using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanAI.Application.Interfaces
{
    public interface IVectorService
    {
            Task StoreVectorAsync(int id, float[] vector, string text);
            Task<List<string>> SearchVectorAsync(float[] queryVector, int topK=3);
    }
}
