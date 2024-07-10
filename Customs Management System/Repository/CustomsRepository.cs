using Customs_Management_System.DbContexts;
using Customs_Management_System.DBContexts.Models;
using Customs_Management_System.DTOs;
using Customs_Management_System.IRepository;
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

        public async Task<List<MonitoringDto>> GetMonitorings(MonitoringDto monitoringDto)
        {
            try
            {
                var monitorings = await _context.Monitorings
                    .Where(m => m.MethodOfShipment == monitoringDto.MethodOfShipment)
                    .Where(m => m.PortOfDeparture == monitoringDto.PortOfDeparture)
                    .Where(m => m.PortOfDestination == monitoringDto.PortOfDestination)
                    .Where(m => m.DepartureDate == monitoringDto.DepartureDate)
                    .Where(m => m.ArrivalDate == monitoringDto.ArrivalDate)
                    .Where(m => m.Status == monitoringDto.Status)
                    .Select(m => new MonitoringDto
                    {
                        MethodOfShipment = m.MethodOfShipment,
                        PortOfDeparture = m.PortOfDeparture,
                        PortOfDestination = m.PortOfDestination,
                        DepartureDate = m.DepartureDate,
                        ArrivalDate = m.ArrivalDate,
                        Status = m.Status,
                        Declaration = new DeclarationDto
                        {
                           
                            UserId = m.Declaration.UserId,
                            DeclarationDate = m.Declaration.DeclarationDate,
                            Status = m.Declaration.Status,
                            Products = m.Declaration.Products.Select(p => new ProductDto
                            {
                                ProductName = p.ProductName,
                                Quantity = p.Quantity,
                                Weight = p.Weight,
                                CountryOfOrigin = p.CountryOfOrigin,
                                Hscode = p.Hscode,
                                DeclarationId = p.DeclarationId
                            }).ToList(),
                            Shipments = m.Declaration.Shipments.Select(s => new ShipmentDto
                            {
                                MethodOfShipment = s.MethodOfShipment,
                                PortOfDeparture = s.PortOfDeparture,
                                PortOfDestination = s.PortOfDestination,
                                DepartureDate = s.DepartureDate,
                                ArrivalDate = s.ArrivalDate
                            }).ToList()
                        }
                    }).ToListAsync();

                return monitorings;
               
            }
            catch (Exception e)
            {
                throw new Exception($"Error retrieving monitorings: {e.Message}");
            }

        }
    }
}
