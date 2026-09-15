using System.Net.Mime;
using System.Reflection;
using DrinksAPI.Models;
using DrinksAPI.Services;
using Spectre.Console;

namespace DrinksApp;

public class UI
{
    #region Menu Setups
    public static void WelcomeMessage()
    {
        DrawTitle();
        AnsiConsole.MarkupLine("[bold orange3]Press Enter to Continue...[/]");
        Console.ReadKey();
    }
    private static void DrawTitle()
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
        List<string> categoryMenu = new();
        try
        {
            categoryMenu = await LoadCategories();
        } 
        catch (HttpRequestException e)
        {
            AnsiConsole.MarkupLine($"[bold italic red]An error occurred accessing online data: {e.Message}[/]");
            AnsiConsole.MarkupLine($"\n[bold]Press [green]ENTER[/] to exit the application[/]");
            Console.ReadLine();
            return "Exit";
        }
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
        List<KeyValuePair<int, string>> drinksMenuWithId = new();
        try
        {
            drinksMenuWithId = await LoadDrinks(category);
        } 
        catch (HttpRequestException e)
        {
            AnsiConsole.MarkupLine($"[bold italic red]An error occurred accessing online data: {e.Message}[/]");
            AnsiConsole.MarkupLine($"\n[bold]Press [green]ENTER[/] to exit the application[/]");
            Console.ReadLine();
            return "Exit";
        }
		    
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
        RecipeResponse recipe = new();
        try
        {
            recipe = await ApiHelper.GetRecipe(drinkChoiceId);
        } 
        catch (HttpRequestException e)
        {
            AnsiConsole.MarkupLine($"[bold italic red]An error occurred accessing online data: {e.Message}[/]");
            AnsiConsole.MarkupLine($"\n[bold]Press [green]ENTER[/] to exit the application[/]");
            Console.ReadLine();
            return "Exit";
        }
        
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
        RecipeResponse recipe = new();
        try
        {
            recipe = await ApiHelper.GetRecipe(drinkChoiceId);
        }
        catch (HttpRequestException e)
        {
            AnsiConsole.MarkupLine($"[bold italic red]An error occurred accessing online data: {e.Message}[/]");
            Console.ReadLine();
        }

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
    private static async Task<List<string>> LoadCategories()
    {
        var categoryMenu = new List<string>();

        categoryMenu!.AddRange(await ApiHelper.GetAllCategories());
        categoryMenu.Insert(0, "Exit");
        return categoryMenu;
    }

    private static async Task<List<KeyValuePair<int, string>>> LoadDrinks(string category)
    {
        var drinksMenu = new List<KeyValuePair<int, string>>();
        drinksMenu.AddRange(await ApiHelper.GetDrinksList(category));
        drinksMenu.Insert(0, new KeyValuePair<int, string>(drinksMenu.Count, "Back"));
        return drinksMenu;
    }
    #endregion
}