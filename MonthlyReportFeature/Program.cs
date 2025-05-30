
var customers = SampleData.Customers;
var products = SampleData.Products;
var orders = SampleData.Orders;

// start LINQ Query
var category = ProductCategory.Electronics;

var report = orders
    .Join(
        products,
        order => order.ProductId,
        product => product.Id,
        (order, product) => new { Order = order, Product = product }
    )
    .Join(
        customers,
        joined => joined.Order.CustomerId,
        customer => customer.Id,
        (joined, customer) => new { joined.Order, joined.Product, Customer = customer }
    )
    // filter products that include Electronics category
    .Where(item => (item.Product.Category & category) == category)
    .GroupBy(
        item => new
        {
            item.Customer.Name,
            Year = item.Order.OrderDate.Year,
            Month = item.Order.OrderDate.Month
        }
    )
    .Select(group => new CustomerSummary
    {
        CustomerName = group.Key.Name,
        Category = category,
        Year = group.Key.Year,
        Month = group.Key.Month,
        TotalQuantity = group.Sum(item => item.Order.Quantity),
        UniqueProductsOrdered = group.Select(item => item.Product.Id).Distinct().Count()
    })
    .OrderBy(summary => summary.CustomerName)
    .ThenBy(summary => summary.Year)
    .ThenBy(summary => summary.Month)
    .ToList();

Console.WriteLine($"Monthly {category} Product Report");
Console.WriteLine("----------------------------------");
foreach (var summary in report)
{
    Console.WriteLine($"Customer: {summary.CustomerName}    Time: {summary.Year}/{summary.Month:D2}    Total Quantity: {summary.TotalQuantity}    Unique Products: {summary.UniqueProductsOrdered}");
    Console.WriteLine("----------------------------------");
}


#region Dataset

public static class SampleData
{
    public static List<Customer> Customers =>
    [
        new() { Id = 1, Name = "Alice" },
        new() { Id = 2, Name = "Bob" },
        new() { Id = 3, Name = "Charlie" },
        new() { Id = 4, Name = "Diana" },
        new() { Id = 5, Name = "Ethan" }
    ];

    public static List<Product> Products =>
    [
        new() { Id = 1, Name = "Smartphone", Category = ProductCategory.Electronics },
        new() { Id = 2, Name = "Laptop", Category = ProductCategory.Electronics },
        new()
        {
            Id = 3, Name = "Refrigerator", Category = ProductCategory.HomeAppliances | ProductCategory.Electronics
        },
        new() { Id = 4, Name = "Running Shoes", Category = ProductCategory.Sports },
        new() { Id = 5, Name = "T-Shirt", Category = ProductCategory.Clothing },
        new() { Id = 6, Name = "Electric Kettle", Category = ProductCategory.HomeAppliances },
        new() { Id = 7, Name = "Book: C# in Depth", Category = ProductCategory.Books },
        new() { Id = 8, Name = "Tablet", Category = ProductCategory.Electronics },
        new() { Id = 9, Name = "Dumbbells", Category = ProductCategory.Sports },
        new() { Id = 10, Name = "Face Cream", Category = ProductCategory.Beauty }
    ];

    public static List<Order> Orders =>
    [
        new() { Id = 1, CustomerId = 1, ProductId = 1, Quantity = 2, OrderDate = DateTime.Now.AddDays(-100) },
        new() { Id = 2, CustomerId = 1, ProductId = 2, Quantity = 1, OrderDate = DateTime.Now.AddDays(-330) },
        new() { Id = 3, CustomerId = 1, ProductId = 3, Quantity = 1, OrderDate = DateTime.Now.AddDays(-60) },
        new() { Id = 4, CustomerId = 2, ProductId = 2, Quantity = 1, OrderDate = DateTime.Now.AddDays(-20) },
        new() { Id = 5, CustomerId = 2, ProductId = 8, Quantity = 2, OrderDate = DateTime.Now.AddDays(-370) },
        new() { Id = 6, CustomerId = 3, ProductId = 3, Quantity = 1, OrderDate = DateTime.Now.AddDays(-15) },
        new() { Id = 7, CustomerId = 3, ProductId = 1, Quantity = 3, OrderDate = DateTime.Now.AddDays(-5) },
        new() { Id = 8, CustomerId = 4, ProductId = 2, Quantity = 1, OrderDate = DateTime.Now.AddDays(-90) },
        new() { Id = 9, CustomerId = 4, ProductId = 9, Quantity = 1, OrderDate = DateTime.Now.AddDays(-30) },
        new() { Id = 10, CustomerId = 4, ProductId = 8, Quantity = 1, OrderDate = DateTime.Now.AddDays(-7) },
        new() { Id = 11, CustomerId = 5, ProductId = 10, Quantity = 2, OrderDate = DateTime.Now.AddDays(-12) },
        new() { Id = 12, CustomerId = 5, ProductId = 3, Quantity = 1, OrderDate = DateTime.Now.AddDays(-3) },
        new() { Id = 13, CustomerId = 2, ProductId = 3, Quantity = 1, OrderDate = DateTime.Now.AddDays(-85) },
        new() { Id = 14, CustomerId = 1, ProductId = 8, Quantity = 1, OrderDate = DateTime.Now.AddDays(-25) },
        new() { Id = 15, CustomerId = 5, ProductId = 1, Quantity = 1, OrderDate = DateTime.Now.AddDays(-1) }
    ];
}

#endregion

#region Entity

[Flags]
public enum ProductCategory
{
    None = 0,
    Electronics = 1 << 0,
    HomeAppliances = 1 << 1,
    Clothing = 1 << 2,
    Sports = 1 << 3,
    Books = 1 << 4,
    Beauty = 1 << 5
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ProductCategory Category { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTime OrderDate { get; set; }
}

#endregion

#region Dto

public class CustomerSummary
{
    public string CustomerName { get; set; }
    public ProductCategory Category { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public int TotalQuantity { get; set; }
    public int UniqueProductsOrdered { get; set; }
}

#endregion