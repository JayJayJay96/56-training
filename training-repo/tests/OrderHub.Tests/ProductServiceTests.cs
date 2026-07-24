using OrderHub.Core.Domain;

namespace OrderHub.Tests;

public class ProductServiceTests
{
    [Fact]
    public async Task GetAll_ReturnsAllProductsIncludingInactive()
    {
        using var db = TestSetup.CreateContext();
        var service = TestSetup.CreateProductService(db);
        TestSetup.AddProduct(db, sku: "SKU-A001");
        TestSetup.AddProduct(db, sku: "SKU-A002", isActive: false);

        var products = await service.GetAllAsync();

        Assert.Equal(2, products.Count);
    }

    [Fact]
    public async Task GetActive_ExcludesInactiveProducts()
    {
        using var db = TestSetup.CreateContext();
        var service = TestSetup.CreateProductService(db);
        TestSetup.AddProduct(db, sku: "SKU-A001");
        TestSetup.AddProduct(db, sku: "SKU-A002", isActive: false);

        var products = await service.GetActiveAsync();

        Assert.All(products, p => Assert.True(p.IsActive));
        Assert.Single(products);
    }

    [Fact]
    public async Task GetLowStock_FiltersByThresholdAndOrdersByStockAscending()
    {
        using var db = TestSetup.CreateContext();
        var service = TestSetup.CreateProductService(db);
        TestSetup.AddProduct(db, sku: "SKU-LOW", stock: 3);
        TestSetup.AddProduct(db, sku: "SKU-MID", stock: 7);
        TestSetup.AddProduct(db, sku: "SKU-EQUAL", stock: 10);
        TestSetup.AddProduct(db, sku: "SKU-HIGH", stock: 20);

        var result = await service.GetLowStockAsync(10);

        Assert.Equal(new[] { "SKU-LOW", "SKU-MID" }, result.Select(r => r.Sku));
    }

    [Fact]
    public async Task GetLowStock_ExcludesInactiveProducts()
    {
        using var db = TestSetup.CreateContext();
        var service = TestSetup.CreateProductService(db);
        TestSetup.AddProduct(db, sku: "SKU-ACTIVE", stock: 2);
        TestSetup.AddProduct(db, sku: "SKU-INACTIVE", stock: 1, isActive: false);

        var result = await service.GetLowStockAsync(10);

        Assert.Single(result);
        Assert.Equal("SKU-ACTIVE", result[0].Sku);
    }

    [Fact]
    public async Task GetLowStock_SoldLast30Days_ExcludesCancelledAndOutOfWindowOrders()
    {
        using var db = TestSetup.CreateContext();
        var service = TestSetup.CreateProductService(db);
        var customer = TestSetup.AddCustomer(db);
        var product = TestSetup.AddProduct(db, sku: "SKU-SOLD", stock: 5);

        var withinWindowConfirmed = TestSetup.AddOrder(db, customer, OrderStatus.Confirmed, DateTime.UtcNow.AddDays(-5));
        TestSetup.AddOrderItem(db, withinWindowConfirmed, product, quantity: 5);

        var withinWindowCancelled = TestSetup.AddOrder(db, customer, OrderStatus.Cancelled, DateTime.UtcNow.AddDays(-5));
        TestSetup.AddOrderItem(db, withinWindowCancelled, product, quantity: 3);

        var outsideWindowConfirmed = TestSetup.AddOrder(db, customer, OrderStatus.Confirmed, DateTime.UtcNow.AddDays(-31));
        TestSetup.AddOrderItem(db, outsideWindowConfirmed, product, quantity: 2);

        var result = await service.GetLowStockAsync(10);

        var row = Assert.Single(result);
        Assert.Equal(5, row.SoldLast30Days);
    }
}
