using Customs_Management_System.DbContexts;
using Customs_Management_System.DBContexts.Models;
using Customs_Management_System.DTOs;
using Customs_Management_System.IRepository;
using Customs_Management_System.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Customs_Management_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CMSController : ControllerBase
    {
        private readonly CMSDbContext _context;
        private static ICustomsRepository _customsRepo;
        private readonly ILogger<CMSController> _logger;

        public CMSController(ILogger<CMSController> logger, ICustomsRepository customsRepo, CMSDbContext context)
        {
            _logger=logger;
            _customsRepo=customsRepo;
            _context=context;

        }
        //----------------------------------------------Importer Api  --------------------------------------

        /*                              Total 6 api 
                                                            1. Declaration submit---- done
                                                            2. get all Declaration--- done
                                                            3. payment submit --- bug
                                                            4. get monitoring list-- done
                                                            5. create Report -- bug
                                                            6. for Dashboard -- remain
         
         */
        [HttpPost("/CreateDeclarationImporter")]
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
        [HttpGet("GetDeclarationsImporter")]
        public async Task<ActionResult<IEnumerable<DeclarationDto>>> GetDeclarations()
        {
            try
            {
                var declarations = await _context.Declarations
                    .Include(d => d.Products)
                    .Include(d => d.Shipments)
                    .Select(d => new DeclarationDto
                    {
                        DeclarationId = d.DeclarationId,
                        UserId = d.UserId,
                        DeclarationDate = d.DeclarationDate,
                        Status = d.Status,
                        // Add other properties from Products and Shipments as needed
                    })
                    .ToListAsync();

                return Ok(declarations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        [HttpGet("GetMonitoringImporter")]
        public async Task<ActionResult<List<MonitoringDto>>> GetMonitorings()
        {
            try
            {
                var monitorings = await _customsRepo.GetMonitoringsAsync();
                return Ok(monitorings);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }


        [HttpPost("CreateReportImporter")]
        public async Task<IActionResult> CreateReport([FromBody] ReportDto reportDto)
        {
            try
            {
                await _customsRepo.CreateReportAsync(reportDto);
                return StatusCode(201, "Report created successfully");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating report");
                return StatusCode(500, "An error occurred while creating the report");
            }
        }

        // For Payment & Report

        [HttpGet("GetDeclarationsByUserIdImporter/{userId}")]
        public async Task<ActionResult<IEnumerable<DeclarationDto>>> GetDeclarationsByUserIdAsync(int userId)
        {
            try
            {
                var declarations = await _customsRepo.GetDeclarationsByUserIdAsync(userId);

                var declarationDtos = declarations.Select(d => new DeclarationDto
                {
                    DeclarationId = d.DeclarationId,
                    DeclarationDate = d.DeclarationDate,
                    Status = d.Status,
                    Products = d.Products.Select(p => new ProductDto
                    {

                        ProductName = p.ProductName,
                        Quantity = p.Quantity,
                        // Add other properties as needed
                    }).ToList()
                }).ToList();

                return Ok(declarationDtos);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
          
        }

        [HttpPost("SubmitPayment")]
        public async Task<ActionResult> SubmitPayment(PaymentDto paymentDto)
        {
            var payment = new Payment
            {
                UserId = paymentDto.UserId,
                DeclarationId = paymentDto.DeclarationId,
                ProductId = paymentDto.ProductId,
                Amount = paymentDto.Amount,
                Date = DateTime.UtcNow,
                Status = "Pending" // Or whatever initial status you need
            };

            await _customsRepo.AddPaymentAsync(payment);

            return Ok(new { Message = "Payment submitted successfully" });
        }


        // ------------------------------------------------------------------------------------------------------------------------------------------------------------
        [HttpGet("dashboard-overview")]
        public async Task<ActionResult<DashboardOverViewDto>> GetDashboardOverviewForImporters()
        {
            try
            {
                var dashboardOverview = await _customsRepo.GetDashboardOverviewAsync();
                return Ok(dashboardOverview);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }































    }
};

