namespace Customs_Management_System.IRepository
{
    public interface IPaymentService
    {
        Task<string> InitiatePaymentAsync(decimal amount, string transactionId, string successUrl, string failUrl, string cancelUrl);
    }
}
