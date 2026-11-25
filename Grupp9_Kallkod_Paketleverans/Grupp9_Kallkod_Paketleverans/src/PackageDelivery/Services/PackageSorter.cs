using PackageDelivery.Domain;

namespace PackageDelivery.Services;

public static class PackageSorter
{
    public static void InsertionSortByPriority(List<Package> packages)
    {
        for (int i = 1; i < packages.Count; i++)
        {
            Package current = packages[i];
            int j = i - 1;

            while (j >= 0 && packages[j].ShippingStrategy.Priority < current.ShippingStrategy.Priority)
            {
                packages[j + 1] = packages[j];
                j--;
            }

            packages[j + 1] = current;
        }
    }
}
