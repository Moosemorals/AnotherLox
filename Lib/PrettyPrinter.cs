using System;
using System.Collections.Generic;
using System.Text;

namespace Lib {
    public class PrettyPrinter : Expr.IVisitor<string> {

        public string Print(Expr expr) => expr.Accept(this);

        public string VisitAssignExpr(Expr.Assign expr) => throw new NotImplementedException();
        public string VisitBinaryExpr(Expr.Binary expr) => $"{expr.Left.Accept(this)} {expr.Op.Lexeme} {expr.Right.Accept(this)}";
        public string VisitGroupingExpr(Expr.Grouping expr) => $"({expr.Expr.Accept(this)})";
        public string VisitLiteralExpr(Expr.Literal expr) => expr.Value == null 
            ? "nil" 
            : expr.Value.ToString() ?? "nil";

        public string VisitUnaryExpr(Expr.Unary expr) => $"{expr.Op.Lexeme} {expr.Right.Accept(this)}";
    }
}
