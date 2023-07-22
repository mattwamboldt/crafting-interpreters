using System;
using System.Collections.Generic;
using System.IO;

namespace lox.net
{
    class Program
    {
        const int ERROR_INVALID_DATA = 13;
        const int ERROR_BAD_ARGUMENTS = 160;
        const int ERROR_INTERNAL_ERROR = 1359;

        private static readonly Interpreter interpreter = new Interpreter();
        static bool hadError = false;
        static bool hadRuntimeError = false;

        static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: nlox [script]");
                System.Environment.Exit(ERROR_BAD_ARGUMENTS);
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

            if (hadError) System.Environment.Exit(ERROR_INVALID_DATA);
            if (hadRuntimeError) System.Environment.Exit(ERROR_INTERNAL_ERROR);
        }

        // TODO: Challenge 8.1 - Allow the REPL to print expression values again
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
            Parser parser = new Parser(tokens);
            List<Stmt> statements = parser.Parse();

            // Stop if there was a syntax error.
            if (hadError) return;

            interpreter.Interpret(statements);
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

        public static void Error(Token token, string message)
        {
            if (token.type == TokenType.EOF)
            {
                Report(token.line, " at end", message);
            }
            else
            {
                Report(token.line, " at '" + token.lexeme + "'", message);
            }
        }

        public static void RuntimeError(RuntimeError error)
        {
            Console.WriteLine(error.Message + "\n[line " + error.token.line + "]");
            hadRuntimeError = true;
        }
    }
}
