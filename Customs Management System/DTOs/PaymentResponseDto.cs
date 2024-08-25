namespace Customs_Management_System.DTOs
{
    public class PaymentResponseDto
    {
        public string Status { get; set; } // e.g., "SUCCESS", "FAILED"
        public string TransactionId { get; set; }
        public int DeclarationId { get; set; }
        public string GatewayPageURL { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorCode { get; set; }
        public DateTime ResponseDate { get; set; }
    }
}
