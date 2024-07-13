using Customs_Management_System.DBContexts.Models;
using Customs_Management_System.DTOs;

namespace Customs_Management_System.IRepository
{
    public interface ICustomsRepository
    {
       
        Task<string> CreateDeclarationAsync(DeclarationDto declaration);
        Task<string> GetDeclarationByIdAsync(int id); 
        Task<List<MonitoringDto>> GetMonitoringsAsync( );

        // report part importer
        Task CreateReportAsync(ReportDto reportDto);
        Task<IEnumerable<ReportDto>> GetReportsAsync();
        Task<ReportDto> GetReportByIdAsync(int reportId);

        //payment
        Task<IEnumerable<Declaration>> GetDeclarationsByUserIdAsync(int userId);
        Task AddPaymentAsync(Payment payment);



    }
}
