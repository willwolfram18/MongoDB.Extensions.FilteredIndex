using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MongoDB.Extensions.FilteredIndex
{
    internal class FilteredIndexFieldExpression<TSource, TField> : FieldDefinition<TSource, TField>
    {
        public FilteredIndexFieldExpression(string identifier, Expression<Func<TSource, IEnumerable<TField>>> expression)
        {
            Identifier = identifier;
            Expression = expression;
        }

        public string Identifier { get; }

        public Expression<Func<TSource, IEnumerable<TField>>> Expression { get; }

        public override RenderedFieldDefinition<TField> Render(IBsonSerializer<TSource> documentSerializer, IBsonSerializerRegistry serializerRegistry, LinqProvider linqProvider)
        {
            string fieldName = $"{GetFieldName(serializerRegistry)}.$[{Identifier}]";
            IBsonSerializer<TField> serializer = serializerRegistry.GetSerializer<TField>();

            return new RenderedFieldDefinition<TField>(fieldName, serializer, serializer, serializer);
        }

        private string GetFieldName(IBsonSerializerRegistry registry) => GetElementNameFor(registry, GetPropertyInfoForExpression());

        private PropertyInfo GetPropertyInfoForExpression()
        {
            if (Expression.Body is not MemberExpression member)
            {
                throw new InvalidOperationException("not a member expression");
            }

            if (member.Member is not PropertyInfo property)
            {
                throw new InvalidOperationException("not a property expression");
            }

            var type = typeof(TSource);
            if (type != property.ReflectedType && !type.IsSubclassOf(property.ReflectedType))
            {
                throw new InvalidOperationException("Not correct source type");
            }

            return property;
        }

        private string GetElementNameFor(IBsonSerializerRegistry registry, PropertyInfo propertyInfo)
        {
            if (registry.GetSerializer<TSource>() is BsonClassMapSerializer<TSource> serializer &&
                serializer.TryGetMemberSerializationInfo(propertyInfo.Name, out var serializerInfo))
            {
                return serializerInfo.ElementName;
            }

            BsonClassMap? classMap = BsonClassMap.LookupClassMap(typeof(TSource));
            if (classMap is null)
            {
                throw new InvalidOperationException($"Unable to find class map for {typeof(TSource)}");
            }

            BsonMemberMap? memberMap = classMap.GetMemberMap(propertyInfo.Name);
            if (memberMap is null)
            {
                throw new InvalidOperationException($"Unable to find member map for property {propertyInfo}");
            }

            return memberMap.ElementName;
        }
    }
}
