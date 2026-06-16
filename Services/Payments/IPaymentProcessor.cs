using MYGROCER.Models;

namespace MYGROCER.Services.Payments
{
    public interface IPaymentProcessor
    {
        Task<PaymentResult> ProcessPaymentAsync(decimal amount, IDictionary<string, string?> details);
    }
}
