using System;

namespace TRVSCore
{
    /// <summary>
    ///     Provides functionality for interacting with a console.
    /// </summary>
    public static class ConsoleIO
    {
        /// <summary>
        ///     Gives a yes/no prompt and evaluates user response.
        /// </summary>
        /// <returns>
        ///     <see langword="true"/> if <c>yes</c>/<c>y</c>, <see langword="false"/> if <c>no</c>/<c>n</c>
        /// </returns>
        public static bool UserPromptYesNo(string promptText = "")
        {
            if (string.IsNullOrEmpty(promptText))
                promptText = "Yes or no? [y/n]: ";

            bool value = false;
            bool validInput = false;
            while (!validInput)
            {
                Console.Write(promptText);
                string inputString = Console.ReadLine();
                Console.WriteLine();
                if (!string.IsNullOrEmpty(inputString))
                {
                    inputString = inputString.ToLower();
                    if (inputString == "yes" || inputString == "y")
                    {
                        value = true;
                        validInput = true;
                    }
                    else if (inputString == "no" || inputString == "n")
                    {
                        validInput = true;
                    }
                }
            }

            return value;
        }

        /// <summary>
        ///     Pretty-prints a colored header sized <paramref name="printWidth"/> wide.
        /// </summary>
        /// <param name="title">Header title</param>
        /// <param name="subtitle">Header subtitle</param>
        /// <param name="foregroundColor">Foreground color</param>
        /// <param name="printWidth">Set the width of the console output.</param>
        /// <remarks>
        ///     Credits: https://stackoverflow.com/a/48755366/10466817.
        /// </remarks>
        public static void PrintHeader(string title, string subtitle = "", ConsoleColor foregroundColor = ConsoleColor.Gray, int printWidth = 80)
        {
            string titleContent = HeaderHelper(title, "║");
            string subtitleContent = HeaderHelper(subtitle, "║");
            string borderLine = new string('═', printWidth - 2);
            
            Console.ForegroundColor = foregroundColor;

            Console.WriteLine($"╔{borderLine}╗");   // Top border
            Console.WriteLine(titleContent);        // First line
            if (!string.IsNullOrEmpty(subtitle))
                Console.WriteLine(subtitleContent); // Second line
            Console.WriteLine($"╚{borderLine}╝");   // Bottom border

            Console.ResetColor();
        }

        /// <summary>
        ///     Returns <paramref name="text"/> centered over <paramref name="printWidth"/>
        ///     with <paramref name="edgeString"/> on each side.
        ///     If <paramref name="text"/> is too wide, it is returned with only a space
        ///     between <paramref name="text"/> and <paramref name="edgeString"/>.
        /// </summary>
        /// <param name="text">Text to center</param>
        /// <param name="edgeString">Left and right edge text, default is none</param>
        /// <param name="printWidth">Width of the console output</param>
        /// <returns>
        ///     Center-aligned <paramref name="text"/> with a maximum gap
        ///     between <paramref name="text"/> and <paramref name="edgeString"/>.
        /// </returns>
        private static string HeaderHelper(string text, string edgeString = "", int printWidth = 80)
        {
            int width = printWidth - 2 * edgeString.Length;
            if (width < text.Length)
                return $"{edgeString} {text} {edgeString}";
            return string.Format(
                $"{edgeString}{{0,{width / 2 + text.Length / 2}}}{{1,{width - width / 2 - text.Length / 2 + edgeString.Length}}}",
                text,
                edgeString
            );
        }

        /// <summary>
        ///     Print a one-line banner from <paramref name="text"/>.
        ///     Prints normally if <paramref name="text"/> is too wide.
        /// </summary>
        /// <param name="text">Text to print</param>
        /// <param name="foregroundColor">Foreground color</param>
        /// <param name="printWidth">Width of the output</param>
        public static void PrintCentered(string text, ConsoleColor foregroundColor = ConsoleColor.Gray, int printWidth = 80)
        {
            PrintWithColor(text.Length > printWidth ? text : HeaderHelper(text), foregroundColor);
        }

        /// <summary>
        ///     Writes to console in a given color, then resets the console color.
        /// </summary>
        /// <param name="text">Text to print</param>
        /// <param name="foregroundColor">Foreground color</param>
        public static void PrintWithColor(string text, ConsoleColor foregroundColor)
        {
            Console.ForegroundColor = foregroundColor;
            Console.WriteLine(text);
            Console.ResetColor();
        }
    }
}
