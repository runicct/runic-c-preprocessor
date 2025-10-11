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
using System.Text;

namespace Runic.C
{
    public partial class Preprocessor : ITokenStream
    {
        /// <summary>
        /// Can be used to represent tokens that are ignored by the preprocessor. This is useful for internal types, custom keywords, etc.
        /// </summary>
        public abstract class IgnoredToken : Token
        {
            public IgnoredToken(int StartLine, int StartColumn, int EndLine, int EndColumn, string File, string Value) : base(StartLine, StartColumn, EndLine, EndColumn, File, Value)
            {
            }
        }
        public abstract class Macro
        {
            string _name;
            public string Name { get { return _name; } }
            Macro(string Name)
            {
                _name = Name;
            }

            public class Regular : Macro
            {
                Token[] _value;
                public Token[] Value { get { return _value; } }
                public Regular(string Name, Token[] Value) : base(Name)
                {
                    _value = Value;
                }
            }
            public class WithArguments : Macro
            {
                Dictionary<string, int> _arguments;
                public Dictionary<string, int> Arguments { get { return _arguments; } }
                Token[] _value;
                public Token[] Value { get { return _value; } }
                public WithArguments(string Name, Dictionary<string, int> Arguments, Token[] Value) : base(Name)
                {
                    _arguments = Arguments;
                    _value = Value;
                }
                string Stringify(Token Argument, bool AddQuote)
                {
                    StringBuilder builder = new StringBuilder();
                    if (AddQuote) { builder.Append("\""); }
                    for (int c = 0; c < Argument.Value.Length; c++)
                    {
                        switch (Argument.Value[c])
                        {
                            case '\\':
                                builder.Append("\\\\");
                                break;
                            case '"':
                                builder.Append("\\\"");
                                break;
                            default:
                                builder.Append(Argument.Value[c]);
                                break;
                        }
                    }
                    if (AddQuote) { builder.Append("\""); }
                    return builder.ToString();
                }
                string Stringify(Token[] Argument)
                {
                    StringBuilder builder = new StringBuilder();
                    builder.Append("\"");
                    bool previousSpace = false;
                    for (int n = 0; n < Argument.Length; n++)
                    {
                        if (Argument[n].Value == " " || Argument[n].Value == "\n")
                        {
                            if (!previousSpace)
                            {
                                builder.Append(" ");
                                previousSpace = true;
                            }
                        }
                        else
                        {
                            previousSpace = false;
                            builder.Append(Stringify(Argument[n], false));
                        }
                       
                    }
                    builder.Append("\"");
                    return builder.ToString();
                }
                static bool IsValidToken(Token? token)
                {
                    if (token == null) { return true; }
                    if (token.Value == null) { return true; }
                    if (token.Value.Length < 2) { return true; }

                    if (token.Value[0] == '"')
                    {
                        // TODO Check that it is a valid string
                        return true;
                    }

                    if (token.Value[0] == '\'')
                    {
                        // TODO Check that it is a valid char
                        return true;
                    }

                    for (int n = 0; n < token.Value.Length; n++)
                    {
                        switch (token.Value[n])
                        {
                            case '(':
                            case ')':
                            case '{':
                            case '}':
                            case '[':
                            case ']':
                            case ',':
                            case ':':
                            case ';':
                            case '?':
                            case ' ':
                            case '\t':
                            case '\n':
                            case '\r':
                                return false;
                            case '*':
                                return token.Value == "*=";
                            case '/':
                                return token.Value == "/=";
                            case '+':
                                return token.Value == "++" || token.Value == "+=";
                            case '-':
                                return token.Value == "--" || token.Value == "-=";
                            case '>':
                                return token.Value == ">>" || token.Value == ">=";
                            case '<':
                                return token.Value == "<<" || token.Value == "<=";
                            case '%':
                                return token.Value == "%=";
                            case '#':
                                return token.Value == "##";
                        }
                    }

                    return true;
                }
                public Token[] Expand(Preprocessor Context, Token[][] Arguments)
                {

                    if (Arguments.Length != _arguments.Count) { throw new ArgumentException("Invalid argument count"); }
                    List<Token> result = new List<Token>();
                    for (int n = 0; n < _value.Length; n++)
                    {
                        if (n < _value.Length - 1)
                        {
                            int lookAhead = n + 1;
                            for (; lookAhead < _value.Length && (_value[lookAhead].Value == " " || _value[lookAhead].Value == "\\\n"); lookAhead++) ;

                            if (lookAhead < _value.Length)
                            {
                                if (_value[lookAhead].Value == "##")
                                {
                                    Token a = _value[n];
                                    while (true)
                                    {
                                        n = lookAhead + 1;
                                        for (; n < _value.Length && (_value[n].Value == " " || _value[n].Value == "\\\n"); n++) ;
                                        if (n >= _value.Length)
                                        {
                                            // __TODO__ Forward a warning here of an incomplete concatenation
                                            return result.ToArray();
                                        }

                                        Token b = _value[n];

                                        bool bIsMacro = Preprocessor.IsValidMacroName(b.Value);
                                        Token[] aTokens;
                                        if (Preprocessor.IsValidMacroName(a.Value))
                                        {
                                            int argumentIndex = 0;
                                            if (_arguments.TryGetValue(a.Value, out argumentIndex)) { aTokens = Arguments[argumentIndex]; }
                                            else { aTokens = new Token[] { a }; }
                                        }
                                        else { aTokens = new Token[] { a }; }

                                        Token[] bTokens;
                                        if (Preprocessor.IsValidMacroName(b.Value))
                                        {
                                            int argumentIndex = 0;
                                            if (_arguments.TryGetValue(b.Value, out argumentIndex)) { bTokens = Arguments[argumentIndex]; }
                                            else { bTokens = new Token[] { b }; }
                                        }
                                        else { bTokens = new Token[] { b }; }

                                        if (aTokens.Length > 0)
                                        {
                                            for (int x = 0; x < aTokens.Length - 1; x++) { result.Add(aTokens[x]); }
                                            Token lastAToken = aTokens[aTokens.Length - 1];
                                            string bValue = "";
                                            if (bTokens.Length > 0) { bValue = bTokens[0].Value; }
                                            Token concatenatedResult =  Context.TokenFactory.CreateToken(lastAToken.StartLine, lastAToken.StartColumn, lastAToken.EndLine, lastAToken.EndColumn, lastAToken.File, lastAToken.Value + bValue);
                                            if (!IsValidToken(concatenatedResult))
                                            {
                                                Context.Error_InvalidTokenProducedByConcatenation(this, concatenatedResult);
                                                result.Add(lastAToken);
                                                if (bTokens.Length > 0) { result.Add(bTokens[0]); }
                                            }
                                            else
                                            {
                                                result.Add(concatenatedResult);
                                            }
                                        }
                                        else if (bTokens.Length > 0)
                                        {
                                            Token firstBToken = bTokens[0];
                                            result.Add(Context.TokenFactory.CreateToken(firstBToken.StartLine, firstBToken.StartColumn, firstBToken.EndLine, firstBToken.EndColumn, firstBToken.File, firstBToken.Value));
                                        }
                                        for (int x = 1; x < bTokens.Length; x++) { result.Add(bTokens[x]); }

                                        lookAhead = n + 1;
                                        for (; lookAhead < _value.Length && (_value[lookAhead].Value == " " || _value[lookAhead].Value == "\\\n"); lookAhead++) ;
                                        if (lookAhead < _value.Length && _value[lookAhead].Value != "##") { break; }
                                        a = result[result.Count - 1];
                                        result.RemoveAt(result.Count - 1);
                                    }
                                    continue;
                                }
                            }
                        }

                        if (_value[n].Value == "#")
                        {
                            n++;
                            for (; n < _value.Length && (_value[n].Value == " " || _value[n].Value == "\\\n"); n++) ;
                            if (n >= _value.Length)
                            {
                                // __TODO__ Forward a warning here of an incomplete tostring
                                return result.ToArray();
                            }

                            if (Preprocessor.IsValidMacroName(_value[n].Value))
                            {
                                int argumentIndex = 0;
                                if (_arguments.TryGetValue(_value[n].Value, out argumentIndex))
                                {
                                    Token[] argument = Arguments[argumentIndex];
                                    if (argument.Length > 0)
                                    {
                                        result.Add(Context.TokenFactory.CreateToken(argument[0].StartLine, argument[0].StartColumn, argument[0].EndLine, argument[0].EndColumn, argument[0].File, Stringify(argument)));
                                    }
                                }
                            }
                            else
                            {
                                result.Add(Context.TokenFactory.CreateToken(_value[n].StartLine, _value[n].StartColumn, _value[n].EndLine, _value[n].EndColumn, _value[n].File, Stringify(_value[n], true)));
                            }
                            continue;
                        }

                        if (Preprocessor.IsValidMacroName(_value[n].Value))
                        {
                            int argumentIndex = 0;
                            if (_arguments.TryGetValue(_value[n].Value, out argumentIndex))
                            {
                                // C Preprocessor rules forces us to expand the macro argument here
                                for (int x = 0; x < Arguments[argumentIndex].Length; x++)
                                {
                                    Token[] argumentTokens = Arguments[argumentIndex];
                                    Macro? macro = null;
                                    if (Context._Macros.TryGetValue(argumentTokens[x].Value, out macro) && macro != null)
                                    {
                                        Macro.Regular macroRegular = macro as Macro.Regular;
                                        if (macroRegular != null)
                                        {
                                            result.AddRange(macroRegular.Value);
                                            continue;
                                        }

                                        Macro.WithArguments macroWithArguments = macro as Macro.WithArguments;
                                        if (macroWithArguments != null)
                                        {
                                            result.Add(argumentTokens[x]);
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        result.Add(Arguments[argumentIndex][x]);
                                    }
                                }
                                continue;
                            }
                        }
                        result.Add(_value[n]);
                    }
                    return result.ToArray();
                }
            }
        }
        TokenQueue _tokenQueue;
        ITokenFactory _tokenFactory;
        internal ITokenFactory TokenFactory { get { return _tokenFactory; } }
        public Preprocessor(ITokenFactory tokenFactory, ITokenStream TokenStream)
        {
            _tokenQueue = new TokenQueue(TokenStream);
            _tokenFactory = tokenFactory;
        }
        bool _preprocessor = false;
        bool _newLine = true;
        public virtual Macro? ResolveMacro(string MacroName)
        {
            return null;
        }
        public virtual void UndefMacro(Macro macro)
        {
            return;
        }
        public virtual void Warning_IgnoredExtraArgument(Token Directive) { }
        public virtual void Warning_IncompletePreprocessorDirective(Token Token) { }
        public virtual void Warning_InvalidMacroDefinition(Token Directive, Token MacroName) { }
        public virtual void Warning_InvalidMacroName(Token Directive, Token MacroName) { }
        public virtual void Warning_MacroArgumentRedefinition(Token Directive, Token MacroName, string ArgumentName) { }
        public virtual void Warning_InvalidPreprocessorDirective(Token Directive) { }
        public virtual void Warning_IfDirectiveUndefinedValue(Token Directive) { }
        public virtual void Warning_MismatchedDirective(Token Directive) { }
        public virtual void Warning_MacroRedefinition(Token Directive, string MacroName, Macro OldMacro, Macro NewMacro) { }
        public virtual void Warning_ExtraToken(Token Directive, Token Extra) { }
        public virtual void Error_InvalidMacroCall(Macro Name, Token Token) { }
        public virtual void Error_IfDirectiveInvalidExpression(Token Token) { }
        public virtual void Error_IfDirectiveInvalidFunction(Token Function, Token Token) { }
        public virtual void Error_IfDirectiveFloatingPointInExpression(Token Token) { }
        public virtual void Error_IfDirectiveStringInExpression(Token Token) { }
        public virtual void Error_IncompleteMacroDefinition(Token Directive, Token MacroName) { }
        public virtual void Error_MissingMacroArgument(Macro Name, Token Token, int ArgumentIndex) { }
        public virtual void Error_ExtraMacroArgument(Macro Name, Token Token) { }
        public virtual void Error_InvalidIncludeDirective(Token Include, Token InvalidToken) { }
        public virtual void Error_IncludeDirectiveFileNotFound(Token Include, string File) { }
        public virtual void Error_UserDefinedError(Token Directive, string ErrorMessage) { }
        public virtual void Error_InvalidTokenProducedByConcatenation(Macro Macro, Token Token) { }
        public static bool IsValidMacroName(string Name)
        {
            if (Name == null) { return false; }
            if (Name.Length == 0) { return false; }
            if (char.IsDigit(Name[0])) { return false; }
            for (int n = 0; n < Name.Length; n++)
            {
                switch (Name[n])
                {
                    case '\\':
                    case '+':
                    case '-':
                    case '*':
                    case '/':
                    case '%':
                    case '!':
                    case '(':
                    case ')':
                    case '[':
                    case ']':
                    case '{':
                    case '}':
                    case ',':
                    case ':':
                    case '=':
                    case '>':
                    case '<':
                    case '?':
                    case '"':
                    case '\'':
                    case '|':
                    case '&':
                    case '#':
                    case '.': return false;

                    default:
                        if (char.IsWhiteSpace(Name[n])) { return false; }
                        break;
                }
            }
            return true;
        }
        public virtual Token[]? ResolveInclude(Token directive, Token file, bool systemHeadersFirst)
        {
            return null;
        }
        void processIncludeDirective(Token Directive, Token[] Tokens)
        {
            if (Tokens.Length == 0)
            {
                Warning_IncompletePreprocessorDirective(Directive);
                return;
            }
            for (int n = 0; n < Tokens.Length; n++)
            {
                if (Tokens[n].Value == "") { continue; }
                if (Tokens[n].Value.StartsWith("<") && Tokens[n].Value.EndsWith(">"))
                {
                    string fileName = Tokens[n].Value.Substring(1, Tokens[n].Value.Length - 2);
                    Token file = _tokenFactory.CreateToken(Tokens[n].StartLine, Tokens[n].StartColumn, Tokens[n].EndLine, Tokens[n].EndColumn, Tokens[n].File, fileName);
                    Token[]? tokens = ResolveInclude(Directive, file, true);
                    if (tokens == null) { Error_IncludeDirectiveFileNotFound(Directive, fileName); }
                    else { _tokenQueue.FrontLoadTokens(tokens); }
                }
                else if(Tokens[n].Value.StartsWith("\"") && Tokens[n].Value.EndsWith("\""))
                {
                    string fileName = Tokens[n].Value.Substring(1, Tokens[n].Value.Length - 2);
                    Token file = _tokenFactory.CreateToken(Tokens[n].StartLine, Tokens[n].StartColumn, Tokens[n].EndLine, Tokens[n].EndColumn, Tokens[n].File, fileName);
                    Token[]? tokens = ResolveInclude(Directive, file, false);
                    if (tokens == null) { Error_IncludeDirectiveFileNotFound(Directive, fileName); }
                    else { _tokenQueue.FrontLoadTokens(tokens); }
                }
                else
                {
                    Error_InvalidIncludeDirective(Directive, Tokens[n]);
                }
            }
        }
        void DefineMacro(Token directive, string name)
        {
            Macro.Regular newMacro = new Macro.Regular(name, new Token[0]);
            Macro? existingMacro = ResolveMacro(name);
            if (existingMacro != null)
            {
                Macro.Regular existingValueMacro = existingMacro as Macro.Regular;
                if (existingValueMacro == null ||
                    existingValueMacro.Value.Length != 0)
                {
                    Warning_MacroRedefinition(directive, name, existingMacro, newMacro);
                    if (_Macros.ContainsKey(name)) { _Macros[name] = newMacro; }
                    else { _Macros.Add(name, newMacro); }
                }
            }
            else
            {
                _Macros.Add(name, newMacro);
            }
            return;
        }

        Dictionary<string, Macro> _Macros = new Dictionary<string, Macro>();
        void processDefineDirective(Token Directive, Token[] Tokens)
        {
            if (Tokens.Length == 0)
            {
                Warning_IncompletePreprocessorDirective(Directive);
                return;
            }
            int n = 0;

            Token nameToken = null;
            for (; n < Tokens.Length; n++)
            {
                if (Tokens[n].Value != " " && Tokens[n].Value != "\\\n")
                {
                    nameToken = Tokens[n];
                    break;
                }
            }
            if (nameToken == null)
            {
                Warning_IncompletePreprocessorDirective(Directive);
                return;
            }

            string name = nameToken.Value;
            if (!IsValidMacroName(name))
            {
                Warning_InvalidMacroName(Directive, nameToken);
                return;
            }
            n++;

            bool hasSpaces = false;
            Token firstToken = null;
            for (; n < Tokens.Length; n++)
            {
                if (Tokens[n].Value != " " && Tokens[n].Value != "\\\n")
                {
                    firstToken = Tokens[n];
                    break;
                }
                hasSpaces = true;
            }

            if (firstToken == null)
            {
                DefineMacro(Directive, name);
                return;
            }

            if (firstToken.Value == "(" && !hasSpaces)
            {
                n++;
                int argumentIndex = 0;
                Dictionary<string, int> arguments = new Dictionary<string, int>();
                for (;; n++)
                {
                    if (n >= Tokens.Length)
                    {
                        Error_IncompleteMacroDefinition(Directive, nameToken);
                        return;
                    }
                    if (Tokens[n].Value == " " || Tokens[n].Value == "\\\n") { continue; }
                    string argumentName = Tokens[n].Value;
                    if (n >= Tokens.Length - 1)
                    {
                        Error_IncompleteMacroDefinition(Directive, nameToken);
                        return;
                    }

                    if (arguments.ContainsKey(argumentName))
                    {
                        Warning_MacroArgumentRedefinition(Directive, nameToken, argumentName);
                        return;
                    }

                    arguments.Add(argumentName, argumentIndex);

                    n++;
                    while ((Tokens[n].Value == " " || Tokens[n].Value == "\\\n") && n < Tokens.Length) { n++; }

                    if (n >= Tokens.Length)
                    {
                        Error_IncompleteMacroDefinition(Directive, nameToken);
                        return;
                    }

                    if (Tokens[n].Value == ")") { n++; break; }
                    if (Tokens[n].Value != ",")
                    {
                        Warning_InvalidMacroDefinition(Directive, nameToken);
                        return;
                    }

                    argumentIndex++;
                }

                List<Token> value = new List<Token>();
                Token lastSpaceToken = null;
                for (int i = 0; n < Tokens.Length; n++, i++)
                {
                    if (Tokens[n].Value == " " || Tokens[n].Value == "\\\n")
                    {
                        lastSpaceToken = Tokens[n];
                    }
                    else
                    {
                        if (lastSpaceToken != null)
                        {
                            if (value.Count > 0) { value.Add(lastSpaceToken); }
                            lastSpaceToken = null;
                        }
                        value.Add(Tokens[n]);
                    }
                }
                Macro newMacro = new Macro.WithArguments(name, arguments, value.ToArray());
                Macro? existingMacro = ResolveMacro(name);
                if (existingMacro != null)
                {
                    Warning_MacroRedefinition(Directive, name, existingMacro, newMacro);
                    if (_Macros.ContainsKey(name)) { _Macros[name] = newMacro; }
                    else { _Macros.Add(name, newMacro); }
                }
                else
                {
                    _Macros.Add(name, newMacro);
                }
                return;
            }

            {
                List<Token> value = new List<Token>();
                Token lastSpaceToken = null;
                for (int x = 0; n < Tokens.Length; n++, x++)
                {
                    if (Tokens[n].Value == " " || Tokens[n].Value == "\\\n")
                    {
                        lastSpaceToken = Tokens[n];
                    }
                    else
                    {
                        if (lastSpaceToken != null && value.Count > 0) { value.Add(lastSpaceToken); }
                        value.Add(Tokens[n]);
                        lastSpaceToken = null;
                    }
                }

                Macro newMacro = new Macro.Regular(name, value.ToArray());

                Macro? existingMacro = ResolveMacro(name);
                if (existingMacro != null)
                {
                    Macro.Regular existingValueMacro = existingMacro as Macro.Regular;
                    if (existingValueMacro == null ||
                        existingValueMacro.Value.Length != value.Count)
                    {
                        Warning_MacroRedefinition(Directive, name, existingMacro, newMacro);
                        if (_Macros.ContainsKey(name)) { _Macros[name] = newMacro; }
                        else { _Macros.Add(name, newMacro); }
                        return;
                    }
                    else
                    {
                        for (int x = 0; x < value.Count; x++)
                        {
                            if (value[x].Value != existingValueMacro.Value[x].Value)
                            {
                                Warning_MacroRedefinition(Directive, name, existingMacro, newMacro);
                                if (_Macros.ContainsKey(name)) { _Macros[name] = newMacro; }
                                else { _Macros.Add(name, newMacro); }
                                return;
                            }
                        }
                        return;
                    }
                }
                else
                {
                    _Macros.Add(name, newMacro);
                }
            }
        }

        void processUndefDirective(Token Directive, Token[] Tokens)
        {
            if (Tokens.Length == 0)
            {
                Warning_IncompletePreprocessorDirective(Directive);
                return;
            }
            int n = 0;
            Token nameToken = null;
            for (; n < Tokens.Length; n++)
            {
                if (Tokens[n].Value != " " && Tokens[n].Value != "\\\n")
                {
                    nameToken = Tokens[n];
                    break;
                }
            }
            if (nameToken == null)
            {
                Warning_IncompletePreprocessorDirective(Directive);
                return;
            }


            if (_Macros.ContainsKey(nameToken.Value))
            {
                _Macros.Remove(nameToken.Value);
            }

            Macro? existingMacro = ResolveMacro(nameToken.Value);
            if (existingMacro != null)
            {
                UndefMacro(existingMacro);
            }
        }

        PreprocessorIfElseStack preprocessorIfElseStack = new PreprocessorIfElseStack();
        bool EvaluateExpression(Token[] Expression)
        {
            return PreprocessorExpressionEval.Evaluate(this, Expression);
        }

        void processIfDirective(Token Directive, Token[] Tokens, bool ElseIf)
        {
            if (preprocessorIfElseStack.Disabled)
            {
                if (ElseIf) { preprocessorIfElseStack.EnterDisabledElseIf(Directive); }
                else { preprocessorIfElseStack.EnterDisabledIf(Directive); }
                return;
            }

            if (!preprocessorIfElseStack.CurrentState)
            {
                if (!ElseIf) 
                {
                    preprocessorIfElseStack.EnterDisabledIf(Directive);
                    return;
                }
            }

            if (Tokens.Length == 0)
            {
                Warning_IncompletePreprocessorDirective(Directive);
                return;
            }
            int n = 0;
            for (; n < Tokens.Length; n++)
            {
                if (Tokens[n].Value != " " && Tokens[n].Value != "\\\n") { break; }
            }

            List<Token> expression = new List<Token>();
            Token lastSpaceToken = null;
            for (; n < Tokens.Length; n++)
            {
                if (Tokens[n].Value == " " || Tokens[n].Value == "\\\n")
                {
                    lastSpaceToken = Tokens[n];
                }
                else
                {
                    if (lastSpaceToken != null)
                    {
                        expression.Add(lastSpaceToken);
                        lastSpaceToken = null;
                    }
                    expression.Add(Tokens[n]);
                }
            }

            bool expressionResult = false;
            if (expression.Count > 0)
            {
                expressionResult = EvaluateExpression(expression.ToArray());
            }

            if (ElseIf) { preprocessorIfElseStack.EnterElseIf(Directive, expressionResult); }
            else { preprocessorIfElseStack.EnterIf(Directive, expressionResult); }
        }
        void processIfdefDirective(Token Directive, Token[] Tokens, bool Defined)
        {
            if (!preprocessorIfElseStack.CurrentState)
            {
                preprocessorIfElseStack.EnterDisabledIf(Directive);
                return;
            }

            if (Tokens.Length == 0)
            {
                Warning_IncompletePreprocessorDirective(Directive);
                return;
            }
            int n = 0;
            Token nameToken = null;
            for (; n < Tokens.Length; n++)
            {
                if (Tokens[n].Value != " " && Tokens[n].Value != "\\\n")
                {
                    nameToken = Tokens[n];
                    break;
                }
            }
            if (nameToken == null)
            {
                Warning_IncompletePreprocessorDirective(Directive);
                return;
            }
            n++;
            for (; n < Tokens.Length; n++)
            {
                if (Tokens[n].Value != " " && Tokens[n].Value != "\\\n") { break; }
            }
            if (n < Tokens.Length)
            {
                Warning_ExtraToken(Directive, Tokens[n]);
            }

            bool condition = (ResolveMacro(nameToken.Value) != null);
            if (!Defined) { condition = !condition; }
            preprocessorIfElseStack.EnterIf(Directive, condition);
        }
        void processElseDirective(Token directive, Token[] tokens)
        {
            if (preprocessorIfElseStack.Disabled)
            {
                preprocessorIfElseStack.EnterDisabledElse(directive);
                return;
            }

            preprocessorIfElseStack.EnterElse(directive);
        }
        void processEndifDirective(Token directive, Token[] tokens)
        {
            preprocessorIfElseStack.ExitIf(directive);
        }
        void processErrorDirective(Token directive, Token[] tokens)
        {
            StringBuilder message = new StringBuilder();

            for (int n = 0; n < tokens.Length; n++)
            {
                message.Append(tokens[n].Value);
            }

            Error_UserDefinedError(directive, message.ToString());
        }
        void ProcessDirective(Token directive, Token[] Arguments)
        {
            string directiveName = directive.Value.ToLowerInvariant();
            switch (directive.Value.ToLowerInvariant())
            {
                case "ifdef": processIfdefDirective(directive, Arguments, true); return;
                case "if": processIfDirective(directive, Arguments, false); return;
                case "ifndef": processIfdefDirective(directive, Arguments, false); return;
                case "else": processElseDirective(directive, Arguments); return;
                case "elif": processIfDirective(directive, Arguments, true); return;
                case "endif": processEndifDirective(directive, Arguments); return;
            }

            if (!preprocessorIfElseStack.CurrentState) { return; }

            switch (directive.Value.ToLowerInvariant())
            {
                case "include": if (preprocessorIfElseStack.CurrentState) { processIncludeDirective(directive, Arguments); } return;
                case "define": if (preprocessorIfElseStack.CurrentState) { processDefineDirective(directive, Arguments); } return;
                case "undef": if (preprocessorIfElseStack.CurrentState) { processUndefDirective(directive, Arguments); } return;
                case "error": processErrorDirective(directive, Arguments); return;
            }
        }
        public Token ReadNextToken()
        {
            restart:

            Token token = _tokenQueue.ReadNextToken();
            if (token == null) { return null; }
            if (token is IgnoredToken) { return token; }
            switch (token.Value)
            {
                case "#":
                    if (_newLine)
                    {
                        // Preprocessor Directive
                        List<Token> arguments = new List<Token>();
                        Token? directiveToken = _tokenQueue.ReadNextToken();
                        while (directiveToken != null && directiveToken.Value == " ")
                        {
                            directiveToken = _tokenQueue.ReadNextToken();
                        }
                        if (directiveToken == null) { return null; }

                        Token? argumentToken = _tokenQueue.ReadNextToken();
                        while (true)
                        {
                            if (argumentToken == null)
                            {
                                ProcessDirective(directiveToken, arguments.ToArray());
                                return null;
                            }

                            switch (argumentToken.Value)
                            {
                                case "\\\n": break;
                                case "\n":
                                       ProcessDirective(directiveToken, arguments.ToArray());
                                    _newLine = true;
                                    _preprocessor = false;
                                    goto restart;
                                default:
                                    arguments.Add(argumentToken);
                                    break;
                            }

                            argumentToken = _tokenQueue.ReadNextToken();
                        }

                    }
                    return token;
                case "\n":
                    _newLine = true;
                    _preprocessor = false;
                    return token;
                case " ":
                    return token;
                default:
                    if (token.Value.StartsWith("/*")) { goto restart; }
                    if (token.Value.StartsWith("//")) { goto restart; }
                    _newLine = false;
                    if (!preprocessorIfElseStack.CurrentState) { goto restart; }
                    if (IsValidMacroName(token.Value))
                    {
                        Macro? macro = ResolveMacro(token.Value);
                        if (macro != null)
                        {
                            Token macroToken = token;
                            Macro.Regular macroRegular = macro as Macro.Regular;
                            if (macroRegular != null)
                            {
                                Token[] clonedToken = new Token[macroRegular.Value.Length];
                                for (int n = 0; n < clonedToken.Length; n++)
                                {
                                    Token sourceToken = macroRegular.Value[n];
                                    clonedToken[n] = _tokenFactory.CreateToken(token.StartLine, token.StartColumn, token.EndLine, token.EndColumn, token.File, sourceToken.Value);
                                }
                                _tokenQueue.FrontLoadTokens(clonedToken);
                                goto restart;
                            }

                            Macro.WithArguments macroWithArguments = macro as Macro.WithArguments;
                            if (macroWithArguments != null)
                            {
                                int parenthesis = 0;
                                Token previousToken = token;
                                Token[][] arguments = new Token[macroWithArguments.Arguments.Count][];
                                token = _tokenQueue.Skip("\n", " ");
                                if (token == null)
                                {
                                    Error_InvalidMacroCall(macroWithArguments, previousToken);
                                    return null;
                                }

                                if (token.Value != "(")
                                {
                                    _tokenQueue.FrontLoadToken(token);
                                    // Assume this is not a macro
                                    return macroToken;
                                }
                                for (int n = 0; ; n++)
                                {
                                    if (n >= arguments.Length)
                                    {
                                        Error_ExtraMacroArgument(macroWithArguments, previousToken);

                                        // Keep going until we find the separator. This is a fatal syntax error
                                        while (true)
                                        {
                                            token = _tokenQueue.ReadNextToken();
                                            if (token == null)
                                            {
                                                return null;
                                            }

                                            if (token.Value == ")")
                                            {
                                                goto restart;
                                            }
                                        }
                                    }
                                    List<Token> argument = new List<Token>();

                                    Token? argumentFragment = _tokenQueue.ReadNextToken();
                                    if (argumentFragment == null)
                                    {
                                        Error_InvalidMacroCall(macroWithArguments, token);
                                        return null;
                                    }
                                    if (argumentFragment.Value == "(") { parenthesis++; }

                                    Token previousSpaceToken = null;
                                    while (argumentFragment.Value != "," || parenthesis > 0)
                                    {
                                        previousToken = token;
                                        if (argumentFragment.Value == " " || argumentFragment.Value == "\n")
                                        {
                                            previousSpaceToken = argumentFragment;
                                        }
                                        else
                                        {
                                            if (previousSpaceToken != null && argument.Count > 0)
                                            {
                                                argument.Add(previousSpaceToken);
                                            }
                                            previousSpaceToken = null;
                                            argument.Add(argumentFragment);
                                        }
                                        argumentFragment = _tokenQueue.ReadNextToken();
                                        if (argumentFragment == null)
                                        {
                                            Error_InvalidMacroCall(macroWithArguments, previousToken);
                                            return null;
                                        }

                                        previousToken = token;
                                        if (argumentFragment.Value == ")")
                                        {
                                            if (parenthesis > 0)
                                            {
                                                parenthesis--;
                                                continue;
                                            }
                                            if (n != arguments.Length - 1)
                                            {
                                                Error_MissingMacroArgument(macroWithArguments, token, n);
                                                // Here it seems that the user made a mistake by not passing enough arguments
                                                // This is a fatal error, but it is also a good point to restart parsing
                                                // after the ')' as it is likely going to yield good result
                                                goto restart;
                                            }
                                            arguments[n] = argument.ToArray();
                                            goto processMacro;
                                        }
                                        if (argumentFragment.Value == "(")
                                        {
                                            parenthesis++;
                                        }
                                    }
                                    arguments[n] = argument.ToArray();
                                }

                                processMacro:
                                {
                                    Token[] expandedMacro = macroWithArguments.Expand(this, arguments);
                                    Token[] clonedToken = new Token[expandedMacro.Length];
                                    for (int n = 0; n < clonedToken.Length; n++)
                                    {
                                        Token sourceToken = expandedMacro[n];
                                        clonedToken[n] = _tokenFactory.CreateToken(token.StartLine, token.StartColumn, token.EndLine, token.EndColumn, token.File, sourceToken.Value);
                                    }
                                    _tokenQueue.FrontLoadTokens(clonedToken);
                                }
                                goto restart;
                            }

                        }
                    }
                    return token;
            }
        }
    }
}
