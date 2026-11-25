namespace PackageDelivery.Domain;

public enum PackageStatus
{
    Registered,
    InTransit,
    OutForDelivery,
    Delivered,
    Delayed
}
