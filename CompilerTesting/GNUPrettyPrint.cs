using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaseLanguage
{
    public static class GNUPrettyPrint
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
            o.AppendLine(function.prototype.returnType);

            Indent(o, indent);
            o.Append(function.prototype.name + " (");
            foreach (var argument in function.prototype.argumentNames)
            {
                o.Append(argument + " ");
            }
            o.AppendLine(")");

            Indent(o, indent);
            o.AppendLine("{");
            foreach (var statement in function.body)
            {
                Statement(o, statement, indent + 1);
            }
            o.AppendLine("}");
        }

        public static void Statement(StringBuilder o, Statement statement, int indent)
        {
            if (statement is If) If(o, statement as If, indent);
            else if (statement is FunctionCall) FunctionCallStatement(o, statement as FunctionCall, indent);
            else if (statement is Declaration) Declaration(o, statement as Declaration, indent);
            else if (statement is Assignment) Assignment(o, statement as Assignment, indent);
            else throw new NotImplementedException("Statement type not implemented");
        }

        public static void Assignment(StringBuilder o, Assignment assignment, int indent)
        {
            Indent(o, indent);
            if (assignment.operation.type == TokenType.OperatorIncrement || assignment.operation.type == TokenType.OperatorDecrement)
            {
                o.Append(assignment.identifier);
                o.Append(assignment.operation.text);
            }
            else
            {
                o.Append(assignment.identifier);
                o.Append(" ");
                o.Append(assignment.operation.text);
                o.Append(" ");
                Expression(o, assignment.expression);
            }
            o.AppendLine(";");
        }

        public static void Declaration(StringBuilder o, Declaration declaration, int indent)
        {
            Indent(o, indent);
            o.Append(declaration.type.text);
            o.Append(" ");
            o.Append(declaration.identifier);
            if (declaration.expression != null)
            {
                o.Append(" = ");
                Expression(o, declaration.expression);
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
                o.Append("if (");
                Expression(o, ifStatement.condition);
                o.Append(")");
            }
            o.AppendLine();

            indent += 1;

            Indent(o, indent);
            o.AppendLine("{");

            foreach (var statement in ifStatement.body)
            {
                Statement(o, statement, indent + 1);
            }

            Indent(o, indent);
            o.AppendLine("}");

            indent -= 1;

            if (ifStatement.elseStatement != null)
            {
                Indent(o, indent);
                o.Append("else ");
                If(o, ifStatement.elseStatement, indent, true);
            }
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
            o.Append("'");
            o.Append(literalString.value);
            o.Append("'");
        }

        public static void Variable(StringBuilder o, Variable v)
        {
            o.Append(v.name);
        }
    }
}
