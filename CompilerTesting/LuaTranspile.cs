using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BaseLanguage;

namespace LuaTranspile
{
    public static class Transpiler
    {
        public static string Indent(StringBuilder o, int indent)
        {
            for (int i = 0; i < indent; i++)
            {
                o.Append("    ");
            }
            return o.ToString();
        }

        public static void Function(StringBuilder o, Function function, int indent)
        {
            Indent(o, indent);
            o.Append("function ");
            o.Append(function.prototype.name);
            o.Append("(");
            for (int i = 0; i < function.prototype.argumentNames.Count; i++)
            {
                if (i != 0)
                {
                    o.Append(", ");
                }
                o.Append(function.prototype.argumentNames[i]);
            }
            o.AppendLine(")");

            foreach (var statement in function.body)
            {
                Statement(o, statement, indent + 1);
            }

            o.AppendLine("end");
            o.AppendLine();
        }

        public static void Statement(StringBuilder o, Statement statement, int indent)
        {
            if (statement is If) If(o, statement as If, indent);
            else if (statement is FunctionCall) FunctionCallStatement(o, statement as FunctionCall, indent);
            else if (statement is Declaration) Declaration(o, statement as Declaration, indent);
            else if (statement is Assignment) Assignment(o, statement as Assignment, indent);
            else if (statement is Return) Return(o, statement as Return, indent);
            else throw new NotImplementedException("Statement type not implemented");
        }

        public static void Assignment(StringBuilder o, Assignment assignment, int indent)
        {
            Indent(o, indent);
            switch (assignment.operation.type)
            {
                case TokenType.OperatorIncrement:
                    o.Append(assignment.identifier);
                    o.Append(" = ");
                    o.Append(assignment.identifier);
                    o.Append(" + 1");
                    break;
                case TokenType.OperatorDecrement:
                    o.Append(assignment.identifier);
                    o.Append(" = ");
                    o.Append(assignment.identifier);
                    o.Append(" - 1");
                    break;
                case TokenType.OperatorAddAssignment:
                    o.Append(assignment.identifier);
                    o.Append(" = ");
                    o.Append(assignment.identifier);
                    o.Append(" + ");
                    Expression(o, assignment.expression);
                    break;
                case TokenType.OperatorSubtractAssignment:
                    o.Append(assignment.identifier);
                    o.Append(" = ");
                    o.Append(assignment.identifier);
                    o.Append(" - ");
                    Expression(o, assignment.expression);
                    break;
                case TokenType.OperatorMultiplyAssignment:
                    o.Append(assignment.identifier);
                    o.Append(" = ");
                    o.Append(assignment.identifier);
                    o.Append(" * ");
                    Expression(o, assignment.expression);
                    break;
                case TokenType.OperatorDivideAssignment:
                    o.Append(assignment.identifier);
                    o.Append(" = ");
                    o.Append(assignment.identifier);
                    o.Append(" / ");
                    Expression(o, assignment.expression);
                    break;
                case TokenType.OperatorAssignment:
                    o.Append(assignment.identifier);
                    o.Append(" = ");
                    Expression(o, assignment.expression);
                    break;
                default:
                    throw new NotImplementedException("Assignment type not implemented: " + assignment.operation);
            }
            o.AppendLine(";");
        }

        public static void Declaration(StringBuilder o, Declaration declaration, int indent)
        {
            if (declaration.expression == null)
            {
                return;
            }

            Indent(o, indent);
            o.Append("local ");
            o.Append(declaration.identifier);
            o.Append(" = ");
            Expression(o, declaration.expression);
            if (declaration.expression != null)
            {
            }
            o.AppendLine(";");
        }

        public static void FunctionCallStatement(StringBuilder o, FunctionCall functionCall, int indent)
        {
            Indent(o, indent);
            FunctionCallExpression(o, functionCall);
            o.AppendLine(";");
        }

        public static void If(StringBuilder o, If ifStatement, int indent, bool isElse = false)
        {
            if (!isElse) Indent(o, indent);

            if (ifStatement.condition != null)
            {
                o.Append("if ");
                Expression(o, ifStatement.condition);
                o.Append(" then");
            }
            o.AppendLine();

            foreach (var statement in ifStatement.body)
            {
                Statement(o, statement, indent + 1);
            }

            if (ifStatement.elseStatement != null)
            {
                Indent(o, indent);
                o.Append("else");
                If(o, ifStatement.elseStatement, indent, true);
            }
            else
            {
                Indent(o, indent);
                o.AppendLine("end");
            }
        }

        public static void Return(StringBuilder o, Return returnStatement, int indent)
        {
            Indent(o, indent);
            o.Append("return ");
            Expression(o, returnStatement.expression);
            o.AppendLine(";");
        }

        public static void Expression(StringBuilder o, Expression expression)
        {
            if (expression is FunctionCall) FunctionCallExpression(o, expression as FunctionCall);
            else if (expression is Computation) Computation(o, expression as Computation);
            else if (expression is LiteralInt) LiteralInt(o, expression as LiteralInt);
            else if (expression is LiteralString) LiteralString(o, expression as LiteralString);
            else if (expression is Variable) Variable(o, expression as Variable);
            else throw new NotImplementedException("Expression type not implemented");
        }

        public static void FunctionCallExpression(StringBuilder o, FunctionCall functionCall)
        {
            o.Append(functionCall.functionName);
            o.Append(" (");
            foreach (var expression in functionCall.arguments)
            {
                Expression(o, expression);
            }
            o.Append(")");
        }

        public static void Computation(StringBuilder o, Computation computation)
        {
            Expression(o, computation.left);
            o.Append(" ");
            o.Append(computation.op.text);
            o.Append(" ");
            Expression(o, computation.right);
        }

        public static void LiteralInt(StringBuilder o, LiteralInt literalInt)
        {
            o.Append(literalInt.value);
        }

        public static void LiteralString(StringBuilder o, LiteralString literalString)
        {
            o.Append("\"");
            o.Append(literalString.value);
            o.Append("\"");
        }

        public static void Variable(StringBuilder o, Variable v)
        {
            o.Append(v.name);
        }
    }
}
