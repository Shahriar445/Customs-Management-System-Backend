namespace Customs_Management_System.DTOs
{
    
        public class ReportDto
    {
        public int ReportId { get; set; }
        public int UserId { get; set; }
        public string? ReportType { get; set; }
        public string? Content { get; set; }
        public DateTime? CreateAt { get; set; }


        // Including related details for Declaration, Product, and Shipment
        public DeclarationDto Declaration { get; set; } = null!;
       
    }
    
}
