namespace DrinksAPI.Services;

public static class Formatter
{
    public static string InstructionsFormat(string? instructions)
    {
        // Formats drink instructions into a list format in the console output
        instructions = instructions ?? "";
        return instructions.Replace(". ", ".\n");
    }
}
