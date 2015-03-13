using System;

namespace JSONAPI.ActionFilters
{
    internal class QueryableTransformException : Exception
    {
        public QueryableTransformException(string message)
            : base(message)
        {

        }
    }
}
