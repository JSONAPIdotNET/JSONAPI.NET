using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JSONAPI.Core
{
    public interface IModelManager
    {
        PropertyInfo GetIdProperty(Type type);
    }
}
