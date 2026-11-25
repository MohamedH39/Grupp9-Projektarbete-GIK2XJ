using PackageDelivery.Notifications;
using PackageDelivery.Shipping;

namespace PackageDelivery.Domain;

public class Package
{
    private readonly Queue<Station> _remainingRoute = new();
    private readonly List<TrackingEvent> _history = new();
    private readonly List<ITrackingObserver> _observers = new();

    public string TrackingNumber { get; }
    public Address SenderAddress { get; }
    public Address ReceiverAddress { get; }
    public double WeightKg { get; }
    public double DistanceKm { get; }
    public IShippingStrategy ShippingStrategy { get; }
    public PackageStatus CurrentStatus { get; private set; }
    public string CurrentLocation { get; private set; }
    public double Price => ShippingStrategy.CalculatePrice(WeightKg, DistanceKm);
    public IReadOnlyList<TrackingEvent> History => _history.AsReadOnly();

    public Package(
        string trackingNumber,
        Address senderAddress,
        Address receiverAddress,
        double weightKg,
        double distanceKm,
        IShippingStrategy shippingStrategy)
    {
        if (string.IsNullOrWhiteSpace(trackingNumber))
            throw new ArgumentException("Spårningsnumret får inte vara tomt.");
        if (weightKg <= 0 || distanceKm <= 0)
            throw new ArgumentException("Vikt och avstånd måste vara positiva tal.");

        TrackingNumber = trackingNumber;
        SenderAddress = senderAddress;
        ReceiverAddress = receiverAddress;
        WeightKg = weightKg;
        DistanceKm = distanceKm;
        ShippingStrategy = shippingStrategy;
        CurrentStatus = PackageStatus.Registered;
        CurrentLocation = senderAddress.City;
        _history.Add(new TrackingEvent(CurrentStatus, CurrentLocation));
    }

    public void AddStation(Station station) => _remainingRoute.Enqueue(station);

    public void Subscribe(ITrackingObserver observer) => _observers.Add(observer);

    public bool VisitNextStation()
    {
        if (_remainingRoute.Count == 0 || CurrentStatus == PackageStatus.Delivered)
            return false;

        Station station = _remainingRoute.Dequeue();

        if (CurrentStatus == PackageStatus.Registered || CurrentStatus == PackageStatus.Delayed)
            return UpdateStatus(PackageStatus.InTransit, station.Name);

        CurrentLocation = station.Name;
        var trackingEvent = new TrackingEvent(CurrentStatus, CurrentLocation);
        _history.Add(trackingEvent);
        NotifyObservers(trackingEvent);
        return true;
    }

    public bool UpdateStatus(PackageStatus newStatus, string location)
    {
        if (!IsTransitionAllowed(CurrentStatus, newStatus))
            return false;

        CurrentStatus = newStatus;
        CurrentLocation = string.IsNullOrWhiteSpace(location) ? CurrentLocation : location;
        var trackingEvent = new TrackingEvent(CurrentStatus, CurrentLocation);
        _history.Add(trackingEvent);
        NotifyObservers(trackingEvent);
        return true;
    }

    public string GetTrackingSummary()
    {
        string status = GetSwedishStatus(CurrentStatus);
        return $"{TrackingNumber}: {status}, plats: {CurrentLocation}, frakt: {ShippingStrategy.Name}, pris: {Price:F0} kr";
    }

    private void NotifyObservers(TrackingEvent trackingEvent)
    {
        foreach (ITrackingObserver observer in _observers)
            observer.OnPackageChanged(this, trackingEvent);
    }

    private static bool IsTransitionAllowed(PackageStatus current, PackageStatus next)
    {
        return current switch
        {
            PackageStatus.Registered => next is PackageStatus.InTransit or PackageStatus.Delayed,
            PackageStatus.InTransit => next is PackageStatus.OutForDelivery or PackageStatus.Delayed,
            PackageStatus.Delayed => next is PackageStatus.InTransit or PackageStatus.OutForDelivery,
            PackageStatus.OutForDelivery => next is PackageStatus.Delivered or PackageStatus.Delayed,
            PackageStatus.Delivered => false,
            _ => false
        };
    }

    public static string GetSwedishStatus(PackageStatus status)
    {
        return status switch
        {
            PackageStatus.Registered => "Inlämnad",
            PackageStatus.InTransit => "Under transport",
            PackageStatus.OutForDelivery => "Ute för leverans",
            PackageStatus.Delivered => "Levererad",
            PackageStatus.Delayed => "Försenad",
            _ => status.ToString()
        };
    }
}
