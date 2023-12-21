using MongoDB.Driver;



namespace webapp.Services;


public class MongoDBConnectionService
{

    public MongoClient Connection { get; }


    public MongoDBConnectionService()
    {
        Connection = new MongoClient(Environment.GetEnvironmentVariable("MONGODB_URL"));
    }



}