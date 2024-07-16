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
                        DeclarationId = p.DeclarationId
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

                // Create corresponding report record
                var report = new Report
                {
                    UserId = declarationDto.UserId,
                    ReportType = "Declaration Created",
                    Content = $"Declaration ID {declaration.DeclarationId} created.",
                    CreateAt = DateTime.UtcNow
                };

                await _context.Reports.AddAsync(report);
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



        // for Report part 


        public async Task CreateReportAsync(ReportDto reportDto)
        {
            // First, create the Declaration and related entities
            var declaration = new Declaration
            {
                UserId = reportDto.Declaration.UserId,
                DeclarationDate = reportDto.Declaration.DeclarationDate,
                Status = reportDto.Declaration.Status,
                Products = reportDto.Declaration.Products.Select(p => new Product
                {
                    ProductName = p.ProductName,
                    Quantity = p.Quantity,
                    Weight = p.Weight,
                    CountryOfOrigin = p.CountryOfOrigin,
                    Hscode = p.Hscode,
                    Category = p.Category
                }).ToList(),
                Shipments = reportDto.Declaration.Shipments.Select(s => new Shipment
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

            // Then, create the Report and associate it with the Declaration
            var report = new Report
            {
                UserId = reportDto.UserId,
                ReportType = reportDto.ReportType,
                Content = reportDto.Content,
                CreateAt = DateTime.UtcNow,
                 // assuming you have this field in Report model
            };

            await _context.Reports.AddAsync(report);
            await _context.SaveChangesAsync();
        }





        // Get Report by id -- no need 

        public async Task<IEnumerable<ReportDto>> GetReportsByRoleQueryable()
        {
            int roleId = 1;
            List<ReportDto> reportDtos = new List<ReportDto>();

            try
            {
                var roleIdParam = new SqlParameter("@RoleId", roleId);

                // Execute stored procedure
                var reports = await _context.Reports
                    .FromSqlRaw("EXECUTE dbo.GetReportsByRoleId @RoleId", roleIdParam)
                    .ToListAsync();

                foreach (var report in reports)
                {
                    // Fetch related declaration details
                    var declarations = await _context.Declarations
                        .Where(d => d.UserId == report.UserId)
                        .Include(d => d.Products)
                        .Include(d => d.Shipments)
                        .ToListAsync();

                    // Map to DeclarationDto
                    var declarationDtos = declarations.Select(d => new DeclarationDto
                    {
                        UserId = d.UserId,
                        DeclarationId = d.DeclarationId,
                        DeclarationDate = d.DeclarationDate,
                        Status = d.Status,
                        Products = d.Products.Select(p => new ProductDto
                        {
                            ProductName = p.ProductName,
                            Quantity = p.Quantity,
                            Weight = p.Weight,
                            CountryOfOrigin = p.CountryOfOrigin,
                            Hscode = p.Hscode,
                            Category = p.Category
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

                    // Create ReportDto
                    var reportDto = new ReportDto
                    {
                        ReportId = report.ReportId,
                        UserId = report.UserId,
                        ReportType = report.ReportType,
                        Content = report.Content,
                        CreateAt = report.CreateAt,
                        Declaration = declarationDtos.FirstOrDefault() // Adjust based on your logic
                    };

                    reportDtos.Add(reportDto);
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions as needed
                Console.Error.WriteLine($"Error fetching reports: {ex.Message}");
            }

            return reportDtos;
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

        //-----------For Dashboard overView

        public async Task<int> GetTotalDeclarationsAsync(int userId)
        {
            return await _context.Declarations.CountAsync(d => d.UserId == userId);
        }

        public async Task<int> GetPendingPaymentsAsync(int userId)
        {
            return await _context.Payments.CountAsync(p => p.UserId == userId && p.Status == "Pending");
        }

        public async Task<int> GetShipmentMonitoringAsync(int userId)
        {
            return await _context.Shipments
                .Include(s => s.Declaration)
                .CountAsync(s => s.Declaration.UserId == userId);
        }

        public async Task<int> GetGeneratedReportsAsync(int userId)
        {
            return await _context.Reports.CountAsync(r => r.UserId == userId);
        }

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

                int generatedReports = await _context.Reports
                    .CountAsync(r => importerUsers.Select(u => u.UserId).Contains(r.UserId));

                return new DashboardOverViewDto
                {
                    TotalDeclarations = totalDeclarations,
                    PendingPayments = pendingPayments,
                    ShipmentMonitoring = shipmentMonitoring,
                    GeneratedReports = generatedReports
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"An unexpected error occurred while fetching dashboard overview: {ex.Message}");
            }
        }


    }
}

