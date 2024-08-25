using Customs_Management_System.DbContexts;
using Customs_Management_System.DBContexts.Models;
using Customs_Management_System.DTOs;
using Customs_Management_System.IRepository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

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
            var failUrl = $"{Request.Scheme}://{Request.Host}/api/Payment/fail";
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

            var payment = new Payment
            {
                DeclarationId = declarationId,
                UserId = userId,
                Amount = await _paymentService.GetTotalAmountByDeclarationAsync(declarationId),
                Date = DateTime.UtcNow,
                Status = "Completed",
                TransactionId = transactionId,
                PaymentMethod = "SSLCommerz",
                Currency = "BDT"
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // Redirect to the frontend success page
            return Redirect(returnUrl);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred while processing payment success: " + ex.Message });
        }
    }


    [HttpGet("fail")]
    public IActionResult PaymentFail([FromQuery] string transactionId)
    {
        if (string.IsNullOrEmpty(transactionId))
        {
            return BadRequest(new { Message = "Invalid transaction ID." });
        }

        try
        {
            // Handle failure logic here
            return Ok(new { Message = "Payment failed." });
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
}

// Define a request model for initiating payment
public class PaymentRequest
{
    public int DeclarationId { get; set; }
    public string ReturnUrl {  get; set; }
}
