using PackageDelivery.Domain;

namespace PackageDelivery.Notifications;

public class CustomerNotifier : ITrackingObserver
{
    private readonly string _customerContact;

    public CustomerNotifier(string customerContact)
    {
        _customerContact = customerContact;
    }

    public void OnPackageChanged(Package package, TrackingEvent trackingEvent)
    {
        Console.WriteLine(
            $"Notis till {_customerContact}: {package.TrackingNumber} är " +
            $"{Package.GetSwedishStatus(trackingEvent.Status)} vid {trackingEvent.Location}.");
    }
}
