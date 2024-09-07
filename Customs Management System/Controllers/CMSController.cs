using Customs_Management_System.DbContexts;
using Customs_Management_System.DBContexts.Models;
using Customs_Management_System.DTOs;
using Customs_Management_System.IRepository;
using Customs_Management_System.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Reflection.Metadata;
using iTextSharp.text.pdf;


using Document = System.Reflection.Metadata.Document;
using iTextSharp.text;
using System.Xml.Linq;
using PdfSharp.Pdf;
using PdfSharp.Drawing;

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
                        TotalPrice = p.TotalPrice,
                        Weight = p.Weight,
                        Category=p.Category,
                        Hscode = p.Hscode,
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

        [HttpGet("dashboardOverview/{userId}")]
        public async Task<ActionResult<DashboardOverViewDto>> GetDashboardOverviewForImporter(int userId)
        {
            try
            {
                var dashboardOverview = await _customsRepo.GetDashboardOverviewAsync(userId);
                return Ok(dashboardOverview);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/ShipmentDetails/country/USA
        [HttpGet("/country/{country}")]
        public async Task<ActionResult<IEnumerable<ShipmentDetailsDto>>> GetPortsByCountry(string country)
        {
            var shipmentDetails = await _customsRepo.GetPortsByCountryAsync(country);

            if (shipmentDetails == null || !shipmentDetails.Any())
            {
                return NotFound();
            }

            return Ok(shipmentDetails);
        }
        [HttpGet("/Getcountries")]
        public async Task<IActionResult> GetAllPortCountry()
        {
            var countrys = await _customsRepo.GetAllCountriesAsync();
            return Ok(countrys);
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

        [HttpPost("/CreateDeclarationExporter")]
        public async Task<IActionResult> CreateDeclarationExporters(DeclarationDto declarationDto)
        {
            try
            {
                var result = await _customsRepo.CreateDeclarationExporters(declarationDto);
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
                    PendingApprovals = pendingApprovals
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


        //----------------------------------customes officer api -------------

        // API for Exporters
        [HttpGet("importer-summary")]
        public async Task<ActionResult<CustomesDashboardSummaryDto>> GetImporterSummary()
        {
            var summary = await _customsRepo.GetImporterSummaryAsync();
            return Ok(summary);
        }

        [HttpGet("exporter-summary")]
        public async Task<ActionResult<CustomesDashboardSummaryDto>> GetExporterSummary()
        {
            var summary = await _customsRepo.GetExporterSummaryAsync();
            return Ok(summary);
        }

        [HttpGet("PendingDeclarations")]
        public IActionResult GetPendingDeclarations()
        {
            var pendingDeclarations = _customsRepo.GetPendingDeclaration();
            return Ok(pendingDeclarations);
        }

        // GET: api/CustomsOfficer/RunningDeclarations
        [HttpGet("RunningDeclarations")]
        public IActionResult GetRunningDeclarations()
        {
            var runningDeclarations = _customsRepo.GetRunningDeclaration();
            return Ok(runningDeclarations);
        }

        //For co permisition 

        [HttpGet("PendingShipments")]
        public async Task<IActionResult> GetPendingShipments()
        {
            var pendingShipments = await _context.Shipments
                .Include(s => s.Declaration)
                .Where(s => s.Status == "Pending")
                .Select(s => new
                {
                    s.ShipmentId,
                    s.DeclarationId,
                    s.MethodOfShipment,
                    s.PortOfDeparture,
                    s.PortOfDestination,
                    s.DepartureDate,
                    s.ArrivalDate,
                    DeclarationStatus = s.Declaration.Status,
                    s.Declaration.UserId,
                    Payments = _context.Payments
                        .Where(p => p.DeclarationId == s.DeclarationId && p.Status == "Pending")
                        .Select(p => new { p.PaymentId, p.Amount, p.Date })
                        .ToList() // Convert IQueryable to List
                })
                .ToListAsync();

            return Ok(pendingShipments);
        }

        [HttpGet("GetRunningShipments")]
        public async Task<IActionResult> GetRunningShipments()
        {
            var runningShipments = _context.Shipments
                .Where(s => s.Status == "Running")
                .Select(s => new
                {
                    s.ShipmentId,
                    s.DeclarationId,
                    s.MethodOfShipment,
                    s.PortOfDeparture,
                    s.PortOfDestination,
                    s.DepartureDate,
                    s.ArrivalDate,

                })
                .ToList();

            return Ok(runningShipments);
        }

        // GET: api/CMS/GetRejectedShipments
        [HttpGet("GetRejectedShipments")]
        public async Task<IActionResult> GetRejectedShipments()
        {
            var rejectedShipments = _context.Shipments
                .Where(s => s.Status == "Rejected")
                .Select(s => new
                {
                    s.ShipmentId,
                    s.DeclarationId,
                    s.MethodOfShipment,
                    s.PortOfDeparture,
                    s.PortOfDestination,
                    s.DepartureDate,
                    s.ArrivalDate
                })
                .ToList();

            return Ok(rejectedShipments);
        }


        [HttpGet("GetCompleteShipments")]
        public async Task<IActionResult> GetCompleteShipments()
        {
            var shipments = from s in _context.Shipments
                            join d in _context.Declarations on s.DeclarationId equals d.DeclarationId
                            join u in _context.Users on d.UserId equals u.UserId
                            join r in _context.Roles on u.UserRoleId equals r.RoleId
                            join p in _context.Payments on new { d.UserId, d.DeclarationId } equals new { p.UserId, p.DeclarationId } into payments
                            from payment in payments.DefaultIfEmpty() // Left join to handle cases with no payments
                            select new
                            {
                                s.ShipmentId,
                                s.DeclarationId,
                                s.MethodOfShipment,
                                s.PortOfDeparture,
                                s.PortOfDestination,
                                DepartureDate = s.DepartureDate.ToString("yyyy-MM-dd"),
                                ArrivalDate = s.ArrivalDate.ToString("yyyy-MM-dd"),
                                ShipmentStatus = s.Status,
                                UserName = u.UserName,
                                UserRole = r.RoleName,
                                DeclarationStatus = d.Status,
                                PaymentStatus = payment != null ? payment.Status : "No Payment"
                            };

            var result = await shipments.ToListAsync();
            return Ok(result);
        }

        // POST: api/CustomsOfficer/ApproveShipment/{shipmentId}
        [HttpPost("ApproveShipment/{shipmentId}")]
        public async Task<IActionResult> ApproveShipment(int shipmentId)
        {
            var shipment = await _context.Shipments
                .Include(s => s.Declaration)
                .FirstOrDefaultAsync(s => s.ShipmentId == shipmentId);

            if (shipment == null)
            {
                return NotFound("Shipment not found.");
            }

            var payments = await _context.Payments
                .Where(p => p.DeclarationId == shipment.DeclarationId && p.Status == "Completed")
                .ToListAsync();

            if (!payments.Any())
            {
                return BadRequest("Payment not completed for this declaration.");
            }

            shipment.Status = "Approved";
            shipment.Declaration.Status = "Approved";

            await _context.SaveChangesAsync();

            return Ok("Shipment approved successfully.");
        }

        // POST: api/CustomsOfficer/RejectShipment/{shipmentId}
        [HttpPost("RejectShipment/{shipmentId}")]
        public async Task<IActionResult> RejectShipment(int shipmentId)
        {
            var shipment = await _context.Shipments
                .FirstOrDefaultAsync(s => s.ShipmentId == shipmentId);

            if (shipment == null)
            {
                return NotFound("Shipment not found.");
            }

            shipment.Status = "Rejected";

            await _context.SaveChangesAsync();

            return Ok("Shipment rejected successfully.");
        }


       


          
        [HttpGet("/Get-officer-report")]
        public async Task<ActionResult<IEnumerable<ReportDto>>> GetCustomsOfficerReport()
        {
            var reports = await _customsRepo.GetCustomsOfficerReportsAsync();
            return Ok(reports);
        }

        [HttpGet("/download-report")]
        public async Task<IActionResult> DownloadReport()
        {
            try
            {
                // Fetch reports from the repository
                var reports = await _customsRepo.GetCustomsOfficerReportsAsync();

                // Generate the PDF report
                var createdBy = "Customes Officer"; // Replace with dynamic value if needed
                var reportDate = DateTime.UtcNow; // Current date and time
                var pdfBytes = _customsRepo.GeneratePdfReport(reports, createdBy, reportDate);

                // Return the PDF file to the client
                return File(pdfBytes, "application/pdf", "Report.pdf");
            }
            catch (Exception ex)
            {
                // Handle any errors that occur during the PDF generation or file download
                // Log the exception if needed
                // Return a status code or error message to the client
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("/payment-history-show")]
        public async Task<IActionResult> GetPaymentHistory()
        {
            var paymentHistory = await _customsRepo.GetPaymentHistoryAsync();
            return Ok(paymentHistory);
        }





        // payment 
        
       











    }
};



