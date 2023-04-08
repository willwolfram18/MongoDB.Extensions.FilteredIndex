using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Extensions.FilteredIndex;
using Testcontainers.MongoDb;

await using var mongodb = new MongoDbBuilder()
    .WithUsername("user")
    .WithPassword("password")
    .Build();

await mongodb.StartAsync();

var client = new MongoClient(mongodb.GetConnectionString());
var database = client.GetDatabase("foobar");
var collection = database.GetCollection<Foo>("students");

await collection.InsertManyAsync(
    new Foo[]
    {
        new Foo { Scores = new[] { 95, 92, 90 } },
        new Foo { Scores = new[] { 98, 100, 102 } },
        new Foo { Scores = new[] { 95, 110, 100 } }
    }
);

await collection.UpdateManyAsync(
    Builders<Foo>.Filter.Empty,
    Builders<Foo>.Update.Set("elem", x => x.Scores, 100),
    new UpdateOptions
    {
        ArrayFilters = new ArrayFilterDefinition[]
        {
            Builders<int>.Filter.ItemGte("elem", 100)
        }
    });

foreach (var item in await collection.Find(Builders<Foo>.Filter.Empty).ToListAsync())
{
    Console.WriteLine(item.ToJson());
}

var set = Builders<Foo>.Update.Set("elem", x => x.Scores, 0);
var content = set.Render(collection.DocumentSerializer, BsonSerializer.SerializerRegistry);

Console.WriteLine(content);

var equalFilter = Builders<int>.Filter.ItemEq("elem", 10);
content = equalFilter.Render(BsonSerializer.LookupSerializer<int>(), BsonSerializer.SerializerRegistry, LinqProvider.V3);

Console.WriteLine(content);

var gteFilter = Builders<int>.Filter.ItemGte("elem", 90);
content = gteFilter.Render(BsonSerializer.LookupSerializer<int>(), BsonSerializer.SerializerRegistry, LinqProvider.V3);

Console.WriteLine(content);

var x = Builders<Bar>.Filter.ItemEq("elem", x => x.MyProperty, 10);
content = x.Render(BsonSerializer.LookupSerializer<Bar>(), BsonSerializer.SerializerRegistry, LinqProvider.V3);

Console.WriteLine(content);