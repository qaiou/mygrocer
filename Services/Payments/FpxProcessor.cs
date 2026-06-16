using MYGROCER.Models;

namespace MYGROCER.Services.Payments
{
    public class FpxProcessor : IPaymentProcessor
    {
        public Task<PaymentResult> ProcessPaymentAsync(decimal amount, IDictionary<string, string?> details)
        {
            // Dummy success simulation for FPX
            return Task.FromResult(new PaymentResult
            {
                Success = true,
                Message = "FPX payment simulated successfully.",
                TransactionId = "FPX-" + Guid.NewGuid().ToString("N").Substring(0, 8)
            });
        }
    }
}
