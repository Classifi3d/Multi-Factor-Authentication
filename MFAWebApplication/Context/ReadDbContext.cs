using MongoDB.Bson;
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
            var mongoSettings = MongoClientSettings.FromConnectionString(connectionString);

            mongoSettings.GuidRepresentation = GuidRepresentation.Standard;

            var client = new MongoClient(mongoSettings);
            _database = client.GetDatabase(databaseName);
        }
        catch (Exception ex)
        {
            throw;
        }

    }

    public IMongoCollection<T> GetCollection<T>(string name)
    {
        return _database.GetCollection<T>(name);
    }

}