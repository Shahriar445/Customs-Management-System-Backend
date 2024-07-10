using Customs_Management_System.DBContexts.Models;
using Customs_Management_System.DTOs;
using Customs_Management_System.IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
                var result = await _customsRepo.CreateDeclarationAsync(declarationDto);
                return StatusCode(StatusCodes.Status201Created, result);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }
        // GET: api/Monitoring
        [HttpGet("GetMonitorings")]
        public async Task<ActionResult> GetMonitorings(MonitoringDto monitoringdto)
        {
            try
            {
                var result = await _customsRepo.GetMonitorings(monitoringdto);
                return StatusCode(StatusCodes.Status201Created, result);

               
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving monitorings: {ex.Message}");
            }
        }






    }
}
