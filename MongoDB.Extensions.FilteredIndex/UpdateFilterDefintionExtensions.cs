using MongoDB.Driver;
using System.Linq.Expressions;

namespace MongoDB.Extensions.FilteredIndex
{
    public static class UpdateFilterDefintionExtensions
    {
        public static UpdateDefinition<TSource> Set<TSource, TField>(this UpdateDefinitionBuilder<TSource> update,
            string identifier,
            Expression<Func<TSource, IEnumerable<TField>>> expression,
                TField value) => update.Set(new FilteredIndexFieldExpression<TSource, TField>(identifier, expression), value);
    }
}
