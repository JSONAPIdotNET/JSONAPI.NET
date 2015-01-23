using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Threading;
using System.Threading.Tasks;

namespace JSONAPI.EntityFramework.Tests.Helpers
{
    internal class WaitsUntilCancellationDbAsyncEnumerator<T> : IDbAsyncEnumerator<T>
    {
        private readonly int _timeout;
        private readonly IEnumerator<T> _inner;

        public WaitsUntilCancellationDbAsyncEnumerator(int timeout, IEnumerator<T> inner)
        {
            _timeout = timeout;
            _inner = inner;
        }

        public void Dispose()
        {
            _inner.Dispose();
        }

        public async Task<bool> MoveNextAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(_timeout, cancellationToken);

            return _inner.MoveNext();
        }

        public T Current
        {
            get { return _inner.Current; }
        }

        object IDbAsyncEnumerator.Current
        {
            get { return Current; }
        }
    }
}
