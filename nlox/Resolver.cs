using System.Collections.Generic;

// TODO: Challenge 11.3 - Report an error if a local variable is never used
// TODO: Challenge 11.4 - Change environment to array lookup by associating each local variable with an index
namespace lox.net
{
    public class Resolver : Expr.IVisitor<object>, Stmt.IVisitor<object>
    {
        private enum FunctionType
        {
            NONE,
            FUNCTION,
            INITIALIZER,
            METHOD
        }

        private enum ClassType
        {
            NONE,
            CLASS,
            SUBCLASS
        }

        private readonly Interpreter interpreter;
        private readonly Stack<Dictionary<string, bool>> scopes = new Stack<Dictionary<string, bool>>();
        private FunctionType currentFunction = FunctionType.NONE;
        private ClassType currentClass = ClassType.NONE;

        public Resolver(Interpreter interpreter)
        {
            this.interpreter = interpreter;
        }

        public void Resolve(List<Stmt> statements)
        {
            foreach (Stmt statement in statements)
            {
                Resolve(statement);
            }
        }

        public object VisitBlockStmt(Stmt.Block stmt)
        {
            BeginScope();
            Resolve(stmt.statements);
            EndScope();
            return null;
        }

        public object VisitClassStmt(Stmt.Class stmt)
        {
            ClassType enclosingClass = currentClass;
            currentClass = ClassType.CLASS;

            Declare(stmt.name);
            Define(stmt.name);

            if (stmt.superclass != null)
            {
                if (stmt.name.lexeme == stmt.superclass.name.lexeme)
                {
                    Program.Error(stmt.superclass.name, "A class can't inherit from itself.");
                }

                currentClass = ClassType.SUBCLASS;
                Resolve(stmt.superclass);
            }

            if (stmt.superclass != null)
            {
                BeginScope();
                scopes.Peek()["super"] = true;
            }

            BeginScope();
            scopes.Peek()["this"] = true;

            foreach (Stmt.Function method in stmt.methods)
            {
                FunctionType declaration = FunctionType.METHOD;
                if (method.name.lexeme == "init")
                {
                    declaration = FunctionType.INITIALIZER;
                }

                ResolveFunction(method, declaration);
            }

            EndScope();

            if (stmt.superclass != null) EndScope();

            currentClass = enclosingClass;
            return null;
        }

        public object VisitExpressionStmt(Stmt.Expression stmt)
        {
            Resolve(stmt.expression);
            return null;
        }

        public object VisitFunctionStmt(Stmt.Function stmt)
        {
            Declare(stmt.name);
            Define(stmt.name);

            ResolveFunction(stmt, FunctionType.FUNCTION);
            return null;
        }

        public object VisitIfStmt(Stmt.If stmt)
        {
            Resolve(stmt.condition);
            Resolve(stmt.thenBranch);
            if (stmt.elseBranch != null) Resolve(stmt.elseBranch);
            return null;
        }

        public object VisitPrintStmt(Stmt.Print stmt)
        {
            Resolve(stmt.expression);
            return null;
        }

        public object VisitReturnStmt(Stmt.Return stmt)
        {
            if (currentFunction == FunctionType.NONE)
            {
                Program.Error(stmt.keyword, "Can't return from top-level code.");
            }

            if (stmt.value != null)
            {
                if (currentFunction == FunctionType.INITIALIZER)
                {
                    Program.Error(stmt.keyword, "Can't return a value from an initializer.");
                }

                Resolve(stmt.value);
            }

            return null;
        }

        public object VisitVarStmt(Stmt.Var stmt)
        {
            Declare(stmt.name);
            if (stmt.initializer != null)
            {
                Resolve(stmt.initializer);
            }

            Define(stmt.name);
            return null;
        }

        public object VisitWhileStmt(Stmt.While stmt)
        {
            Resolve(stmt.condition);
            Resolve(stmt.body);
            return null;
        }

        public object VisitAssignExpr(Expr.Assign expr)
        {
            Resolve(expr.value);
            ResolveLocal(expr, expr.name);
            return null;
        }

        public object VisitBinaryExpr(Expr.Binary expr)
        {
            Resolve(expr.left);
            Resolve(expr.right);
            return null;
        }

        public object VisitCallExpr(Expr.Call expr)
        {
            Resolve(expr.callee);

            foreach (Expr argument in expr.arguments)
            {
                Resolve(argument);
            }

            return null;
        }

        public object VisitGetExpr(Expr.Get expr)
        {
            Resolve(expr.obj);
            return null;
        }

        public object VisitGroupingExpr(Expr.Grouping expr)
        {
            Resolve(expr.expression);
            return null;
        }

        public object VisitLiteralExpr(Expr.Literal expr)
        {
            return null;
        }

        public object VisitLogicalExpr(Expr.Logical expr)
        {
            Resolve(expr.left);
            Resolve(expr.right);
            return null;
        }

        public object VisitSetExpr(Expr.Set expr)
        {
            Resolve(expr.value);
            Resolve(expr.obj);
            return null;
        }

        public object VisitSuperExpr(Expr.Super expr)
        {
            if (currentClass == ClassType.NONE)
            {
                Program.Error(expr.keyword, "Can't use 'super' outside of a class.");
            }
            else if (currentClass != ClassType.SUBCLASS)
            {
                Program.Error(expr.keyword, "Can't use 'super' in a class with no superclass.");
            }

            ResolveLocal(expr, expr.keyword);
            return null;
        }

        public object VisitThisExpr(Expr.This expr)
        {
            if (currentClass == ClassType.NONE)
            {
                Program.Error(expr.keyword, "Can't use 'this' outside of a class.");
                return null;
            }

            ResolveLocal(expr, expr.keyword);
            return null;
        }

        public object VisitUnaryExpr(Expr.Unary expr)
        {
            Resolve(expr.right);
            return null;
        }

        public object VisitVariableExpr(Expr.Variable expr)
        {
            if (scopes.Count > 0 && scopes.Peek().TryGetValue(expr.name.lexeme, out bool val) && val == false)
            {
                Program.Error(expr.name, "Can't read local variable in its own initializer.");
            }

            ResolveLocal(expr, expr.name);
            return null;
        }

        private void Resolve(Stmt stmt)
        {
            stmt.Accept(this);
        }

        private void Resolve(Expr expr)
        {
            expr.Accept(this);
        }

        private void ResolveFunction(Stmt.Function function, FunctionType type)
        {
            FunctionType enclosingFunction = currentFunction;
            currentFunction = type;

            BeginScope();
            foreach (Token param in function.parameters)
            {
                Declare(param);
                Define(param);
            }

            Resolve(function.body);
            EndScope();

            currentFunction = enclosingFunction;
        }

        private void BeginScope()
        {
            scopes.Push(new Dictionary<string, bool>());
        }

        private void EndScope()
        {
            scopes.Pop();
        }

        private void Declare(Token name)
        {
            if (scopes.Count == 0) return;

            var scope = scopes.Peek();
            if (scope.ContainsKey(name.lexeme))
            {
                Program.Error(name, "Already a variable with this name in this scope.");
            }

            scope[name.lexeme] = false;
        }

        private void Define(Token name)
        {
            if (scopes.Count == 0) return;

            var scope = scopes.Peek();
            scope[name.lexeme] = true;
        }

        private void ResolveLocal(Expr expr, Token name)
        {
            int numScopes = 0;
            foreach (var scope in scopes)
            {
                if (scope.ContainsKey(name.lexeme))
                {
                    interpreter.Resolve(expr, numScopes);
                    return;
                }

                ++numScopes;
            }
        }
    }
}
