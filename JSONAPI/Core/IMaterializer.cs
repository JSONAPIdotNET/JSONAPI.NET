using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSONAPI.Core
{
    public interface IMaterializer
    {
        Task<T> GetByIdAsync<T>(params Object[] keyValues);
        Task<object> GetByIdAsync(Type type, params Object[] keyValues);
        Task<T> MaterializeAsync<T>(T ephemeral);
        Task<object> MaterializeAsync(Type type, object ephemeral);
        Task<T> MaterializeUpdateAsync<T>(T ephemeral);
        Task<object> MaterializeUpdateAsync(Type type, object ephemeral);

        [Obsolete]T GetById<T>(params Object[] keyValues);
        [Obsolete]object GetById(Type type, params Object[] keyValues);
        [Obsolete]T Materialize<T>(T ephemeral);
        [Obsolete]object Materialize(Type type, object ephemeral);
        [Obsolete]T MaterializeUpdate<T>(T ephemeral);
        [Obsolete]object MaterializeUpdate(Type type, object ephemeral);
    }
}
