/*
 * MIT License
 * 
 * Copyright (c) 2025 Runic Compiler Toolkit Contributors
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using System;
using System.Numerics;

namespace Runic.C
{
    public partial class Preprocessor : ITokenStream
    {
        internal class PreprocessorExpressionEval
        {
            internal class Value
            {

            }
            internal class BooleanValue : Value
            {
                bool _value;
                public bool Value { get { return _value; } }
                public BooleanValue(bool Value) { _value = Value; }
            }
            internal class IntegerValue : Value
            {
                BigInteger _value;
                public BigInteger Value { get { return _value; } }
                public IntegerValue(BigInteger Value) { _value = Value; }
            }
            abstract class Expression
            {
                Token _Token;
                public Token Token { get { return _Token; } }
                public Expression(Token Token) { _Token = Token; }
                internal abstract Value Evaluate(Preprocessor Context);
            }
            class CstInteger : Expression
            {
                BigInteger _value;
                public CstInteger(Token Token, BigInteger Value) : base(Token)
                {
                    _value = Value;
                }
                public override string ToString() { return _value.ToString(); }
                internal override Value Evaluate(Preprocessor Context) { return new IntegerValue(_value); }
            }
            class CstBool : Expression
            {
                bool _value;
                public CstBool(Token Token, bool Value) : base(Token)
                {
                    _value = Value;
                }
                public override string ToString() { return _value.ToString(); }
                internal override Value Evaluate(Preprocessor Context) { return new BooleanValue(_value); }
            }

            class Defined : Expression
            {
                bool _value;
                Token _name;
                public Defined(Token Name, bool Value) : base(Name)
                {
                    _value = Value;
                    _name = Name;
                }
                public override string ToString() { return "defined(" + _name.Value.ToString() + ")"; }
                internal override Value Evaluate(Preprocessor Context) { return new BooleanValue(_value); }
            }

            class Add : Expression
            {
                Expression _left;
                Expression _right;
                public Add(Token Token, Expression left, Expression right) : base(Token)
                {
                    _left = left;
                    _right = right;
                }
                public override string ToString() { return "(" + _left.ToString() + " + " + _right.ToString() + ")"; }
                internal override Value Evaluate(Preprocessor Context)
                {
                    Value leftValue = _left.Evaluate(Context);
                    Value rightValue = _right.Evaluate(Context);

                    switch (leftValue)
                    {
                        case IntegerValue intLeftValue:
                            {
                                switch (rightValue)
                                {
                                    case IntegerValue intRightValue: return new IntegerValue(intLeftValue.Value + intRightValue.Value);
                                    case BooleanValue boolRightValue: return new IntegerValue(intLeftValue.Value + (boolRightValue.Value ? 1 : 0));
                                }
                                return intLeftValue;
                            }
                        case BooleanValue boolLeftValue:
                            {
                                switch (rightValue)
                                {
                                    case IntegerValue intRightValue: return new IntegerValue((boolLeftValue.Value ? 1 : 0) + intRightValue.Value);
                                    case BooleanValue boolRightValue: return new IntegerValue((boolLeftValue.Value ? 1 : 0) + (boolRightValue.Value ? 1 : 0));
                                }
                                return boolLeftValue;
                            }
                    }
                    return new BooleanValue(false);
                }
            }
            class Sub : Expression
            {
                Expression _left;
                Expression _right;
                public Sub(Token Token, Expression left, Expression right) : base(Token)
                {
                    _left = left;
                    _right = right;
                }
                public override string ToString() { return "(" + _left.ToString() + " - " + _right.ToString() + ")"; }
                internal override Value Evaluate(Preprocessor Context)
                {
                    return new BooleanValue(false);
                }
            }
            class Mul : Expression
            {
                Expression _left;
                Expression _right;
                public Mul(Token Token, Expression left, Expression right) : base(Token)
                {
                    _left = left;
                    _right = right;
                }
                public override string ToString() { return "(" + _left.ToString() + " * " + _right.ToString() + ")"; }
                internal override Value Evaluate(Preprocessor Context)
                {
                    return new BooleanValue(false);
                }
            }
            class Div : Expression
            {
                Expression _left;
                Expression _right;
                public Div(Token Token, Expression left, Expression right) : base(Token)
                {
                    _left = left;
                    _right = right;
                }
                public override string ToString() { return "(" + _left.ToString() + " / " + _right.ToString() + ")"; }
                internal override Value Evaluate(Preprocessor Context)
                {
                    return new BooleanValue(false);
                }
            }
            class Mod : Expression
            {
                Expression _left;
                Expression _right;
                public Mod(Token Token, Expression left, Expression right) : base(Token)
                {
                    _left = left;
                    _right = right;
                }
                public override string ToString() { return "(" + _left.ToString() + " % " + _right.ToString() + ")"; }
                internal override Value Evaluate(Preprocessor Context)
                {
                    return new BooleanValue(false);
                }
            }
            class Xor : Expression
            {
                Expression _left;
                Expression _right;
                public Xor(Token Token, Expression left, Expression right) : base(Token)
                {
                    _left = left;
                    _right = right;
                }
                public override string ToString() { return "(" + _left.ToString() + " ^ " + _right.ToString() + ")"; }
                internal override Value Evaluate(Preprocessor Context)
                {
                    return new BooleanValue(false);
                }
            }
            class Cmp : Expression
            {
                public enum Operator
                {
                    LowerThan,
                    GreaterThan,
                    LowerOrEqual,
                    GreaterOrEqual,
                    Equal,
                    NotEqual
                }
                Expression _left;
                Expression _right;
                Operator _operator;
                public Cmp(Token Token, Expression left, Operator op, Expression right) : base(Token)
                {
                    _left = left;
                    _right = right;
                    _operator = op;
                }
                public override string ToString()
                {
                    string op = "";
                    switch (_operator)
                    {
                        case Operator.Equal: op = " == "; break;
                        case Operator.NotEqual: op = " != "; break;
                        case Operator.GreaterOrEqual: op = " >= "; break;
                        case Operator.LowerOrEqual: op = " <= "; break;
                        case Operator.LowerThan: op = " < "; break;
                        case Operator.GreaterThan: op = " > "; break;
                    }
                    return "(" + _left.ToString() + op + _right.ToString() + ")";
                }
                internal override Value Evaluate(Preprocessor Context)
                {
                    Value leftValue = _left.Evaluate(Context);
                    Value rightValue = _right.Evaluate(Context);
                    switch (leftValue)
                    {
                        case IntegerValue intLeftValue:
                            {
                                switch (rightValue)
                                {
                                    case IntegerValue intRightValue:
                                        switch (_operator)
                                        {
                                            case Operator.Equal: return new BooleanValue(intLeftValue.Value == intRightValue.Value);
                                            case Operator.NotEqual: return new BooleanValue(intLeftValue.Value != intRightValue.Value);
                                            case Operator.GreaterOrEqual: return new BooleanValue(intLeftValue.Value >= intRightValue.Value);
                                            case Operator.LowerOrEqual: return new BooleanValue(intLeftValue.Value <= intRightValue.Value);
                                            case Operator.GreaterThan: return new BooleanValue(intLeftValue.Value > intRightValue.Value);
                                            case Operator.LowerThan: return new BooleanValue(intLeftValue.Value < intRightValue.Value);
                                            default: return new BooleanValue(false);
                                        }
                                    case BooleanValue boolRightValue:
                                        switch (_operator)
                                        {
                                            case Operator.Equal: return new BooleanValue((intLeftValue.Value != 0) == boolRightValue.Value);
                                            case Operator.NotEqual: return new BooleanValue((intLeftValue.Value != 0) != boolRightValue.Value);
                                            case Operator.GreaterOrEqual: return new BooleanValue(intLeftValue.Value >= (boolRightValue.Value ? 1 : 0));
                                            case Operator.LowerOrEqual: return new BooleanValue(intLeftValue.Value <= (boolRightValue.Value ? 1 : 0));
                                            case Operator.GreaterThan: return new BooleanValue(intLeftValue.Value > (boolRightValue.Value ? 1 : 0));
                                            case Operator.LowerThan: return new BooleanValue(intLeftValue.Value < (boolRightValue.Value ? 1 : 0));
                                            default: return new BooleanValue(false);
                                        }
                                }
                                return intLeftValue;
                            }
                        case BooleanValue boolLeftValue:
                            {
                                switch (rightValue)
                                {
                                    case IntegerValue intRightValue:
                                        switch (_operator)
                                        {
                                            case Operator.Equal: return new BooleanValue(boolLeftValue.Value == (intRightValue.Value != 0));
                                            case Operator.NotEqual: return new BooleanValue(boolLeftValue.Value != (intRightValue.Value != 0));
                                            case Operator.GreaterOrEqual: return new BooleanValue((boolLeftValue.Value ? 1 : 0) >= intRightValue.Value);
                                            case Operator.LowerOrEqual: return new BooleanValue((boolLeftValue.Value ? 1 : 0) <= intRightValue.Value);
                                            case Operator.GreaterThan: return new BooleanValue((boolLeftValue.Value ? 1 : 0) > intRightValue.Value);
                                            case Operator.LowerThan: return new BooleanValue((boolLeftValue.Value ? 1 : 0) < intRightValue.Value);
                                            default: return new BooleanValue(false);
                                        }
                                    case BooleanValue boolRightValue:
                                        switch (_operator)
                                        {
                                            case Operator.Equal: return new BooleanValue(boolLeftValue.Value == boolRightValue.Value);
                                            case Operator.NotEqual: return new BooleanValue(boolLeftValue.Value != boolRightValue.Value);
                                            case Operator.GreaterOrEqual: return new BooleanValue(boolLeftValue.Value);
                                            case Operator.LowerOrEqual: return new BooleanValue(boolRightValue.Value);
                                            case Operator.GreaterThan: return new BooleanValue(boolLeftValue.Value && !boolRightValue.Value);
                                            case Operator.LowerThan: return new BooleanValue(!boolLeftValue.Value && boolRightValue.Value);
                                            default: return new BooleanValue(false);
                                        }
                                }
                                return boolLeftValue;
                            }
                    }
                    return new BooleanValue(false);
                }
            }
            class Ternary : Expression
            {
                Expression _condition;
                Expression _onTrue;
                Expression _onFalse;
                public Ternary(Token Token, Expression Condition, Expression OnTrue, Expression OnFalse) : base(Token)
                {
                    _condition = Condition;
                    _onTrue = OnTrue;
                    _onFalse = OnFalse;
                }
                public override string ToString()
                {
                    return "(" + _condition.ToString() + ") ? (" + _onTrue.ToString() + ") : (" + _onFalse.ToString() + ")";
                }
                internal override Value Evaluate(Preprocessor Context)
                {
                    Value cond = _condition.Evaluate(Context);
                    switch (cond)
                    {
                        case BooleanValue boolValue: if (boolValue.Value) { return _onTrue.Evaluate(Context); } return _onFalse.Evaluate(Context);
                        case IntegerValue intValue: if (intValue.Value != 0) { return _onTrue.Evaluate(Context); } return _onFalse.Evaluate(Context);
                    }
                    return new BooleanValue(false);
                }
            }

            class Not : Expression
            {
                Expression _value;
                public Not(Token Token, Expression Value) : base(Token)
                {
                    _value = Value;
                }
                public override string ToString()
                {
                    return "!(" + _value.ToString() + ")";
                }
                internal override Value Evaluate(Preprocessor Context)
                {
                    Value cond = _value.Evaluate(Context);
                    switch (cond)
                    {
                        case BooleanValue boolValue: return new BooleanValue(!boolValue.Value);
                        case IntegerValue intValue: return new BooleanValue(intValue.Value != 0);
                    }
                    return new BooleanValue(false);
                }
            }

            class BooleanOr : Expression
            {
                Expression _left;
                Expression _right;
                public BooleanOr(Token Token, Expression left, Expression right) : base(Token)
                {
                    _left = left;
                    _right = right;
                }
                public override string ToString() { return "(" + _left.ToString() + " || " + _right.ToString() + ")"; }
                internal override Value Evaluate(Preprocessor Context)
                {
                    Value left = _left.Evaluate(Context);
                    Value right = _right.Evaluate(Context);

                    bool bLeft = false;
                    bool bRight = false;

                    switch (left)
                    {
                        case BooleanValue boolValue: bLeft = boolValue.Value; break;
                        case IntegerValue intValue: bLeft = (intValue.Value != 0); break;
                    }
                    switch (right)
                    {
                        case BooleanValue boolValue: bRight = boolValue.Value; break;
                        case IntegerValue intValue: bRight = (intValue.Value != 0); break;
                    }
                    return new BooleanValue(bLeft || bRight);
                }
            }

            class BooleanAnd : Expression
            {
                Expression _left;
                Expression _right;
                public BooleanAnd(Token Token, Expression left, Expression right) : base(Token)
                {
                    _left = left;
                    _right = right;
                }
                public override string ToString() { return "(" + _left.ToString() + " && " + _right.ToString() + ")"; }
                internal override Value Evaluate(Preprocessor Context)
                {
                    Value left = _left.Evaluate(Context);
                    Value right = _right.Evaluate(Context);

                    bool bLeft = false;
                    bool bRight = false;

                    switch (left)
                    {
                        case BooleanValue boolValue: bLeft = boolValue.Value; break;
                        case IntegerValue intValue: bLeft = (intValue.Value != 0); break;
                    }
                    switch (right)
                    {
                        case BooleanValue boolValue: bRight = boolValue.Value; break;
                        case IntegerValue intValue: bRight = (intValue.Value != 0); break;
                    }
                    return new BooleanValue(bLeft && bRight);
                }
            }

            Stack<Token> operatorStack = new Stack<Token>();
            static bool IsOperator(Token Token)
            {
                if (Token == null) { return false; }
                switch (Token.Value)
                {
                    case "+":
                    case "-":
                    case "*":
                    case "/":
                    case "%":
                    case "^":
                    case "<":
                    case ">":
                    case "<=":
                    case ">=":
                    case "==":
                    case "!=":
                    case "&&":
                    case "||":
                    case "!":
                    case "&":
                    case "|":
                    case "<<":
                    case ">>":
                    case "~":
                    case "++":
                    case "--":
                    case "?":
                    case ":":
                    case "(":
                    case ")":
                        return true;
                }
                return false;
            }
            static bool IsSpecialOperator(Token Token)
            {
                if (Token == null) { return false; }
                switch (Token.Value)
                {
                    case "defined":
                        return true;
                }
                return false;
            }
            static bool IsInvalidPreprocessorOperator(Token Token)
            {
                switch (Token.Value)
                {
                    case "=":
                    case "+=":
                    case "-=":
                    case "*=":
                    case "/=":
                    case "%=":
                    case "<<=":
                    case ">>=":
                    case "&=":
                    case "^=":
                    case "|=":
                        return true;
                }
                return false;
            }
            static Expression CreateExpression(Token Operator, Stack<Expression> Expression)
            {
                if (Operator.Value == "!")
                {
                    return new Not(Operator, Expression.Pop());
                }

                // Assume we have here a binary operation
                if (Expression.Count < 2)
                {
                    return null;
                }
                Expression right = Expression.Pop();
                Expression left = Expression.Pop();
                switch (Operator.Value)
                {
                    case "-": return (new Sub(Operator, left, right));
                    case "+": return (new Add(Operator, left, right));
                    case "*": return (new Mul(Operator, left, right));
                    case "/": return (new Div(Operator, left, right));
                    case "%": return (new Mod(Operator, left, right));
                    case "^": return (new Xor(Operator, left, right));
                    case "&&": return (new BooleanAnd(Operator, left, right));
                    case "||": return (new BooleanOr(Operator, left, right));
                    case "<": return (new Cmp(Operator, left, Cmp.Operator.LowerThan, right));
                    case ">": return (new Cmp(Operator, left, Cmp.Operator.GreaterThan, right));
                    case "==": return (new Cmp(Operator, left, Cmp.Operator.Equal, right));
                    case "!=": return (new Cmp(Operator, left, Cmp.Operator.NotEqual, right));
                    case ">=": return (new Cmp(Operator, left, Cmp.Operator.GreaterOrEqual, right));
                    case "<=": return (new Cmp(Operator, left, Cmp.Operator.LowerOrEqual, right));
                }

                throw new Exception("Internal error: Invalid operator: " + Operator.Value + "(" + Operator.File + ":l" + Operator.StartLine + ")");
            }
            static void ProcessOperator(Token Operator, Stack<Token> Operators, Stack<Expression> Expressions)
            {
                switch (Operator.Value)
                {
                    case ":":
                        Expression right = Expressions.Pop();
                        while (Operators.Count > 0 && (Operators.Peek().Value != "?"))
                        {
                            ProcessOperator(Operators.Pop(), Operators, Expressions);
                        }
                        if (Operators.Count > 0)
                        {
                            Operators.Pop();
                        }
                        else
                        {
                            // __TODO__ syntax error
                        }
                        Expression left = Expressions.Pop();
                        Expression cond = Expressions.Pop();
                        Expressions.Push(new Ternary(Operator, cond, left, right));
                        return;
                }
                Expression expression = CreateExpression(Operator, Expressions);
                if (expression == null)
                {
                    // _TODO__ Return an error here
                    return;
                }
                Expressions.Push(expression);
            }
            public static bool Evaluate(Preprocessor Context, Token[] Expression)
            {
                TokenQueue tokens = new TokenQueue(Expression);
                Stack<Expression> expressions = new Stack<Expression>();
                Stack<Token> operators = new Stack<Token>();
                Token? token = null;
                while (true)
                {
                    token = tokens.ReadNextToken();
                    if (token == null)
                    {
                        break;
                    }

                    if (IsInvalidPreprocessorOperator(token))
                    {
                        Context.Error_IfDirectiveInvalidExpression(token);
                        return false;
                    }



                    if (IsOperator(token))
                    {
                        if (token.Value == ":")
                        {
                            while (operators.Count > 0 && (operators.Peek().Value != "?"))
                            {
                                ProcessOperator(operators.Pop(), operators, expressions);
                            }
                        }
                        else if (token.Value == ")")
                        {
                            while (operators.Count > 0 && (operators.Peek().Value != "("))
                            {
                                ProcessOperator(operators.Pop(), operators, expressions);
                            }
                            operators.Pop();
                            continue;
                        }

                        int expressionPrecedence = OperatorPrecedence.GetPrecendence(token.Value);

                        while (operators.Count > 0 && OperatorPrecedence.GetPrecendence(operators.Peek().Value) < expressionPrecedence && (operators.Peek().Value != "("))
                        {
                            ProcessOperator(operators.Pop(), operators, expressions);
                        }

                        switch (token.Value)
                        {
                            case "?":
                                operators.Push(token);
                                break;
                            case ":":
                                operators.Push(token);
                                break;
                            default:
                                operators.Push(token);
                                break;
                        }
                        continue;
                    }

                    if (IsSpecialOperator(token))
                    {
                        Token specialOperator = token;
                        token = tokens.Skip(" ", "\\\n");
                        if (token == null || token.Value != "(")
                        {
                            if (!Preprocessor.IsValidMacroName(token.Value))
                            {
                                Context.Error_IfDirectiveInvalidFunction(specialOperator, token);
                                return false;
                            }
                            expressions.Push(new Defined(token, Context.ResolveMacro(token.Value) != null));
                            continue;
                        }
                        token = tokens.Skip(" ", "\\\n");
                        if (!Preprocessor.IsValidMacroName(token.Value))
                        {
                            Context.Error_IfDirectiveInvalidFunction(specialOperator, token);
                            return false;
                        }
                        expressions.Push(new Defined(token, Context.ResolveMacro(token.Value) != null));
                        token = tokens.Skip(" ", "\\\n");
                        if (token == null || token.Value != ")")
                        {
                            Context.Error_IfDirectiveInvalidFunction(specialOperator, token);
                            return false;
                        }
                        continue;
                    }

                    if (Preprocessor.IsValidMacroName(token.Value))
                    {
                        Preprocessor.Macro? macro = Context.ResolveMacro(token.Value);
                        if (macro != null)
                        {
                            Preprocessor.Macro.Regular macroRegular = macro as Preprocessor.Macro.Regular;
                            if (macroRegular != null)
                            {
                                tokens.FrontLoadTokens(macroRegular.Value);
                                continue;
                            }
                        }
                        else
                        {
                            Context.Warning_IfDirectiveUndefinedValue(token);
                            expressions.Push(new CstBool(token, false));
                        }
                    }
                    else
                    {
                        if (token.Value == " " || token.Value == "\\\n" || token.Value.StartsWith("/*"))
                        {
                            continue;
                        }
                        Literal? literal = Literal.Parse(token);
                        if (literal == null)
                        {
                            Context.Warning_IfDirectiveUndefinedValue(token);
                            expressions.Push(new CstBool(token, false));
                        }
                        else if (literal.IsFloat || literal.IsDouble || literal.IsLongDouble)
                        {
                            Context.Error_IfDirectiveFloatingPointInExpression(token);
                            return false;
                        }
                        else if (literal.IsString || literal.IsChar)
                        {
                            Context.Error_IfDirectiveStringInExpression(token);
                            return false;
                        }
                        else if (literal.IsBoolean)
                        {
                            expressions.Push(new CstBool(token, literal.IntegralPart != 0));
                        }
                        else
                        {
                            expressions.Push(new CstInteger(token, literal.IntegralPart));
                        }
                    }
                }

                while (operators.Count > 0)
                {
                    ProcessOperator(operators.Pop(), operators, expressions);
                }
                if (expressions.Count >= 1)
                {
                    Value result = expressions.Pop().Evaluate(Context);
                    if (expressions.Count > 1)
                    {
                        Expression expression = expressions.Pop();
                        Context.Error_IfDirectiveInvalidExpression(expression.Token);
                    }
                    switch (result)
                    {
                        case BooleanValue booleanValue: return booleanValue.Value;
                        case IntegerValue intValue: return intValue.Value != 0;
                    }
                }
                else
                {
                    // __TODO__ Report the error
                }
                return false;
            }
        }
    }
}