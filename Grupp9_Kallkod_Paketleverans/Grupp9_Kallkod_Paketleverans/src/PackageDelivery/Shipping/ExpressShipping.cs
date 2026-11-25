namespace PackageDelivery.Shipping;

public class ExpressShipping : ShippingStrategyBase
{
    public override string Name => "Express";
    public override int Priority => 3;

    public override double CalculatePrice(double weightKg, double distanceKm)
        => CalculateBasePrice(weightKg, distanceKm) * 1.60;
}
