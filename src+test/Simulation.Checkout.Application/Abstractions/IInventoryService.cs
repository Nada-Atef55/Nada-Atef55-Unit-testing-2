using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation.Checkout.Application.Abstractions;

public interface IInventoryService
{
    Task<bool> IsAvailableAsync(
        Guid productId,
        int quantity,
        CancellationToken cancellationToken = default);
}
