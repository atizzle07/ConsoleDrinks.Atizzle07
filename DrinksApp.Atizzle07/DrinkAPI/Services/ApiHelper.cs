using DrinksAPI.Models;
using DrinksApp.Models;
using Newtonsoft.Json;

namespace DrinksAPI.Services;

public static class ApiHelper
{
    private static HttpClient? ApiClient { get; set; }

    public static void InitializeClient()
    {
        ApiClient = new();
        
        // ApiClient.BaseAddress = new Uri("https://www.thecocktaildb.com/api/json/v1/1/");
        ApiClient.BaseAddress = new Uri("https://www.thecocktaildb.com/api/json/v1/10/");
        ApiClient.DefaultRequestHeaders.Accept.Clear();
        ApiClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public static async Task<List<string>> GetAllCategories()
    {
        using HttpResponseMessage response = await ApiClient!.GetAsync("list.php?c=list");

        if (response.IsSuccessStatusCode)
        {
            string jsonResponse = await response.Content.ReadAsStringAsync();
            CategoryResponse cr = JsonConvert.DeserializeObject<CategoryResponse>(jsonResponse)!;

            List<string> items = new();
            foreach (CategoryItem item in cr.Drinks)
            {
                items.Add(item.Name);
            }
            return items;
        }
        else
        { 
            throw new HttpRequestException(response.ReasonPhrase);
        }
    }

    public static async Task<List<KeyValuePair<int, string>>> GetDrinksList(string category)
    {
        using HttpResponseMessage response = await ApiClient!.GetAsync($"filter.php?c={category}");

        if (response.IsSuccessStatusCode)
        {
            string jsonResponse = await response.Content.ReadAsStringAsync();
            DrinkResponse dr = JsonConvert.DeserializeObject<DrinkResponse>(jsonResponse)!;

            List<KeyValuePair<int, string>> items = new();
            foreach (DrinkItem item in dr.Drinks)
            {
                items.Add(new KeyValuePair<int, string>(item.Id, item.Name));
            }
            return items;
        }
        else
        {
            throw new HttpRequestException(response.ReasonPhrase);
        }
    }

    public static async Task<RecipeResponse> GetRecipe(string recipeId)
    {
        using HttpResponseMessage response = await ApiClient!.GetAsync($"lookup.php?i={recipeId}");

        if (response.IsSuccessStatusCode)
        {
            string jsonResponse = await response.Content.ReadAsStringAsync();

            ApiResponse? apiResponse = JsonConvert.DeserializeObject<ApiResponse>(jsonResponse)!; //Takes the raw api response and maps it to the apiresponse class
            RecipeDTO? recipeDto = apiResponse?.Drinks?.FirstOrDefault(); //Takes the api response and pulls the list into a DTO object

            RecipeResponse recipeResponse = ResponseMapper.ReturnRecipeData(recipeDto!); // map recipeDTO to recipe object
            recipeResponse.InstructionsText = Formatter.InstructionsFormat(recipeResponse.InstructionsText);
            return recipeResponse;
        }
        else
        {
            throw new HttpRequestException(response.ReasonPhrase);
        }
    }

}