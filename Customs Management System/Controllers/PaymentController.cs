using Customs_Management_System.DbContexts;
using Customs_Management_System.DBContexts.Models;
using Customs_Management_System.DTOs;
using Customs_Management_System.IRepository;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using iTextSharp.text.pdf.draw;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ICustomsRepository _customsRepository;
    private readonly CMSDbContext _context;

    public PaymentController(IPaymentService paymentService, ICustomsRepository customsRepository, CMSDbContext context)
    {
        _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
        _customsRepository = customsRepository ?? throw new ArgumentNullException(nameof(customsRepository));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    [HttpPost("initiate")]
    public async Task<IActionResult> InitiatePayment([FromBody] PaymentRequest request)
    {
        if (request == null || request.DeclarationId <= 0 || string.IsNullOrEmpty(request.ReturnUrl))
        {
            return BadRequest(new { Message = "Invalid payment request or return URL." });
        }

        try
        {
            var transactionId = Guid.NewGuid().ToString();
            var successUrl = $"{Request.Scheme}://{Request.Host}/api/Payment/success" +
                $"?transactionId={Uri.EscapeDataString(transactionId)}" +
                $"&declarationId={request.DeclarationId}" +
                $"&returnUrl={Uri.EscapeDataString(request.ReturnUrl)}";

            var failUrl = $"{Request.Scheme}://{Request.Host}/api/Payment/fail" +
                $"?transactionId={Uri.EscapeDataString(transactionId)}" +
                 $"&declarationId={request.DeclarationId}" +
                $"&returnUrl={Uri.EscapeDataString(request.FailReturnUrl)}"; 

            var cancelUrl = $"{Request.Scheme}://{Request.Host}/api/payment/cancel";

            var paymentUrl = await _paymentService.InitiatePaymentAsync(request.DeclarationId, transactionId, successUrl, failUrl, cancelUrl);
            return Ok(new { Url = paymentUrl });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred while initiating payment: " + ex.Message });
        }
    }




    [HttpPost("success")]
    public async Task<IActionResult> PaymentSuccess([FromQuery] string transactionId, [FromQuery] int declarationId, [FromQuery] string returnUrl)
    {
        if (string.IsNullOrEmpty(transactionId) || declarationId <= 0 || string.IsNullOrEmpty(returnUrl))
        {
            return BadRequest(new { Message = "Invalid transaction ID, declaration ID, or return URL." });
        }

        try
        {
            // Process the payment success
            await _paymentService.UpdateProductPaymentStatusAsync(declarationId);

            var userId = await _paymentService.GetUserIdByDeclarationIdAsync(declarationId);
            var product = await _context.Products
           .FirstOrDefaultAsync(p => p.DeclarationId == declarationId);
            var payment = new Payment
            {
                DeclarationId = declarationId,
                UserId = userId,
                Amount = await _paymentService.GetTotalAmountByDeclarationAsync(declarationId),
                Date = DateTime.UtcNow,
                Status = "Completed",
                ProductId=product?.ProductId,
                TransactionId = transactionId,
                PaymentMethod = "SSLCommerz",
                Currency = "USD",
                
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // Create the invoice
            var invoice = new Invoice
            {
                UserId = userId,
                DeclarationId = declarationId,
                Amount = payment.Amount,
                InvoiceDate = DateTime.UtcNow,
                PaymentMethod = payment.PaymentMethod,
                Currency = payment.Currency,
                PaymentId=payment.PaymentId
            };
            _context.Invoices.Add(invoice); 
            await _context.SaveChangesAsync();

            return Redirect(returnUrl);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred while processing payment success: " + ex.Message });
        }
    }


    [HttpPost("fail")]
    public IActionResult PaymentFail([FromQuery] string transactionId, [FromQuery] string returnUrl)
    {
        if (string.IsNullOrEmpty(transactionId))
        {
            return BadRequest(new { Message = "Invalid transaction ID." });
        }

        try
        {
            // Handle failure logic here
            return Redirect(returnUrl);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred while processing payment failure: " + ex.Message });
        }
    }

    [HttpGet("cancel")]
    public IActionResult PaymentCancel([FromQuery] string transactionId)
    {
        if (string.IsNullOrEmpty(transactionId))
        {
            return BadRequest(new { Message = "Invalid transaction ID." });
        }

        try
        {
            // Handle cancellation logic here
            return Ok(new { Message = "Payment cancelled." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred while processing payment cancellation: " + ex.Message });
        }
    }

    [HttpGet("user-invoices/{userId}")]
    public async Task<IActionResult> GetUserInvoices(int userId)
    {
        try
        {
            // Fetch the user's invoices
            var invoices = await _context.Invoices
                .Where(i => i.UserId == userId)
                .Select(i => new InvoiceDto
                {
                    InvoiceId = i.InvoiceId,
                    UserId = i.UserId,
                    DeclarationId = (int)i.DeclarationId,
                    Amount = i.Amount,
                    InvoiceDate = i.InvoiceDate,
                    PaymentMethod = i.PaymentMethod,
                    Currency = i.Currency,
                    
                })
                .ToListAsync();

          
            if (invoices == null || !invoices.Any())
            {
                return NotFound(new { Message = $"No invoices found for user with ID {userId}." });
            }

            // Return the found invoices
            return Ok(invoices);
        }
        catch (Exception ex)
        {
         
            return StatusCode(500, new { Message = "An error occurred while retrieving invoices.", Error = ex.Message });
        }
    }


    [HttpGet("{invoiceId}/download")]
    public async Task<IActionResult> DownloadInvoice(int invoiceId)
    {
        // Fetch invoice details from the database
        var invoice = await _context.Invoices.FindAsync(invoiceId);
        if (invoice == null)
        {
            return NotFound("Invoice not found");
        }

        using (MemoryStream stream = new MemoryStream())
        {
           
            Document document = new Document(new Rectangle(250f, 320f), 5, 5, 5, 5); 
            PdfWriter writer = PdfWriter.GetInstance(document, stream);
            document.Open();

            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 22, BaseColor.BLUE);
            document.Add(new Paragraph("Invoice", titleFont) { Alignment = Element.ALIGN_CENTER });
         

            // Add Invoice ID at the top right
            var invoiceIdFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.DARK_GRAY);
            document.Add(new Paragraph($"Invoice ID: {invoice.InvoiceId}", invoiceIdFont) { Alignment = Element.ALIGN_RIGHT });
            document.Add(new Paragraph($"Generated on \n {DateTime.UtcNow.ToString("MMMM dd, yyyy \n hh:mm tt")}", FontFactory.GetFont(FontFactory.HELVETICA, 10)) { Alignment = Element.ALIGN_RIGHT });

            // Add a line separator
            document.Add(new Chunk(new LineSeparator(1f, 100f, BaseColor.BLACK, Element.ALIGN_CENTER, -1)));
         

            // Add invoice details with compact styles
            var detailsFont = FontFactory.GetFont(FontFactory.HELVETICA, 12, BaseColor.BLACK);
            document.Add(new Paragraph($"User ID: {invoice.UserId}", detailsFont));
            document.Add(new Paragraph($"Declaration ID: {invoice.DeclarationId}", detailsFont));
            document.Add(new Paragraph($"Amount: {invoice.Amount} {invoice.Currency}", detailsFont));
            document.Add(new Paragraph($"Invoice Date: {invoice.InvoiceDate.ToShortDateString()}", detailsFont));
            document.Add(new Paragraph($"Payment Method: {invoice.PaymentMethod}", detailsFont));

            // Create a table to hold the logo at the bottom right
            PdfPTable logoTable = new PdfPTable(1)
            {
                WidthPercentage = 100,
                DefaultCell = { Border = Rectangle.NO_BORDER }
            };

        
            iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance("D:\\Customs-Management-System-Backend\\Customs Management System\\wwwroot\\Images\\LogoF.png");
            logo.ScaleToFit(100f, 100f); // Reduce logo size
            PdfPCell logoCell = new PdfPCell(logo)
            {
                HorizontalAlignment = Element.ALIGN_RIGHT,
                VerticalAlignment = Element.ALIGN_BOTTOM,
                Border = Rectangle.NO_BORDER // No border for the logo cell
            };

            logoTable.AddCell(logoCell);

            // Add the logo table to the document
            document.Add(logoTable);

            // Close the document
            document.Close();

            // Return the generated PDF
            byte[] pdfBytes = stream.ToArray();
            return File(pdfBytes, "application/pdf", $"invoice_{invoiceId}.pdf");
        }
    }



}


// Define a request model for initiating payment
public class PaymentRequest
{
    public int DeclarationId { get; set; }
    public string ReturnUrl {  get; set; }
    public string FailReturnUrl { get; set; } 

}
