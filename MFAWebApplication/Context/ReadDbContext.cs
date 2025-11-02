using MongoDB.Driver;

namespace MFAWebApplication.Context;

public class ReadDbContext
{
    private readonly IMongoDatabase _database;

    public ReadDbContext(IConfiguration configuration)
    {
        var connectionString = configuration["MongoSettings:ConnectionString"];
        var databaseName = configuration["MongoSettings:DatabaseName"];

        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }

    public IMongoCollection<T> GetCollection<T>(string name)
    {
        return _database.GetCollection<T>(name);
    }

}