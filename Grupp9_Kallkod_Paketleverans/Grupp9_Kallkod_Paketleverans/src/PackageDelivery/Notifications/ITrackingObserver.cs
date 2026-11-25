using PackageDelivery.Domain;

namespace PackageDelivery.Notifications;

public interface ITrackingObserver
{
    void OnPackageChanged(Package package, TrackingEvent trackingEvent);
}
