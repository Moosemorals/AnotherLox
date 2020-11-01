using System;
using System.Collections.Generic;
using System.Text;

namespace Lib {
   public class Token {
        public TokenType Type {get; }
        public string Lexeme {get; }
        public object? Literal {get; }
        public int Line {get; }
        public Token(TokenType type, string lexeme, object? literal, int line) {
            Type = type;
            Lexeme = lexeme;
            Literal = literal;
            Line = Line;
        }

        public override string ToString() => $"{Type} {Lexeme} {Literal ?? "-"}";

    }
}
