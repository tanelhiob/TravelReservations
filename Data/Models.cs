namespace TravelReservations.Data;

public class Leg
{
    public Guid Id { get; set; }
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public long Distance { get; set; }
    public List<Provider> Providers { get; set; } = new List<Provider>();
}

public class Provider
{
    public Guid Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime FlightStart { get; set; }
    public DateTime FlightEnd { get; set; }
    public Guid LegId { get; set; }
    public Leg? Leg { get; set; }
}

public class Reservation
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Routes { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public TimeSpan TotalTravelTime { get; set; }
    public string Companies { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}