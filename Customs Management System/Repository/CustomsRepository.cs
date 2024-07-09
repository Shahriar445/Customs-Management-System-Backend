using Customs_Management_System.DbContexts;
using Customs_Management_System.DTOs;
using Customs_Management_System.IRepository;

namespace Customs_Management_System.Repository
{
    public class CustomsRepository : ICustomsRepository
    {
        private readonly CMSDbContext _context;
        private readonly IConfiguration _configuration;

        public CustomsRepository(CMSDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public Task<string> CreateDeclaration(DeclarationDto declarationDto)
        {
            throw new NotImplementedException();
        }
    }
}
