using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

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

        public static ArrayFilterDefinition<TSource> foo<TSource>(this FilterDefinitionBuilder<TSource> builder,
            string identifier,
            FilterDefinition<TSource> filter)
        {
            var x = filter.Render(BsonSerializer.LookupSerializer<TSource>(), BsonSerializer.SerializerRegistry);
            throw new NotImplementedException();
        }
    }
}
