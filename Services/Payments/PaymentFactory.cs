using MYGROCER.Models;

namespace MYGROCER.Services.Payments
{
    public class PaymentFactory
    {
        private readonly IServiceProvider _services;

        public PaymentFactory(IServiceProvider services)
        {
            _services = services;
        }

        public IPaymentProcessor? Create(string method)
        {
            return method?.ToLower() switch
            {
                "fpx" => _services.GetService(typeof(FpxProcessor)) as IPaymentProcessor,
                "card" => _services.GetService(typeof(CardProcessor)) as IPaymentProcessor,
                _ => null
            };
        }
    }
}
