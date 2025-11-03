using MongoDB.Bson.Serialization.Attributes;

namespace MFAWebApplication.Enteties;

public class ReadModel
{
    [BsonElement("concurrencyIndex")]
    public ulong ConcurrencyIndex { get; set; }
}
