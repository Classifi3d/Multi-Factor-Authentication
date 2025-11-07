using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MFAWebApplication.Enteties;

public class UserReadModel : ReadModel
{
    [BsonElement("_id")]
    public string Id { get; set; }
    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;        
    [BsonElement("username")]
    public string Username { get; set; } = string.Empty;
    [BsonElement("password")]
    public string Password { get; set; } = string.Empty;
    [BsonElement("isMfaEnabled")]
    public bool IsMfaEnabled { get; set; }

}