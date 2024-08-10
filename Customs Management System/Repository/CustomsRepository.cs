using Customs_Management_System.DbContexts;
using Customs_Management_System.DBContexts.Models;
using Customs_Management_System.DTOs;
using Customs_Management_System.IRepository;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

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

        /*
        public async Task<string> CreateDeclarationAsync(DeclarationDto declarationDto)
        {
            try
            {
                // Check if UserId exists
                var userExists = await _context.Users.AnyAsync(u => u.UserId == declarationDto.UserId);
                if (!userExists)
                {
                    throw new Exception($"User with UserId {declarationDto.UserId} does not exist.");
                }

                var declaration = new Declaration
                {
                    UserId = declarationDto.UserId,
                    DeclarationDate = declarationDto.DeclarationDate,
                    Status = declarationDto.Status,
                    Products = declarationDto.Products.Select(p => new Product
                    {
                        ProductName = p.ProductName,
                        Quantity = p.Quantity,
                        Weight = p.Weight,
                        CountryOfOrigin = p.CountryOfOrigin,
                        Hscode = p.Hscode
                    }).ToList(),
                    Shipments = declarationDto.Shipments.Select(s => new Shipment
                    {
                        MethodOfShipment = s.MethodOfShipment,
                        PortOfDeparture = s.PortOfDeparture,
                        PortOfDestination = s.PortOfDestination,
                        DepartureDate = s.DepartureDate,
                        ArrivalDate = s.ArrivalDate
                    }).ToList()
                };

                await _context.Declarations.AddAsync(declaration);
                await _context.SaveChangesAsync();

                // Create a monitoring record
                var monitoring = new Monitoring
                {
                    DeclarationId = declaration.DeclarationId,
                    MethodOfShipment = declaration.Shipments.FirstOrDefault().MethodOfShipment,
                    PortOfDeparture = declaration.Shipments.FirstOrDefault().PortOfDeparture,
                    PortOfDestination = declaration.Shipments.FirstOrDefault().PortOfDestination,
                    DepartureDate = declaration.Shipments.FirstOrDefault().DepartureDate,
                    ArrivalDate = declaration.Shipments.FirstOrDefault().ArrivalDate,
                    Status = "Pending" // Initial status
                };

                await _context.Monitorings.AddAsync(monitoring);
                await _context.SaveChangesAsync();

                return "Declaration and Monitoring Created Successfully";
            }
            catch (Exception e)
            {
                throw new Exception($"An unexpected error occurred. Details: {e.Message}");
            }
        }

        */
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

                var declaration = new Declaration
                {
                    UserId = declarationDto.UserId,
                    DeclarationDate = declarationDto.DeclarationDate,
                    Status = declarationDto.Status,
                    IsActive= declarationDto.IsActive,
                    Products = declarationDto.Products.Select(p => new Product
                    {
                        ProductName = p.ProductName,
                        Category = p.Category, // Add this line to include the category
                        Quantity = p.Quantity,
                        Weight = p.Weight,
                        CountryOfOrigin = p.CountryOfOrigin,
                        Hscode = p.Hscode,
                        DeclarationId = p.DeclarationId,
                        TotalPrice=p.TotalPrice
                    }).ToList(),
                    Shipments = declarationDto.Shipments.Select(s => new Shipment
                    {
                        MethodOfShipment = s.MethodOfShipment,
                        PortOfDeparture = s.PortOfDeparture,
                        PortOfDestination = s.PortOfDestination,
                        DepartureDate = s.DepartureDate,
                        ArrivalDate = s.ArrivalDate
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

        public async Task<List<MonitoringDto>> GetMonitoringsAsync()
        {
            try
            {
                var monitorings = await _context.Monitorings
                    .Include(m => m.Declaration)
                    .ThenInclude(d => d.Products)
                    .Include(m => m.Declaration)
                    .ThenInclude(d => d.Shipments)
                    .Select(m => new MonitoringDto
                    {
                        
                        DeclarationId = m.DeclarationId,
                        MethodOfShipment = m.MethodOfShipment,
                        PortOfDeparture = m.PortOfDeparture,
                        PortOfDestination = m.PortOfDestination,
                        DepartureDate = m.DepartureDate,
                        ArrivalDate = m.ArrivalDate,
                        Status = m.Status,
                        ProductName = m.Declaration.Products.FirstOrDefault().ProductName ?? "N/A", // Default value if no products
                        Quantity = m.Declaration.Products.FirstOrDefault().Quantity  , // Default value if no products
                        Weight = (double)m.Declaration.Products.FirstOrDefault().Weight, // Default value if no products
                        CountryOfOrigin = m.Declaration.Products.FirstOrDefault().CountryOfOrigin ?? "N/A", // Default value if no products
                        Hscode = m.Declaration.Products.FirstOrDefault().Hscode ?? "N/A" // Default value if no products
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
                .Where(d => d.UserId == userId)
                .ToListAsync();
        }

        public async Task AddPaymentAsync(Payment payment)
        {
            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();
        }

        //-----------For Dashboard overView importer

        public async Task<DashboardOverViewDto> GetDashboardOverviewAsync()
        {
            try
            {
                // Find users with RoleId == 2 (Importer)
                var importerUsers = await _context.Users
                    .Where(u => u.UserRoleId == 2)
                    .ToListAsync();

                int totalDeclarations = await _context.Declarations
                    .CountAsync(d => importerUsers.Select(u => u.UserId).Contains(d.UserId));

                int pendingPayments = await _context.Payments
                    .CountAsync(p => importerUsers.Select(u => u.UserId).Contains(p.UserId) && p.Status == "Pending");

                int shipmentMonitoring = await _context.Shipments
                    .Include(s => s.Declaration)
                    .CountAsync(s => importerUsers.Select(u => u.UserId).Contains(s.Declaration.UserId));

                return new DashboardOverViewDto
                {
                    TotalDeclarations = totalDeclarations,
                    PendingPayments = pendingPayments,
                    ShipmentMonitoring = shipmentMonitoring
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"An unexpected error occurred while fetching dashboard overview: {ex.Message}");
            }
        }



        // -------------------------------------------------Exporter part ---------------------------------
       public async Task<DashboardOverViewExporterDto> GetDashboardOverviewExporter()
        {
            try
            {
                // Find users with RoleId == 3 (Exporter)
                var ExporterUsers = await _context.Users
                    .Where(u => u.UserRoleId == 3)
                    .ToListAsync();

                int totalDeclarations = await _context.Declarations
                    .CountAsync(d => ExporterUsers.Select(u => u.UserId).Contains(d.UserId));

                int pendingPayments = await _context.Payments
                    .CountAsync(p => ExporterUsers.Select(u => u.UserId).Contains(p.UserId) && p.Status == "Pending");

                int shipmentMonitoring = await _context.Shipments
                    .Include(s => s.Declaration)
                    .CountAsync(s => ExporterUsers.Select(u => u.UserId).Contains(s.Declaration.UserId));

                return new DashboardOverViewExporterDto
                {
                    TotalDeclarations = totalDeclarations,
                    PendingPayments = pendingPayments,
                    ShipmentMonitoring = shipmentMonitoring
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"An unexpected error occurred while fetching dashboard overview: {ex.Message}");
            }
        }

        public async Task<string> CreateDeclarationExporter(DeclarationDto declarationDto)
        {
            try
            {
                // Check if UserId exists
                var userExists = await _context.Users.AnyAsync(u => u.UserId == declarationDto.UserId);
                if (!userExists)
                {
                    throw new Exception($"User with UserId {declarationDto.UserId} does not exist.");
                }

                var declaration = new Declaration
                {
                    UserId = declarationDto.UserId,
                    DeclarationDate = declarationDto.DeclarationDate,
                    Status = declarationDto.Status,
                    Products = declarationDto.Products.Select(p => new Product
                    {
                        ProductName = p.ProductName,
                        Quantity = p.Quantity,
                        Weight = p.Weight,
                        CountryOfOrigin = p.CountryOfOrigin,
                        Hscode = p.Hscode,
                        DeclarationId = p.DeclarationId,
                        TotalPrice= p.TotalPrice,
                    }).ToList(),
                    Shipments = declarationDto.Shipments.Select(s => new Shipment
                    {
                        MethodOfShipment = s.MethodOfShipment,
                        PortOfDeparture = s.PortOfDeparture,
                        PortOfDestination = s.PortOfDestination,
                        DepartureDate = s.DepartureDate,
                        ArrivalDate = s.ArrivalDate
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
            // Step 1: Retrieve UserIds for active Importers
            var importerUserIds = await _context.Users
                .Where(u => u.UserRoleId == 2 && u.IsActive)
                .Select(u => u.UserId)
                .ToListAsync();

            // Step 2: Count active declarations for these UserIds
            var totalDeclarations = await _context.Declarations
                .Where(d => importerUserIds.Contains(d.UserId) )
                .CountAsync();

            // Count pending shipments for Importers
            var pendingShipments = await _context.Shipments
                .Where(s => s.Status == "Pending" && s.Declaration.User.UserRoleId == 2 && s.Declaration.IsActive)
                .CountAsync();

            // Count running shipments for Importers
            var runningShipments = await _context.Shipments
                .Where(s => s.Status == "Running" && s.Declaration.User.UserRoleId == 2 && s.Declaration.IsActive)
                .CountAsync();

            return new CustomesDashboardSummaryDto
            {
                TotalDeclarations = totalDeclarations,
                PendingShipments = pendingShipments,
                RunningShipments = runningShipments
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
            var pendingShipments = await _context.Shipments
                .Where(s => s.Status == "Pending" && s.Declaration.User.UserRoleId == 3 && s.Declaration.IsActive)
                .CountAsync();

            // Count running shipments for Exporters
            var runningShipments = await _context.Shipments
                .Where(s => s.Status == "Running" && s.Declaration.User.UserRoleId == 3 && s.Declaration.IsActive)
                .CountAsync();

            return new CustomesDashboardSummaryDto
            {
                TotalDeclarations = totalDeclarations,
                PendingShipments = pendingShipments,
                RunningShipments = runningShipments
            };
        }


    }
}

