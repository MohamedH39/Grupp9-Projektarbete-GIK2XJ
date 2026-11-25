using PackageDelivery.Data;
using PackageDelivery.Domain;
using PackageDelivery.Notifications;
using PackageDelivery.Services;
using PackageDelivery.Shipping;

var repository = new PackageRepository();
var service = new PackageService(repository);

while (true)
{
    Console.WriteLine("\n=== Paketleverans och spårning ===");
    Console.WriteLine("1. Registrera paket");
    Console.WriteLine("2. Spåra paket");
    Console.WriteLine("3. Flytta paket till nästa station");
    Console.WriteLine("4. Ändra paketstatus");
    Console.WriteLine("5. Visa paket efter prioritet");
    Console.WriteLine("6. Kör demonstrationer");
    Console.WriteLine("7. Kör självtester");
    Console.WriteLine("0. Avsluta");
    Console.Write("Val: ");

    string? choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            RegisterPackage(service);
            break;
        case "2":
            TrackPackage(service);
            break;
        case "3":
            MovePackage(service);
            break;
        case "4":
            ChangeStatus(service);
            break;
        case "5":
            ShowByPriority(service);
            break;
        case "6":
            DemoRunner.Run();
            break;
        case "7":
            SelfTestRunner.Run();
            break;
        case "0":
            return;
        default:
            Console.WriteLine("Ogiltigt val. Försök igen.");
            break;
    }
}

static void RegisterPackage(PackageService service)
{
    Console.Write("Spårningsnummer: ");
    string trackingNumber = Console.ReadLine()?.Trim() ?? "";

    if (string.IsNullOrWhiteSpace(trackingNumber))
    {
        Console.WriteLine("Spårningsnumret får inte vara tomt.");
        return;
    }

    Console.Write("Vikt i kg: ");
    if (!double.TryParse(Console.ReadLine(), out double weight) || weight <= 0)
    {
        Console.WriteLine("Vikten måste vara ett positivt tal.");
        return;
    }

    Console.Write("Avstånd i km: ");
    if (!double.TryParse(Console.ReadLine(), out double distance) || distance <= 0)
    {
        Console.WriteLine("Avståndet måste vara ett positivt tal.");
        return;
    }

    Console.WriteLine("Frakttyp: 1 Standard, 2 Express, 3 Skört gods");
    IShippingStrategy? strategy = Console.ReadLine() switch
    {
        "1" => new StandardShipping(),
        "2" => new ExpressShipping(),
        "3" => new FragileShipping(),
        _ => null
    };

    if (strategy is null)
    {
        Console.WriteLine("Ogiltig frakttyp.");
        return;
    }

    string senderStreet = ReadRequired("Avsändarens gata: ");
    string senderCity = ReadRequired("Avsändarens ort: ");
    string senderPostalCode = ReadRequired("Avsändarens postnummer: ");
    string receiverStreet = ReadRequired("Mottagarens gata: ");
    string receiverCity = ReadRequired("Mottagarens ort: ");
    string receiverPostalCode = ReadRequired("Mottagarens postnummer: ");
    string firstStation = ReadRequired("Första terminalen: ");

    if (new[]
        {
            senderStreet, senderCity, senderPostalCode,
            receiverStreet, receiverCity, receiverPostalCode, firstStation
        }.Any(string.IsNullOrWhiteSpace))
    {
        Console.WriteLine("Adresser och den första terminalen måste fyllas i.");
        return;
    }

    string secondStation = ReadRequired("Andra terminalen (valfri): ");
    var route = new List<Station> { new(firstStation) };
    if (!string.IsNullOrWhiteSpace(secondStation))
        route.Add(new Station(secondStation));

    string customerContact = ReadRequired("Kundkontakt (tom = kund@example.se): ");
    if (string.IsNullOrWhiteSpace(customerContact))
        customerContact = "kund@example.se";

    try
    {
        var package = service.RegisterPackage(
            trackingNumber,
            new Address(senderStreet, senderCity, senderPostalCode),
            new Address(receiverStreet, receiverCity, receiverPostalCode),
            weight,
            distance,
            strategy,
            route);

        package.Subscribe(new CustomerNotifier(customerContact));
        Console.WriteLine($"Paketet registrerades. Pris: {package.Price:F0} kr");
    }
    catch (InvalidOperationException exception)
    {
        Console.WriteLine(exception.Message);
    }
}

static string ReadRequired(string prompt)
{
    Console.Write(prompt);
    return Console.ReadLine()?.Trim() ?? "";
}

static void TrackPackage(PackageService service)
{
    Console.Write("Spårningsnummer: ");
    string trackingNumber = Console.ReadLine()?.Trim() ?? "";
    Package? package = service.TrackPackage(trackingNumber);

    if (package is null)
    {
        Console.WriteLine("Paketet hittades inte.");
        return;
    }

    Console.WriteLine(package.GetTrackingSummary());
}

static void MovePackage(PackageService service)
{
    Console.Write("Spårningsnummer: ");
    string trackingNumber = Console.ReadLine()?.Trim() ?? "";

    if (!service.MoveToNextStation(trackingNumber))
        Console.WriteLine("Paketet hittades inte eller har inga stationer kvar.");
}

static void ChangeStatus(PackageService service)
{
    Console.Write("Spårningsnummer: ");
    string trackingNumber = Console.ReadLine()?.Trim() ?? "";
    Console.WriteLine("Ny status: 1 Under transport, 2 Ute för leverans, 3 Levererad, 4 Försenad");

    PackageStatus? status = Console.ReadLine() switch
    {
        "1" => PackageStatus.InTransit,
        "2" => PackageStatus.OutForDelivery,
        "3" => PackageStatus.Delivered,
        "4" => PackageStatus.Delayed,
        _ => null
    };

    if (status is null)
    {
        Console.WriteLine("Ogiltig status.");
        return;
    }

    Console.Write("Plats: ");
    string location = Console.ReadLine()?.Trim() ?? "Okänd plats";

    if (!service.UpdateStatus(trackingNumber, status.Value, location))
        Console.WriteLine("Paketet hittades inte eller statusövergången är inte tillåten.");
}

static void ShowByPriority(PackageService service)
{
    foreach (Package package in service.GetPackagesSortedByPriority())
        Console.WriteLine($"{package.TrackingNumber}: {package.ShippingStrategy.Name}, prioritet {package.ShippingStrategy.Priority}");
}
