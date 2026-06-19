using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace MYGROCER.Services.Payments
{
    public enum PaymentMethod
    {
        Fpx,
        Card
    }

    public class PaymentFactory
    {
        private readonly IServiceProvider _services;
        private readonly Dictionary<PaymentMethod, Type> _map = new()
        {
            { PaymentMethod.Fpx, typeof(FpxProcessor) },
            { PaymentMethod.Card, typeof(CardProcessor) }
        };

        public PaymentFactory(IServiceProvider services) => _services = services;

        //returns the payment method selected
        public IPaymentProcessor Create(PaymentMethod method)
        {
            if (!_map.TryGetValue(method, out var implType))
                throw new NotSupportedException($"Payment method '{method}' is not supported.");

            return (IPaymentProcessor)_services.GetRequiredService(implType);
        }
}
    }
