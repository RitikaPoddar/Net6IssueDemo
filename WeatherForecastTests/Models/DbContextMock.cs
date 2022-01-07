using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace WeatherForecastTests.Models
{
    public static class DbContextMock
    {

        /// <summary>
        /// Return a DbSet of the specified generic type
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="sourceList">entity collection</param>
        /// <returns>Mock DbSet</returns>
        public static DbSet<T> GetQueryableMockDbSet<T>(List<T> sourceList) where T : class
        {
            var queryable = sourceList.AsQueryable();
            var dbSet = new Mock<DbSet<T>>();
            dbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            dbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
            dbSet.Setup(d => d.Add(It.IsAny<T>())).Callback<T>((s) => sourceList.Add(s));
            dbSet.Setup(d => d.Remove(It.IsAny<T>())).Callback<T>(s => sourceList.Remove(s));
            return dbSet.Object;
        }

        /// <summary>
        /// Return a DbSet of the specified generic type with support for async operations
        /// </summary>
        /// <typeparam name="T">EntityType</typeparam>
        /// <param name="sourceList">entity collection</param>
        /// <param name="data">data that needs to be added to entity collection in case of Create operation.</param>
        /// <returns>Mock DbSet with support of async operation.</returns>
        public static Mock<DbSet<T>> GetDbSet<T>(List<T> sourceList, T data = null) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();
            var testData = sourceList.AsQueryable();
            mockSet.As<IAsyncEnumerable<T>>().Setup(x => x.GetAsyncEnumerator(default)).Returns(new TestAsyncEnumerator<T>(testData.GetEnumerator()));
            mockSet.As<IQueryable<T>>().Setup(x => x.Provider).Returns(new TestAsyncQueryProvider<T>(testData.Provider));
            mockSet.As<IQueryable<T>>().Setup(x => x.Expression).Returns(testData.Expression);
            mockSet.As<IQueryable<T>>().Setup(x => x.ElementType).Returns(testData.ElementType);
            mockSet.As<IQueryable<T>>().Setup(x => x.GetEnumerator()).Returns(testData.GetEnumerator());

            var entry = new Mock<InternalEntityEntry>(new Mock<IStateManager>().Object,
                new EntityType(typeof(T), new Model(), true, ConfigurationSource.Explicit)).Object;
            var entityEntry = new Mock<EntityEntry<T>>(entry);
            entityEntry.SetupGet(self => self.Entity).Returns(data);
            mockSet.Setup(d => d.AddAsync(It.IsAny<T>(), default)).Returns(() => new ValueTask<EntityEntry<T>>(entityEntry.Object));
            mockSet.Setup(d => d.Remove(It.IsAny<T>())).Callback<T>(s => sourceList.Remove(s));
            return mockSet;
        }
    }

    /// <summary>
    /// Async query provider for unit testing
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        internal TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return new TestAsyncEnumerable<TEntity>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new TestAsyncEnumerable<TElement>(expression);
        }

        public object Execute(Expression expression)
        {
            return _inner.Execute(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return _inner.Execute<TResult>(expression);
        }

        public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
        {
            return new TestAsyncEnumerable<TResult>(expression);
        }

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            var expectedResultType = typeof(TResult).GetGenericArguments()[0];
            var executionResult = typeof(IQueryProvider)
                .GetMethods()
                .First(method => method.Name == nameof(IQueryProvider.Execute) && method.IsGenericMethod)
                .MakeGenericMethod(expectedResultType)
                .Invoke(this, new object[] { expression });

            return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))
                .MakeGenericMethod(expectedResultType)
                .Invoke(null, new[] { executionResult });
        }
    }

    /// <summary>
    /// Async enumerable for unit testing
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable)
            : base(enumerable)
        { }

        public TestAsyncEnumerable(Expression expression)
            : base(expression)
        { }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        IQueryProvider IQueryable.Provider
        {
            get { return new TestAsyncQueryProvider<T>(this); }
        }
    }

    /// <summary>
    /// Async enumerator for unit testing
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public void Dispose()
        {
            _inner.Dispose();
        }

        public T Current
        {
            get
            {
                return _inner.Current;
            }
        }

        public Task<bool> MoveNext(CancellationToken cancellationToken)
        {
            return Task.FromResult(_inner.MoveNext());
        }

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(_inner.MoveNext());
        }

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return new ValueTask();
        }
    }
}
