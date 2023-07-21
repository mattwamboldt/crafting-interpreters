namespace tool
{
    public class Program
    {
        const int ERROR_BAD_ARGUMENTS = 160;

        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.Error.WriteLine("Usage: generate_ast <output directory>");
                Environment.Exit(ERROR_BAD_ARGUMENTS);
            }

            string outputDir = args[0];
            DefineAst(outputDir, "Expr", new List<string>()
            {
              "Binary   : Expr left, Token op, Expr right",
              "Grouping : Expr expression",
              "Literal  : object value",
              "Unary    : Token op, Expr right"
            });
        }

        private static void DefineAst(string outputDir, string baseName, List<string> types)
        {
            string path = outputDir + "/" + baseName + ".cs";
            using(var writer = File.CreateText(path))
            {
                writer.WriteLine("namespace lox.net");
                writer.WriteLine("{");
                writer.WriteLine("\tpublic abstract class " + baseName);
                writer.WriteLine("\t{");

                DefineVisitor(writer, baseName, types);

                // The AST classes.
                foreach (string type in types)
                {
                    string className = type.Split(":")[0].Trim();
                    string fields = type.Split(":")[1].Trim();
                    DefineType(writer, baseName, className, fields);
                }

                writer.WriteLine();
                writer.WriteLine("\t\tpublic abstract R Accept<R>(IVisitor<R> visitor);");

                writer.WriteLine("\t}");
                writer.WriteLine("}");
            }
        }

        private static void DefineVisitor(StreamWriter writer, string baseName, List<string> types)
        {
            writer.WriteLine("\t\tpublic interface IVisitor<R>");
            writer.WriteLine("\t\t{");

            foreach (string type in types)
            {
                string typeName = type.Split(":")[0].Trim();
                writer.WriteLine("\t\t\tR Visit" + typeName + baseName + "(" +
                    typeName + " " + baseName.ToLower() + ");");
            }

            writer.WriteLine("\t\t}");
        }

        private static void DefineType(StreamWriter writer, string baseName, string className, string fieldList)
        {
            writer.WriteLine("\t\tpublic class " + className + " : " + baseName);
            writer.WriteLine("\t\t{");

            // Constructor.
            writer.WriteLine("\t\t\tpublic " + className + "(" + fieldList + ")");
            writer.WriteLine("\t\t\t{");

            // Store parameters in fields.
            string[] fields = fieldList.Split(", ");
            foreach (string field in fields)
            {
                string name = field.Split(" ")[1];
                writer.WriteLine("\t\t\t\tthis." + name + " = " + name + ";");
            }

            writer.WriteLine("\t\t\t}");

            // Visitor Pattern.
            writer.WriteLine();
            writer.WriteLine("\t\t\tpublic override R Accept<R>(IVisitor<R> visitor)");
            writer.WriteLine("\t\t\t{");
            writer.WriteLine("\t\t\t\treturn visitor.Visit" + className + baseName + "(this);");
            writer.WriteLine("\t\t\t}");

            // Fields.
            writer.WriteLine();
            foreach (string field in fields)
            {
                writer.WriteLine("\t\t\tpublic readonly " + field + ";");
            }

            writer.WriteLine("\t\t}");
        }
    }
}