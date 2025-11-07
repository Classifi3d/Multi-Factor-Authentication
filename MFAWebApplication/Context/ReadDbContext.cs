using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace MFAWebApplication.Context;

public class ReadDbContext
{
    private readonly IMongoDatabase _database;


    public ReadDbContext(IConfiguration configuration)
    {


        var connectionString = configuration.GetConnectionString("MongoDB_Read_Connection_String");
        var databaseName = configuration["MongoSettings:DatabaseName"];

             if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(databaseName))
        {
            throw new InvalidOperationException("MongoDB connection string or database name is missing in configuration.");
        }

        try
        {
            //BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
            //BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.CSharpLegacy));

            var mongoSettings = MongoClientSettings.FromConnectionString(connectionString);
            var client = new MongoClient(mongoSettings);
            _database = client.GetDatabase(databaseName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            throw;
        }

    }

    public IMongoCollection<T> GetCollection<T>(string name)
    {
        return _database.GetCollection<T>(name);
    }

}