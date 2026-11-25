namespace PackageDelivery.Shipping;

public class StandardShipping : ShippingStrategyBase
{
    public override string Name => "Standard";
    public override int Priority => 1;

    public override double CalculatePrice(double weightKg, double distanceKm)
        => CalculateBasePrice(weightKg, distanceKm);
}
