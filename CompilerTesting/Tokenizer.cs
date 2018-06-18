using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testing2
{
    public enum TokenType
    {
        KeywordInt,
        KeywordBool,
        KeywordReturn,
        KeywordIf,
        KeywordElse,

        SymbolParenthesesOpen,
        SymbolParenthesesClose,
        SymbolBraceOpen,
        SymbolBraceClose,
        SymbolSemicolon,
        SymbolComma,

        Identifier,
        LiteralInteger,             // Need to add more types of numbers
        LiteralString,                     // Need to add escape sequences

        OperatorAdd,
        OperatorSubtract,
        OperatorMultiply,
        OperatorDivide,
        OperatorAssignment,
        OperatorIncrement,
        OperatorDecrement,
        OperatorEqual,
        OperatorNotEqual,
        OperatorAddAssignment,      // Should we do this during tokenizing or parsing?
        OperatorSubtractAssignment,
        OperatorMultiplyAssignment,
        OperatorDivideAssignment,
        OperatorLessThan,
        OperatorGreaterThan,
        OperatorLessThanOrEqual,
        OperatorGreaterThanOrEqual,
        OperatorNot,
        OperatorModulus,
        OperatorRemainder,
        OperatorPower,
        OperatorOr,
        OperatorAnd,

        UnexpectedCharacter,

        EOF
    }

    public struct Token
    {
        private static Dictionary<TokenType, int> precedence = new Dictionary<TokenType, int>();

        static Token()
        {
            precedence[TokenType.OperatorIncrement] = 130;
            precedence[TokenType.OperatorDecrement] = 130;
            //precedence[TokenType.SymbolParenthesesOpen] = 130;
            //precedence[TokenType.SymbolParenthesesClose] = 130;

            // TODO: Unary operators

            precedence[TokenType.OperatorRemainder] = 110;
            precedence[TokenType.OperatorModulus] = 110;
            precedence[TokenType.OperatorMultiply] = 110;
            precedence[TokenType.OperatorDivide] = 110;

            precedence[TokenType.OperatorSubtract] = 100;
            precedence[TokenType.OperatorAdd] = 100;

            // TODO: Shift?

            precedence[TokenType.OperatorGreaterThanOrEqual] = 80;
            precedence[TokenType.OperatorGreaterThan] = 80;
            precedence[TokenType.OperatorLessThanOrEqual] = 80;
            precedence[TokenType.OperatorLessThan] = 80;

            precedence[TokenType.OperatorNotEqual] = 70;
            precedence[TokenType.OperatorEqual] = 70;

            // TODO: Bitwise AND

            // TODO: Bitwise XOR

            // TODO: Bitwise OR

            precedence[TokenType.OperatorAnd] = 30;

            precedence[TokenType.OperatorOr] = 20;

            // TODO: Conditional?

            precedence[TokenType.OperatorDivideAssignment] = 0;
            precedence[TokenType.OperatorMultiplyAssignment] = 0;
            precedence[TokenType.OperatorSubtractAssignment] = 0;
            precedence[TokenType.OperatorAddAssignment] = 0;
            precedence[TokenType.OperatorAssignment] = 0;

        }

        public static bool IsKeyword(Token t)
        {
            return
            t.type == TokenType.KeywordInt
            || t.type == TokenType.KeywordBool
            || t.type == TokenType.KeywordReturn
            || t.type == TokenType.KeywordIf
            || t.type == TokenType.KeywordElse;
        }

        public static bool IsPrimitive(Token t)
        {
            return t.type == TokenType.KeywordInt
                || t.type == TokenType.KeywordBool;
        }

        public static bool IsOperator(Token t)
        {
            return
            t.type == TokenType.OperatorAdd
            || t.type == TokenType.OperatorSubtract
            || t.type == TokenType.OperatorMultiply
            || t.type == TokenType.OperatorDivide
            || t.type == TokenType.OperatorAssignment
            || t.type == TokenType.OperatorIncrement
            || t.type == TokenType.OperatorDecrement
            || t.type == TokenType.OperatorEqual
            || t.type == TokenType.OperatorNotEqual
            || t.type == TokenType.OperatorAddAssignment
            || t.type == TokenType.OperatorSubtractAssignment
            || t.type == TokenType.OperatorMultiplyAssignment
            || t.type == TokenType.OperatorDivideAssignment
            || t.type == TokenType.OperatorLessThan
            || t.type == TokenType.OperatorGreaterThan
            || t.type == TokenType.OperatorLessThanOrEqual
            || t.type == TokenType.OperatorGreaterThanOrEqual
            || t.type == TokenType.OperatorNot
            || t.type == TokenType.OperatorModulus
            || t.type == TokenType.OperatorRemainder
            || t.type == TokenType.OperatorPower
            || t.type == TokenType.OperatorOr
            || t.type == TokenType.OperatorAnd;
        }

        public static bool IsAssignment(Token t)
        {
            return t.type == TokenType.OperatorAssignment
                || t.type == TokenType.OperatorAddAssignment
                || t.type == TokenType.OperatorSubtractAssignment
                || t.type == TokenType.OperatorMultiplyAssignment
                || t.type == TokenType.OperatorDivideAssignment
                || t.type == TokenType.OperatorIncrement
                || t.type == TokenType.OperatorDecrement;
        }

        public static bool IsSymbol(Token t)
        {
            return t.type == TokenType.SymbolParenthesesOpen
            || t.type == TokenType.SymbolParenthesesClose
            || t.type == TokenType.SymbolBraceOpen
            || t.type == TokenType.SymbolBraceClose
            || t.type == TokenType.SymbolSemicolon
            || t.type == TokenType.SymbolComma;
        }

        public readonly TokenType type;
        public readonly string text;

        public Token(TokenType type, string text)
        {
            this.type = type;
            this.text = text;
        }

        public int Precedence
        {
            get
            {
                if (precedence.ContainsKey(type))
                {
                    return precedence[type];
                }
                else
                {
                    return -1;
                }
            }
        }
    }

    public class Tokenizer
    {
        const string WHITESPACE = " \t\r\n";
        const string DIGITS = "0123456789";
        const string ID_STARTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        const string ID_CHARS = ID_STARTERS + DIGITS + "_";
        const string OPERATOR_CHARS = "+-*/=><!&|%";
        const string SYMBOLS = ";(){},";

        public List<Token> tokens = new List<Token>();
        public bool EOF;

        int index;
        char[] file;
        StringBuilder buffer = new StringBuilder();
        Token currentToken;

        public Tokenizer(string file)
        {
            this.file = file.ToCharArray();
        }

        public Token GetToken()
        {
            if (index == file.Length)
            {
                if (tokens.Count > 0 && EOF)
                {
                    return tokens.Last();
                }
                else
                {
                    Token t = new Token(TokenType.EOF, "");
                    tokens.Add(t);
                    EOF = true;
                    return t;
                }
            }

            EatWhitespace();
            char c = Peek();
            if (c == '/')
            {
                if (CheckForComment())
                {
                    // TODO: Do this with a loop instead of recursion
                    return GetToken();
                }
            }

            if (DIGITS.IndexOf(c) >= 0)
            {
                ReadNumber();
            }
            else if (ID_STARTERS.IndexOf(c) >= 0)
            {
                ReadIdentifier();
            }
            else if (SYMBOLS.IndexOf(c) >= 0)
            {
                ReadSymbol();
            }
            else if (OPERATOR_CHARS.IndexOf(c) >= 0)
            {
                ReadOperator();
            }
            else if (c == '"')
            {
                Next();
                ReadString();
            }
            else
            {
                Next();
                AddToken(TokenType.UnexpectedCharacter, c.ToString());
            }

            return currentToken;
        }

        public List<Token> GetTokens()
        {
            while (index < file.Length)
            {
                GetToken();
            }

            return tokens;
        }

        char Next()
        {
            Debug.Assert(index < file.Length, "Read at EOF");
            return file[index++];
        }

        char Peek()
        {
            Debug.Assert(index < file.Length, "Peek at EOF");
            return file[index];
        }

        void EatWhitespace()
        {
            BasicRead(WHITESPACE);
        }

        void AddToken(TokenType type, string text)
        {
            // TODO: More assertions
            currentToken = new Token(type, text);
            tokens.Add(currentToken);
        }

        bool CheckForComment()
        {
            char c = Next();
            Debug.Assert(c == '/', "Unexpected comment starting character: " + c);
            if (index == file.Length)
            {
                AddToken(TokenType.OperatorDivide, "/");
                return false;
            }

            c = Peek();
            if (c == '/')
            {
                while (Peek() != '\n' && index < file.Length)
                {
                    Next(); // Consume the rest of the line
                }
                Next();
                return true;
            }
            else if (c == '=')
            {
                Next();
                AddToken(TokenType.OperatorDivideAssignment, "/=");
            }
            else
            {
                AddToken(TokenType.OperatorDivide, "/");
            }
            return false;
        }

        void ReadNumber()
        {
            string number = BasicRead(DIGITS);
            // TODO: Change type of number depending on contents
            AddToken(TokenType.LiteralInteger, number);
        }

        void ReadIdentifier()
        {
            string identifier = BasicRead(ID_CHARS);
            switch (identifier)
            {
                case "int":
                    AddToken(TokenType.KeywordInt, identifier);
                    break;
                case "bool":
                    AddToken(TokenType.KeywordBool, identifier);
                    break;
                case "return":
                    AddToken(TokenType.KeywordReturn, identifier);
                    break;
                case "if":
                    AddToken(TokenType.KeywordIf, identifier);
                    break;
                case "else":
                    AddToken(TokenType.KeywordElse, identifier);
                    break;
                default:
                    AddToken(TokenType.Identifier, identifier);
                    break;
            }
        }

        void ReadSymbol()
        {
            char c = Peek();
            switch (c)
            {
                case ';':
                    AddToken(TokenType.SymbolSemicolon, ";");
                    Next();
                    break;
                case '(':
                    AddToken(TokenType.SymbolParenthesesOpen, "(");
                    Next();
                    break;
                case ')':
                    AddToken(TokenType.SymbolParenthesesClose, ")");
                    Next();
                    break;
                case '{':
                    AddToken(TokenType.SymbolBraceOpen, "{");
                    Next();
                    break;
                case '}':
                    AddToken(TokenType.SymbolBraceClose, "}");
                    Next();
                    break;
                case ',':
                    AddToken(TokenType.SymbolComma, ",");
                    Next();
                    break;
                default:
                    Debug.Assert(false, "Invalid symbol: " + c);
                    break;
            }
        }

        void ReadOperator()
        {
            char c = Peek();
            switch (c)
            {
                case '+':
                    Next();
                    c = Peek();
                    if (c == '+')
                    {
                        Next();
                        AddToken(TokenType.OperatorIncrement, "++");
                    }
                    else if (c == '=')
                    {
                        Next();
                        AddToken(TokenType.OperatorAddAssignment, "+=");
                    }
                    else
                    {
                        AddToken(TokenType.OperatorAdd, "+");
                    }
                    break;
                case '-':
                    Next();
                    c = Peek();
                    if (c == '-')
                    {
                        Next();
                        AddToken(TokenType.OperatorDecrement, "--");
                    }
                    else if (c == '=')
                    {
                        Next();
                        AddToken(TokenType.OperatorSubtractAssignment, "-=");
                    }
                    else
                    {
                        AddToken(TokenType.OperatorSubtract, "-");
                    }
                    break;
                case '*':
                    Next();
                    c = Peek();
                    if (c == '=')
                    {
                        Next();
                        AddToken(TokenType.OperatorMultiplyAssignment, "*=");
                    }
                    else if (c == '*')
                    {
                        Next();
                        AddToken(TokenType.OperatorPower, "**");
                    }
                    else
                    {
                        AddToken(TokenType.OperatorMultiply, "*");
                    }
                    break;
                //case '/':
                //    Next();
                //    c = Peek();
                //    if (c == '=')
                //    {
                //        Next();
                //        AddToken(TokenType.OperatorDivideAssignment, "/=");
                //    }
                //    else
                //    {
                //        AddToken(TokenType.OperatorDivide, "/");
                //    }
                //    break;
                case '=':
                    Next();
                    c = Peek();
                    if (c == '=')
                    {
                        Next();
                        AddToken(TokenType.OperatorEqual, "==");
                    }
                    else
                    {
                        AddToken(TokenType.OperatorAssignment, "=");
                    }
                    break;
                case '>':
                    Next();
                    c = Peek();
                    if (c == '=')
                    {
                        Next();
                        AddToken(TokenType.OperatorGreaterThanOrEqual, ">=");
                    }
                    else
                    {
                        AddToken(TokenType.OperatorGreaterThan, ">");
                    }
                    break;
                case '<':
                    Next();
                    c = Peek();
                    if (c == '=')
                    {
                        Next();
                        AddToken(TokenType.OperatorLessThanOrEqual, "<=");
                    }
                    else
                    {
                        AddToken(TokenType.OperatorLessThan, "<");
                    }
                    break;
                case '!':
                    Next();
                    c = Peek();
                    if (c == '=')
                    {
                        Next();
                        AddToken(TokenType.OperatorNotEqual, "!=");
                    }
                    else
                    {
                        AddToken(TokenType.OperatorNot, "!");
                    }
                    break;
                case '%':
                    Next();
                    AddToken(TokenType.OperatorRemainder, "%");
                    break;
                case '&':
                    Next();
                    if (c == '&')
                    {
                        Next();
                        AddToken(TokenType.OperatorAnd, "&&");
                    }
                    else
                    {
                        AddToken(TokenType.UnexpectedCharacter, c.ToString());
                    }
                    break;
                case '|':
                    Next();
                    if (c == '|')
                    {
                        Next();
                        AddToken(TokenType.OperatorOr, "||");
                    }
                    else
                    {
                        AddToken(TokenType.UnexpectedCharacter, c.ToString());
                    }
                    break;
            }
        }

        void ReadString()
        {
            // TODO: Allow escape sequences
            while (index < file.Length && Peek() != '"')
            {
                buffer.Append(Next());
            }
            Next();
            AddToken(TokenType.LiteralString, buffer.ToString());
            buffer.Clear();
        }

        string BasicRead(string matches)
        {
            while (index < file.Length && matches.IndexOf(Peek()) >= 0)
            {
                buffer.Append(Next());
            }
            string s = buffer.ToString();
            buffer.Clear();
            return s;
        }
    }
}
