using MYGROCER.Models;

namespace MYGROCER.Services.Payments
{
    public class CardProcessor : IPaymentProcessor
    {
        public Task<PaymentResult> ProcessPaymentAsync(decimal amount, IDictionary<string, string?> details)
        {
            // Very simple validation simulation
            if (!details.ContainsKey("cardNumber") || string.IsNullOrEmpty(details["cardNumber"]))
            {
                return Task.FromResult(new PaymentResult { Success = false, Message = "Card number missing." });
            }

            return Task.FromResult(new PaymentResult
            {
                Success = true,
                Message = "Card charged (simulated).",
                TransactionId = "CARD-" + Guid.NewGuid().ToString("N").Substring(0, 8)
            });
        }
    }
}
