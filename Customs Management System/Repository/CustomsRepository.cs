using Customs_Management_System.DbContexts;
using Customs_Management_System.DBContexts.Models;
using Customs_Management_System.DTOs;
using Customs_Management_System.IRepository;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;
using System.Linq;

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

        public async Task<IEnumerable<ProductPriceDto>> GetProductsByCategoryAsync(string category)
        {
            var sql = "SELECT PriceId, Category, ProductName, Price " +
                      "FROM ProductPrices " +
                      "WHERE LOWER(Category) = LOWER(@category)";

            var products = await _context.ProductPrices
                .FromSqlRaw(sql, new SqlParameter("@category", category))
                .Select(p => new ProductPriceDto
                {
                    PriceId = p.PriceId,
                    Category = p.Category,
                    ProductName = p.ProductName,
                    Price = (decimal)p.Price
                })
                .ToListAsync();

            return products;
        }

      
        public async Task<string> CreateDeclarationAsync(DeclarationDto declarationDto)
        {
            try
            {
                // Check if UserId exists
                var userExists = await _context.Users.AnyAsync(u => u.UserId == declarationDto.UserId && u.UserRoleId == 2);
                if (!userExists)
                {
                    throw new Exception($"User with UserId {declarationDto.UserId} does not exist.");
                }

                var productNames = declarationDto.Products.Select(p => p.ProductName).Distinct().ToList();
                var productPrices = await _context.ProductPrices
                    .Where(pp => productNames.Contains(pp.ProductName))
                    .ToListAsync();

                var productPriceDict = productPrices
                   .ToDictionary(pp => pp.ProductName, pp => pp.HsCode);

                var declaration = new Declaration
                {
                    UserId = declarationDto.UserId,
                    DeclarationDate = declarationDto.DeclarationDate,
                    Status = declarationDto.Status,
                    IsActive= declarationDto.IsActive,
                    IsPayment=false,
                    Products = declarationDto.Products.Select(p => new Product
                    {
                        ProductName = p.ProductName,
                        Category = p.Category, 
                        Quantity = p.Quantity,
                        Weight = p.Weight,
                        CountryOfOrigin = p.CountryOfOrigin,
                        Hscode = productPriceDict.ContainsKey(p.ProductName) ? productPriceDict[p.ProductName] : null, // Auto-update HsCode
                        DeclarationId = p.DeclarationId,
                        TotalPrice=p.TotalPrice,
                        IsPayment=false,
                        
                        
                    }).ToList(),
                    Shipments = declarationDto.Shipments.Select(s => new Shipment
                    {
                        MethodOfShipment = s.MethodOfShipment,
                        PortOfDeparture = s.PortOfDeparture,
                        PortOfDestination = s.PortOfDestination,
                        DepartureDate = s.DepartureDate,
                        ArrivalDate = s.ArrivalDate,
                        Status="Pending"
                    }).ToList()
                };

                await _context.Declarations.AddAsync(declaration);
                await _context.SaveChangesAsync();

                // Create corresponding monitoring record
                var monitoring = new Monitoring
                {
                    DeclarationId = declaration.DeclarationId,
                    MethodOfShipment = declarationDto.Shipments.First().MethodOfShipment,
                    PortOfDeparture = declarationDto.Shipments.First().PortOfDeparture,
                    PortOfDestination = declarationDto.Shipments.First().PortOfDestination,
                    DepartureDate = declarationDto.Shipments.First().DepartureDate,
                    ArrivalDate = declarationDto.Shipments.First().ArrivalDate,
                    Status = "Pending"
                };

                await _context.Monitorings.AddAsync(monitoring);
                await _context.SaveChangesAsync();

                return "Declaration Created Successfully";
            }
            catch (Exception e)
            {
                throw new Exception($"An unexpected error occurred. Details: {e.Message}");
            }
        }




        //public async Task<List<MonitoringDto>> GetMonitoringsAsync()
        //{
        //    try
        //    {
        //        var monitorings = await _context.Monitorings
        //            .Include(m => m.Declaration)
        //            .ThenInclude(d => d.Products)
        //            .Include(m => m.Declaration)
        //            .ThenInclude(d => d.Shipments)
        //            .Select(m => new MonitoringDto
        //            {

        //                DeclarationId = m.DeclarationId,
        //                MethodOfShipment = m.MethodOfShipment,
        //                PortOfDeparture = m.PortOfDeparture,
        //                PortOfDestination = m.PortOfDestination,
        //                DepartureDate = m.DepartureDate,
        //                ArrivalDate = m.ArrivalDate,
        //                Status = m.Status,
        //                ProductName = m.Declaration.Products.FirstOrDefault().ProductName ?? "N/A", // Default value if no products
        //                Quantity = m.Declaration.Products.FirstOrDefault().Quantity  , // Default value if no products
        //                Weight = (double)m.Declaration.Products.FirstOrDefault().Weight, // Default value if no products
        //                CountryOfOrigin = m.Declaration.Products.FirstOrDefault().CountryOfOrigin ?? "N/A", // Default value if no products
        //                Hscode = m.Declaration.Products.FirstOrDefault().Hscode ?? "N/A" // Default value if no products
        //            })
        //            .ToListAsync();

        //        return monitorings;
        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception($"An unexpected error occurred. Details: {e.Message}");
        //    }
        //}

        public async Task<List<MonitoringDto>> GetUserMonitoringsAsync(int userId)
        {
            try
            {
                var monitorings = await _context.Monitorings
                    .Include(m => m.Declaration)
                        .ThenInclude(d => d.Products)
                    .Include(m => m.Declaration)
                        .ThenInclude(d => d.Shipments)
                    .Where(m => m.Declaration.UserId == userId) // Filter for the specific user ID
                    .Select(m => new MonitoringDto
                    {
                        DeclarationId = m.DeclarationId,
                        MethodOfShipment = m.MethodOfShipment,
                        PortOfDeparture = m.PortOfDeparture,
                        PortOfDestination = m.PortOfDestination,
                        DepartureDate = m.DepartureDate,
                        ArrivalDate = m.ArrivalDate,
                        Status = m.Status,
                        ProductName = m.Declaration.Products.FirstOrDefault().ProductName ?? "N/A", 
                        Quantity = m.Declaration.Products.FirstOrDefault().Quantity  , 
                        Weight = (double)m.Declaration.Products.FirstOrDefault().Weight,
                        CountryOfOrigin = m.Declaration.Products.FirstOrDefault().CountryOfOrigin ?? "N/A", 
                        Hscode = m.Declaration.Products.FirstOrDefault().Hscode ?? "N/A" 
                    })
                    .ToListAsync();

                return monitorings;
            }
            catch (Exception e)
            {
                throw new Exception($"An unexpected error occurred. Details: {e.Message}");
            }
        }


        // payment 
        public async Task<IEnumerable<Declaration>> GetDeclarationsByUserIdAsync(int userId)
        {
            return await _context.Declarations
                .Include(d => d.Products)
                .Where(d => d.UserId == userId && d.IsPayment==false)
                .ToListAsync();
        }

        public async Task AddPaymentAsync(Payment payment)
        {
            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();
        }

        //-----------For Dashboard overView importer and exporter

       public async Task<DashboardOverViewDto> GetDashboardOverviewAsync(int userId)
{
    try
    {

        var importerUser = await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == userId );

        if (importerUser == null)
        {
            throw new Exception("Importer not found");
        }

     
        int totalDeclarations = await _context.Declarations
            .CountAsync(d => d.UserId == userId);


              int pendingPayments = await _context.Declarations
            .CountAsync(p => p.UserId == userId && p.IsPayment == false);

                int shipmentMonitoring = await _context.Shipments
            .Include(s => s.Declaration)
            .CountAsync(s => s.Declaration.UserId == userId);

        // Grouping monitoring records by status for the user
        var shipmentCounts = await _context.Monitorings
            .Include(m => m.Declaration)
            .Where(m => m.Declaration.UserId == userId)
            .GroupBy(m => m.Status)
            .Select(g => new
            {
                Status = g.Key,
                Count = g.Count()
            })
            .ToListAsync();

        // Aggregate shipment counts based on status
        int shipmentRunning = shipmentCounts.FirstOrDefault(s => s.Status == "Running")?.Count ?? 0;
        int shipmentCompleted = shipmentCounts.FirstOrDefault(s => s.Status == "Completed")?.Count ?? 0;
        int pendingShipment = shipmentCounts.FirstOrDefault(s => s.Status == "Pending")?.Count ?? 0;
        int shipmentRejected = shipmentCounts.FirstOrDefault(s => s.Status == "Rejected")?.Count ?? 0;

        // Return the dashboard overview data
        return new DashboardOverViewDto
        {
            TotalDeclarations = totalDeclarations,
            PendingPayments = pendingPayments,
            ShipmentMonitoring = shipmentMonitoring,
            TotalPendingShipment = pendingShipment,
            TotalRunningShipmet = shipmentRunning,
            TotalCompletedShipment = shipmentCompleted,
            TotalRejectedShipment = shipmentRejected,
        };
    }
    catch (Exception ex)
    {
        // Consider logging the exception for better diagnostics
        throw new Exception($"An unexpected error occurred while fetching dashboard overview: {ex.Message}");
    }
}


        // -------------------------------------------------Exporter part ---------------------------------
       


        public async Task<string> CreateDeclarationExporters(DeclarationDto declarationDto)
        {
            try
            {
                // Check if UserId exists
                var userExists = await _context.Users.AnyAsync(u => u.UserId == declarationDto.UserId && u.UserRoleId == 3);
                if (!userExists)
                {
                    throw new Exception($"User with UserId {declarationDto.UserId} does not exist.");
                }

                var productNames = declarationDto.Products.Select(p => p.ProductName).Distinct().ToList();
                var productPrices = await _context.ProductPrices
                    .Where(pp => productNames.Contains(pp.ProductName))
                    .ToListAsync();

                var productPriceDict = productPrices
                   .ToDictionary(pp => pp.ProductName, pp => pp.HsCode);

                var declaration = new Declaration
                {
                    UserId = declarationDto.UserId,
                    DeclarationDate = declarationDto.DeclarationDate,
                    Status = declarationDto.Status,
                    IsActive= declarationDto.IsActive,
                    IsPayment=false,
                    Products = declarationDto.Products.Select(p => new Product
                    {
                        ProductName = p.ProductName,
                        Category = p.Category,
                        Quantity = p.Quantity,
                        Weight = p.Weight,
                        CountryOfOrigin = p.CountryOfOrigin,
                        Hscode = productPriceDict.ContainsKey(p.ProductName) ? productPriceDict[p.ProductName] : null, // Auto-update HsCode
                        DeclarationId = p.DeclarationId,
                        TotalPrice=p.TotalPrice,
                        IsPayment=false,


                    }).ToList(),
                    Shipments = declarationDto.Shipments.Select(s => new Shipment
                    {
                        MethodOfShipment = s.MethodOfShipment,
                        PortOfDeparture = s.PortOfDeparture,
                        PortOfDestination = s.PortOfDestination,
                        DepartureDate = s.DepartureDate,
                        ArrivalDate = s.ArrivalDate,
                        Status="Pending"
                    }).ToList()
                };

                await _context.Declarations.AddAsync(declaration);
                await _context.SaveChangesAsync();

                // Create corresponding monitoring record
                var monitoring = new Monitoring
                {
                    DeclarationId = declaration.DeclarationId,
                    MethodOfShipment = declarationDto.Shipments.First().MethodOfShipment,
                    PortOfDeparture = declarationDto.Shipments.First().PortOfDeparture,
                    PortOfDestination = declarationDto.Shipments.First().PortOfDestination,
                    DepartureDate = declarationDto.Shipments.First().DepartureDate,
                    ArrivalDate = declarationDto.Shipments.First().ArrivalDate,
                    Status = "Pending"
                };

                await _context.Monitorings.AddAsync(monitoring);
                await _context.SaveChangesAsync();

                return "Declaration Created Successfully";
            }
            catch (Exception e)
            {
                throw new Exception($"An unexpected error occurred. Details: {e.Message}");
            }
        }



        // Customes officer 

        public async Task<CustomesDashboardSummaryDto> GetImporterSummaryAsync()
        {
           
            var importerUserIds = await _context.Users
                .Where(u => u.UserRoleId == 2 && u.IsActive)
                .Select(u => u.UserId)
                .ToListAsync();
           
            var totalDeclarations = await _context.Declarations
                .Where(d => importerUserIds.Contains(d.UserId))
                .CountAsync();

           
            var pendingShipments = await _context.Declarations
                .Where(d => d.Status == "Pending" && importerUserIds.Contains(d.UserId))
                .CountAsync();

            var runningShipments = await _context.Shipments
                .Where(s => s.Status == "Approved" &&
                            importerUserIds.Contains(s.Declaration.UserId))
                .CountAsync();

           
            var completedShipments = await _context.Shipments
                .Where(s => s.Status == "Completed" &&
                            importerUserIds.Contains(s.Declaration.UserId))
                .CountAsync();
            ;

            return new CustomesDashboardSummaryDto
            {
                TotalDeclarations = totalDeclarations,
                PendingShipments = pendingShipments,
                RunningShipments = runningShipments,
                CompletedShipments=completedShipments
            };
        }
        public async Task<CustomesDashboardSummaryDto> GetExporterSummaryAsync()
        {
            // Step 1: Retrieve UserIds for active Importers
            var exporterUserIds = await _context.Users
                .Where(u => u.UserRoleId == 3 && u.IsActive)
                .Select(u => u.UserId)
                .ToListAsync();

            // Step 2: Count active declarations for these UserIds
            var totalDeclarations = await _context.Declarations
                .Where(d => exporterUserIds.Contains(d.UserId))
                .CountAsync();

            // Count pending shipments for Exporters
            var pendingShipments = await _context.Declarations
                .Where(d => d.Status == "Pending" && exporterUserIds.Contains(d.UserId))
                .CountAsync();

            // Count running shipments for Exporters
            var runningShipments = await _context.Shipments
                .Where(s => s.Status == "Approved" &&
                            exporterUserIds.Contains(s.Declaration.UserId) )
                .CountAsync();

            // Count running shipments for Exporters
            var completedShipments = await _context.Shipments
                .Where(s => s.Status == "Completed" &&
                            exporterUserIds.Contains(s.Declaration.UserId))
                .CountAsync();
            ;

            return new CustomesDashboardSummaryDto
            {
                TotalDeclarations = totalDeclarations,
                PendingShipments = pendingShipments,
                RunningShipments = runningShipments,
                CompletedShipments=completedShipments
            };
        }

        public IEnumerable<DeclarationDto> GetPendingDeclaration()
        {
            return _context.Declarations
                .Where(d => d.Status == "Pending")
                .Include(d => d.Products) // Assuming your entity includes these navigation properties
                .Include(d => d.Shipments)
                .Select(d => new DeclarationDto
                {
                    UserId = d.UserId,
                    DeclarationId = d.DeclarationId,
                    DeclarationDate = d.DeclarationDate,
                    Status = d.Status,
                    IsActive = d.IsActive,
                    Products = d.Products.Select(p => new ProductDto
                    {
                        ProductName = p.ProductName,
                        Quantity = p.Quantity,
                        Weight = p.Weight,
                        CountryOfOrigin = p.CountryOfOrigin,
                        Hscode = p.Hscode
                    }).ToList(),
                    Shipments = d.Shipments.Select(s => new ShipmentDto
                    {
                        ShipmentId = s.ShipmentId,
                        MethodOfShipment = s.MethodOfShipment,
                        PortOfDeparture = s.PortOfDeparture,
                        PortOfDestination = s.PortOfDestination,
                        DepartureDate = s.DepartureDate,
                        ArrivalDate = s.ArrivalDate
                    }).ToList()
                }).ToList();
        }

        public IEnumerable<DeclarationDto> GetRunningDeclaration()
        {
            return _context.Declarations
                .Where(d => d.Status == "Running" && d.IsActive)
                .Include(d => d.Products)
                .Include(d => d.Shipments)
                .Select(d => new DeclarationDto
                {
                    UserId = d.UserId,
                    DeclarationId = d.DeclarationId,
                    DeclarationDate = d.DeclarationDate,
                    Status = d.Status,
                    IsActive = d.IsActive,
                    Products = d.Products.Select(p => new ProductDto
                    {
                        ProductName = p.ProductName,
                        Quantity = p.Quantity,
                        Weight = p.Weight,
                        CountryOfOrigin = p.CountryOfOrigin,
                        Hscode = p.Hscode
                    }).ToList(),
                    Shipments = d.Shipments.Select(s => new ShipmentDto
                    {
                        ShipmentId = s.ShipmentId,
                        MethodOfShipment = s.MethodOfShipment,
                        PortOfDeparture = s.PortOfDeparture,
                        PortOfDestination = s.PortOfDestination,
                        DepartureDate = s.DepartureDate,
                        ArrivalDate = s.ArrivalDate
                    }).ToList()
                }).ToList();
        }


        //report

        public async Task<IEnumerable<ReportDto>> GetCustomsOfficerReportsAsync()
        {
            var reports = await (from d in _context.Declarations
                                 join u in _context.Users on d.UserId equals u.UserId
                                 join r in _context.Roles on u.UserRoleId equals r.RoleId
                                 join p in _context.Payments on d.DeclarationId equals p.DeclarationId into paymentGroup
                                 from pg in paymentGroup.DefaultIfEmpty() // Left join with Payments
                                 join pr in _context.Products on d.DeclarationId equals pr.DeclarationId into productGroup
                                 from prg in productGroup.DefaultIfEmpty() // Left join with Products
                                 join pp in _context.ProductPrices on new { prg.Category, prg.ProductName } equals new { pp.Category, pp.ProductName } into priceGroup
                                 from ppg in priceGroup.DefaultIfEmpty() // Left join with ProductPrices
                                 select new ReportDto
                                 {
                                     DeclarationId = d.DeclarationId,
                                     UserName = u.UserName,
                                     RoleName = r.RoleName,
                                     DeclarationDate = d.DeclarationDate,
                                     Status = d.Status,
                                     Amount = pg != null ? pg.Amount : 0, // Explicit null check for payment amount
                                     ProductName = prg != null ? prg.ProductName : "No Product", // Explicit null check for product name
                                     Quantity = prg != null ? prg.Quantity : 0, // Explicit null check for quantity
                                     UnitPrice = ppg != null ? ppg.Price ?? 0 : 0, // Explicit null check for price
                                     TotalPrice = (prg != null ? prg.Quantity : 0) * (ppg != null ? ppg.Price ?? 0 : 0) // Explicit null check for quantity and price
                                 }).ToListAsync();

            return reports;
        }
        public byte[] GeneratePdfReport(IEnumerable<ReportDto> reports, string createdBy, DateTime reportDate)
        {
            using (var memoryStream = new MemoryStream())
            {
                // Create a new PDF document
                var document = new Document(PageSize.A4);
                PdfWriter.GetInstance(document, memoryStream);
                document.Open();

                // Add title
                var titleFont = FontFactory.GetFont(FontFactory.TIMES_BOLD, 20);
                var titleParagraph = new Paragraph("Customs Officer Report", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 10f
                };
                document.Add(titleParagraph);

                // Add creator and date
                var metadataFont = FontFactory.GetFont(FontFactory.TIMES, 12);
                var metadataParagraph = new Paragraph
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20f,
                    Font = metadataFont 
                };
                metadataParagraph.Add($"Created by: {createdBy}\n");
                metadataParagraph.Add($"Date: {reportDate.ToString("MMMM d, yyyy HH:mm:ss")}");
                document.Add(metadataParagraph);

                // Add table
                var table = new PdfPTable(9) // Updated column count to 9
                {
                    WidthPercentage = 100
                };
                table.SetWidths(new float[] { 8, 15, 15, 15, 10, 15, 12, 12, 15 }); // Adjust column widths

                // Add table headers
                AddCellToHeader(table, "ID");
                AddCellToHeader(table, "User Name");
                AddCellToHeader(table, "Role");
                AddCellToHeader(table, "Declaration Date");
                AddCellToHeader(table, "Status");
                AddCellToHeader(table, "Payment");
                AddCellToHeader(table, "Product Name");
                AddCellToHeader(table, "Unit Price");
                AddCellToHeader(table, "Total Price");

                // Add table rows
                foreach (var report in reports)
                {
                    AddCellToBody(table, report.DeclarationId.ToString());
                    AddCellToBody(table, report.UserName);
                    AddCellToBody(table, report.RoleName);
                    AddCellToBody(table, report.DeclarationDate.ToShortDateString());
                    AddCellToBody(table, report.Status);
                    AddCellToBody(table, report.Amount.ToString("C"));
                    AddCellToBody(table, report.ProductName);
                    AddCellToBody(table, report.UnitPrice.ToString("C"));
                    AddCellToBody(table, report.TotalPrice.ToString("C"));
                }

                document.Add(table);

                // Close the document
                document.Close();

                return memoryStream.ToArray();
            }
        }

        private void AddCellToHeader(PdfPTable table, string text)
        {
            var headerFont = FontFactory.GetFont(FontFactory.TIMES_BOLD, 12);
            var cell = new PdfPCell(new Phrase(text, headerFont))
            {
                BackgroundColor = new BaseColor(204, 204, 204), // Light gray background
                HorizontalAlignment = Element.ALIGN_CENTER,
                Padding = 8,
                BorderColor = BaseColor.BLACK
            };
            table.AddCell(cell);
        }

        private void AddCellToBody(PdfPTable table, string text)
        {
            var bodyFont = FontFactory.GetFont(FontFactory.TIMES, 10);
            var cell = new PdfPCell(new Phrase(text, bodyFont))
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                Padding = 8,
                BorderColor = BaseColor.LIGHT_GRAY // Light gray border
            };
            table.AddCell(cell);
        }


        public async Task<IEnumerable<PaymentDto>> GetPaymentHistoryAsync()
        {
            return await _context.Payments
                .Select(p => new PaymentDto
                {
                    PaymentId = p.PaymentId,
                    UserId = p.UserId,
                    Amount = p.Amount,
                    Date = p.Date,
                    Status = p.Status,
                    DeclarationId = p.DeclarationId,
                    
                    ProductName = _context.Products.FirstOrDefault(pr => pr.ProductId == p.ProductId).ProductName // Assuming ProductName is in a separate Products table
                })
                .ToListAsync();
        }

        // importer details 

        public async Task<IEnumerable<ShipmentDetailsDto>> GetPortsByCountryAsync(string country)
        {
            return await _context.ShipmentDetails
                .Where(sd => sd.Country == country)
                .Select(sd => new ShipmentDetailsDto
                {
                    ShipmentdetailsId = sd.ShipmentDetailId,
                    Country = sd.Country,
                    Port = sd.Port,
                    Vat = (decimal)sd.Vat,
                    Tax = (decimal)sd.Tax
                })
                .ToListAsync();
        }
        public async Task<IEnumerable<string>> GetAllCountriesAsync()
        {
            return await _context.ShipmentDetails
                .Select(sd => sd.Country)
                .Distinct()
                .ToListAsync();
        }




        public async Task<List<UserActivityDto>> GetUserActivitiesAsync()
        {
            return await _context.Users
                .Where(u => u.IsActive) 
                .Select(u => new UserActivityDto
                {
                    Name = u.UserName,
                    Value = (int)u.LoginCount // This is your "activity" value
                })
                .ToListAsync();
        }





    }
}

