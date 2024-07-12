using Customs_Management_System.DbContexts;
using Customs_Management_System.DBContexts.Models;
using Customs_Management_System.DTOs;
using Customs_Management_System.IRepository;
using Microsoft.AspNetCore.Http.HttpResults;
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

                return "Declaration Created Successfully";
            }
            catch (DbUpdateException dbEx)
            {
                var innerExceptionMessage = dbEx.InnerException?.Message;
                throw new Exception($"An error occurred while saving the entity changes. Details: {innerExceptionMessage}");
            }
            catch (Exception e)
            {
                throw new Exception($"An unexpected error occurred. Details: {e.Message}");
            }
        }


        public Task<string> GetDeclarationByIdAsync(int id)
        {
            throw new NotImplementedException();
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
                    ProductName = m.Declaration.Products.FirstOrDefault().ProductName, // Assumes single product per declaration
                    Quantity = m.Declaration.Products.FirstOrDefault().Quantity,
                    Weight = (double)m.Declaration.Products.FirstOrDefault().Weight,
                    CountryOfOrigin = m.Declaration.Products.FirstOrDefault().CountryOfOrigin,
                    Hscode = m.Declaration.Products.FirstOrDefault().Hscode
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
                    Hscode = p.Hscode
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


        public async Task<IEnumerable<ReportDto>> GetReportsAsync()
        {
#pragma warning disable CS8601 // Possible null reference assignment.
            var reports = await _context.Reports
                .Include(r => r.User) // Include related User if needed
                .Select(r => new ReportDto
                {
                    ReportId = r.ReportId,
                    UserId = r.UserId,
                    ReportType = r.ReportType,
                    Content = r.Content,
                    CreateAt = r.CreateAt,
                    Declaration = _context.Declarations
                        .Where(d => d.UserId == r.UserId) // Example of how to find Declaration by UserId
                        .Select(d => new DeclarationDto
                        {
                            DeclarationId = d.DeclarationId,
                            UserId = d.UserId,
                            DeclarationDate = d.DeclarationDate,
                            Status = d.Status,
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
                                MethodOfShipment = s.MethodOfShipment,
                                PortOfDeparture = s.PortOfDeparture,
                                PortOfDestination = s.PortOfDestination,
                                DepartureDate = s.DepartureDate,
                                ArrivalDate = s.ArrivalDate
                            }).ToList()
                        })
                        .FirstOrDefault()
                })
                .ToListAsync();
#pragma warning restore CS8601 // Possible null reference assignment.

            return reports;
        }






        public async Task<ReportDto> GetReportByIdAsync(int reportId)
        {
            var report = await _context.Reports
                .Include(r => r.User) // Include related User if needed
                .FirstOrDefaultAsync(r => r.ReportId == reportId);

            if (report == null)
            {
                return null; // Return null if no report found with the given reportId
            }

            // Fetch associated Declaration details using a query based on UserId or any relevant identifier
            var declaration = await _context.Declarations
                .Include(d => d.Products)
                .Include(d => d.Shipments)
                .FirstOrDefaultAsync(d => d.UserId == report.UserId); // Adjust based on your actual relationship

            if (declaration == null)
            {
                return null; // Handle case where associated declaration is not found
            }

            // Map the retrieved Report and Declaration to ReportDto
            var reportDto = new ReportDto
            {
                ReportId = report.ReportId,
                UserId = report.UserId,
                ReportType = report.ReportType,
                Content = report.Content,
                CreateAt = report.CreateAt,
                Declaration = new DeclarationDto
                {
                    DeclarationId = declaration.DeclarationId,
                    UserId = declaration.UserId,
                    DeclarationDate = declaration.DeclarationDate,
                    Status = declaration.Status,
                    Products = declaration.Products.Select(p => new ProductDto
                    {
                        ProductName = p.ProductName,
                        Quantity = p.Quantity,
                        Weight = p.Weight,
                        CountryOfOrigin = p.CountryOfOrigin,
                        Hscode = p.Hscode
                    }).ToList(),
                    Shipments = declaration.Shipments.Select(s => new ShipmentDto
                    {
                        MethodOfShipment = s.MethodOfShipment,
                        PortOfDeparture = s.PortOfDeparture,
                        PortOfDestination = s.PortOfDestination,
                        DepartureDate = s.DepartureDate,
                        ArrivalDate = s.ArrivalDate
                    }).ToList()
                }
            };

            return reportDto;
        }



    }
}

