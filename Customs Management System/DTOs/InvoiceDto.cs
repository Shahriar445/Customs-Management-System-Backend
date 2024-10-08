namespace Customs_Management_System.DTOs;

    public class InvoiceDto
    {
    public int InvoiceId { get; set; }
    public int UserId { get; set; }
    public int DeclarationId { get; set; }
    public decimal Amount { get; set; }
    public DateTime InvoiceDate { get; set; }
    public string PaymentMethod { get; set; }
    public string Currency { get; set; }
}

