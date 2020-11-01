using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Lib {
    public class Scanner {
        private readonly static IDictionary<string, TokenType> keywords = new Dictionary<string, TokenType>() {
            { "and", TokenType.And },
            { "class", TokenType.Class },
            { "else", TokenType.Else },
            { "false", TokenType.False },
            { "for", TokenType.For },
            { "fun", TokenType.Fun },
            { "if", TokenType.If },
            { "nil", TokenType.Nil },
            { "or", TokenType.Or }, 
            { "print", TokenType.Print },
            { "return", TokenType.Return },
            { "super", TokenType.Super },
            { "this", TokenType.This },
            { "true", TokenType.True },
            { "var", TokenType.Var },
            { "while", TokenType.While }, 
        };

        private readonly string _source;
        private readonly List<Token> _tokens = new List<Token>();
        private readonly IErrorReporter _log;
        private int _start = 0;
        private int _current = 0;
        private int _line = 1;

        public Scanner(IErrorReporter log, string source) {
            _log = log;
            _source = source;
        }

        public List<Token> ScanTokens() {
            while (!IsAtEnd()) {
                _start = _current;
                ScanToken();
            }

            _tokens.Add(new Token(TokenType.EOF, "", null, _line));
            return _tokens;
        }

        private void ScanToken() {
            char c = Advance();
            switch (c) {
            case '(': AddToken(TokenType.LeftParen); break;
            case ')': AddToken(TokenType.RightParen); break;
            case '{': AddToken(TokenType.LeftBrace); break;
            case '}': AddToken(TokenType.RightBrace); break;
            case ',': AddToken(TokenType.Comma); break;
            case '.': AddToken(TokenType.Dot); break;
            case '-': AddToken(TokenType.Minus); break;
            case '+': AddToken(TokenType.Plus); break;
            case ';': AddToken(TokenType.Semicolon); break;
            case '*': AddToken(TokenType.Star); break;
            case '!': AddToken(Match('=') ? TokenType.BangEqual : TokenType.Bang); break;
            case '=': AddToken(Match('=') ? TokenType.EqualEqual : TokenType.Equal); break;
            case '<': AddToken(Match('=') ? TokenType.LessEqual : TokenType.Less); break;
            case '>': AddToken(Match('=') ? TokenType.GreaterEqual : TokenType.Greater); break;
            case '/':
                if (Match('/')) {
                    // Comments run to the end of the line
                    while (Peek() != '\n' && !IsAtEnd()) {
                        Advance();
                    }
                } else {
                    AddToken(TokenType.Slash);
                }
                break;
            case ' ':
            case '\r':
            case '\t':
                // Ignorable whitespace
                break;
            case '\n':
                _line += 1;
                break;
            case '"': String(); break;
            default:
                if (IsDigit(c)) {
                    Number();
                } else if (IsAlpha(c)) {
                    Identifier();
                } else {
                    _log.Error("Unexpected character {0} at line {1}.", c, _line);
                }
                break;
            }
        }

        private void String() {
            while (Peek() != '"' && !IsAtEnd()) {
                if (Peek() == '\n') {
                    _line += 1;
                }
                Advance();
            }
            if (IsAtEnd()) {
                _log.Error("Unterminated string at end of input.");
                return;
            }

            Advance(); // closing quote

            string value = _source.Substring(_start + 1, _current - _start - 2);
            AddToken(TokenType.String, value);

        }

        private void Number() {
            while (IsDigit(Peek())) {
                Advance();
            }

            if (Peek() == '.' && IsDigit(PeekNext())) {
                // consume '.'
                Advance();

                while (IsDigit(Peek())) {
                    Advance();
                }
            }

            AddToken(TokenType.Number, double.Parse(_source[_start.._current]));
        }

        private void Identifier() {
            while (IsAlphaNumeic(Peek())) {
                Advance();
            }

            string text = _source[_start.._current];
            if (keywords.ContainsKey(text)) {
                AddToken(keywords[text]);
            } else {
                AddToken(TokenType.Identifier);
            }
        }

        private bool IsAtEnd() => _current >= _source.Length;

        private char Advance() {
            _current += 1;
            return _source[_current - 1];
        }

        private bool Match(char expected) {
            if (IsAtEnd()) {
                return false;
            }
            if (_source[_current] != expected) {
                return false;
            }
            _current += 1;
            return true;
        }

        private char Peek() => IsAtEnd() 
            ? '\0'
            : _source[_current];

        private char PeekNext() => _current + 1 >= _source.Length 
            ? '\0' 
            : _source[_current + 1];

        private bool IsDigit(char c) => c >= '0' && c <= '9';

        private bool IsAlpha(char c) => 
            (c >= 'a' && c <= 'z')
            || (c >= 'A' && c <= 'Z')
            || c == '_';

        private bool IsAlphaNumeic(char c) => IsAlpha(c) || IsDigit(c);

        private void AddToken(TokenType type, object? literal = null) {
            string lexeme = _source[_start.._current ];
            _tokens.Add(new Token(type, lexeme, literal, _line));
        }
    }
}
