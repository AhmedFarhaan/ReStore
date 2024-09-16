using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReStore.Data;
using ReStore.DTOs;
using ReStore.Entities;
using ReStore.Entities.OrderAggregate;
using ReStore.Extensions;

namespace ReStore.Controllers
{
    [Authorize]
    public class OrdersController : BaseApiController
    {
        private readonly StoreContext _conetxt;

        public OrdersController(StoreContext conetxt)
        {
            _conetxt = conetxt;
        }

        [HttpGet]
        public async Task<ActionResult<List<OrderDto>>> GetOrders()
        {
            return await _conetxt.Orders
                .ProjectOrderToOrderDto()
                .Where(x => x.BuyerId == User.Identity.Name)
                .ToListAsync();

        }
        [HttpGet("{id}",Name = "GetOrder")]
        public async Task<ActionResult<OrderDto>> GetOrder(int id)
        {
            return await _conetxt.Orders
                .ProjectOrderToOrderDto()
                .Where(x => x.BuyerId == User.Identity.Name && x.Id == id)
                .FirstOrDefaultAsync();
        }

        [HttpPost]
        public async Task<ActionResult<int>> CreatOrder(CreateOrderDto orderDto)
        {
            var basket = await _conetxt.Baskets
                .RetrieveBasketWithItems(User.Identity.Name)
                .FirstOrDefaultAsync();
            if (basket == null) return BadRequest(new ProblemDetails { Title = "Could not locate basket" });

            var items = new List<OrderItem>();
            foreach (var item in basket.Items)
            {
                var productItem = await _conetxt.Products.FindAsync(item.ProductId);
                var itemOrdered = new ProductItemOrdered
                {
                    ProductId = productItem.Id,
                    Name = productItem.Name,
                    PictureUrl = productItem.PictureUrl
                };
                var orderItem = new OrderItem
                {
                    ItemOrdered = itemOrdered,
                    Price = productItem.Price,
                    Quantity = item.Quantity
                };
                items.Add(orderItem);
                productItem.QuantityInStock -= item.Quantity;
            }
            var subTotal = items.Sum(items => items.Price * items.Quantity);
            var deliveryFee = subTotal > 10000 ? 0 : 500;

            var order = new Order
            {
                OrderItems = items,
                BuyerId = User.Identity.Name,
                ShippingAddress = orderDto.ShippingAddress,
                Subtotal = subTotal,
                DeliveryFee = deliveryFee,
                PaymentIntentId = basket.PaymentIntetId,
            };
            _conetxt.Orders.Add(order);
            _conetxt.Baskets.Remove(basket);

            if (orderDto.SaveAddress)
            {
                var user = await _conetxt.Users
                    .Include(a=>a.Address)
                    .FirstOrDefaultAsync(x => x.UserName == User.Identity.Name);
                var address = new UserAddress
                {
                    FullName = orderDto.ShippingAddress.FullName,
                    Address1 = orderDto.ShippingAddress.Address1,
                    Address2 = orderDto.ShippingAddress.Address2,
                    City = orderDto.ShippingAddress.City,
                    State = orderDto.ShippingAddress.State,
                    Zip = orderDto.ShippingAddress.Zip,
                    Country = orderDto.ShippingAddress.Country
                };
                user.Address = address;
            }

            var result = await _conetxt.SaveChangesAsync() > 0;
            if (result) return CreatedAtRoute("GetOrder", new { id = order.Id }, order.Id);
            return BadRequest("Problem creating order");
        }
    }
}
