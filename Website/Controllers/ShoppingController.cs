using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Website.Services;

namespace Website.Controllers
{
    [Route("api/[controller]")]
    public class ShoppingController : Controller
    {
        private readonly IShoppingService _shoppingService;

        public ShoppingController(IShoppingService shoppingService)
        {
            _shoppingService = shoppingService;
        }

        [HttpGet("[action]")]
        public async Task<List<ShoppingItem>> Items()
        {
            var shoppingItems = await _shoppingService.GetItemsAsync();
            return shoppingItems;
        }
    }
}
