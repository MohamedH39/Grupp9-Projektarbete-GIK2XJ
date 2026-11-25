namespace PackageDelivery.Shipping;

public interface IShippingStrategy
{
    string Name { get; }
    int Priority { get; }
    double CalculatePrice(double weightKg, double distanceKm);
}
