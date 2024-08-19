using Customs_Management_System.IRepository;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("/payment/initiate")]
    public async Task<IActionResult> InitiatePayment(decimal amount)
    {
        try
        {
            var transactionId = Guid.NewGuid().ToString();
            var successUrl = $"{Request.Scheme}://{Request.Host}/payment-success";
            var failUrl = $"{Request.Scheme}://{Request.Host}/payment-fail";
            var cancelUrl = $"{Request.Scheme}://{Request.Host}/payment-cancel";

            var paymentUrl = await _paymentService.InitiatePaymentAsync(amount, transactionId, successUrl, failUrl, cancelUrl);
            return Ok(new { Url = paymentUrl });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
}
