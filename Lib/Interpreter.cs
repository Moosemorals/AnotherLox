using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Lib {
    public class Interpreter : Expr.IVisitor<Expression> {
        private readonly IErrorReporter _log;

        public Interpreter(IErrorReporter log) => _log = log;


        public LambdaExpression? Interpret(Expr expr) {
            try {
                return Expression.Lambda(Evaluate(expr));
            } catch (RuntimeError e) {
                _log.Error("Line {0}: {1} at {2}", e.Token.Line, e.Message, e.Token.Lexeme);
                return null;
            }
        }

        public Expression VisitAssignExpr(Expr.Assign expr) => throw new NotImplementedException();

        public Expression VisitBinaryExpr(Expr.Binary expr) {
            Expression left = Evaluate(expr.Left);
            Expression right = Evaluate(expr.Right);

            switch (expr.Op.Type) {
            case TokenType.Minus:
                AreNumbers(expr.Op, left, right);
                return Expression.Subtract(left, right);
            case TokenType.Slash:
                AreNumbers(expr.Op, left, right);
                return Expression.Divide(left, right);
            case TokenType.Star:
                AreNumbers(expr.Op, left, right);
                return Expression.Multiply(left, right);
            case TokenType.Plus:
                if (left.Type == typeof(double) && right.Type == typeof(double)) {
                    return Expression.Add(left, right);
                } else if (left.Type == typeof(string) && right.Type == typeof(string)) {
                    return Concat(left, right);
                }
                throw new RuntimeError(expr.Op, $"Arguments to {expr.Op} must both be strings or numbers");
            case TokenType.Greater:
                AreNumbers(expr.Op, left, right);
                return Expression.GreaterThan(left, right);
            case TokenType.GreaterEqual:
                AreNumbers(expr.Op, left, right);
                return Expression.GreaterThanOrEqual(left, right);
            case TokenType.Less:
                AreNumbers(expr.Op, left, right);
                return Expression.LessThan(left, right);
            case TokenType.LessEqual:
                AreNumbers(expr.Op, left, right);
                return Expression.LessThanOrEqual(left, right);
            case TokenType.BangEqual:
                return Expression.Not(AreEqual(left, right));
            case TokenType.EqualEqual:
                return AreEqual(left, right);
            }

            throw new Exception("Unreachable code reached");
        }

        public Expression VisitGroupingExpr(Expr.Grouping expr) => Evaluate(expr.Expr);

        public Expression VisitLiteralExpr(Expr.Literal expr) => Expression.Constant(expr.Value);

        public Expression VisitUnaryExpr(Expr.Unary expr) {
            Expression right = Evaluate(expr.Right);

            switch (expr.Op.Type) {
            case TokenType.Bang: {
                return Expression.Not(IsTruthy(right));
            }
            case TokenType.Minus:
                IsNumber(expr.Op, right);
                return Expression.Negate(right);
            }

            throw new Exception("Unreachable code");
        }

        private Expression Evaluate(Expr expr) => expr.Accept(this);

        private Expression IsTruthy(Expression expr) {
            return expr.Type == typeof(bool)
                ? expr
                : Expression.Not(IsNull(expr));
        }

        private Expression IsNull(Expression expr) {
            return Expression.Equal(
                Expression.Convert(expr, typeof(object)),
                Expression.Constant(null)
             );
        }

        private void IsNumber(Token op, Expression expr) {
            if (expr.Type != typeof(double)) {
                throw new RuntimeError(op, $"Argument to {op.Lexeme} must be a number");
            }
        }

        private void AreNumbers(Token op, Expression left, Expression right) {
            if (!(left.Type == typeof(double) && right.Type == typeof(double))) {
                throw new RuntimeError(op, $"Arguments to {op.Lexeme} must be numbers");
            }
        }

        private Expression AreEqual(Expression left, Expression right) {
            if (left.Type != right.Type) {
                return Expression.Constant(false);
            } else {
                return Expression.Condition(
                    Expression.And(IsNull(left), IsNull(right)),
                    Expression.Constant(true),
                    Expression.Condition(
                       IsNull(left),
                       Expression.Constant(false),
                       Expression.Equal(left, right)
                    )
                );
            }

        }

        private Expression Concat(Expression left, Expression right) {
            Type s = typeof(string);

            return Expression.Call(s.GetMethod(nameof(string.Concat), new[] { s, s }), left, right);
        }
    }

    public class RuntimeError : Exception {
        public Token Token {get; private set; }

        public RuntimeError(Token token, string message) : base(message) => Token = token;
    }
}
