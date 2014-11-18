using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSONAPI.Core
{
    public interface IMaterializer
    {

        T GetById<T>(params Object[] keyValues);
        object GetById(Type type, params Object[] keyValues);
        T Materialize<T>(T ephemeral);
        object Materialize(Type type, object ephemeral);
        T MaterializeUpdate<T>(T ephemeral);
        object MaterializeUpdate(Type type, object ephemeral);
    }
}
