using KurguWebsite.API.Controllers.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KurguWebsite.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BaseApiController : ControllerBase
    {
        private static readonly List<Product> _products = new()
        {
            new Product
            {
                Id = 1,
                Name = "Laptop",
                Description = "High-performance laptop",
                Price = 999.99m,
                Stock = 10,
                Category = "Electronics",
                CreatedDate = DateTime.Now.AddDays(-30)
            },
            new Product
            {
                Id = 2,
                Name = "Smartphone",
                Description = "Latest model smartphone",
                Price = 699.99m,
                Stock = 25,
                Category = "Electronics",
                CreatedDate = DateTime.Now.AddDays(-15)
            },
            new Product
            {
                Id = 3,
                Name = "Headphones",
                Description = "Wireless noise-cancelling headphones",
                Price = 199.99m,
                Stock = 50,
                Category = "Accessories",
                CreatedDate = DateTime.Now.AddDays(-7)
            },
            new Product
            {
                Id = 4,
                Name = "Keyboard",
                Description = "Mechanical gaming keyboard",
                Price = 89.99m,
                Stock = 35,
                Category = "Accessories",
                CreatedDate = DateTime.Now.AddDays(-20)
            },
            new Product
            {
                Id = 5,
                Name = "Monitor",
                Description = "27-inch 4K monitor",
                Price = 399.99m,
                Stock = 15,
                Category = "Electronics",
                CreatedDate = DateTime.Now.AddDays(-10)
            }
        };

        // GET: api/products
        [HttpGet]
        public ActionResult<IEnumerable<Product>> GetAllProducts()
        {
            return Ok(_products);
        }

        // GET: api/products/5
        [HttpGet("{id}")]
        public ActionResult<Product> GetProduct(int id)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound(new { message = $"Product with ID {id} not found" });
            }

            return Ok(product);
        }

        // GET: api/products/category/{category}
        [HttpGet("category/{category}")]
        public ActionResult<IEnumerable<Product>> GetProductsByCategory(string category)
        {
            var products = _products.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!products.Any())
            {
                return NotFound(new { message = $"No products found in category: {category}" });
            }

            return Ok(products);
        }

        // POST: api/products
        [HttpPost]
        public ActionResult<Product> CreateProduct(Product product)
        {
            product.Id = _products.Max(p => p.Id) + 1;
            product.CreatedDate = DateTime.Now;
            _products.Add(product);

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        // PUT: api/products/5
        [HttpPut("{id}")]
        public IActionResult UpdateProduct(int id, Product product)
        {
            var existingProduct = _products.FirstOrDefault(p => p.Id == id);

            if (existingProduct == null)
            {
                return NotFound(new { message = $"Product with ID {id} not found" });
            }

            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Price = product.Price;
            existingProduct.Stock = product.Stock;
            existingProduct.Category = product.Category;

            return NoContent();
        }

        // DELETE: api/products/5
        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(int id)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound(new { message = $"Product with ID {id} not found" });
            }

            _products.Remove(product);

            return NoContent();
        }

        // GET: api/products/search?name=laptop
        [HttpGet("search")]
        public ActionResult<IEnumerable<Product>> SearchProducts([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new { message = "Search term cannot be empty" });
            }

            var products = _products.Where(p => p.Name.Contains(name, StringComparison.OrdinalIgnoreCase)).ToList();

            return Ok(products);
        }
    }
}