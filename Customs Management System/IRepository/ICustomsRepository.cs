using Customs_Management_System.DBContexts.Models;
using Customs_Management_System.DTOs;

namespace Customs_Management_System.IRepository
{
    public interface ICustomsRepository
    {
       
        Task<string> CreateDeclarationAsync(DeclarationDto declaration);
        Task<string> GetDeclarationByIdAsync(int id); 
        Task<List<MonitoringDto>> GetMonitorings(MonitoringDto monitoringDto);
    }
}
