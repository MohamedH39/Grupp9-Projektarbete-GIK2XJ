namespace PackageDelivery.Domain;

public class TrackingEvent
{
    public PackageStatus Status { get; }
    public string Location { get; }
    public DateTime Time { get; }

    public TrackingEvent(PackageStatus status, string location)
    {
        Status = status;
        Location = location;
        Time = DateTime.Now;
    }
}
