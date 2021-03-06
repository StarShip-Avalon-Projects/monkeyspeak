﻿#region Usings

using Monkeyspeak.Extensions;
using Monkeyspeak.Lexical;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

#endregion Usings

namespace Monkeyspeak
{
    /// <summary>
    /// Converts a reader containing a Monkeyspeak script into a
    /// </summary>
    public sealed class Lexer : AbstractLexer
    {
        private int lineNo = 1, columnNo, rawPos, currentChar;
        private char varDeclSym, stringBeginSym, stringEndSym, lineCommentSym;

        public event Action<MonkeyspeakException> Error;

        /// <summary>
        /// Initializes a new instance of the <see cref="Lexer"/> class.
        /// </summary>
        /// <param name="engine">The engine.</param>
        /// <param name="reader">The reader.</param>
        public Lexer(MonkeyspeakEngine engine, SStreamReader reader)
            : base(engine, reader)
        {
            varDeclSym = engine.Options.VariableDeclarationSymbol;
            stringBeginSym = engine.Options.StringBeginSymbol;
            stringEndSym = engine.Options.StringEndSymbol;
            lineCommentSym = engine.Options.LineCommentSymbol;
        }

        /// <summary>
        /// Reads the tokens from the reader. Used by the Parser.
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<Token> ReadToEnd()
        {
            int character = 0;
            char c = (char)character;
            Token token = default(Token), lastToken = default(Token);
            while (character != -1 || token.Type != TokenType.END_OF_FILE)
            {
                token = default(Token); // needed to clear Token state
                character = LookAhead(1);
                c = (char)character;
                if (character == -1)
                {
                    token = CreateToken(TokenType.END_OF_FILE);
                    goto END;
                }
                /*else if (c == Engine.Options.BlockCommentBeginSymbol[0])
                {
                    if (IsMatch(Engine.Options.BlockCommentBeginSymbol))
                    {
                        SkipBlockComment();
                        token = Token.None;
                        goto FINISH;
                    }
                }*/
                else if (c == lineCommentSym)
                {
                    SkipLineComment();
                    token = default(Token);
                }
                else if (c == stringBeginSym)
                {
                    token = MatchString();
                }
                else if (c == varDeclSym)
                {
                    token = MatchVariable();
                }
                else
                {
                    switch (c)
                    {
                        case '\r':
                        case '\n':
                            //token = CreateToken(TokenType.END_STATEMENT);
                            Next();
                            break;

                        case '.':
                        case ',':
                            //token = CreateToken(TokenType.END_STATEMENT);
                            Next();
                            break;

                        case '-':
                            if (char.IsDigit((char)LookAhead(2)))
                                token = MatchNumber();
                            else
                                token = CreateToken(TokenType.MINUS);
                            break;

                        case '%':
                            token = CreateToken(TokenType.MOD);
                            break;

                        case '0':
                            if (LookAhead(2) == 'x' || LookAhead(2) == 'X')
                                token = MatchNumber(); // support for hex numbers 0x3333333 (8 digit)
                            else token = MatchTrigger();
                            break;

                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                            token = MatchTrigger();
                            break;

                        default: Next(); break;
                    }
                }
                if (token.Type != TokenType.NONE)
                {
                    //Logger.Debug<Lexer>(token);
                    lastToken = token;
                    yield return token;
                }
            }
            END:;
        }

        public override void Reset()
        {
            if (reader.BaseStream.CanSeek)
            {
                reader.BaseStream.Seek(0, SeekOrigin.Begin);
            }
        }

        public bool IsMatch(string str)
        {
            for (int i = 0; i <= str.Length - 1; i++)
            {
                int c = LookAhead(1 + i);
                if (str[i] != c) return false;
            }
            return true;
        }

        public bool IsMatch(char c)
        {
            return currentChar == c;
        }

        public bool IsMatch(int c)
        {
            return currentChar == c;
        }

        public override bool CheckMatch(string str)
        {
            if (currentChar != str[0])
            {
                Error?.Invoke(new MonkeyspeakException($"Expected '{str}' but got '{((char)currentChar).EscapeForCSharp()}'", CurrentSourcePosition));
                return false;
            }
            var found = LookAheadToString(str.Length - 1);
            if (found != str)
            {
                Error?.Invoke(new MonkeyspeakException($"Expected '{str}' but got '{found}'", CurrentSourcePosition));
                return false;
            }
            else
            {
                Next(str.Length);
                return true;
            }
        }

        public override bool CheckMatch(char c)
        {
            if (currentChar != c)
            {
                Error?.Invoke(new MonkeyspeakException(String.Format("Expected '{0}' but got '{1}'", c.EscapeForCSharp(), ((char)currentChar).EscapeForCSharp()), CurrentSourcePosition));
                return false;
            }
            return true;
        }

        public override bool CheckMatch(int c)
        {
            int input = currentChar;
            if (input != c)
            {
                string inputChar = (input != -1) ? ((char)input).ToString(CultureInfo.InvariantCulture) : "END_OF_FILE";
                Error?.Invoke(new MonkeyspeakException($"Expected '{((char)c).EscapeForCSharp()}' but got '{inputChar}'", CurrentSourcePosition));
                return false;
            }
            return true;
        }

        public bool CheckMatch(char a, char b)
        {
            if (a != b)
            {
                Error?.Invoke(new MonkeyspeakException($"Expected '{b.EscapeForCSharp()}' but got '{a.EscapeForCSharp()}'", CurrentSourcePosition));
                return false;
            }
            return true;
        }

        public bool CheckMatch(int a, int b)
        {
            if (a != b)
            {
                Error?.Invoke(new MonkeyspeakException($"Expected '{((char)b).EscapeForCSharp()}' but got '{((char)a).EscapeForCSharp()}'", CurrentSourcePosition));
                return false;
            }
            return true;
        }

        public override bool CheckIsDigit(char c = '\0')
        {
            if (!char.IsDigit(c))
            {
                Error?.Invoke(new MonkeyspeakException($"Expected number but got '{c.EscapeForCSharp()}'", CurrentSourcePosition));
                return false;
            }
            return true;
        }

        public override bool CheckEOF(int c)
        {
            if (c == -1)
            {
                Error?.Invoke(new MonkeyspeakException("Unexpected end of file", CurrentSourcePosition));
                return false;
            }
            return true;
        }

        private Token CreateToken(TokenType type)
        {
            var sourcePos = CurrentSourcePosition;
            long startPos = reader.Position;
            int length = 1;
            Next();
            return new Token(type, startPos, length, sourcePos);
        }

        private Token CreateToken(TokenType type, string str)
        {
            var sourcePos = CurrentSourcePosition;
            long startPos = reader.Position;
            int length = str.Length;
            for (int i = 0; i <= str.Length - 1; i++) Next();
            return new Token(type, startPos, length, sourcePos);
        }

        public override char[] Read(long startPosInStream, int length)
        {
            if (!reader.BaseStream.CanSeek)
            {
                throw new NotSupportedException("Stream does not support forward reading");
            }
            if (!reader.BaseStream.CanRead)
            {
                throw new NotSupportedException("Stream cannot be read from");
            }
            long oldPos = reader.Position;
            reader.Position = startPosInStream;

            var buf = new char[length];
            reader.Read(buf, 0, length);

            reader.Position = oldPos;

            return buf;
        }

        /// <summary> Peeks ahead in the reader </summary> <param name="steps"></param> <returns>The
        /// character number of steps ahead or -1/returns>
        public override int LookAhead(int steps)
        {
            if (!reader.BaseStream.CanSeek)
            {
                throw new NotSupportedException("Stream does not support seeking");
            }
            if (!reader.BaseStream.CanRead)
            {
                throw new NotSupportedException("Stream cannot be read from");
            }
            int ahead = -1;
            if (steps > 0)
            {
                long oldPosition = reader.Position;
                // Subtract 1 from the steps so that the Peek method looks at the right value
                reader.Position = reader.Position + (steps - 1);

                ahead = reader.Peek();

                reader.Position = oldPosition;
            }
            else
            {
                ahead = reader.Peek();
            }
            return ahead;
        }

        /// <summary> Peeks ahead in the reader </summary> <param name="steps"></param> <returns>The
        /// character number of steps ahead or -1/returns>
        public string LookAheadToString(int steps)
        {
            if (!reader.BaseStream.CanSeek)
            {
                throw new NotSupportedException("Stream does not support seeking");
            }
            if (!reader.BaseStream.CanRead)
            {
                throw new NotSupportedException("Stream cannot be read from");
            }
            if (steps > 0)
            {
                long oldPosition = reader.Position;

                char[] charArray = new char[steps];
                for (int i = 0; i <= charArray.Length - 1; i++)
                {
                    reader.Position = reader.Position + i;
                    charArray[i] = (char)reader.Peek();
                }

                reader.Position = oldPosition;
                return new string(charArray);
            }
            else
            {
                return string.Empty;
            }
        }

        public override int LookBack(int steps)
        {
            if (!reader.BaseStream.CanSeek)
            {
                throw new NotSupportedException("Stream does not support seeking");
            }
            if (!reader.BaseStream.CanRead)
            {
                throw new NotSupportedException("Stream cannot be read from");
            }
            int aback = -1;
            long oldPosition = reader.Position;
            // Subtract 1 from the steps so that the Peek method looks at the right value
            if (reader.Position - (steps + 1) > 0)
                reader.Position -= (steps + 1);
            else reader.Position = 0;
            aback = reader.Peek();

            reader.Position = oldPosition;
            return aback;
        }

        public override int Next(int steps = 1)
        {
            if (!reader.BaseStream.CanRead)
            {
                throw new NotSupportedException("Stream cannot be read from");
            }
            int before = LookBack(1);
            for (int i = 0; i <= steps - 1; i++)
            {
                int c = reader.Read();
                if (c != -1)
                {
                    if (c == '\n')
                    {
                        lineNo++;
                        columnNo = 0;
                    }
                    else
                    {
                        columnNo++;
                    }
                    rawPos++;
                }
                currentChar = c;
            }
            return currentChar;
        }

        private Token MatchNumber()
        {
            bool @decimal = false, hex = false;
            var sourcePos = CurrentSourcePosition;
            long startPos = reader.Position;
            Next();
            int length = 0;
            char c = (char)currentChar;
            if (c == '-')
            {
                Next();
                length++;
                c = (char)currentChar;
            }

            Action<char> skipCommas = (char _c) =>
            {
                if (_c == ',')
                {
                    Next(); length++; c =
                    (char)currentChar;
                }
            };

            while (char.IsDigit(c))
            {
                if (!CheckEOF(currentChar)) return Token.None;
                Next();
                length++;
                c = (char)currentChar;

                // hex numbers
                if ((c == 'x' || c == 'X'))
                {
                    if (LookBack(1) == '0')
                    {
                        Next();
                        length++;
                        c = (char)currentChar;
                        hex = true;
                    }
                    else CheckIsDigit(c);
                }

                skipCommas(c);

                // decimal numbers
                if (c == '.')
                {
                    if (!hex)
                    {
                        if (!@decimal)
                        {
                            @decimal = true;
                            Next();
                            length++;
                            c = (char)currentChar;
                        }
                        else break; // we don't want duplicate decimal points
                    }
                    else if (hex && char.IsDigit((char)LookAhead(1)))
                        throw new MonkeyspeakException($"Unexpected {c.EscapeForCSharp()} in hexadecimal number", CurrentSourcePosition);
                }

                // support for exponent
                if (c == 'E' || c == 'e')
                {
                    if (!hex)
                    {
                        Next();
                        length++;
                        c = (char)currentChar;
                        if (c == '-' || c == '+')
                        {
                            Next();
                            length++;
                            c = (char)currentChar;
                            // now resume the loop because the rest are digits
                        }
                    }
                    else throw new MonkeyspeakException($"Unexpected {c.EscapeForCSharp()} in hexadecimal number", CurrentSourcePosition);
                }
            }
            return new Token(TokenType.NUMBER, startPos, length, sourcePos);
        }

        private Token MatchString()
        {
            Next();
            if (!CheckMatch(stringBeginSym)) return Token.None;
            var stringLenLimit = Engine.Options.StringLengthLimit;
            var sourcePos = CurrentSourcePosition;
            long startPos = reader.Position;
            int length = 0;
            while (true)
            {
                if (!CheckEOF(currentChar)) return Token.None;
                if (length >= stringLenLimit)
                {
                    Error?.Invoke(new MonkeyspeakException($"String exceeded limit or was not terminated with a '{stringEndSym}'", sourcePos));
                    return Token.None;
                }
                Next();
                length++;
                if (currentChar == stringEndSym)
                {
                    length--;
                    break;
                }
            }
            if (!CheckMatch(stringEndSym)) return Token.None;
            return new Token(TokenType.STRING_LITERAL, startPos, length, sourcePos);
        }

        private Token MatchTrigger()
        {
            if (LookAhead(2) != ':') // is trigger
            {
                return MatchNumber(); // is not trigger
            }
            var sourcePos = CurrentSourcePosition;
            long startPos = reader.Position;
            int length = 1;
            Next(); // trigger category
            if (!CheckIsDigit((char)currentChar)) return Token.None;
            Next(); // seperator
            length++;
            if (!CheckMatch(':')) return Token.None;
            Next();

            char c = (char)currentChar;
            if (!CheckIsDigit(c)) return Token.None;
            while (CheckIsDigit(c))
            {
                if (!CheckEOF(currentChar)) return Token.None;
                Next();
                length++;
                c = (char)currentChar;
                if (!char.IsNumber(c))
                {
                    break;
                }
            }
            return new Token(TokenType.TRIGGER, startPos, length, sourcePos);
        }

        private Token MatchVariable()
        {
            long startPos = reader.Position;
            int length = 0;
            Next();
            length++;
            var sourcePos = CurrentSourcePosition;

            if (!CheckMatch(varDeclSym)) return Token.None;

            Next();
            length++;
            char c = (char)currentChar;
            while (IsValidChar(currentChar))
            {
                if (!CheckEOF(currentChar)) return Token.None;

                Next();
                length++;
                c = (char)currentChar;
            }
            length--;

            if (IsMatch('['))
            {
                Next();
                length++;
                while (IsValidChar(currentChar))
                {
                    if (!CheckEOF(currentChar)) return Token.None;

                    Next();
                    length++;
                }
                length++;
                return new Token(TokenType.TABLE, startPos, length, sourcePos);
            }

            if (IsMatch('.'))
            {
                Next();
                length++;
                if (IsValidChar(currentChar))
                {
                    while (IsValidChar(currentChar))
                    {
                        if (!CheckEOF(currentChar)) return Token.None;

                        Next();
                        length++;
                    }

                    return new Token(TokenType.OBJ_VAR, startPos, length, sourcePos);
                }
            }

            if (!CheckEOF((char)currentChar)) return Token.None;

            return new Token(TokenType.VARIABLE, startPos, length, CurrentSourcePosition);
        }

        private void SkipBlockComment()
        {
            var bcommentBegin = Engine.Options.BlockCommentBeginSymbol;
            var bcommentEnd = Engine.Options.BlockCommentEndSymbol;
            Next();
            if (!CheckMatch(bcommentBegin)) return;
            char c = (char)LookAhead(1);
            while (true)
            {
                if (!CheckEOF(c)) return;
                Next();
                c = (char)currentChar;
                if (LookAheadToString(1) == bcommentEnd) break;
            }
            if (!CheckEOF((char)currentChar)) return;
            CheckMatch(bcommentEnd);
        }

        private void SkipLineComment()
        {
            Next();
            if (!CheckMatch(lineCommentSym)) return;
            char c = (char)LookAhead(1);
            while (true)
            {
                if (currentChar == -1)
                    break;
                Next();
                c = (char)currentChar;
                if (c == '\n') break;
            }
            if (currentChar != -1)
                if (!CheckMatch('\n')) return;
        }

        private bool IsValidChar(int c)
        {
            return (c >= 'a' && c <= 'z')
                           || (c >= 'A' && c <= 'Z')
                           || (c >= '0' && c <= '9')
                           || c == '_' || c == '@'
                           || c == '$'
                           || c == '&';
        }

        public override SourcePosition CurrentSourcePosition => new SourcePosition(lineNo, columnNo, rawPos);

        public int CurrentCharacter { get => currentChar; set => currentChar = value; }
    }
}