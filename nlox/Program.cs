using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Principal;

namespace lox.net
{
    class Program
    {
        const int ERROR_INVALID_DATA = 13;
        const int ERROR_BAD_ARGUMENTS = 160;

        static bool hadError = false;

        static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: nlox [script]");
                Environment.Exit(ERROR_BAD_ARGUMENTS);
            }
            else if (args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
            {
                RunPrompt();
            }
        }

        private static void RunFile(string path)
        {
            string contents = File.ReadAllText(path);
            Run(contents);

            if (hadError) {
                Environment.Exit(ERROR_BAD_ARGUMENTS);
            }
        }

        private static void RunPrompt()
        {
            while (true)
            {
                Console.Write("> ");
                string line = Console.ReadLine();
                if (line == null) break;
                Run(line);
                hadError = false;
            }
        }

        private static void Run(string source)
        {
            Scanner scanner = new Scanner(source);
            List<Token> tokens = scanner.ScanTokens();

            // For now, just print the tokens.
            foreach (var token in tokens)
            {
                Console.WriteLine(token);
            }
        }

        // TODO: Implement a more robust error handling to show the offending line and possibly column
        public static void Error(int line, string message)
        {
            Report(line, "", message);
        }

        private static void Report(int line, string where, string message)
        {
            Console.Error.WriteLine($"[line {line}] Error{where}: {message}");
            hadError = true;
        }
    }
}
