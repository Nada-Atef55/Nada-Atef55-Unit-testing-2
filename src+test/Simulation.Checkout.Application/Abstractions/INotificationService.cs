using Simulation.Checkout.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation.Checkout.Application.Abstractions;

public interface INotificationService
{
    Task SendOrderConfirmationAsync(
        CheckoutReceipt receipt,
        CancellationToken cancellationToken = default);
}
