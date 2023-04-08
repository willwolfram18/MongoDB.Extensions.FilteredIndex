using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.Linq.Expressions;
using System.Reflection;

namespace MongoDB.Extensions.FilteredIndex
{
    public static class FilterDefinitionBuilderExtensions
    {
        public static ArrayFilterDefinition<TSource> ItemEq<TSource>(this FilterDefinitionBuilder<TSource> builder,
            string identifier,
            TSource value)
        {
            IBsonSerializer<TSource> valueSerializer = BsonSerializer.LookupSerializer<TSource>();
            var document = new BsonDocument();
            using (var writer = new BsonDocumentWriter(document))
            {
                var context = BsonSerializationContext.CreateRoot(writer);
                writer.WriteStartDocument();
                writer.WriteName(identifier);
                valueSerializer.Serialize(context, value);
                writer.WriteEndDocument();
            }

            return document;
        }

        public static ArrayFilterDefinition<TSource> ItemGte<TSource>(this FilterDefinitionBuilder<TSource> builder,
            string identifier,
            TSource value)
        {
            IBsonSerializer<TSource> valueSerializer = BsonSerializer.LookupSerializer<TSource>();
            var document = new BsonDocument();
            using (var writer = new BsonDocumentWriter(document))
            {
                var context = BsonSerializationContext.CreateRoot(writer);
                writer.WriteStartDocument();
                writer.WriteName(identifier);
                writer.WriteStartDocument();
                writer.WriteName("$gte");
                valueSerializer.Serialize(context, value);
                writer.WriteEndDocument();
                writer.WriteEndDocument();
            }

            return document;
        }

        public static ArrayFilterDefinition ItemEq<TSource, TField>(this FilterDefinitionBuilder<TSource> builder,
            string identifier,
            Expression<Func<TSource, TField>> expression,
            TField value)
            where TSource : class
        {
            string fieldName = GetFieldNameForExpression(expression, BsonSerializer.SerializerRegistry);
            return Builders<TField>.Filter.ItemEq<TField>($"{identifier}.{fieldName}", value);
        }

        private static string GetFieldNameForExpression<TSource, TValue>(Expression<Func<TSource, TValue>> expression, IBsonSerializerRegistry register) =>
            GetElementNameFor<TSource>(register, GetPropertyInfoForExpression(expression));

        private static PropertyInfo GetPropertyInfoForExpression<TSource, TValue>(Expression<Func<TSource, TValue>> expression)
        {
            if (expression.Body is not MemberExpression member)
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

        private static string GetElementNameFor<TSource>(IBsonSerializerRegistry registry, PropertyInfo propertyInfo)
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
