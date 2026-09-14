using DrinksAPI.Models;
using DrinksApp.Services;
using Spectre.Console;
using System.Reflection;

public class UI
{
    #region Menu Setups
    public static void WelcomeMessage()
    {
        DrawTitle();
        AnsiConsole.MarkupLine("[bold orange3]Press Enter to Continue...[/]");
        Console.ReadKey();
    }
    public static void DrawTitle()
    {
        Rule rule = new();
        rule.Border = BoxBorder.Heavy;
        var figlet = new FigletText("DRINK RECIPES")
        {
            Justification = Justify.Center,
            Color = Color.White,
        };

        AnsiConsole.Write(rule);
        AnsiConsole.Write(figlet);
        AnsiConsole.Write(rule);
        Console.WriteLine("\n\n");
    }
    public static void AddSpace(int lines)
    {
        for (int i = 0; i < lines; i++)
        {
            Console.WriteLine();
        }
    }
    public static async Task<string> GetCategoryChoice()
    {
        // TODO - add exception handling here
        List<string> categoryMenu = await LoadCategories();

        Console.Clear();
        DrawTitle();
        var userInput = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
            .Title("Please select a Category:")
            .AddChoices(categoryMenu));
        return userInput;
    }
    public static async Task<string> GetDrinkChoice(string category)
    {
        // TODO - add exception handling here
        List<KeyValuePair<int, string>> drinksMenuWithId = await LoadDrinks(category);
        List<string> drinksMenu = new();
        foreach (var item in drinksMenuWithId)
            drinksMenu.Add(item.Value);

        Console.Clear();
        DrawTitle();
        AnsiConsole.MarkupLine($"Category Selected: [bold italic orange3]{category.ToUpper()}[/]");
        var userInput = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Please select a drink to view recipe:")
                .AddChoices(drinksMenu)
                .EnableSearch()
                .SearchPlaceholderText("Type to search...")
                .PageSize(20));

        // Select and return Menu Item ID based on the user's input
        if (userInput.ToLower() != "back")
        {
            userInput = drinksMenuWithId.FirstOrDefault(kvp => kvp.Value == userInput).Key.ToString();
            return userInput;
        }
        else return "Back";

    }
    public static async Task<string> DisplayRecipeTable(string drinkChoiceId)
    {
        Console.Clear();
        DrawTitle();
        
        // TODO - add exception handling here
        RecipeResponse recipe = await ApiHelper.GetRecipe(drinkChoiceId);

        var table = new Table().HideHeaders();
        var ingredientsTable = new Table();

        table
            .AddColumn("Type", col => col.RightAligned())
            .AddColumn("Value", col => col.LeftAligned());

        foreach (PropertyInfo property in recipe.GetType().GetProperties())
        {
            if (property.Name == "IngredientList" || property.Name == "Id" || property.Name == "InstructionsText")
                continue;
            else
                table.AddRow(
                    property.Name.ToString(),
                    property.GetValue(recipe)?.ToString() ?? "");
        }

        ingredientsTable
            .AddColumn("Ingredient", col => col.LeftAligned())
            .AddColumn("Amount", col => col.LeftAligned());
        if (recipe.IngredientList != null)
        {
            foreach (var item in recipe.IngredientList)
            {
                string _item = item.Ingredient ?? "";
                if (_item == "")
                    continue;
                else
                    ingredientsTable.AddRow(
                        item.Ingredient ?? "",
                        item.Measurement ?? "");
            }
        }
        AnsiConsole.MarkupLine("[bold orange3]Drink Information[/]");
        AnsiConsole.Write(table);
        AddSpace(2);
        AnsiConsole.MarkupLine("[bold orange3]Ingredient List[/]");
        AnsiConsole.Write(ingredientsTable);
        AddSpace(2);
        AnsiConsole.MarkupLine("[bold orange3]Instructions[/]");
        AnsiConsole.WriteLine(recipe.InstructionsText);
        AddSpace(3);

        string userChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Where would you like to go?")
                .AddChoices(
                    "Drinks",
                    "Categories",
                    "Exit"
                ));

        return userChoice;

    }
    public static async Task DisplayRecipeBasic(string drinkChoiceId)
    {
        Console.Clear();

        // call API to get recipe from ID and load into object
        // TODO - add exception handling here
        RecipeResponse recipe = await ApiHelper.GetRecipe(drinkChoiceId);

        // Print Table headers
        Console.WriteLine("Type\t\tValue");
        for (int i = 0; i < 20; i++)
        {
            Console.Write('=');
        }

        Console.WriteLine();

        foreach (var property in recipe.GetType().GetProperties())
        {
            Console.WriteLine($"{property.Name}\t\t{property.GetValue(recipe)}");
        }
        Console.ReadKey();
    }

    #endregion
    #region LoadData
    public static async Task<List<string>> LoadCategories()
    {
        var _categoryMenu = new List<string>();
        if (_categoryMenu.Count == 0)
        {
            // TODO - add exception handling here
            _categoryMenu!.AddRange(await ApiHelper.GetAllCategories());
            _categoryMenu.Insert(0, "Exit");
            return _categoryMenu;
        }
        else return _categoryMenu;
    }

    public static async Task<List<KeyValuePair<int, string>>> LoadDrinks(string category)
    {
        var _drinksMenu = new List<KeyValuePair<int, string>>();
        // TODO - add exception handling here
        _drinksMenu.AddRange(await ApiHelper.GetDrinksList(category));
        _drinksMenu.Insert(0, new KeyValuePair<int, string>(_drinksMenu.Count, "Back"));
        return _drinksMenu;
    }



    #endregion
}