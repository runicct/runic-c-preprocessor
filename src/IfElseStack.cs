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
        internal class PreprocessorIfElseStack
        {
            class Level
            {
                public bool disabled = false;
                public bool currentState = false;
                public bool validStateSeen = false;
                public Token enterToken = null;
                public Token elseToken = null;
                public Token elifToken = null;
                public Token exitToken = null;
            }
            Stack<Level> _levels = new Stack<Level>();
            public bool CurrentState { get { return _levels.Count == 0 || _levels.Peek().currentState; } }
            public bool Disabled { get { return _levels.Count != 0 && _levels.Peek().disabled; } }

            public void EnterIf(Token Token, bool Condition)
            {
                Level level = new Level();
                level.enterToken = Token;
                level.elseToken = null;
                level.currentState = Condition;
                level.validStateSeen = Condition;
                level.disabled = false;
                _levels.Push(level);
            }
            public void EnterDisabledIf(Token Token)
            {
                Level level = new Level();
                level.enterToken = Token;
                level.elseToken = null;
                level.currentState = false;
                level.validStateSeen = false;
                level.disabled = true;
                _levels.Push(level);
            }
            public void EnterDisabledElse(Token Token)
            {
                Level currentLevel = _levels.Peek();
                currentLevel.elseToken = Token;
            }
            public void EnterElse(Token Token)
            {
                Level currentLevel = _levels.Peek();
                currentLevel.elseToken = Token;
                if (!currentLevel.validStateSeen)
                {
                    currentLevel.currentState = !currentLevel.currentState;
                }
                else
                {
                    currentLevel.currentState = false;
                }
            }
            public void EnterElseIf(Token Token, bool Condition)
            {
                Level currentLevel = _levels.Peek();
                currentLevel.elifToken = Token;
                if (Condition && !currentLevel.validStateSeen)
                {
                    currentLevel.currentState = true;
                    currentLevel.validStateSeen = true;
                }
                else
                {
                    currentLevel.currentState = false;
                }
            }
            public void EnterDisabledElseIf(Token Token)
            {
                Level currentLevel = _levels.Peek();
                currentLevel.elifToken = Token;
            }
            public void ExitIf(Token Token)
            {
                Level level = _levels.Pop();
                level.exitToken = Token;
            }
        }
    }
}
