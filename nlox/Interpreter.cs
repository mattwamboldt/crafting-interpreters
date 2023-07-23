using System;
using System.Collections.Generic;

namespace lox.net
{
    public class Interpreter : Expr.IVisitor<object>, Stmt.IVisitor<object>
    {
        public readonly Environment globals = new Environment();
        private readonly Dictionary<Expr, int> locals = new Dictionary<Expr, int>();
        private Environment environment;

        public Interpreter()
        {
            this.environment = globals;

            globals.Define("clock", new NativeMethod(0, (interpreter, arguments) => {
                return DateTimeOffset.Now.ToUnixTimeMilliseconds() / 1000.0;
            }));
        }

        public void Interpret(List<Stmt> statements)
        {
            try
            {
                foreach (Stmt statement in statements)
                {
                    Execute(statement);
                }
            }
            catch (RuntimeError error)
            {
                Program.RuntimeError(error);
            }
        }

        public object VisitBinaryExpr(Expr.Binary expr)
        {
            object left = Evaluate(expr.left);
            object right = Evaluate(expr.right);

            switch (expr.op.type)
            {
                case TokenType.GREATER:
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left > (double)right;
                case TokenType.GREATER_EQUAL:
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left >= (double)right;
                case TokenType.LESS:
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left < (double)right;
                case TokenType.LESS_EQUAL:
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left <= (double)right;
                case TokenType.MINUS:
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left - (double)right;
                case TokenType.PLUS:
                {
                    if (left is double && right is double)
                    {
                        return (double)left + (double)right;
                    }

                    // TODO: Challenge - If either op is a string, convert the other and concat
                    if (left is string && right is string)
                    {
                        return (string)left + (string)right;
                    }

                    throw new RuntimeError(expr.op, "Operands must be two numbers or two strings.");
                }
                case TokenType.SLASH:
                    // TODO: Challenge - Handle divide by zero with a runtimeerror
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left / (double)right;
                case TokenType.STAR:
                    CheckNumberOperands(expr.op, left, right);
                    return (double)left * (double)right;
                case TokenType.BANG_EQUAL:
                    return !IsEqual(left, right);
                case TokenType.EQUAL_EQUAL: 
                    return IsEqual(left, right);
            }

            // Unreachable
            return null;
        }

        public object VisitCallExpr(Expr.Call expr)
        {
            object callee = Evaluate(expr.callee);

            List<object> arguments = new List<object>();
            foreach (Expr argument in expr.arguments)
            {
                arguments.Add(Evaluate(argument));
            }

            if (callee is not ILoxCallable)
            {
                throw new RuntimeError(expr.paren, "Can only call functions and classes.");
            }

            ILoxCallable function = (ILoxCallable)callee;
            if (arguments.Count != function.Arity())
            {
                throw new RuntimeError(expr.paren, $"Expected {function.Arity()} arguments but got {arguments.Count}.");
            }

            return function.Call(this, arguments);
        }

        public object VisitGetExpr(Expr.Get expr)
        {
            object obj = Evaluate(expr.obj);
            if (obj is LoxInstance)
            {
                return ((LoxInstance)obj).Get(expr.name);
            }

            throw new RuntimeError(expr.name, "Only instances have properties.");
        }

        public object VisitGroupingExpr(Expr.Grouping expr)
        {
            return Evaluate(expr.expression);
        }

        public object VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.value;
        }

        public object VisitLogicalExpr(Expr.Logical expr)
        {
            object left = Evaluate(expr.left);

            if (expr.op.type == TokenType.OR)
            {
                if (IsTruthy(left)) return left;
            }
            else
            {
                if (!IsTruthy(left)) return left;
            }

            return Evaluate(expr.right);
        }

        public object VisitSetExpr(Expr.Set expr)
        {
            object obj = Evaluate(expr.obj);

            if (obj is not LoxInstance)
            {
                throw new RuntimeError(expr.name, "Only instances have fields.");
            }

            object value = Evaluate(expr.value);
            ((LoxInstance)obj).Set(expr.name, value);
            return value;
        }

        public object VisitThisExpr(Expr.This expr)
        {
            return LookUpVariable(expr.keyword, expr);
        }

        public object VisitUnaryExpr(Expr.Unary expr)
        {
            object right = Evaluate(expr.right);

            switch (expr.op.type)
            {
                case TokenType.BANG:
                    return !IsTruthy(right);
                case TokenType.MINUS:
                    CheckNumberOperand(expr.op, right);
                    return -(double)right;
            }

            return null;
        }

        public object VisitVariableExpr(Expr.Variable expr)
        {
            return LookUpVariable(expr.name, expr);
        }

        private object LookUpVariable(Token name, Expr expr)
        {
            if (locals.TryGetValue(expr, out int distance))
            {
                return environment.GetAt(distance, name.lexeme);
            }

            return globals.Get(name);
        }

        private void CheckNumberOperand(Token op, object operand)
        {
            if (operand is double) return;
            throw new RuntimeError(op, "Operand must be a number.");
        }

        private void CheckNumberOperands(Token op, object left, object right)
        {
            if (left is double && right is double) return;
            throw new RuntimeError(op, "Operands must be numbers.");
        }

        private bool IsTruthy(object obj)
        {
            if (obj == null) return false;
            if (obj is bool) return (bool)obj;
            return true;
        }

        private bool IsEqual(object a, object b)
        {
            if (a == null && b == null) return true;
            if (a == null) return false;

            return a.Equals(b);
        }

        private string Stringify(object obj)
        {
            if (obj == null) return "nil";

            if (obj is double) {
                string text = obj.ToString();
                if (text.EndsWith(".0"))
                {
                    text = text.Substring(0, text.Length - 2);
                }

                return text;
            }

            return obj.ToString();
        }

        private object Evaluate(Expr expr)
        {
            return expr.Accept(this);
        }

        private void Execute(Stmt statement)
        {
            statement.Accept(this);
        }

        internal void Resolve(Expr expr, int depth)
        {
            locals[expr] = depth;
        }

        public void ExecuteBlock(List<Stmt> statements, Environment environment)
        {
            Environment previous = this.environment;
            try
            {
                this.environment = environment;

                foreach (Stmt statement in statements)
                {
                    Execute(statement);
                }
            }
            finally
            {
                this.environment = previous;
            }
        }

        public object VisitBlockStmt(Stmt.Block stmt)
        {
            ExecuteBlock(stmt.statements, new Environment(environment));
            return null;
        }

        public object VisitClassStmt(Stmt.Class stmt)
        {
            environment.Define(stmt.name.lexeme, null);

            Dictionary<string, LoxFunction> methods = new Dictionary<string, LoxFunction>();
            foreach (Stmt.Function method in stmt.methods)
            {
                LoxFunction function = new LoxFunction(method, environment, method.name.lexeme.Equals("init"));
                methods[method.name.lexeme] = function;
            }

            LoxClass klass = new LoxClass(stmt.name.lexeme, methods);
            environment.Assign(stmt.name, klass);
            return null;
        }

        public object VisitExpressionStmt(Stmt.Expression stmt)
        {
            Evaluate(stmt.expression);
            return null;
        }

        public object VisitFunctionStmt(Stmt.Function stmt)
        {
            LoxFunction function = new LoxFunction(stmt, environment, false);
            environment.Define(stmt.name.lexeme, function);
            return null;
        }

        public object VisitIfStmt(Stmt.If stmt)
        {
            if (IsTruthy(Evaluate(stmt.condition)))
            {
                Execute(stmt.thenBranch);
            }
            else if (stmt.elseBranch != null)
            {
                Execute(stmt.elseBranch);
            }

            return null;
        }

        public object VisitPrintStmt(Stmt.Print stmt)
        {
            object value = Evaluate(stmt.expression);
            Console.WriteLine(Stringify(value));
            return null;
        }

        public object VisitReturnStmt(Stmt.Return stmt)
        {
            object value = null;
            if (stmt.value != null) value = Evaluate(stmt.value);

            throw new Return(value);
        }

        // TODO: Challenge 8.2 - Instead of initializing to null, throw runtime error for accessing unintialized variables (needs undefined marker like js)
        public object VisitVarStmt(Stmt.Var stmt)
        {
            object value = null;
            if (stmt.initializer != null)
            {
                value = Evaluate(stmt.initializer);
            }

            environment.Define(stmt.name.lexeme, value);
            return null;
        }

        public object VisitWhileStmt(Stmt.While stmt)
        {
            while (IsTruthy(Evaluate(stmt.condition)))
            {
                Execute(stmt.body);
            }

            return null;
        }

        public object VisitAssignExpr(Expr.Assign expr)
        {
            object value = Evaluate(expr.value);
            if (locals.TryGetValue(expr, out int distance))
            {
                environment.AssignAt(distance, expr.name, value);
            }
            else
            {
                globals.Assign(expr.name, value);
            }

            return value;
        }
    }
}
