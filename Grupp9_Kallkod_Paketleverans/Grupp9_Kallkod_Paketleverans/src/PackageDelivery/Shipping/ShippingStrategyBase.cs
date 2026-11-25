namespace PackageDelivery.Shipping;

public abstract class ShippingStrategyBase : IShippingStrategy
{
    public abstract string Name { get; }
    public abstract int Priority { get; }
    public abstract double CalculatePrice(double weightKg, double distanceKm);

    protected double CalculateBasePrice(double weightKg, double distanceKm)
    {
        return 49 + weightKg * 10 + distanceKm * 0.50;
    }
}
