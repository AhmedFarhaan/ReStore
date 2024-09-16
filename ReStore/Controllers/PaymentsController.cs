using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReStore.Data;
using ReStore.DTOs;
using ReStore.Entities.OrderAggregate;
using ReStore.Extensions;
using ReStore.Services;
using Stripe;

namespace ReStore.Controllers
{
    public class PaymentsController :BaseApiController
    {
        private readonly PaymentService _paymentService;
        private readonly StoreContext _context;
        private readonly IConfiguration _config;

        public PaymentsController(PaymentService paymentService,StoreContext context,IConfiguration config)
        {
            _paymentService = paymentService;
            _context = context;
            _config = config;
        }
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<BasketDto>> CreateOrUpdatePaymentIntent()
        {
            var basket = await _context.Baskets
                .RetrieveBasketWithItems(User.Identity.Name)
                .FirstOrDefaultAsync();

            if (basket == null) return NotFound();

            var intent = await _paymentService.CreateOrUpdatePaymentIntent(basket);

            if (intent == null) return BadRequest(new ProblemDetails { Title = "problem creating payment intent"});

            basket.PaymentIntetId = intent.Id ?? intent.Id;
            basket.ClientSecret = intent.ClientSecret ?? intent.ClientSecret;

            _context.Update(basket);

            var results = await _context.SaveChangesAsync() > 0;

            if (!results) return BadRequest(new ProblemDetails { Title = "Problem Updating basket with intent" });

            return basket.MapBasketToDto();
        }
        [HttpPost("webhook")]
        public async Task<ActionResult> StripeWebHook()
        {
            var json =await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"],
               _config["StripeSettings:WhSecret"]);

            var charge = (Charge)stripeEvent.Data.Object;

            var order = await _context.Orders.FirstOrDefaultAsync(x => x.PaymentIntentId == charge.PaymentIntentId);

            if (charge.Status == "succeeded") order.OrderStatus = OrderStatus.PaymentRecieved;
            await _context.SaveChangesAsync();
            return new EmptyResult();
        }
    }
}
