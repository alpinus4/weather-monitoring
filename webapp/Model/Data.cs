using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace webapp.Model;

public class Data
{
    public string? type { get; set; }
    public int sensor_id { get; set; }
    public DateTime timestamp { get; set; }
    public double value { get; set; }
}
