using PackageDelivery.Data;
using PackageDelivery.Domain;
using PackageDelivery.Notifications;
using PackageDelivery.Shipping;

namespace PackageDelivery.Services;

public static class DemoRunner
{
    public static void Run()
    {
        Console.WriteLine("\n--- Demonstration 1: Strategy och pris ---");
        ShowPrice(new StandardShipping(), 2, 100);
        ShowPrice(new ExpressShipping(), 2, 100);
        ShowPrice(new FragileShipping(), 2, 100);

        Console.WriteLine("\n--- Demonstration 2: spårning och Observer ---");
        var service = new PackageService(new PackageRepository());
        Package package = service.RegisterPackage(
            "PKG-1001",
            new Address("Startgatan 1", "Borlänge", "784 30"),
            new Address("Målgatan 5", "Stockholm", "111 20"),
            2,
            220,
            new ExpressShipping(),
            new[] { new Station("Borlänge terminal"), new Station("Västerås terminal") });

        package.Subscribe(new CustomerNotifier("kund@example.se"));
        Console.WriteLine(package.GetTrackingSummary());
        service.MoveToNextStation("PKG-1001");
        service.MoveToNextStation("PKG-1001");
        service.UpdateStatus("PKG-1001", PackageStatus.OutForDelivery, "Stockholm");
        service.UpdateStatus("PKG-1001", PackageStatus.Delivered, "Målgatan 5");
        Console.WriteLine(package.GetTrackingSummary());
    }

    private static void ShowPrice(IShippingStrategy strategy, double weight, double distance)
    {
        double price = strategy.CalculatePrice(weight, distance);
        Console.WriteLine($"{strategy.Name}: {price:F0} kr, prioritet {strategy.Priority}");
    }
}
