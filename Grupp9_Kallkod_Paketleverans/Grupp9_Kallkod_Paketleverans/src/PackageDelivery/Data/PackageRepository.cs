using PackageDelivery.Domain;

namespace PackageDelivery.Data;

public class PackageRepository
{
    private readonly Dictionary<string, Package> _packages = new(StringComparer.OrdinalIgnoreCase);

    public bool Add(Package package)
    {
        return _packages.TryAdd(package.TrackingNumber, package);
    }

    public Package? FindByTrackingNumber(string trackingNumber)
    {
        _packages.TryGetValue(trackingNumber, out Package? package);
        return package;
    }

    public List<Package> GetAll() => _packages.Values.ToList();
}
