using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReStore.Data;
using ReStore.DTOs;
using ReStore.Entities;

namespace ReStore.Controllers
{
    public class BasketController : BaseApiController
    {
        public readonly StoreContext _context;
        public BasketController(StoreContext context)
        {
            _context = context;
        }


        //endpoint for fetching new basket
        [HttpGet(Name ="GetBasket")]
        public async Task<ActionResult<BasketDto>> GetBasket()
        {
            var basket = await RetrieveBasket();
            if (basket == null) return NotFound();
            return MapBasketToDto(basket);
        }

        



        //endpoint for adding item to basket
        [HttpPost]
        public async Task<ActionResult> AddItemToBasket(int productId,int quantity)
        {
            //get basket || creat basket
            var basket = await RetrieveBasket();

            if (basket == null) basket= CreatBasket();
            //get product
            var product = await _context.Products.FindAsync(productId);

            if (product == null) return NotFound();
            //add item
            basket.AddItem(product, quantity);
            //save changes
            var result = await _context.SaveChangesAsync()>0;

            if(result) return CreatedAtRoute("GetBasket",MapBasketToDto(basket));

            return BadRequest(new ProblemDetails { Title = "problem saving the item to the basket" });
        }

        

        [HttpDelete]
        public async Task<ActionResult> RemoveBasketItem(int productId, int quantity)
        {
            //get basket
            var basket = await RetrieveBasket();
            if (basket == null) return NotFound();
            //remove item
            basket.RemoveItem(productId, quantity);
            //save changes
            var result= await _context.SaveChangesAsync()>0;
            if (result) return Ok();
            return BadRequest(new ProblemDetails { Title="problem removing item from the basket"});
        }


        private async Task<Basket> RetrieveBasket()
        {
            var basket = await _context.Baskets
                .Include(i => i.Items) //get item that include the product
                .ThenInclude(p => p.Product)
                .FirstOrDefaultAsync(x => x.BuyerId == Request.Cookies["buyerId"]);
            return basket;
        }
        private Basket CreatBasket()
        {
            var buyerId = Guid.NewGuid().ToString();
            var cookieOptions = new CookieOptions { IsEssential = true, Expires = DateTime.Now.AddDays(30) };
            Response.Cookies.Append("buyerId",buyerId,cookieOptions);
            var basket = new Basket { BuyerId = buyerId };
            _context.Baskets.Add(basket);
            return basket;
        }
        private BasketDto MapBasketToDto(Basket basket)
        {
            return new BasketDto
            {
                Id = basket.Id,
                BuyerId = basket.BuyerId,
                Items = basket.Items.Select(item => new BasketItemDto
                {
                    ProductId = item.ProductId,
                    Name = item.Product.Name,
                    Price = item.Product.Price,
                    PictureUrl = item.Product.PictureUrl,
                    Type = item.Product.Type,
                    Brand = item.Product.Brand,
                    Quantity = item.Quantity
                }).ToList()
            };
        }
    }
}
