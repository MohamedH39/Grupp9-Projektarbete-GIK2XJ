using PackageDelivery.Data;
using PackageDelivery.Domain;
using PackageDelivery.Shipping;

namespace PackageDelivery.Services;

public class PackageService
{
    private readonly PackageRepository _repository;

    public PackageService(PackageRepository repository)
    {
        _repository = repository;
    }

    public Package RegisterPackage(
        string trackingNumber,
        Address sender,
        Address receiver,
        double weightKg,
        double distanceKm,
        IShippingStrategy shippingStrategy,
        IEnumerable<Station> route)
    {
        var package = new Package(
            trackingNumber,
            sender,
            receiver,
            weightKg,
            distanceKm,
            shippingStrategy);

        foreach (Station station in route)
            package.AddStation(station);

        if (!_repository.Add(package))
            throw new InvalidOperationException("Spårningsnumret finns redan.");

        return package;
    }

    public Package? TrackPackage(string trackingNumber)
        => _repository.FindByTrackingNumber(trackingNumber);

    public bool MoveToNextStation(string trackingNumber)
    {
        Package? package = TrackPackage(trackingNumber);
        return package is not null && package.VisitNextStation();
    }

    public bool UpdateStatus(string trackingNumber, PackageStatus status, string location)
    {
        Package? package = TrackPackage(trackingNumber);
        return package is not null && package.UpdateStatus(status, location);
    }

    public List<Package> GetPackagesSortedByPriority()
    {
        List<Package> packages = _repository.GetAll();
        PackageSorter.InsertionSortByPriority(packages);
        return packages;
    }
}
