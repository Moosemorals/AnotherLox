using System;
using System.Collections.Generic;
using System.Text;

namespace Lib {
    public class Parser {
        private readonly IErrorReporter _log;
        private readonly IList<Token> _tokens;
        private int _current = 0;

        public Parser(IErrorReporter log, IList<Token> tokens) {
            _log = log;
            _tokens = tokens;
        }

        public Expr? Parse() {
            try {
                return Expression();
            } catch (ParseError) {
                return null;
            }
        }


        private Expr Expression() => Equality();

        private Expr Equality() {
            Expr expr = Comparison();

            while (Match(TokenType.BangEqual, TokenType.EqualEqual)) {
                Token op = Previous();
                Expr right = Comparison();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Comparison() {
            Expr expr = Term();

            while (Match(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual)) {
                Token op = Previous();
                Expr right = Term();
                expr = new Expr.Binary(expr, op, right);
            }
            return expr;
        }

        private Expr Term() {
            Expr expr = Factor();

            while (Match(TokenType.Minus, TokenType.Plus)) {
                Token op = Previous();
                Expr right = Factor();
                expr = new Expr.Binary(expr, op, right);
            }
            return expr;
        }

        private Expr Factor() {
            Expr expr = Unary();

            while (Match(TokenType.Slash, TokenType.Star)) {
                Token op = Previous();
                Expr right = Unary();
                expr = new Expr.Binary(expr, op, right);
            }
            return expr;
        }

        private Expr Unary() {
            if (Match(TokenType.Bang, TokenType.Minus)) {
                Token op = Previous();
                Expr right = Unary();
                return new Expr.Unary(op, right);
            }
            return Primary();
        }

        private Expr Primary() {
            if (Match(TokenType.False)) {
                return new Expr.Literal(false);
            }
            if (Match(TokenType.True)) {
                return new Expr.Literal(true);
            }
            if (Match(TokenType.Nil)) {
                return new Expr.Literal(null);
            }

            if (Match(TokenType.Number, TokenType.String)) {
                return new Expr.Literal(Previous().Literal);
            }

            if (Match(TokenType.LeftParen)) {
                Expr expr = Expression();
                Consume(TokenType.RightParen, "Expected ')' after expression.");
                return new Expr.Grouping(expr);
            }

            throw Error(Peek(), "Expected expression.");
        }

        private bool Match(params TokenType[] types) {
            foreach (TokenType tt in types) {
                if (Check(tt)) {
                    Advance();
                    return true;
                }
            }

            return false;
        }

        private Token Consume(TokenType type, string message) =>
            Check(type) ? Advance() : throw Error(Peek(), message);

        private bool Check(TokenType type) =>
            !IsAtEnd() && Peek().Type == type;

        private Token Advance() {
            if (!IsAtEnd()) {
                _current += 1;
            }
            return Previous();
        }

        private bool IsAtEnd() => Peek().Type == TokenType.EOF;

        private Token Peek() => _tokens[_current];

        private Token Previous() => _tokens[_current - 1];

        private ParseError Error(Token token, string message) {
            _log.Error("Line {0}: {1} at {2}", token.Line, message, token.Lexeme);
            return new ParseError();
        }

        private void Synchronize() {
            Advance();

            while (!IsAtEnd()) {
                if (Previous().Type == TokenType.Semicolon) {
                    return;
                }

                switch (Peek().Type) {
                case TokenType.Class:
                case TokenType.For:
                case TokenType.Fun:
                case TokenType.If:
                case TokenType.Print:
                case TokenType.Return:
                case TokenType.Var:
                case TokenType.While:
                    return;
                }

                Advance();
            }
        }

        private class ParseError : Exception { }
    }
}
