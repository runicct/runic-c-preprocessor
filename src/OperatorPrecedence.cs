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

namespace Runic.C
{
    public partial class Preprocessor : ITokenStream
    {
        public static class OperatorPrecedence
        {
            public static int GetPrecendence(string Operator)
            {
                switch (Operator)
                {
                    case "(":
                    case ")":
                    case "[":
                    case "]":
                    case "++":
                    case "--":
                    case ".":
                    case "->":
                        return 1;
                    case "!":
                    case "~":
                        return 2;
                    case "*":
                    case "/":
                    case "%":
                        return 3;
                    case "+":
                    case "-":
                        return 4;
                    case "<<":
                    case ">>":
                        return 5;
                    case "<=":
                    case ">=":
                    case "<":
                    case ">":
                        return 6;
                    case "==":
                    case "!=":
                        return 7;
                    case "&":
                        return 8;
                    case "^":
                        return 9;
                    case "|":
                        return 10;
                    case "&&":
                        return 11;
                    case "||":
                        return 12;
                    case "?":
                    case ":":
                        return 13;
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
                        return 14;
                    case ",":
                        return 15;
                }

                return 0;
            }
        }
    }
}