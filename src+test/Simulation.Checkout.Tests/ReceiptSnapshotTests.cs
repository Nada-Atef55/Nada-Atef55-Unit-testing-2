using Moq;
using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;
using Simulation.Checkout.Infrastructur;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation.Checkout.Tests;

public class ReceiptSnapshotTests
{
    // Task 4
    [Fact]
    public async Task CheckoutAsync_ExampleC_MatchesSnapshot()
    {
        var order = new Order
        {
            Id = new Guid("8f3a9c21-8d64-4b52-a1e9-6c8d2f5b7a30"),
            CustomerName = "Mona",
            CustomerEmail = "mona@example.com",
            Items =
            [
                new OrderItem
                        {
                            ProductId = new Guid("7f3a9c21-8d64-4b52-a1e9-6c8d2f5b7a30"),
                            ProductName = "Lipstick",
                            UnitPrice = 500,
                            Quantity = 3
                        }
            ]
        };


        var notification = new Mock<INotificationService>();
        var inventory = new Mock<IInventoryService>();
        inventory.Setup(x => x.IsAvailableAsync(It.IsAny<Guid>(), It.IsAny<int>())).ReturnsAsync(true);
        var repository = new InMemoryOrderRepository();

        var checkout = new CheckoutService(repository, inventory.Object, notification.Object);

        // Act
        var receipt = await checkout.CheckoutAsync(order);

        // Assert
        await Verifier.Verify(receipt)
            .IgnoreMember("CreatedAt");
    }
}
