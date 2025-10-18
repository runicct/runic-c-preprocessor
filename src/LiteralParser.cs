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
        internal class LiteralParser
        {
            public static bool ParseNumerical(Token token, out bool isHex, out bool isBinary, out bool isOctal, out bool isUnsigned, out bool isLong, out bool isLongLong, out bool isFloat, out bool isDouble, out bool isLongDouble, out bool negative, out BigInteger integralPart, out BigInteger decimalPart, out BigInteger decimalPartFraction, out BigInteger exponantPart, out bool exponantNegative)
            {
                return ParseNumerical(token.Value, out isHex, out isBinary, out isOctal, out isUnsigned, out isLong, out isLongLong, out isFloat, out isDouble, out isLongDouble, out negative, out integralPart, out decimalPart, out decimalPartFraction, out exponantPart, out exponantNegative);
            }
            static bool ParseUnsignedLongLongSuffix(string token, int index, out bool isUnsigned, out bool isLong, out bool isLongLong)
            {

                isUnsigned = false;
                isLong = false;
                isLongLong = false;
                if (index >= token.Length) { return true; }

                // Check for U, UL, ULL, LU and LLU
                if (token[index] == 'u' || token[index] == 'U')
                {
                    isUnsigned = true;
                    index++;
                    if (index >= token.Length) { return true; }
                    if (token[index] == 'l' || token[index] == 'L')
                    {
                        isLong = true;
                        index++;
                        if (index >= token.Length) { return true; }
                        if (token[index] == 'l' || token[index] == 'L')
                        {
                            isLong = false;
                            isLongLong = true;
                            index++;
                            if (index >= token.Length) { return true; }
                            return false;
                        }
                    }
                }
                else if (token[index] == 'l' || token[index] == 'L')
                {
                    isLong = true;
                    index++;
                    if (index >= token.Length) { return true; }
                    if (token[index] == 'l' || token[index] == 'L')
                    {
                        isLong = false;
                        isLongLong = true;
                        index++;
                        if (index >= token.Length) { return true; }
                        if (token[index] == 'u' || token[index] == 'U')
                        {
                            isUnsigned = true;
                            index++;
                            if (index >= token.Length) { return true; }
                            return false;
                        }
                    }
                }
                return false;
            }
            static bool Contains(string Token, char Chr)
            {
                for (int n = 0; n < Token.Length; n++)
                {
                    if (Token[n] == Chr) { return true; }
                }
                return false;
            }
            public static bool ParseNumerical(string token, out bool isHex, out bool isBinary, out bool isOctal, out bool isUnsigned, out bool isLong, out bool isLongLong, out bool isFloat, out bool isDouble, out bool isLongDouble, out bool negative, out BigInteger integralPart, out BigInteger decimalPart, out BigInteger decimalPartFraction, out BigInteger exponantPart, out bool exponantNegative)
            {
                isUnsigned = false;
                isLong = false;
                isLongLong = false;
                isLongDouble = false;

                isHex = false;
                isBinary = false;
                isOctal = false;
                isFloat = false;
                isDouble = false;
                negative = false;
                integralPart = 0;
                decimalPart = 0;
                decimalPartFraction = 0;
                exponantPart = 0;
                exponantNegative = false;

                int index = 0;

                if (token[0] == '-') { negative = true; index++; }
                if (index >= token.Length) { return false; }
                if (token[index] == '0' && !Contains(token, '.'))
                {
                    index++;
                    if (index >= token.Length) { return true; }
                    if (token[index] == 'x' || token[index] == 'X')
                    {
                        index++;
                        isHex = true;
                        // Parse as hex
                        for (; index < token.Length; index++)
                        {
                            if (token[index] >= '0' && token[index] <= '9')
                            {
                                integralPart = (integralPart * 16UL) + (ulong)(token[index] - '0');
                            }
                            else if (token[index] >= 'A' && token[index] <= 'F')
                            {
                                integralPart = (integralPart * 16UL) + ((ulong)(token[index] - 'A') + 10UL);
                            }
                            else if (token[index] >= 'a' && token[index] <= 'f')
                            {
                                integralPart = (integralPart * 16UL) + ((ulong)(token[index] - 'a') + 10UL);
                            }
                            else { break; }
                        }
                        if (index >= token.Length) { return true; }
                        return ParseUnsignedLongLongSuffix(token, index, out isUnsigned, out isLong, out isLongLong);
                    }
                    if (token[index] == 'b' || token[index] == 'B')
                    {
                        index++;
                        isBinary = true;
                        // Parse as binary (Not in every standard)
                        for (; index < token.Length; index++)
                        {
                            if (token[index] < '0' || token[index] > '1') { break; }
                            integralPart = (integralPart * 2UL) + (ulong)(token[index] - '0');
                        }
                        if (index >= token.Length) { return true; }
                        return ParseUnsignedLongLongSuffix(token, index, out isUnsigned, out isLong, out isLongLong);
                    }
                    else
                    {
                        index++;
                        isOctal = true;
                        // Parse as octal
                        for (; index < token.Length; index++)
                        {
                            if (token[index] < '0' || token[index] > '7') { break; }
                            integralPart = (integralPart * 8UL) + (ulong)(token[index] - '0');
                        }
                        if (index >= token.Length) { return true; }
                        return ParseUnsignedLongLongSuffix(token, index, out isUnsigned, out isLong, out isLongLong);
                    }
                }
                else
                {
                    for (; index < token.Length; index++)
                    {
                        if (token[index] < '0' || token[index] > '9') { break; }
                        integralPart = (integralPart * 10UL) + (ulong)(token[index] - '0');
                    }
                    if (index >= token.Length) { return true; }
                    if (token[index] == '.')
                    {
                        decimalPartFraction = 1;
                        isDouble = true;
                        index++;
                        for (; index < token.Length; index++)
                        {
                            if (token[index] < '0' || token[index] > '9') { break; }
                            decimalPart = (ulong)(decimalPart * 10UL) + (ulong)(token[index] - '0');
                            decimalPartFraction *= 10;
                        }
                        if (index >= token.Length) { return true; }
                        if (token[index] == 'f' || token[index] == 'F')
                        {
                            isDouble = false;
                            isFloat = true;
                            index++;
                            if (index >= token.Length) { return true; }
                            return false;
                        }
                        if (token[index] == 'l' || token[index] == 'L')
                        {
                            isDouble = false;
                            isLongDouble = true;
                            index++;
                            if (index >= token.Length) { return true; }
                            return false;
                        }
                        // Parse as double (Or Float / Long Double)
                    }
                    else if (token[index] == 'f' || token[index] == 'F')
                    {
                        isFloat = true;
                        index++;
                        if (index >= token.Length) { return true; }
                        return false;
                    }
                    else
                    {
                        return ParseUnsignedLongLongSuffix(token, index, out isUnsigned, out isLong, out isLongLong);
                    }
                    return false;
                }
            }

        }
    }
}