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
using Microsoft.ML;
using Customs_Management_System.Services;

namespace Customs_Management_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class CMSController : ControllerBase
    {
        private readonly CMSDbContext _context;
        private static ICustomsRepository _customsRepo;
        private readonly ILogger<CMSController> _logger;
        private readonly EmailService _emailService;
        public CMSController(ILogger<CMSController> logger, ICustomsRepository customsRepo, CMSDbContext context, EmailService emailService)
        {
            _logger=logger;
            _customsRepo=customsRepo;
            _context=context;
            _emailService=emailService;

        }
        //----------------------------------------------Importer Api  --------------------------------------

        /*                              Total 6 api 
                                                            1. Declaration submit---- done
                                                            2. get all Declaration--- done
                                                            3. payment submit --- bug
                                                            4. get monitoring list-- done
                                                            5. for Dashboard -- done
         
         */

        [HttpGet("/product-categories")]
        public async Task<ActionResult<IEnumerable<CategoryInfoDto>>> GetCategories()
        {
            var categories = await _context.ProductPrices
                .Select(p => new CategoryInfoDto
                {
                    Category = p.Category,
                    MaxWeight =
                        p.Category == "electronics" ? 50 :
                        p.Category == "clothing" ? 20 :
                        p.Category == "furniture" ? 200 :
                        p.Category == "food" ? 100 : 0,
                    WeightUnit =
                        p.Category == "electronics" || p.Category == "furniture" ? "pieces" : "kg",
                    MaxQuantity =
                        p.Category == "electronics" ? 10 :
                        p.Category == "clothing" ? 50 :
                        p.Category == "furniture" ? 5 :
                        p.Category == "food" ? 20 : 0
                })
                .Distinct()
                .ToListAsync();

            return Ok(categories);
        }


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

        [HttpGet("GetUserMonitorings/{userId}")]
        public async Task<IActionResult> GetUserMonitorings(int userId)
        {
            var monitorings = await _customsRepo.GetUserMonitoringsAsync(userId);
            return Ok(monitorings);
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

      


        //-------------------------------------------------------------- dashbord for importer and exporter 

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


        [HttpGet("Calender/events/{userId}")]
        public async Task<IActionResult> GetExporterCalendarEvents(int userId)
        {
            // Fetch declarations with product names
            var declarations = await _context.Declarations
                .Where(d => d.UserId == userId)
                .SelectMany(d => _context.Products
                    .Where(p => p.DeclarationId == d.DeclarationId)
                    .Select(p => new CalendarEventDto
                    {
                        Id = d.DeclarationId,
                        Title = p.ProductName,
                        Start = d.DeclarationDate,
                        Status = d.Status
                    })
                ).ToListAsync();

            var shipments = await _context.Shipments
                .Where(s => s.Declaration.UserId == userId)
                .SelectMany(s => _context.Products
                    .Where(p => p.DeclarationId == s.DeclarationId)
                    .Select(p => new CalendarEventDto
                    {
                        Id = s.ShipmentId,
                        Title = s.MethodOfShipment,
                        Start = s.DepartureDate,
                        End = s.ArrivalDate,
                        Status = s.Status
                    })
                ).ToListAsync();

            var events = declarations.Concat(shipments).ToList();
            return Ok(events);
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


      


        [HttpGet("/monitoring/{userId}")]
        public async Task<ActionResult<ExporterMonitorDto>> GetMonitoringOverview(int userId)
        {
            try
            {
               

                
                var user = await _context.Users
                                                     .Where(u => u.UserId == userId )
                                 .FirstOrDefaultAsync();

                if (user == null)
                {
                    return NotFound("User not found or the user is not an Exporter.");
                }

                // Total processed shipments for Exporters
                int processedShipments = await _context.Declarations
                                                        .Where(d => d.IsActive == true && d.UserId==userId)
                                                        .CountAsync();

                // Total pending shipments for Exporters
                int pendingShipments = await _context.Declarations
                                                     .Where(d => d.IsActive == false && d.UserId==userId)
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

        [Authorize(Roles = "Admin")]
        // POST: api/ProductPrices
        [HttpPost("/addProduct")]
        public async Task<IActionResult> AddProduct([FromBody] ProductPrice productPrice)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var existingProduct = await _context.ProductPrices
                .FirstOrDefaultAsync(p => p.Category == productPrice.Category && p.ProductName == productPrice.ProductName);

            if (existingProduct != null)
            {
                return Conflict(new { message = "A product with the same category and name already exists." });
            }
            try
            {
                // Add the new product to the database
                _context.ProductPrices.Add(productPrice);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Product added successfully!" });
            }
            catch (Exception ex)
            {
                
                return StatusCode(500, new { message = "An error occurred while adding the product.", error = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        
        [HttpPut("updateProduct/{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto productDto)
        {
            var existingProduct = await _context.ProductPrices.FindAsync(id);

            if (existingProduct == null)
            {
                return NotFound(new { message = "Product not found." });
            }

            // Update product details
            existingProduct.Category = productDto.Category;
            existingProduct.ProductName = productDto.ProductName;
            existingProduct.Price = productDto.Price;
            existingProduct.HsCode = productDto.HsCode;

            _context.ProductPrices.Update(existingProduct);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Product updated successfully." });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("deleteProduct/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.ProductPrices.FindAsync(id);

            if (product == null)
            {
                return NotFound(new { message = "Product not found." });
            }

            _context.ProductPrices.Remove(product);
            await _context.SaveChangesAsync();
            

            var maxId = _context.ProductPrices.Any() ? _context.ProductPrices.Max(p => p.PriceId) : 0;
            if (maxId > 0)
            {
                await _context.Database.ExecuteSqlRawAsync($"DBCC CHECKIDENT ('ProductPrices', RESEED, {maxId});");
            }

            return Ok(new { message = "Product deleted successfully." });
        }

        [HttpGet("GetAllProduct")]
        public async Task<ActionResult<IEnumerable<ProductPrice>>> GetAllProductPrices()
        {
            var productPrices = await _context.ProductPrices.ToListAsync();
            return Ok(productPrices);
        }


        //----------------------------------customes officer api -------------

        [HttpGet("importer-summary")]
        public async Task<ActionResult<CustomesDashboardSummaryDto>> GetImporterSummary()
        {
            try
            {
                var summary = await _customsRepo.GetImporterSummaryAsync();
                if (summary == null)
                {
                    return NotFound("No summary found for importers.");
                }
                return Ok(summary);
            }
            catch (Exception ex)
            {
                // Log the exception (optional)
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error retrieving importer summary: {ex.Message}");
            }
        }

        [HttpGet("exporter-summary")]
        public async Task<ActionResult<CustomesDashboardSummaryDto>> GetExporterSummary()
        {
            try
            {
                var summary = await _customsRepo.GetExporterSummaryAsync();
                if (summary == null)
                {
                    return NotFound("No summary found for exporters.");
                }
                return Ok(summary);
            }
            catch (Exception ex)
            {
                // Log the exception (optional)
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error retrieving exporter summary: {ex.Message}");
            }
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


            var pendingShipments = from s in _context.Shipments.Where(s => s.Status == """Pending""")
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
                                       paymentStatus = s.Declaration.IsPayment == true ? "Completed" : "Not Payment"
                                   };


            return Ok(pendingShipments);
        }

        [HttpGet("GetRunningShipments")]
        public IActionResult GetRunningShipments()
        {
            var runningShipments = from s in _context.Shipments.Where(s => s.Status == """Approved""")
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
                paymentStatus = s.Declaration.IsPayment == true ? "Completed" : "Not Payment"
            };


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
                    s.ArrivalDate,
                    ShipmentStatus= s.Status,
                    paymentStatus = s.Declaration.IsPayment == true ? "Completed" : "Not Payment"
                })
                .ToList();

            return Ok(rejectedShipments);
        }


        [HttpGet("GetCompleteShipments")]
        public async Task<IActionResult> GetCompleteShipments()
        {
            var shipments = from s in _context.Shipments where s.Status=="Completed"
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
                                CompletedDate = s.CompletedDate != null ? s.CompletedDate.Value.ToString("yyyy-MM-dd") : null,
                                paymentStatus = s.Declaration.IsPayment == true ? "Completed" : "Not Payment"
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

            var monitoring = await _context.Monitorings
                .FirstOrDefaultAsync(m => m.DeclarationId==shipment.DeclarationId);
            if (monitoring==null)
            {
                return NotFound("Monitoring record not found for this declaration");
            }

            if (!payments.Any())
            {
                return BadRequest("Payment not completed for this declaration.");
            }

            shipment.Status = "Approved";
            shipment.Declaration.Status = "Approved";
            shipment.Declaration.IsActive=true;
            monitoring.Status="Running";

            await _context.SaveChangesAsync();

            return Ok("Shipment approved successfully.");
        }
        [HttpPost("CompletedShipment/{shipmentId}")]

        public async Task<IActionResult> Completed(int shipmentId)
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

            var monitoring = await _context.Monitorings
                .FirstOrDefaultAsync(m => m.DeclarationId == shipment.DeclarationId);
            if (monitoring == null)
            {
                return NotFound("Monitoring record not found for this declaration.");
            }

            var departureDate = shipment.DepartureDate;
            shipment.Status = "Completed";
            shipment.CompletedDate = DateTime.Now;
            shipment.Declaration.Status = "Completed";
            shipment.Declaration.IsActive = true;
            monitoring.Status = shipment.Status = "Completed";
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(shipment.Declaration.UserId);
            if (user != null)
            {
                // Check if CompletedDate is later than DepartureDate
                if (shipment.CompletedDate > departureDate)
                {
                    string apologySubject = $"Apology for the Delay – Shipment ID {shipmentId}";
                    string apologyBody = $"Dear {user.UserName},<br><br>" +
                                         $"We are very sorry to inform you that your shipment with ID {shipmentId}, which was scheduled to depart from {shipment.PortOfDeparture}, has been delayed due to some unforeseen issues. " +
                                         "We sincerely apologize for the inconvenience this may have caused." +
                                         "<br><br>Best regards,<br>Customs Management System";

                    var sentAt = DateTime.UtcNow.AddHours(6);

                    // Save apology email to the database
                    var apologyEmail = new UserEmail
                    {
                        UserId = user.UserId,
                        Subject = apologySubject,
                        Body = apologyBody,
                        SentAt = sentAt
                    };
                    _context.UserEmails.Add(apologyEmail);
                    await _context.SaveChangesAsync();

                    // Send apology email
                    await _emailService.SendEmailAsync(user.Email, apologySubject, apologyBody);
                }

                // Regular shipment completed email
                string subject = $"Shipment ID {shipmentId} Successfully Shipped – Collection Ready";
                string body = $"Dear {user.UserName},<br><br>" +
                              $"We are pleased to inform you that your shipment with ID {shipmentId}, departing from {shipment.PortOfDeparture}, has been successfully shipped. You may now collect your products." +
                              "<br><br>Best regards,<br>Customs Management System";

                var completionSentAt = DateTime.UtcNow.AddHours(6);

                // Save shipment completion email to the database
                var userEmail = new UserEmail
                {
                    UserId = user.UserId,
                    Subject = subject,
                    Body = body,
                    SentAt = completionSentAt
                };
                _context.UserEmails.Add(userEmail);
                await _context.SaveChangesAsync();

                // Send completion email
                await _emailService.SendEmailAsync(user.Email, subject, body);
            }

            return Ok("Shipment Completed successfully.");
        }


        [HttpGet("GetUserEmails/{userId}")]
        public async Task<IActionResult> GetUserEmails(int userId)
        {
            var emails = await _context.UserEmails
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.SentAt)
                .ToListAsync();

            return Ok(emails);
        }

        [HttpGet("/customsOfficerMonitor")]
        public async Task<IActionResult> GetCustomsOfficerMonitorData()
        {
            var monitoringData = await _context.Monitorings
                .Join(_context.Declarations,
                    m => m.DeclarationId,
                    d => d.DeclarationId,
                    (m, d) => new { m, d })
                .Join(_context.Products,
                    md => md.d.DeclarationId,
                    p => p.DeclarationId,
                    (md, p) => new MonitoringResponseDto
                    {
                        Username = md.d.User.UserName, 
                        ProductName = p.ProductName,
                        DeclarationId = md.d.DeclarationId,
                        MonitoringId = md.m.MonitoringId,
                        Status = md.m.Status,
                        Quantity = p.Quantity,
                        TotalPrice = (decimal)p.TotalPrice
                    })
                .ToListAsync();

            return Ok(monitoringData);
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

            var monitoring = await _context.Monitorings
                .FirstOrDefaultAsync(m => m.DeclarationId==shipment.DeclarationId);
            if (monitoring==null)
            {
                return NotFound("Monitoring record not found for this declaration");
            }
            monitoring.Status="Rejected";

            shipment.Status = "Rejected";

            await _context.SaveChangesAsync();

            return Ok("Shipment rejected successfully.");
        }


        // POST: api/Shipments/RevertShipmentToPending/{shipmentId}
        [HttpPost("RevertShipmentToPending/{shipmentId}")]
        public async Task<IActionResult> RevertShipmentToPending(int shipmentId)
        {
            if (shipmentId <= 0)
            {
                return BadRequest("Invalid shipment ID.");
            }

            // Find the shipment in the database
            var shipment = await _context.Shipments.FindAsync(shipmentId);
            if (shipment == null)
            {
                return NotFound(new { message = "Shipment not found." });
            }

            // Check if the shipment is in 'Rejected' status
            if (shipment.Status != "Rejected")
            {
                return BadRequest(new { message = "Shipment is not in a rejected state." });
            }

            var monitoring = await _context.Monitorings
                .FirstOrDefaultAsync(m => m.DeclarationId==shipment.DeclarationId);
            if (monitoring==null)
            {
                return NotFound("Monitoring record not found for this declaration");
            }

            // Change the status to 'Pending'
            shipment.Status = "Pending";
            monitoring.Status="Pending";
            // Update the shipment entity in the database
            _context.Shipments.Update(shipment);
            await _context.SaveChangesAsync(); // Save changes to the database

            return Ok(new { message = "Shipment reverted to pending successfully." });
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
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("/payment-history-show")]
        public async Task<IActionResult> GetPaymentHistory()
        {
            var paymentHistory = await _customsRepo.GetPaymentHistoryAsync();
            return Ok(paymentHistory);
        }



        // GET: api/adminmonitor
        [HttpGet("/admin-monitor")]
        public async Task<ActionResult<IEnumerable<UserActivityDto>>> GetUserActivity()
        {
            var userActivities = await _customsRepo.GetUserActivitiesAsync();
            return Ok(userActivities);
        }





    }
};



