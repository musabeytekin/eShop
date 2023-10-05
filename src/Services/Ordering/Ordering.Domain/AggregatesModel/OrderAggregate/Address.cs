namespace Ordering.Domain.AggregatesModel.OrderAggregate;

public class Address : ValueObject
{
    public string Street { get; private set; }
    public string City { get; private set; }
    public string Country { get; private set; }
    public string ZipCode { get; private set; }

    public Address() { }
    
    public Address(string street, string city, string country, string zipCode)
    {
        Street = street;
        City = city;
        Country = country;
        ZipCode = zipCode;
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return Country;
        yield return ZipCode;
    }
}