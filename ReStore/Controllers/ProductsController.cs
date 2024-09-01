using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReStore.Data;
using ReStore.Entities;

namespace ReStore.Controllers
{
    [ApiController]
    [Route("ReStore/[controller]")]
    public class ProductsController:ControllerBase
    {
        private readonly StoreContext _context;
        public ProductsController(StoreContext context)
        {
            _context = context;
        }


        //creat Endpoint
        [HttpGet]
        public async Task <ActionResult<List<Product>>> GetProducts()
        {
            return await _context.Products.ToListAsync();
            
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            return await _context.Products.FindAsync(id);
        }
    }
}
