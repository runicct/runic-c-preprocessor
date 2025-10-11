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
using System.Collections.Generic;
using System.Numerics;

namespace Runic.C
{
    public partial class Preprocessor : ITokenStream
    {
        internal class Literal
        {
            public enum Special
            {
                None = 0x0,
                BoolTrue = 0x1, // Since C23
                BoolFalse = 0x2, // Since C23
            }
            Special _special = Special.None;
            public Special SpecialValue { get { return _special; } }
            bool _isBoolean;
            public bool IsBoolean { get { return _isBoolean; } }
            bool _isString;
            public bool IsString { get { return _isString; } }
            bool _isChar;
            public bool IsChar { get { return _isChar; } }
            bool _isHex;
            public bool IsHex { get { return _isHex; } }
            bool _isBinary;
            public bool IsBinary { get { return _isBinary; } }
            bool _isOctal;
            public bool IsOctal { get { return _isOctal; } }
            bool _isUnsigned;
            public bool IsUnsigned { get { return _isUnsigned; } }
            bool _isLong;
            public bool IsLong { get { return _isLong; } }
            bool _isLongLong;
            public bool IsLongLong { get { return _isLongLong; } }
            bool _isFloat;
            public bool IsFloat { get { return _isFloat; } }
            bool _isDouble;
            public bool IsDouble { get { return _isDouble; } }
            bool _isLongDouble;
            public bool IsLongDouble { get { return _isLongDouble; } }
            bool _negative;
            public bool Negative { get { return _negative; } }
            BigInteger _integralPart;
            public BigInteger IntegralPart { get { return _integralPart; } }
            BigInteger _decimalPart;
            public BigInteger DecimalPart { get { return _decimalPart; } }
            BigInteger _decimalPartFraction;
            public BigInteger DecimalPartFractionFraction { get { return _decimalPartFraction; } }
            BigInteger _exponantPart;
            public BigInteger ExponantPart { get { return _exponantPart; } }
            bool _exponantNegative;
            public bool ExponantNegative { get { return _exponantNegative; } }
            Token? _token;
            public Token? Token { get { return _token; } }
            public static int ToInt32(BigInteger bigInteger, out bool overflow)
            {
                overflow = false;
                uint result = 0;
                byte[] array = bigInteger.ToByteArray();
                if (array.Length > 0) { result += (uint)array[0]; }
                if (array.Length > 1) { result += (uint)array[1] * 0x100; }
                if (array.Length > 2) { result += (uint)array[2] * 0x10000; }
                if (array.Length > 3) { result += (uint)array[2] * 0x1000000; }
                if (array.Length > 4) { overflow = true; }
                unchecked
                {
                    if (result > (uint)int.MaxValue) { overflow = true; }
                    int sresult = (int)result;
                    return sresult;
                }
            }
            public static Literal? Parse(Token Token)
            {
                Literal literal = new Literal();
                literal._token = Token;
                bool result = LiteralParser.ParseNumerical(Token, out literal._isHex, out literal._isOctal, out literal._isOctal, out literal._isUnsigned, out literal._isLong, out literal._isLongLong, out literal._isFloat, out literal._isDouble, out literal._isLongDouble, out literal._negative, out literal._integralPart, out literal._decimalPart, out literal._decimalPartFraction, out literal._exponantPart, out literal._exponantNegative);
                if (result)
                {
                    return literal;
                }
                return null;
            }
        }
    }
}
