using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
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
    public IActionResult Get(
        [FromQuery] string filters = "{}",
        [FromQuery] string sortBy = "timestamp",
        [FromQuery] string sortDirection = "asc",
        [FromQuery] bool download = false,
        [FromQuery] string format = "json"
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

        var projection = Builders<Data>.Projection.Exclude("_id"); // Exclude the _id field

        var dataList = _mongoDBConnection.GetDatabase("db")
            .GetCollection<Data>("sensor_data")
            .Find(mongoFilter)
            .Project<Data>(projection) // Apply projection here
            .Sort(sort)
            .ToList();

        
        if (download)
        {
            string fileName;
            byte[] fileContent;

            if (format.ToLower() == "csv")
            {
                var csvContent = ConvertToCsv(dataList);
                fileContent = Encoding.UTF8.GetBytes(csvContent);
                fileName = "data.csv";
                return File(fileContent, "text/csv", fileName);
            }
            else // Default to JSON
            {
                var jsonContent = JsonSerializer.Serialize(dataList);
                fileContent = Encoding.UTF8.GetBytes(jsonContent);
                fileName = "data.json";
                return File(fileContent, "application/json", fileName);
            }
        }
        else
        {
            return Ok(dataList);
        }
    }

    private string ConvertToCsv(IEnumerable<Data> dataList)
    {
        var stringBuilder = new StringBuilder();

        // Add CSV header
        stringBuilder.AppendLine("Type,SensorId,Timestamp,Value");

        foreach (var data in dataList)
        {
            // Add data rows
            stringBuilder.AppendLine($"{data.type},{data.sensor_id},{data.timestamp:O},{data.value}");
        }

        return stringBuilder.ToString();
    }
}
