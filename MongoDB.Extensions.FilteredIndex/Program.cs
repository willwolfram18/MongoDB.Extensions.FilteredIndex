using MongoDB.Bson.Serialization;
using MongoDB.Driver;
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

var set = Builders<Foo>.Update.Set("elem", x => x.Scores, 0);
var content = set.Render(collection.DocumentSerializer, BsonSerializer.SerializerRegistry);

Console.WriteLine(content);