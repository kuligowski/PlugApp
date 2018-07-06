using System;

namespace PluginApp
{
    public static class ConsoleManager
    {
        private static string[] topOptions = {"List all", "Execute plugin", "Interactive mode"}; //TODO: use enums

        private static void PrintOptions(int selectedIndex, string[] options)
        {
            Console.SetCursorPosition(0, 0);
            Console.Clear();
            for(int i = 0; i < options.Length; i++)
            {
                if (i == selectedIndex)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write("> ");
                }
                else
                {
                    Console.Write("  ");
                }

                Console.WriteLine(options[i]);
                Console.ResetColor();
            }
        }

        private static void ReadSelectedOption(ref int selectedIndex, ref bool exit, int optionsNumber)
        {
            switch (Console.ReadKey(true).Key)
            {
                case ConsoleKey.UpArrow:
                    selectedIndex = Math.Max(0, selectedIndex - 1);
                    break;
                case ConsoleKey.DownArrow:
                    selectedIndex = Math.Min(optionsNumber - 1, selectedIndex + 1);
                    break;
                case ConsoleKey.Enter:
                case ConsoleKey.Escape:
                    exit = true;
                    break;
            }
        }

        public static void PrintPluginsDescriptions(string[] descriptions)
        {
            for (var i = 0; i < descriptions.Length; i++)
            {
                Console.WriteLine($"{i} {descriptions[i]}");
            }

            Console.ReadKey();
        }

        public static PluginOption GetSelectedTopLevelOption()
        {
            return (PluginOption)GetSelectedOption(topOptions);
        }

        public static int GetSelectedOption(string[] options)
        {
            int selectedIndex = 0;
            bool exit = false;
            while (!exit)
            {
                PrintOptions(selectedIndex, options);
                ReadSelectedOption(ref selectedIndex, ref exit, topOptions.Length);
            }
            
            return selectedIndex;
        }
    }
}