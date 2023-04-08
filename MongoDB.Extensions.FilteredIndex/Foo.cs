using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDB.Extensions.FilteredIndex
{
    internal class Foo
    {
        public ObjectId Id { get; set; }

        [BsonElement("_scores")]
        public IEnumerable<int> Scores { get; set; }

        public IEnumerable<Bar> Bars { get; set; }
    }

    public class Bar
    {
        public int MyProperty { get; set; }
    }
}
