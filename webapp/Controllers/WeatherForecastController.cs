using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using webapp.Model;
using webapp.Services;

namespace webapp.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly MongoClient _mongoDBConnection;
    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, MongoDBConnectionService mongoDBConnectionService)
    {
        _mongoDBConnection = mongoDBConnectionService.Connection;
        _logger = logger;
    }

    [HttpGet]
    public IEnumerable<Data> Get(
        [FromQuery] string filters = "{}",
        [FromQuery] string sortBy = "timestamp",
        [FromQuery] string sortDirection = "asc"
)
    {
        var filter = JsonSerializer.Deserialize<Filters>(filters);

        var mongoFilter = Builders<Data>.Filter.Empty;

        if(filter.dateFrom != null)
        {
            mongoFilter = Builders<Data>.Filter.And(mongoFilter, Builders<Data>.Filter.Gt("timestamp", filter.dateFrom));
        }

        if (filter.dateTo != null)
        {
            mongoFilter = Builders<Data>.Filter.And(mongoFilter, Builders<Data>.Filter.Lt("timestamp", filter.dateTo));
        }

        if (filter.valueFrom != null)
        {
            mongoFilter = Builders<Data>.Filter.And(mongoFilter, Builders<Data>.Filter.Gt("value", filter.valueFrom));
        }

        if (filter.valueTo != null)
        {
            mongoFilter = Builders<Data>.Filter.And(mongoFilter, Builders<Data>.Filter.Lt("value", filter.valueTo));
        }


        if (filter.ids != null && filter.ids.Count > 0)
        {
            var idFilter = Builders<Data>.Filter.Eq("sensor_id", filter.ids[0]);

            foreach (int id in filter.ids)
            {
                idFilter = Builders<Data>.Filter.Or(idFilter, Builders<Data>.Filter.Eq("sensor_id", id));
            }

            mongoFilter = Builders<Data>.Filter.And(mongoFilter, idFilter);

        }

        if (filter.types != null && filter.types.Count > 0)
        {
            var typeFilter = Builders<Data>.Filter.Eq("type", filter.types[0]);

            foreach (string type in filter.types)
            {
                typeFilter = Builders<Data>.Filter.Or(typeFilter, Builders<Data>.Filter.Eq("type", type));
            }

            mongoFilter = Builders<Data>.Filter.And(mongoFilter, typeFilter);

        }

        var sort = Builders<Data>.Sort.Ascending(sortBy);

        if (sortDirection == "desc")
        {
            sort = Builders<Data>.Sort.Descending(sortBy);
        }

        return _mongoDBConnection.GetDatabase("db").GetCollection<Data>("sensor_data").Find(mongoFilter).Sort(sort).ToList().ToArray();
    }
}
