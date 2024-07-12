namespace Customs_Management_System.DTOs
{
    
        public class ReportDto
    {
        public int ReportId { get; set; }
        public int UserId { get; set; }
        public string? ReportType { get; set; }
        public string? Content { get; set; }
        public DateTime? CreateAt { get; set; }

        public DeclarationDto DeclarationId { get; set; } = null!; 

        // Including related details for Declaration, Product, and Shipment
        public DeclarationDto Declaration { get; set; } = null!;
        public List<ProductDto> Products { get; set; } = new List<ProductDto>();
        public List<ShipmentDto> Shipments { get; set; } = new List<ShipmentDto>();
    }
    
}
