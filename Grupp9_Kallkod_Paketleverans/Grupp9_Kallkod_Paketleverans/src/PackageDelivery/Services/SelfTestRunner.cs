using PackageDelivery.Data;
using PackageDelivery.Domain;
using PackageDelivery.Notifications;
using PackageDelivery.Shipping;

namespace PackageDelivery.Services;

public static class SelfTestRunner
{
    public static void Run()
    {
        int passed = 0;
        int total = 4;

        if (Math.Abs(new StandardShipping().CalculatePrice(2, 100) - 119) < 0.01)
            passed += Pass("Standardpriset beräknas rätt");

        var service = new PackageService(new PackageRepository());
        Package package = service.RegisterPackage(
            "TEST-1",
            new Address("A", "Borlänge", "1"),
            new Address("B", "Falun", "2"),
            1,
            50,
            new StandardShipping(),
            new[] { new Station("Terminal") });

        if (service.TrackPackage("TEST-1") == package)
            passed += Pass("Dictionary hittar paketet");

        if (!package.UpdateStatus(PackageStatus.Delivered, "Falun"))
            passed += Pass("Ogiltig statusövergång stoppas");

        var observer = new TestObserver();
        package.Subscribe(observer);
        package.UpdateStatus(PackageStatus.InTransit, "Terminal");
        if (observer.NotificationCount == 1)
            passed += Pass("Observer får en notifiering");

        Console.WriteLine($"Självtester: {passed} av {total} godkända.");
    }

    private static int Pass(string testName)
    {
        Console.WriteLine($"[GODKÄND] {testName}");
        return 1;
    }

    private class TestObserver : ITrackingObserver
    {
        public int NotificationCount { get; private set; }

        public void OnPackageChanged(Package package, TrackingEvent trackingEvent)
        {
            NotificationCount++;
        }
    }
}
