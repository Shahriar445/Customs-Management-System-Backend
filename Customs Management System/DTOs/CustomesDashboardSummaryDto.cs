namespace Customs_Management_System.DTOs
{
    public class CustomesDashboardSummaryDto
    {
        public int TotalDeclarations { get; set; }
        public int PendingShipments { get; set; }
        public int RunningShipments { get; set; }
       public int  CompletedShipments { get; set; }
    }
}
