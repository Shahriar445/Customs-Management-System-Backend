using Customs_Management_System.IRepository;
using Newtonsoft.Json.Linq;

namespace Customs_Management_System.Repository
{
    public class PaymentServiceRepository:IPaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public PaymentServiceRepository(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;

        }

        public async Task<string> InitiatePaymentAsync(decimal amount, string transactionId, string successUrl, string failUrl, string cancelUrl)
        {
            var storeId = _configuration["SSLCommerz:StoreID"];
            var storePassword = _configuration["SSLCommerz:StorePassword"];
            var baseUrl = _configuration["SSLCommerz:BaseUrl"];

            var postData = new Dictionary<string, string>
        {
            { "store_id", storeId },
            { "store_passwd", storePassword },
            { "total_amount", amount.ToString("F2") },
            { "currency", "BDT" },
            { "tran_id", transactionId },
            { "success_url", successUrl },
            { "fail_url", failUrl },
            { "cancel_url", cancelUrl },
            { "cus_name", "Customer Name" },
            { "cus_email", "customer@example.com" },
            { "cus_add1", "Address Line 1" },
            { "cus_phone", "01700000000" }
        };

            var content = new FormUrlEncodedContent(postData);
            var response = await _httpClient.PostAsync($"{baseUrl}/gwprocess/v3/api.php", content);
            var responseString = await response.Content.ReadAsStringAsync();

            var responseData = JObject.Parse(responseString);
            if (responseData["status"].ToString() == "SUCCESS")
            {
                return responseData["GatewayPageURL"].ToString();
            }

            throw new Exception("Failed to initiate payment.");
        }
    }
}
