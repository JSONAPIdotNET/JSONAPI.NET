using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSONAPI.Core
{
    public interface IMaterializer
    {
        Task<T> GetByIdAsync<T>(params Object[] keyValues) where T : class;
        Task<T> MaterializeAsync<T>(T ephemeral) where T : class;
        Task<T> MaterializeUpdateAsync<T>(T ephemeral) where T : class;
    }
}
