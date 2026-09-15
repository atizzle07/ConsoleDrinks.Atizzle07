using DrinksAPI.Services;
using DrinksApp;

string categoryChoice;
string drinkChoiceId;
string recipeChoice;
ApiHelper.InitializeClient();
UI.WelcomeMessage();

while (true)
{
    // step 1 - select and display drink category
    categoryChoice = await UI.GetCategoryChoice();
    if (categoryChoice == "Exit")
        break;

    while (true)
    {
        // step 2 - select and display available drinks and return id of selected drink
        drinkChoiceId = await UI.GetDrinkChoice(categoryChoice);
        if (drinkChoiceId == "Back")
            break;

        // step 3 - select and display the selected recipe
        recipeChoice = await UI.DisplayRecipeTable(drinkChoiceId);

        if (recipeChoice == "Drinks")
            continue;
        if (recipeChoice == "Categories")
            break;
        if (recipeChoice == "Exit")
            return;
    }
}
