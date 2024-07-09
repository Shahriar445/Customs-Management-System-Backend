using Customs_Management_System.DTOs;
using Customs_Management_System.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Customs_Management_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CMSController : ControllerBase
    {

        private static ICustomsRepository _customsRepo;
        private readonly ILogger<CMSController> _logger;
        
        public CMSController(ILogger<CMSController> logger, ICustomsRepository customsRepo)
        {
            _logger=logger;
            _customsRepo=customsRepo;
        }
        //----------------------------------------------Create Items --------------------------------------
        [HttpPost("/CreateDeclaration")]
        public async Task<IActionResult> CreateDeclaration(DeclarationDto declarationDto)
        {
            try
            {
                var result = await _customsRepo.CreateDeclaration(declarationDto);
                return StatusCode(StatusCodes.Status201Created, result);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }



    }
}
