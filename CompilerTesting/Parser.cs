using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CompilerTesting;

namespace Testing2
{
    #region Expression/Statement Classes
    public interface Expression { }
    public interface Statement { }

    public class Variable : Expression
    {
        public readonly string name;
        public Variable(string name) { this.name = name; }
    }

    public class LiteralInt : Expression
    {
        public readonly int value;
        public LiteralInt(int value) { this.value = value; }
    }

    public class LiteralString : Expression
    {
        public readonly string value;
        public LiteralString(string value){ this.value = value; }
    }

    public class Computation : Expression
    {
        public readonly Token op;
        public readonly Expression left;
        public readonly Expression right;
        public Computation(Expression l, Token o, Expression r)
        {
            left = l;
            op = o;
            right = r;
        }
    }

    public class FunctionCall : Expression, Statement
    {
        public readonly string functionName;
        public readonly List<Expression> arguments;
        public FunctionCall(string functionName, List<Expression> arguments)
        {
            this.functionName = functionName;
            this.arguments = arguments;
        }
    }

    public class Declaration : Statement
    {
        public readonly Token type;
        public readonly string identifier;
        public readonly Expression expression;

        public Declaration(Token type, string identifier)
        {
            this.type = type;
            this.identifier = identifier;
        }

        public Declaration(Token type, string identifier, Expression expression)
            : this(type, identifier)
        {
            this.expression = expression;
        }
    }

    public class If : Statement
    {
        public readonly Expression condition;
        public readonly List<Statement> body;
        public readonly If elseStatement;

        public If(Expression condition, List<Statement> body)
        {
            this.condition = condition;
            this.body = body;
        }

        public If(Expression condition, List<Statement> body, If elseStatement)
            : this(condition, body)
        {
            this.elseStatement = elseStatement;
        }
    }

    public class Assignment : Statement
    {
        public readonly string identifier;
        public readonly Token assignment;
        public readonly Expression expression;

        public Assignment(string identifier, Token assignment, Expression expression)
        {
            this.identifier = identifier;
            this.assignment = assignment;
            this.expression = expression;
        }
    }
    #endregion Expression/Statement Objects


    public class FunctionPrototype
    {
        // TODO: Add types to function prototypes
        public readonly string name;
        public readonly string returnType;
        public readonly List<string> argumentNames;
        //public readonly List<Type> argumentTypes;
        public FunctionPrototype(string name, List<string> argumentNames)
        {
            this.name = name;
            this.argumentNames = argumentNames;
        }
    }

    public class Function
    {
        public readonly FunctionPrototype prototype;
        public readonly List<Statement> body;
        public Function(FunctionPrototype prototype, List<Statement> body)
        {
            this.prototype = prototype;
            this.body = body;
        }
    }

    public class Parser
    {
        int index;
        Tokenizer tokenizer;

        bool LoadAhead(int toIndex)
        {
            while (!tokenizer.EOF && tokenizer.tokens.Count <= toIndex)
            {
                tokenizer.GetToken();
            }
            return tokenizer.tokens.Count > toIndex;
        }

        Token Next()
        {
            LoadAhead(index);
            return tokenizer.tokens[index++];
        }

        Token Peek(int ahead = 0)
        {
            Debug.Assert(ahead >= 0, "Cannot peek behind");
            int peekIndex = index + ahead;
            LoadAhead(peekIndex);
            return tokenizer.tokens[peekIndex];
        }

        public Parser(Tokenizer tokenizer)
        {
            this.tokenizer = tokenizer;
        }

        void Error(Token t, string message)
        {
            Console.WriteLine("Error: " + message);
        }

        LiteralString ParseLiteralString()
        {
            var token = Next();
            Debug.Assert(token.type == TokenType.LiteralString, "Expected string literal");

            return new LiteralString(token.text);
        }

        LiteralInt ParseLiteralInteger()
        {
            var token = Next();
            Debug.Assert(token.type == TokenType.LiteralInteger, "Expected integer literal");

            int number = Int32.Parse(token.text);
            return new LiteralInt(number);
        }

        Expression ParseParenthesesExpression()
        {
            var token = Next();
            Debug.Assert(token.type == TokenType.SymbolParenthesesOpen, "Expected (");

            var expression = ParseExpression();
            if (expression == null) return null;
            if (Peek().type != TokenType.SymbolParenthesesClose)
            {
                Error(Peek(), "expected ')'");
                return null;
            }
            Next();
            return expression;
        }

        FunctionCall ParseFunctionCall(string identifier)
        {
            var parentheses = Next();
            Debug.Assert(parentheses.type == TokenType.SymbolParenthesesOpen);

            var arguments = new List<Expression>();
            while (Peek().type != TokenType.SymbolParenthesesClose)
            {
                var arg = ParseExpression();
                if (arg == null)
                {
                    // Expected expression
                    return null;
                }
                else
                {
                    arguments.Add(arg);
                    if (Peek().type == TokenType.SymbolParenthesesClose)
                    {
                        break;
                    }
                    else if (Peek().type == TokenType.SymbolComma)
                    {
                        Next();
                    }
                    else
                    {
                        Error(Peek(), "Expected ')' or ',' in argument list");
                    }
                }
            }
            Next(); // Eat close parentheses
            return new FunctionCall(identifier, arguments);
        }

        Expression ParseIdentifierExpression()
        {
            var token = Next();
            Debug.Assert(token.type == TokenType.Identifier, "Expected identifier");
            string identifier = token.text;

            token = Peek();
            if (token.type != TokenType.SymbolParenthesesOpen)
            {
                // variable ref
                return new Variable(identifier);
            }
            else
            {
                return ParseFunctionCall(identifier);
            }
        }

        Statement ParseStatement()
        {
            var token = Peek();
            if (token.type == TokenType.KeywordIf)
            {
                return ParseIfStatement();
            }
            else if (Token.IsAssignment(Peek(1)))
            {
                return ParseAssignment();
            }
            else if (Peek(1).type == TokenType.SymbolParenthesesOpen)
            {
                return ParseFunctionCallStatement();
            }
            else if (Token.IsPrimitive(token) || token.type == TokenType.Identifier)
            {
                return ParseDeclaration();
            }
            else
            {
                Error(token, "Unexpected token, expecting statement");
                return null;
            }
        }

        FunctionCall ParseFunctionCallStatement()
        {
            var identifier = Next();
            Debug.Assert(identifier.type == TokenType.Identifier, "Expected identifier");

            var functionCall = ParseFunctionCall(identifier.text);
            var token = Peek();
            if (token.type != TokenType.SymbolSemicolon)
            {
                Error(token, "Expecting ';'");
                return null;
            }
            Next();
            return functionCall;
        }

        Declaration ParseDeclaration()
        {
            var typeToken = Next();
            Debug.Assert(typeToken.type == TokenType.Identifier || Token.IsPrimitive(typeToken), "Expecting identifier or primitive for type of variable");

            var identifier = Next();
            Debug.Assert(identifier.type == TokenType.Identifier, "Expecting identifier for variable");

            var token = Peek();
            if (token.type == TokenType.OperatorAssignment)
            {
                Next();
                var expression = ParseExpression();
                if (expression == null)
                {
                    return null;
                }

                token = Peek();
                if (token.type != TokenType.SymbolSemicolon)
                {
                    Error(token, "Expecting ';'");
                    return null;
                }
                Next(); 

                return new Declaration(typeToken, identifier.text, expression);
            }
            else if (token.type == TokenType.SymbolSemicolon)
            {
                Next();
                return new Declaration(typeToken, identifier.text);
            }
            else
            {
                Error(token, "Unexpected token");
                return null;
            }
        }

        // TODO: Will probably need something like TryParseStatement() before this is possible
        //If ParseIfExpression()
        //{
        //    var token = Next();
        //    Debug.Assert(token.type == TokenType.KeywordIf, "Expected if");

        //    var expression = ParseExpression();
        //    if (expression == null)
        //    {
        //        return null;
        //    }

        //    token = Peek();
        //    if (token.type != TokenType.SymbolBraceOpen)
        //    {
        //        Error(token, "Expected '{' for if statement body");
        //        return null;
        //    }

        //    If lastIf;
        //    while (Peek().type != TokenType.SymbolBraceClose)
        //    {

        //    }
        //}

        If ParseIfStatement()
        {
            var token = Next();
            Debug.Assert(token.type == TokenType.KeywordIf, "Expected if");

            var condition = ParseExpression();
            if (condition == null)
            {
                return null;
            }

            token = Peek();
            if (token.type != TokenType.SymbolBraceOpen)
            {
                Error(token, "Expected '{' for if statement body");
                return null;
            }
            Next();

            var statements = new List<Statement>();
            while (Peek().type != TokenType.SymbolBraceClose)
            {
                Statement statement = ParseStatement();
                if (statement == null)
                {
                    return null;
                }
                statements.Add(statement);
            }
            Next();
            if (Peek().type == TokenType.KeywordElse)
            {
                Next();
                If elseStatement;
                if (Peek().type == TokenType.KeywordIf)
                {
                    // else if
                    elseStatement = ParseIfStatement();
                }
                else
                {
                    // else
                    if (Peek().type != TokenType.SymbolBraceOpen)
                    {
                        Error(Peek(), "Expecting '{' for else body");
                    }
                    Next();
                    var elseStatements = new List<Statement>();
                    while (Peek().type != TokenType.SymbolBraceClose)
                    {
                        var statement = ParseStatement();
                        if (statement == null)
                        {
                            return null;
                        }
                        elseStatements.Add(statement);
                    }
                    Next();
                    elseStatement = new If(null, elseStatements);
                }
                return new If(condition, statements, elseStatement);
            }
            else
            {
                // No else
                return new If(condition, statements);
            }
        }

        Expression ParsePrimaryExpression()
        {
            var token = Peek();
            switch (token.type)
            {
                case TokenType.Identifier:
                    return ParseIdentifierExpression();
                case TokenType.LiteralInteger:
                    return ParseLiteralInteger();
                case TokenType.SymbolParenthesesOpen:
                    return ParseParenthesesExpression();
                case TokenType.LiteralString:
                    return ParseLiteralString();

                // TODO: Handle unary operators
                default:
                    Error(token, "Unkown token when expecting an expression");
                    return null;
            }
        }

        Expression ParseExpression()
        {
            var primary = ParsePrimaryExpression();
            if (primary == null)
            {
                return null;
            }
            return ParseComputationRight(0, primary);
        }

        Expression ParseComputationRight(int precedence, Expression left)
        {
            while (true)
            {
                var token = Peek();
                int tokenPrecedence = token.Precedence;
                if (tokenPrecedence < precedence) return left;

                var firstToken = token;
                Next();
                var right = ParsePrimaryExpression();
                if (right == null)
                {
                    return null;
                }

                token = Peek();
                int nextTokenPrecedence = token.Precedence;
                if (tokenPrecedence < nextTokenPrecedence)
                {
                    right = ParseComputationRight(tokenPrecedence + 1, right);
                    if (right == null)
                    {
                        return null;
                    }
                }

                left = new Computation(left, firstToken, right);
            }
        }

        Assignment ParseAssignment()
        {
            var identifier = Next();
            Debug.Assert(identifier.type == TokenType.Identifier, "Expected identifier for assignment");
            var assignment = Next();
            Debug.Assert(Token.IsAssignment(assignment), "Expected assignment token");
            Expression expression = null;
            if (assignment.type == TokenType.OperatorAssignment)
            {
                expression = ParseExpression();
                if (expression == null)
                {
                    return null;
                }
            }
            var token = Peek();
            if (token.type != TokenType.SymbolSemicolon)
            {
                Error(token, "Expecting semicolon");
                return null;
            }
            Next();
            return new Assignment(identifier.text, assignment, expression);
        }

        FunctionPrototype ParseFunctionPrototype()
        {
            var token = Peek();
            string returnType;
            if (token.type == TokenType.Identifier || Token.IsPrimitive(token))
            {
                Next();
                returnType = token.text;
            }

            // TODO: Make this so that we can omit "void"
            token = Peek();
            if (token.type != TokenType.Identifier)
            {
                Error(token, "Expected name for function prototype");
                return null;
            }
            string name = token.text;

            Next();
            token = Peek();
            if (token.type != TokenType.SymbolParenthesesOpen)
            {
                Error(token, "Expected '(' for function prototype");
                return null;
            }
            Next();

            // TODO: Get types
            var arguments = new List<string>();
            while (Peek().type == TokenType.Identifier || Peek().type == TokenType.SymbolComma)
            {
                token = Next();
                if (token.type == TokenType.SymbolComma)
                {
                    continue;
                }
                arguments.Add(token.text);
            }

            token = Peek();
            if (token.type != TokenType.SymbolParenthesesClose)
            {
                Error(token, "Expected ')' for function prototype");
            }
            Next();

            return new FunctionPrototype(name, arguments);
        }

        Function ParseFunction()
        {
            var prototype = ParseFunctionPrototype();
            if (prototype == null)
            {
                return null;
            }
            var token = Peek();
            if (token.type != TokenType.SymbolBraceOpen)
            {
                Error(token, "Expected '{' for function body");
                return null;
            }
            Next();
            // Get all the expressions in the body
            var statements = new List<Statement>();
            while (Peek().type != TokenType.SymbolBraceClose)
            {
                var statement = ParseStatement();
                if (statement == null)
                {
                    return null;
                }
                statements.Add(statement);
            }
            token = Peek();
            if (token.type != TokenType.SymbolBraceClose)
            {
                Error(token, "Expected '}' to clost function body");
                return null;
            }
            Next();
            return new Function(prototype, statements);
        }

        public void Parse()
        {
            while (true)
            {
                var token = Peek();
                if (token.type == TokenType.EOF)
                {
                    return;
                }
                else
                {
                    var func = ParseFunction();
                    Printer.PrettyPrint(func, 0);
                }
            }
        }

        public void GenerateAST()
        {
            while (!tokenizer.EOF)
            {
                var token = Next();
                Console.WriteLine(token.text + " : " + token.type);
            }
        }
    }
}
