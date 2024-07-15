using Customs_Management_System.DBContexts.Models;
using Customs_Management_System.DTOs;

namespace Customs_Management_System.IRepository
{
    public interface ICustomsRepository
    {
       
        Task<string> CreateDeclarationAsync(DeclarationDto declaration);
      
        Task<List<MonitoringDto>> GetMonitoringsAsync( );

        // report part importer
        Task CreateReportAsync(ReportDto reportDto);
        
       

        //payment
        Task<IEnumerable<Declaration>> GetDeclarationsByUserIdAsync(int userId);
        Task AddPaymentAsync(Payment payment);


        //dashboard 
        Task<int> GetTotalDeclarationsAsync(int userId);
        Task<int> GetPendingPaymentsAsync(int userId);
        Task<int> GetShipmentMonitoringAsync(int userId);
        Task<int> GetGeneratedReportsAsync(int userId);
        Task<DashboardOverViewDto> GetDashboardOverviewAsync();


    }
}
