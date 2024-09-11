using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReStore.Data;
using ReStore.Entities;
using ReStore.Extensions;
using ReStore.RequestHelpers;
using System.Text.Json;

namespace ReStore.Controllers
{
    public class ProductsController:BaseApiController
    {
        private readonly StoreContext _context;
        public ProductsController(StoreContext context)
        {
            _context = context;
        }


        //creat Endpoint
        [HttpGet]
        public async Task <ActionResult<PagedList<Product>>> GetProducts([FromQuery]ProductsParams productsParams)
        {
            var query = _context.Products
                .Sort(productsParams.OrderBy)
                .Search(productsParams.SearchTerm)
                .Filter(productsParams.Brands,productsParams.Types)
                .AsQueryable();

            var products = await PagedList<Product>.ToPagedList(query, productsParams.PageNumber, productsParams.PageSize);

            Response.AddPaginationHeader(products.MetaData);
            return products;
            
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            return product;
        }
        [HttpGet("filters")]
        public async Task<IActionResult> GetFilters()
        {
            var brands = await _context.Products.Select(p => p.Brand).Distinct().ToListAsync();
            var types = await _context.Products.Select(p => p.Type).Distinct().ToListAsync();
            return Ok(new { brands, types });
        }
    }
}
