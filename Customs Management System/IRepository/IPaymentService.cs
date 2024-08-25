using Customs_Management_System.DTOs;

namespace Customs_Management_System.IRepository
{
    public interface IPaymentService
    {
        Task<string> InitiatePaymentAsync(int declarationId, string transactionId, string successUrl, string failUrl, string cancelUrl);
        Task<decimal> GetTotalAmountByDeclarationAsync(int declarationId);
        Task UpdateProductPaymentStatusAsync(int declarationId);
        Task<int> GetUserIdByDeclarationIdAsync(int declarationId); // Add this line


    }
}
