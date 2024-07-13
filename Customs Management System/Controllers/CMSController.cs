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

        [HttpGet("GetMonitorings")]
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


        // reate report

        [HttpPost("CreateReport")]
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

        [HttpGet("GetReports")]
        public async Task<IActionResult> GetReports()
        {
            try
            {
                var reports = await _customsRepo.GetReportsAsync();
                return Ok(reports);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error retrieving reports");
                return StatusCode(500, "An error occurred while retrieving reports");
            }
        }

        [HttpGet("GetReport/{reportId}")]
        public async Task<IActionResult> GetReport(int reportId)
        {
            try
            {
                var report = await _customsRepo.GetReportByIdAsync(reportId);
                if (report == null)
                {
                    return NotFound();
                }
                return Ok(report);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error retrieving report");
                return StatusCode(500, "An error occurred while retrieving the report");
            }
        }


        //payment 

        [HttpGet("GetDeclarationsByUserId/{userId}")]
        public async Task<ActionResult<IEnumerable<DeclarationDto>>> GetDeclarationsByUserIdAsync(int userId)
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

    }


};

