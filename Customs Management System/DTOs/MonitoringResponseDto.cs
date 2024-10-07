namespace Customs_Management_System.DTOs;

    public class MonitoringResponseDto
    {
        public string Username { get; set; }
        public string ProductName { get; set; }
        public int DeclarationId { get; set; }
        public int MonitoringId { get; set; }
        public string Status { get; set; }
        public int Quantity { get; set; } 
        public decimal TotalPrice { get; set; } 
    }


