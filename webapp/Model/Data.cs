using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace webapp.Model;

public class Data
{

    [BsonId]
    public ObjectId _id { get; set; }
    public string? type { get; set; }
    public int sensor_id { get; set; }
    public DateTime timestamp { get; set; }
    public double value { get; set; }
}
