// See https://aka.ms/new-console-template for more information
using StarRezTest.Interface;

int MainMenuSelection = 0;
ConsoleKey key;
int vPos = 0;

Console.Title = "StarRez/SharePoint Utilities";

vPos = PrintMainMenuHeader();
do
{
    Console.CursorVisible = false;
    for (int i = 0; i < MenuOption.MenuOptions.Length; i++)
    {
        if (i == MainMenuSelection)
        {
            SetConsoleColours(ConsoleColor.White, ConsoleColor.Black);
        }
        else
        {
            SetConsoleColours(ConsoleColor.Black, ConsoleColor.White);
        }

        Console.WriteLine($"{i + 1}. {MenuOption.MenuOptions[i].Name}");
    }

    SetConsoleColours(ConsoleColor.Black, ConsoleColor.White);

    key = Console.ReadKey(true).Key;
    switch (key)
    {
        case ConsoleKey.DownArrow:
            SetSelectedMenuOption(MainMenuSelection + 1);
            break;
        case ConsoleKey.UpArrow:
            SetSelectedMenuOption(MainMenuSelection - 1);
            break;
        case ConsoleKey.Enter:
            Console.WriteLine();
            Console.CursorVisible = true;
            MenuOption.MenuOptions[MainMenuSelection].OnSelect?.Invoke();
            CLI.ColourPrint("\nFunction executed successfully! Press any key to return to the Main Menu... ", ConsoleColor.Yellow, false);
            Console.ReadKey(true);
            Console.Clear();
            vPos = PrintMainMenuHeader();
            break;
        default:
            Console.Beep();
            break;
    }
    Console.SetCursorPosition(0, vPos);
}
while (key != ConsoleKey.Escape);

void SetConsoleColours(ConsoleColor background, ConsoleColor foreground)
{
    Console.BackgroundColor = background;
    Console.ForegroundColor = foreground;
}

void SetSelectedMenuOption(int index)
{
    MainMenuSelection = Math.Clamp(index, 0, MenuOption.MenuOptions.Length - 1);
}

int PrintMainMenuHeader()
{
    SetConsoleColours(ConsoleColor.Black, ConsoleColor.White);

    Console.WriteLine("MAIN MENU");
    CLI.HorizonalLine('=');
    Console.WriteLine();

    return Console.GetCursorPosition().Top;
}