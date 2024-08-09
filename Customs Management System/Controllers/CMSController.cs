using Customs_Management_System.DbContexts;
using Customs_Management_System.DBContexts.Models;
using Customs_Management_System.DTOs;
using Customs_Management_System.IRepository;
using Customs_Management_System.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
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
                                                            5. for Dashboard -- done
         
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
        [Authorize(Roles = "Importer")]
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
        [HttpGet("GetProductsByCategory")]
        public async Task<IActionResult> GetProductsByCategory([FromQuery] string category)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                return BadRequest("Category is required");
            }

            var products = await _customsRepo.GetProductsByCategoryAsync(category);

            if (products == null || !products.Any())
            {
                return NotFound("No products found for the selected category");
            }

            return Ok(products);
        }
        //product price and category 
        [HttpGet("GetPrice")]
        public async Task<IActionResult> GetPrice(string category, string productName)
        {
            var price = await _context.ProductPrices
                .Where(p => p.Category == category && p.ProductName == productName)
                .Select(p => p.Price)
                .FirstOrDefaultAsync();

            if (price == 0)
            {
                return NotFound();
            }

            return Ok(price);
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




        //-------------------------------------------------------------- importer dashboard api 
        
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
                                                           1. Declaration submit---- done
                                                           2. get all Declaration--- 
                                                           3. payment submit --- 
                                                           4. get monitoring list--
                                                           5. create Report --
                                                           6. Dashboard -- 

        */


        [HttpGet("/Exporter_Dashboard")]
        public async Task<ActionResult<DashboardOverViewDto>> GetDashboardOverviewForExporter()
        {
            try
            {
                var dashboardOverview = await _customsRepo.GetDashboardOverviewExporter();
                return Ok(dashboardOverview);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("/Create-Declaration-Exporter")]
        public async Task<IActionResult> CreateDeclarationExporters(DeclarationDto declarationDto)
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



        [HttpGet("/Exporter-monitoring")]
        public async Task<ActionResult<ExporterMonitorDto>> GetMonitoringOverview()
        {
            try
            {
                // Role ID for Exporters
                int exporterRoleId = 3;

                // Get the UserIds of all users with RoleId == 3
                var exporterUserIds = await _context.Users
                                                    .Where(u => u.UserRoleId == exporterRoleId)
                                                    .Select(u => u.UserId)
                                                    .ToListAsync();

                if (exporterUserIds == null || !exporterUserIds.Any())
                {
                    return NotFound("No users found with the role of Exporter.");
                }

                // Total processed shipments for Exporters
                int processedShipments = await _context.Declarations
                                                        .Where(d => d.IsActive == true && exporterUserIds.Contains(d.UserId))
                                                        .CountAsync();

                // Total pending shipments for Exporters
                int pendingShipments = await _context.Declarations
                                                     .Where(d => d.IsActive == false && exporterUserIds.Contains(d.UserId))
                                                     .CountAsync();

                // Define a custom status based on your criteria
                string currentStatus = processedShipments > 0 ? "Operational" : "Not Operational";

                // Example clearance rate calculation
                double clearanceRate = (processedShipments + pendingShipments) > 0
                    ? (double)processedShipments / (processedShipments + pendingShipments) * 100
                    : 0.0;

                var monitoringData = new ExporterMonitorDto
                {
                    ShipmentsProcessed = processedShipments,
                    ShipmentPending = pendingShipments,
                    CurrentStatus = currentStatus,
                    CustomsClearanceRate = clearanceRate
                };

                return Ok(monitoringData);
            }
            catch (Exception ex)
            {
                // Log the exception details (optional)
                // _logger.LogError(ex, "An error occurred while fetching the monitoring overview.");

                // Return a 500 Internal Server Error with a custom message
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }


        //get all users 

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users.Take(1000).ToListAsync();
            return Ok(users);
        }



        //--------------------------------------work on admin api -----------------------------------------------------------------

        [Authorize(Roles = "Admin")]
        [HttpGet("user-counts")]
        
        public async Task<IActionResult> GetUserCounts()
        {
            try
            {
                var totalExporters = await _context.Users.CountAsync(u => u.UserRole.RoleName.ToLower() == "exporter" && u.IsActive==true);
                var totalImporters = await _context.Users.CountAsync(u => u.UserRole.RoleName.ToLower() == "importer"&& u.IsActive==true);
                var totalCustomsOfficers = await _context.Users.CountAsync(u => u.UserRole.RoleName.ToLower() == "customs officer" && u.IsActive == true);


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

        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin")]
        [HttpGet("user-reports")]
       

        public async Task<IActionResult> GetUserReports(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Fetch declarations and payments for the user
            var declarations = await _context.Declarations
                .Where(d => d.UserId == userId)
                .ToListAsync();

            var payments = await _context.Payments
                .Where(p => p.UserId == userId)
                .ToListAsync();

            var report = new
            {
                UserId = user.UserId,
                UserName = user.UserName,
                Role = await _context.Roles.Where(r => r.RoleId == user.UserRoleId).Select(r => r.RoleName).FirstOrDefaultAsync(),
                Declarations = declarations.Select(d => new
                {
                    d.DeclarationId,
                    d.DeclarationDate,
                    IsActive = d.IsActive ? 1 : 0
                }),
                Payments = payments.Select(p => new
                {
                    p.PaymentId,
                    p.DeclarationId,
                    p.Amount,
                    p.Date,
                    p.Status
                })
            };

            return Ok(report);
        }





    };
}

