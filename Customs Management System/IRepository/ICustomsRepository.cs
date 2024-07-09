using Customs_Management_System.DBContexts.Models;
using Customs_Management_System.DTOs;

namespace Customs_Management_System.IRepository
{
    public interface ICustomsRepository
    {
        Task<string> CreateDeclaration(DeclarationDto declarationId);
        
    }
}
