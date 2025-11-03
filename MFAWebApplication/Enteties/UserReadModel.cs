using MongoDB.Bson.Serialization.Attributes;

namespace MFAWebApplication.Enteties;

public class UserReadModel : ReadModel
{
    [BsonId]
    [BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public Guid Id { get; set; }
    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;        
    [BsonElement("username")]
    public string Username { get; set; } = string.Empty;
    [BsonElement("password")]
    public string Password { get; set; } = string.Empty;
    [BsonElement("isMfaEnabled")]
    public bool IsMfaEnabled { get; set; }
}