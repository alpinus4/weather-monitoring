namespace webapp.Model;


public class Filters
{
    public DateTime? dateFrom { get; set; }
    public DateTime? dateTo { get; set; }
    public List<string>? types { get; set; }
    public List<int>? ids { get; set; }
    public int? valueFrom { get; set; }
    public int? valueTo { get; set; }
}

