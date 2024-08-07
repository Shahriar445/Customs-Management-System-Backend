﻿using Customs_Management_System.DbContexts;
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










        [HttpGet("reportsByRole")]
        public async Task<ActionResult<IEnumerable<ReportDto>>> GetReportsByRole()
        {
            try
            {
                var reports = await _customsRepo.GetReportsByRoleQueryable();
                return Ok(reports);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        //--------------------------------------------------------------dashboard api 

        [HttpGet("dashboardOverview")]
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



        //------------------------------------------------------------------Exporter api --------------------------------------------------------------------------------------------------
        /*                              Total  api 
                                                           1. Declaration submit---- 
                                                           2. get all Declaration--- 
                                                           3. payment submit --- 
                                                           4. get monitoring list--
                                                           5. create Report --
                                                           6. Dashboard -- 

        */

        [HttpPost("/Create-Declaration-Exporter")]
        public async Task<IActionResult> CreateDeclarationExporter(DeclarationDto declarationDto)
        {
            try
            {
                var result = await _customsRepo.CreateDeclarationExporter(declarationDto);
                return StatusCode(StatusCodes.Status201Created, result);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }


        //get all users 

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users.Take(1000).ToListAsync();
            return Ok(users);
        }

        //--------------------------------------work on admin api 

        [HttpGet("user-counts")]
        public async Task<IActionResult> GetUserCounts()
        {
            try
            {
                var totalExporters = await _context.Users.CountAsync(u => u.UserRole.RoleName.ToLower() == "exporter");
                var totalImporters = await _context.Users.CountAsync(u => u.UserRole.RoleName.ToLower() == "importer");
                var totalCustomsOfficers = await _context.Users.CountAsync(u => u.UserRole.RoleName.ToLower() == "customs officer");


                var activeUsers = await _context.Users.CountAsync(u => u.IsActive==true);
                var pendingApprovals = await _context.Users.CountAsync(u => u.IsActive==false);

                var result = new
                {
                    TotalExporters = totalExporters,
                    TotalImporters = totalImporters,
                    TotalCustomsOfficers = totalCustomsOfficers,
                    ActiveUsers = activeUsers,
                    PendingApprovals= pendingApprovals
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log error and return a server error status
                _logger.LogError(ex, "An error occurred while getting user counts.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving user counts.");
            }
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingUsers()
        {
            var pendingUsers = await _context.Users
                .Where(u => !u.IsActive)
                .Select(u => new
                {
                    u.UserId,
                    u.UserName,
                    u.Email,
                    u.UserRoleId,
                    Role = u.UserRoleId == 2 ? "Importer" : u.UserRoleId == 3 ? "Exporter" : u.UserRoleId == 4 ? "Customs Officer" : "Unknown"
                })
                .ToListAsync();

            return Ok(pendingUsers);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveUsers()
        {
            var activeUsers = await _context.Users
                .Where(u => u.IsActive)
                .Select(u => new
                {
                    u.UserId,
                    u.UserName,
                    u.Email,
                    u.UserRoleId,
                    Role = u.UserRoleId == 2 ? "Importer" : u.UserRoleId == 3 ? "Exporter" : u.UserRoleId == 4 ? "Customs Officer" : "Unknown"
                })
                .ToListAsync();

            return Ok(activeUsers);
        }

        [HttpPut("approve-user/{userId}")]
        public async Task<IActionResult> ApproveUser(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            user.IsActive = true;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User approved successfully" });
        }

        [HttpPut("stop-role/{userId}")]
        public async Task<IActionResult> StopUserRole(int userId)
        {
            // Fetch the user from the database
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Update the user's IsActive status to false
            user.IsActive = false;

            // Save changes to the database
            await _context.SaveChangesAsync();

            return Ok("User role stopped successfully. User moved back to pending approval.");
        }

    };
}

