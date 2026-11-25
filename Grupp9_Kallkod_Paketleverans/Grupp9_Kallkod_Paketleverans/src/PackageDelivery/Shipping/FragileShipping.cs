namespace PackageDelivery.Shipping;

public class FragileShipping : ShippingStrategyBase
{
    public override string Name => "Skört gods";
    public override int Priority => 2;

    public override double CalculatePrice(double weightKg, double distanceKm)
        => CalculateBasePrice(weightKg, distanceKm) + 80;
}
