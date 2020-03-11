using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Website.Services
{
    public interface IShoppingService
    {
        Task<List<ShoppingItem>> GetItemsAsync();
    }

    public class ShoppingService : IShoppingService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ShoppingService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<List<ShoppingItem>> GetItemsAsync()
        {
            const string Uri = "https://helperfunctions-dev-as.azurewebsites.net/api/GetIngredientsListFromTrello?code=Tcj1MWtaQiGRHqZDOeyQXJqyR3UEOvxM6B9ZAAAppvUatvkpmQHMUA==";
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetStringAsync(Uri);
            var recipes = JsonConvert.DeserializeObject<List<Recipe>>(response);

            var ingredients = recipes.SelectMany(recipe => SplitIntoIngredients(recipe));
            var shoppingItems = ConvertToShoppingItems(ingredients).ToList();

            return shoppingItems;
        }

        private IEnumerable<(string RecipeName, Ingredient Ingredient)> SplitIntoIngredients(Recipe recipe)
        {
            foreach (var ingredientString in recipe.Ingredients)
            {
                var ingredient = ParseIngredient(ingredientString);
                yield return (recipe.RecipeName, ingredient);
            }
        }

        private Ingredient ParseIngredient(string ingredientString)
        {
            var regex = new Regex(@"(?<Quantity>\d*(\.*)\d*)\s*(?<QuantityType>(g|\b[cans|pack|tsp|cloves|pinch|bottle|tbsp]*\b))\s*(?<Name>[\w\s]+)");
            var match = regex.Match(ingredientString);

            var quantityGroup = match.Groups["Quantity"];
            var quantityTypeGroup = match.Groups["QuantityType"];
            var nameGroup = match.Groups["Name"];

            var quantity = quantityGroup.Success && quantityGroup.Value.Length > 0 
                ? Decimal.Parse(quantityGroup.Value) 
                : 1M;
            var quantityType = quantityTypeGroup.Success && quantityTypeGroup.Value.Length > 0
                ? quantityTypeGroup.Value
                : "";
            var name = nameGroup.Value;

            var ingredient = new Ingredient
            {
                Name = name,
                Quantity = quantity,
                QuantityType = quantityType,
            };
            return ingredient;
        }

        private IEnumerable<ShoppingItem> ConvertToShoppingItems(IEnumerable<(string RecipeName, Ingredient Ingredient)> ingredients)
        {
            var shoppingItems =
                from ingredient in ingredients
                group ingredient by new { Name = ingredient.Ingredient.Name.ToLowerInvariant(), ingredient.Ingredient.QuantityType }
                into ingredientGroup
                select new ShoppingItem
                {
                    Ingredient = new Ingredient
                    {
                        Name = ingredientGroup.Key.Name,
                        Quantity = ingredientGroup.Sum(item => item.Ingredient.Quantity),
                        QuantityType = ingredientGroup.Key.QuantityType,
                    },
                    RecipeNames = ingredientGroup.Select(item => item.RecipeName).ToList(),
                };
            return shoppingItems;
        }
    }

    public class ShoppingItem
    {
        public Ingredient Ingredient { get; set; }
        public List<string> RecipeNames { get; set; }
    }

    public class Ingredient
    {
        public string Name { get; set; }
        public decimal Quantity { get; set; }
        public string QuantityType { get; set; }
    }

    public class Recipe
    {
        public string RecipeName { get; set; }
        public List<string> Ingredients { get; set; }
    }
}
