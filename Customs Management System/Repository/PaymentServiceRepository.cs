using Customs_Management_System.DbContexts;
using Customs_Management_System.DBContexts.Models;
using Customs_Management_System.DTOs;
using Customs_Management_System.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Customs_Management_System.Repository
{
    public class PaymentServiceRepository : IPaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _storeId;
        private readonly string _storePassword;
        private readonly CMSDbContext _context;
       

        public PaymentServiceRepository(IConfiguration configuration, HttpClient httpClient, CMSDbContext context)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _baseUrl = _configuration["SSLCommerz:BaseUrl"];
            _storeId = _configuration["SSLCommerz:StoreID"];
            _storePassword = _configuration["SSLCommerz:StorePassword"];
            _context = context;
          
        }

        public async Task<string> InitiatePaymentAsync(int declarationId, string transactionId, string successUrl, string failUrl, string cancelUrl)
        {
            decimal totalAmount = await GetTotalAmountByDeclarationAsync(declarationId);

            // Retrieve declaration along with user details
            var declaration = await _context.Declarations
                .Include(d => d.User)  // Ensure user data is loaded
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.DeclarationId == declarationId);

            if (declaration == null)
            {
                throw new Exception("Declaration not found.");
            }

            // Extract customer information
            string customerName = declaration.User?.UserName ?? "Default Name"; // Use UserName from Users table
            string email = declaration.User?.Email ?? "customer@example.com"; // Use Email from Users table



            var postData = new Dictionary<string, string>
            {
                { "store_id", _storeId },
                { "store_passwd", _storePassword },
                { "total_amount", totalAmount.ToString("F2") },
                { "currency", "USD" },
                { "tran_id", transactionId },
                { "success_url", successUrl },
                { "fail_url", failUrl },
                { "cancel_url", cancelUrl },
                { "cus_name", customerName },
                { "cus_email", email },
                { "cus_add1", "Address Line 1" },
                { "cus_phone", "01700000000" }
            };

            var content = new FormUrlEncodedContent(postData);
            var response = await _httpClient.PostAsync($"{_baseUrl}/gwprocess/v3/api.php", content);
            var responseString = await response.Content.ReadAsStringAsync();

            var responseData = JObject.Parse(responseString);
            if (responseData["status"].ToString() == "SUCCESS")
            {
                return responseData["GatewayPageURL"].ToString();
            }

            throw new Exception("Failed to initiate payment.");
        }



        public async Task<decimal> GetTotalAmountByDeclarationAsync(int declarationId)
        {
            return (decimal)await _context.Products
                .Where(p => p.DeclarationId == declarationId)
                .SumAsync(p => p.TotalPrice);
        }

        public async Task UpdateProductPaymentStatusAsync(int declarationId)
        {
            var products = await _context.Products
                .Where(p => p.DeclarationId == declarationId)
                .ToListAsync();

            

            foreach (var product in products)
            {
                product.IsPayment = true; // Assuming IsPayment is a boolean field
            }

            _context.Products.UpdateRange(products);

            var declaration = await _context.Declarations
                .FirstOrDefaultAsync(d => d.DeclarationId == declarationId);

            if (declaration != null)
            {
                declaration.IsPayment = true; // Assuming IsPayment is a boolean field
                _context.Declarations.Update(declaration);
            }

            await _context.SaveChangesAsync();
        }


        public async Task<int> GetUserIdByDeclarationIdAsync(int declarationId)
        {
            var declaration = await _context.Declarations
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.DeclarationId == declarationId);

            if (declaration == null)
            {
                throw new Exception("Declaration not found.");
            }

            return declaration.UserId; // Assuming UserId is a property of the Declaration entity
        }
    



}
}
