using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Simulation.Checkout.Infrastructur;

public sealed class InMemoryOrderRepository : IOrderRepository
{
    private readonly ConcurrentDictionary<Guid, Order> _orders = new();

    public Task SaveAsync(Order order, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(order);
        cancellationToken.ThrowIfCancellationRequested();

        _orders[order.Id] = order;
        return Task.CompletedTask;
    }

    public IReadOnlyCollection<Order> GetAll() => _orders.Values.ToList();
}
