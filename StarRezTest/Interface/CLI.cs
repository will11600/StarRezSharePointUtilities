using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarRezTest.Interface
{
    public static class CLI
    {
        public static int ColourPrint(string v, ConsoleColor foreground = ConsoleColor.White, bool includeLineBreak = true, ConsoleColor background = ConsoleColor.Black)
        {
            Console.ForegroundColor = foreground;
            Console.BackgroundColor = background;
            Console.Write($"{v}{(includeLineBreak ? "\n" : "")}");
            Console.ResetColor();
            return v.Length;
        }

        public static void WaitFor(string message, Func<bool> func, ConsoleColor backgroundColour = ConsoleColor.Yellow, ConsoleColor foregroundColour = ConsoleColor.Black)
        {
            DateTime startTime = DateTime.Now;
            ColourPrint(message + " ", foregroundColour, false, backgroundColour);
            var pos = Console.GetCursorPosition();
            do
            {
                Console.SetCursorPosition(pos.Left, pos.Top);
                ColourPrint($"{(DateTime.Now - startTime).TotalSeconds} ms", foregroundColour, false, backgroundColour);
                Thread.Sleep(1);
            }
            while (func());
            ClearLine();
        }

        public static void WaitFor(string message, TimeSpan timeout, ConsoleColor backgroundColour = ConsoleColor.Yellow, ConsoleColor foregroundColour = ConsoleColor.Black)
        {
            DateTime startTime = DateTime.Now;
            WaitFor(message, () => DateTime.Now > (startTime + timeout), backgroundColour, foregroundColour);
        }

        public static void ClearLine(int count = -1, ConsoleColor backgroundColour = ConsoleColor.Black)
        {
            var pos = Console.GetCursorPosition();
            Console.SetCursorPosition(0, pos.Top);
            if (count < 1) { count = Console.WindowWidth; }
            ColourPrint(new string(' ', count), background: backgroundColour);
            Console.SetCursorPosition(0, pos.Top);
        }

        public static void HorizonalLine(char character, string embedString = "", bool includeSpace = true, ConsoleColor foregroundColour = ConsoleColor.White)
        {
            int offset = 0;
            if (!string.IsNullOrWhiteSpace(embedString))
            {
                embedString = embedString.Trim();
                ColourPrint($"{embedString}{(includeSpace ? " " : "")}", foregroundColour, false);
                offset = embedString.Length + 1;
            }
            ColourPrint(new string(character, Console.BufferWidth - offset), foregroundColour);
        }
    }
}
