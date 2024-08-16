namespace Customs_Management_System.DTOs
{
    
        public class ReportDto
    {
        public int DeclarationId { get; set; }
        public string UserName { get; set; }
        public string RoleName { get; set; }
        public DateTime DeclarationDate { get; set; }
        public string? Status { get; set; }
        public decimal Amount { get; set; }
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal UnitPrice { get; set; }


        // Including related details for Declaration, Product, and Shipment




    }
    
}
