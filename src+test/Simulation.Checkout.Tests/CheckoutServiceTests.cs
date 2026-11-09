using Moq;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using Shouldly;
using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;
using Simulation.Checkout.Domain.Exceptions;
using Simulation.Checkout.Infrastructur;

namespace Simulation.Checkout.Tests;

public class CheckoutServiceTests
{
    public static TheoryData<decimal, decimal> Subtotal_Discount => new()
    {
        {999.99m, 0m },
        {1000m, 100m },
        {1500m, 100m }
    };
    public static TheoryData<Order, decimal, decimal, decimal> Subtotal_Discount_Total => new()
    {
        { 
        new Order
        {
            CustomerName = "Nada",
            CustomerEmail = "nada@example.com",
            Items =
            [
                new OrderItem
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Laptop",
                    UnitPrice = 400m,
                    Quantity = 1
                },
                new OrderItem
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Mouse",
                    UnitPrice = 150m,
                    Quantity = 2
                }
            ]
        }
        , 700m, 0m, 700m
        },


        {
        new Order
        {
            CustomerName = "Nada",
            CustomerEmail = "nada@example.com",
            Items =
            [
                new OrderItem
                        {
                            ProductId = Guid.NewGuid(),
                            ProductName = "Laptop",
                            UnitPrice = 500,
                            Quantity = 3
                        }
            ]
        }
        , 1500m, 100m, 1400m 
        }
    };

    public static TheoryData<decimal> Price => new() { { -0.01m } };




    // Task 1 : 1
    [Fact]
    public async Task CheckoutAsync_EmptyOrder_ThrowsEmptyOrderException()
    {
        // Arrange
        var order = new Order
        {
            CustomerName = "Nada",
            CustomerEmail = "nada@example.com",
            Items = []
        };

        var inventory = Substitute.For<IInventoryService>();
        var notification = Substitute.For<INotificationService>();
        var repository = Substitute.For<IOrderRepository>();

        var checkout = new CheckoutService(repository, inventory, notification);

        // Act + Assert
        await Should.ThrowAsync<EmptyOrderException>(() => checkout.CheckoutAsync(order));


    }


    // Task 1 : 2
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CheckoutAsync_NegativeOrZeroQuantity_InvalidQuantityException(int quantity)
    {
        var order = new Order
        {
            CustomerName = "Nada",
            CustomerEmail = "nada@example.com",
            Items =
            [
                new OrderItem
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Laptop",
                    UnitPrice = 1200m,
                    Quantity = quantity
                },
                new OrderItem
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Mouse",
                    UnitPrice = 25m,
                    Quantity = 2
                }
            ]
        };

        var inventory = Substitute.For<IInventoryService>();
        var notification = Substitute.For<INotificationService>();
        var repository = Substitute.For<IOrderRepository>();

        var checkout = new CheckoutService(repository, inventory, notification);

        var exception = await Should.ThrowAsync<InvalidQuantityException>(() => checkout.CheckoutAsync(order));

        exception.Message.ShouldContain("Laptop");
    }



    // Task 1 : 3
    [Theory]
    [MemberData(nameof(Price))]
    public async Task CheckoutAsync_NegativePrice_ThrowsInvalidPriceException(decimal price)
    {
        var order = new Order
        {
            CustomerName = "Nada",
            CustomerEmail = "nada@example.com",
            Items =
            [
                new OrderItem
                        {
                            ProductId = Guid.NewGuid(),
                            ProductName = "Laptop",
                            UnitPrice = 1200m,
                            Quantity = 1
                        },
                        new OrderItem
                        {
                            ProductId = Guid.NewGuid(),
                            ProductName = "Mouse",
                            UnitPrice = price,
                            Quantity = 2
                        }
            ]
        };

        var inventory = Substitute.For<IInventoryService>();
        var notification = Substitute.For<INotificationService>();
        var repository = Substitute.For<IOrderRepository>();

        var checkout = new CheckoutService(repository, inventory, notification);

        await Should.ThrowAsync<InvalidPriceException>(() => checkout.CheckoutAsync(order));
    }



    // Task 1 : 4
    [Fact]
    public async Task CheckoutAsync_PriceZero_Valid()
    {
        var order = new Order
        {
            CustomerName = "Nada",
            CustomerEmail = "nada@example.com",
            Items =
            [
                new OrderItem
                                {
                                    ProductId = Guid.NewGuid(),
                                    ProductName = "Laptop",
                                    UnitPrice = 1200m,
                                    Quantity = 1
                                },
                                new OrderItem
                                {
                                    ProductId = Guid.NewGuid(),
                                    ProductName = "Mouse",
                                    UnitPrice = 0m,
                                    Quantity = 2
                                }
            ]
        };

        // i never test the inventory, i just need it to always return true 
        var inventory = Substitute.For<IInventoryService>();
        inventory.IsAvailableAsync(Arg.Any<Guid>(), Arg.Any<int>()).Returns(true);

        var notification = Substitute.For<INotificationService>();
        var repository = Substitute.For<IOrderRepository>();

        var checkout = new CheckoutService(repository, inventory, notification);

        await checkout.CheckoutAsync(order).ShouldNotBeNull();

    }



    // Task 1 : 5
    [Theory]
    [MemberData(nameof(Subtotal_Discount))]
    public async Task CheckoutAsync_Subtotal_CorrectDiscountAmount(decimal subtotal, decimal expectedDiscount)
    {

        // Arrange
        var order = new Order
        {
            CustomerName = "Nada",
            CustomerEmail = "nada@example.com",
            Items =
            [
                new OrderItem
                        {
                            ProductId = Guid.NewGuid(),
                            ProductName = "Laptop",
                            UnitPrice = subtotal,
                            Quantity = 1
                        }
            ]
        };

        var inventory = Substitute.For<IInventoryService>();
        inventory.IsAvailableAsync(Arg.Any<Guid>(), Arg.Any<int>()).Returns(true);

        var notification = Substitute.For<INotificationService>();
        var repository = Substitute.For<IOrderRepository>();

        var checkout = new CheckoutService(repository, inventory, notification);

        // Act
        var receipt = await checkout.CheckoutAsync(order);


        // Assert
        receipt.Discount.ShouldBe(expectedDiscount);

    }

    // Task 1 : 6
    [Theory]
    [MemberData(nameof(Subtotal_Discount_Total))]
    public async Task CheckoutAsync_CorrectSubtotalDiscountTotal(Order order, decimal subtotal, decimal discount, decimal total)
    {
        var inventory = Substitute.For<IInventoryService>();
        inventory.IsAvailableAsync(Arg.Any<Guid>(), Arg.Any<int>()).Returns(true);

        var notification = Substitute.For<INotificationService>();
        var repository = Substitute.For<IOrderRepository>();

        var checkout = new CheckoutService(repository, inventory, notification);

        // Act
        var receipt = await checkout.CheckoutAsync(order);

        // Assert
        receipt.Subtotal.ShouldBe(subtotal);
        receipt.Discount.ShouldBe(discount);
        receipt.Total.ShouldBe(total);
    }


    // Task 2 : 1

    [Fact]
    public async Task CheckoutAsync_ValidOrder_SavedAndConfirmed()
    {
        // Arrange
        var order = new Order
        {
            CustomerName = "Nada",
            CustomerEmail = "nada@example.com",
            Items =
            [
                new OrderItem
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Laptop",
                    UnitPrice = 1200,
                    Quantity = 1
                }
            ]
        };

        var notification = new Mock<INotificationService>();
        var inventory = new Mock<IInventoryService>();
        inventory.Setup(x => x.IsAvailableAsync(It.IsAny<Guid>(), It.IsAny<int>())).ReturnsAsync(true);
        var repository = new InMemoryOrderRepository();

        var checkout = new CheckoutService(repository, inventory.Object, notification.Object);

        // Act
        await checkout.CheckoutAsync(order);


        // Assert
        repository.GetAll().Count.ShouldBe(1);
        notification.Verify(x => x.SendOrderConfirmationAsync(It.IsAny<CheckoutReceipt>()), Times.Once);
    }



    // Task 2 : 2
    [Fact]
    public async Task CheckoutAsync_ItemOutOfStock_Throws()
    {
        // Arrange
        var order = new Order
        {
            CustomerName = "Nada",
            CustomerEmail = "nada@example.com",
            Items =
            [
                new OrderItem
                        {
                            ProductId = Guid.NewGuid(),
                            ProductName = "Phone",
                            UnitPrice = 800,
                            Quantity = 1
                        }
            ]
        };



        var notification = new Mock<INotificationService>();
        var inventory = new Mock<IInventoryService>();
        inventory.Setup(x => x.IsAvailableAsync(It.IsAny<Guid>(), It.IsAny<int>())).ReturnsAsync(false);
        var repository = new Mock<IOrderRepository>();

        var checkout = new CheckoutService(repository.Object, inventory.Object, notification.Object);

        // Act + Assert
        await Should.ThrowAsync<InsufficientInventoryException>(() => checkout.CheckoutAsync(order));
        repository.Verify(x => x.SaveAsync(order), Times.Never);
        notification.Verify(x => x.SendOrderConfirmationAsync(It.IsAny<CheckoutReceipt>()), Times.Never);

    }


    // Task 2 : 3
    [Fact]
    public async Task CheckoutAsync_InvalidOrder_ThrowsAndNotSavedOrConfirmed()
    {
        // Arrange
        var order = new Order
        {
            CustomerName = "Nada",
            CustomerEmail = "nada@example.com",
            Items =
            [
                new OrderItem
                        {
                            ProductId = Guid.NewGuid(),
                            ProductName = "NoteBook",
                            UnitPrice = 120m,
                            Quantity = 0
                        }
            ]
        };



        var notification = new Mock<INotificationService>();
        var inventory = new Mock<IInventoryService>();
        var repository = new Mock<IOrderRepository>();

        var checkout = new CheckoutService(repository.Object, inventory.Object, notification.Object);

        // Act + Assert
        await Should.ThrowAsync<InvalidQuantityException>(() => checkout.CheckoutAsync(order));
        inventory.Verify(x => x.IsAvailableAsync(It.IsAny<Guid>(), It.IsAny<int>()), Times.Never);
        repository.Verify(x => x.SaveAsync(order), Times.Never);
        notification.Verify(x => x.SendOrderConfirmationAsync(It.IsAny<CheckoutReceipt>()), Times.Never);

    }



}
