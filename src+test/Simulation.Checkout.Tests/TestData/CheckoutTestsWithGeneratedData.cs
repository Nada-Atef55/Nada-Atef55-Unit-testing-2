using AutoFixture;
using Bogus;
using Moq;
using NSubstitute;
using Shouldly;
using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;
using Simulation.Checkout.Infrastructur;


namespace Simulation.Checkout.Tests.TestData;

public class CheckoutTestsWithGeneratedData
{

    // Task 3.1
    [Fact]
    public async Task CheckoutAsync_AutoFixtureValidOrder()
    {
        // Arrange using AutoFixture to generate a valid order with random data
        var fixture = new Fixture();

        var order = fixture.Build<Order>()
            .With(o => o.Items,
                    fixture.Build<OrderItem>()
                    .With(i => i.UnitPrice, () => Random.Shared.Next(0, 100))
                    .With(i => i.Quantity, () => Random.Shared.Next(1, 5))
                    .CreateMany(3)
                    .ToList())
            .Create();

        var repo = new InMemoryOrderRepository();
        var inventory = new Mock<IInventoryService>();
        inventory.Setup(x => x.IsAvailableAsync(It.IsAny<Guid>(), It.IsAny<int>())).ReturnsAsync(true);
        var notification = new Mock<INotificationService>();

        var checkout = new CheckoutService(repo, inventory.Object, notification.Object);

        // Act
        var receipt = await checkout.CheckoutAsync(order);

        // Assert
        receipt.ShouldNotBeNull();

    }



    // Task 3.2
    [Fact]
    public async Task CheckoutAsync_ValidOrderUsingBogus_CorrectCustomerInfo()
    {
        // Arrange using Bogus
        var customer = new Faker<Customer>()
            .UseSeed(55)
            .RuleFor(c => c.Name, f => f.Name.FullName())
            .RuleFor(c => c.Email, f => f.Internet.Email())
            .Generate();

        var order = new Faker<Order>()
            .UseSeed(55)
            .RuleFor(c => c.CustomerName, customer.Name)
            .RuleFor(c => c.CustomerEmail, customer.Email)
            .RuleFor(c => c.Items, new Faker<OrderItem>()
                                    .UseSeed(55)
                                    .RuleFor(i => i.Quantity, 2)
                                    .RuleFor(i => i.UnitPrice, 400m)
                                    .Generate(1)
                                    .ToList())
            .Generate();

        var repo = new InMemoryOrderRepository();
        var inventory = Substitute.For<IInventoryService>();
        inventory.IsAvailableAsync(Arg.Any<Guid>(), Arg.Any<int>()).Returns(true);
        var notification = Substitute.For<INotificationService>();

        var checkout = new CheckoutService(repo, inventory, notification);


        // Act

        var receipt = await checkout.CheckoutAsync(order);


        // Assert
        receipt.CustomerName.ShouldBe(customer.Name);
        receipt.CustomerEmail.ShouldBe(customer.Email);

        repo.GetAll().Count.ShouldBe(1);
        await notification.Received(1).SendOrderConfirmationAsync(receipt);

    }
}


public class Customer
{
    public required string Name { get; init; } 
    public required string Email { get; init; }
}



