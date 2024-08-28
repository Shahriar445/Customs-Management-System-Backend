using Customs_Management_System.DBContexts.Models;
using Customs_Management_System.DTOs;

namespace Customs_Management_System.IRepository
{
    public interface ICustomsRepository
    {
       
        Task<string> CreateDeclarationAsync(DeclarationDto declaration);
      
        Task<List<MonitoringDto>> GetMonitoringsAsync( );
        Task<IEnumerable<ProductPriceDto>> GetProductsByCategoryAsync(string category);



        //payment
        Task<IEnumerable<Declaration>> GetDeclarationsByUserIdAsync(int userId);
        Task AddPaymentAsync(Payment payment);


       

        //dashboard 
       
        Task<DashboardOverViewDto> GetDashboardOverviewAsync(int userId);

        // --------------------Exporter part  

      
        Task<DashboardOverViewExporterDto> GetDashboardOverviewExporter();
        Task<string> CreateDeclarationExporter(DeclarationDto declaration);


        //----------------------- customes officer  ----------------


        Task<CustomesDashboardSummaryDto> GetImporterSummaryAsync();
        Task<CustomesDashboardSummaryDto> GetExporterSummaryAsync();
        IEnumerable<DeclarationDto> GetPendingDeclaration();
        IEnumerable<DeclarationDto> GetRunningDeclaration();



        // Customes officer Report part 
           Task<IEnumerable<ReportDto>> GetCustomsOfficerReportsAsync();
       
        byte[] GeneratePdfReport(IEnumerable<ReportDto> reports, string createdBy, DateTime reportDate);

        Task<IEnumerable<PaymentDto>> GetPaymentHistoryAsync();



    }
}
