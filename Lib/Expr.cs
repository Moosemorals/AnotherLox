using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace Lib {
    abstract public class Expr {

        public interface IVisitor<T> {
            public T VisitAssignExpr(Assign expr);
            public T VisitBinaryExpr(Binary expr);
            public T VisitGroupingExpr(Grouping expr);
            public T VisitLiteralExpr(Literal expr);
            public T VisitUnaryExpr(Unary expr);
        }

        public abstract T Accept<T>(IVisitor<T> visitor);

        public class Assign : Expr {
            public Assign(Token name, Expr value) {
                Name = name;
                Value = value;
            }

            public Token Name {get; }
            public Expr Value {get; }

            public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitAssignExpr(this);
        }

        public class Binary : Expr {
            public Binary(Expr left, Token op, Expr right) {
                Left = left;
                Op = op;
                Right = right;
            }

            public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitBinaryExpr(this);

            public Expr Left {get; }
            public Token Op {get; }
            public Expr Right {get; }           
        }

        public class Grouping : Expr {
            public Grouping(Expr expr) => Expr = expr;

            public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitGroupingExpr(this);

            public Expr Expr {get; }
        }

        public class Literal : Expr {
            public Literal(object? value) => Value = value;

            public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitLiteralExpr(this);

            public object? Value {get; }
        }

        public class Unary : Expr {
            public Unary(Token op, Expr right) {
                Op = op;
                Right = right;
            }

            public override T Accept<T>(IVisitor<T> visitor) => visitor.VisitUnaryExpr(this);

            public Token Op {get; }
            public Expr Right {get; }
        }

    }
}
