using Microsoft.AspNetCore.Mvc;
using NLayer.Core.Service;

namespace NLayer.Web.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _sercive;

        public ProductsController(IProductService sercive)
        {
            _sercive = sercive;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _sercive.GetProductsWithCategory());
        }
    }
}
