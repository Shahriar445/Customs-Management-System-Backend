namespace Customs_Management_System.DTOs
{
    public class ExporterMonitorDto
    {
        public int ShipmentsProcessed { get; set; }
        public int ShipmentPending { get; set; }
        public string? CurrentStatus { get; set; }
        public double CustomsClearanceRate { get; set; }
    }
}
