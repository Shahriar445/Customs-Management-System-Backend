namespace Customs_Management_System.DTOs
{
    public class DashboardOverViewDto
    {
        public int TotalDeclarations { get; set; }
        public int PendingPayments { get; set; }
        public int ShipmentMonitoring { get; set; }
        public int TotalRunningShipmet { get; set; }
        public int TotalCompletedShipment { get; set; }
        public int TotalRejectedShipment { get; set; }
        public int TotalPendingShipment { get; set; }

    }
}
