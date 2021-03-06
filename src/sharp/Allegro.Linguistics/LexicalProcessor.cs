﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;

namespace Allegro
{
    /// <summary>
    /// Provides functionality to extract lexical tokens from Allegro source.
    /// </summary>
    public class LexicalProcessor : IDisposable
    {
        #region "Constructor"
        /// <summary>
        /// Creates a new instance of <c>LexicalProcessor</c>.
        /// </summary>
        /// <param name="source">Source code from which the <c>LexicalProcessor</c> reads.</param>
        /// <exception cref="ArgumentNullException">Thrown when <c>source</c> is <c>null</c>.</exception>
        public LexicalProcessor(TextReader source)
        {
            if (source == null) throw new ArgumentNullException("source");
            _state = State.Initial;
            sourceBuffer = source;
        }
        #endregion

        #region "Methods"

        /// <summary>
        /// Reads the next token from stream.
        /// </summary>
        /// <returns><c>true</c> if the next token was read successfully; otherwise, <c>false</c>.</returns>
        public virtual bool Read()
        {
            if (_state == State.Initial) {
                _textBuffer = new StringBuilder(16);
                _indentLevels = new Stack<int>(8);
                _newline = true;
                _current = new LexicalToken();
                lastToken = new LexicalToken();
                Line = 1;
                Column = 1;

                _state = State.Open;
            }

            if (_state == State.Closed)
                throw new ObjectDisposedException("LexicalProcessor");

            while (_state != State.EndOfFile)
            {
                int c = sourceBuffer.Peek();
                if (c < 0) {
                    _state = State.EndOfFile;
                    break;
                }

                LexicalToken result = null;
                char ch = (char)c;
                try
                {
                    switch (_state)
                    {
                        case State.LineBreak:
                            result = ProcessLineBreak(ch); break;
                        case State.Whitespace: 
                            result = ProcessWhitespace(ch); break;
                        case State.Comment: 
                            result = ProcessComment(ch); break;
                        case State.Token: 
                            result = ProcessToken(ch); break;
                        case State.Directive: 
                            result = ProcessDirective(ch); break;
                        case State.RegularExpression: 
                            result = ProcessRegEx(ch); break;
                        default:
                            result = ProcessOpen(ch); break;
                    }
                }
                catch
                {
                    _state = State.Error;
                    throw;
                }

                if (result != null) {
                    _current = result;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Closes the underlying stream and changes the <see cref="ReadState"/> to <c>Closed</c>.
        /// </summary>
        public virtual void Close()
        {
            _state = State.Closed;
            Dispose(true);
        }
        #endregion

        #region "Protected Methods"

        /// <summary>
        /// Determines if the character is a valid whitespace.
        /// </summary>
        /// <param name="c">Character to check.</param>
        /// <returns><c>true</c> if the character is one of the allowed whitespace character or 
        /// Unicode space character, otherwise, <c>false</c>.</returns>
        protected virtual bool IsWhitespace(char c)
        {
            if ((c == '\u0009') || (c == '\u000b') || (c == '\u000c'))
                return true;
            return (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.SpaceSeparator);
        }

        /// <summary>
        /// Determines if the character is a valid line break character.
        /// </summary>
        /// <param name="c">Character to check.</param>
        /// <returns><c>true</c> if the character is one of the allowed line break character, 
        /// otherwise, <c>false</c>.</returns>
        protected virtual bool IsLineBreak(char c)
        {
            return Array.IndexOf(lineTerminators, c) >= 0;
        }

        /// <summary>
        /// Determines if the character is a valid digit.
        /// </summary>
        /// <param name="c">Character to check.</param>
        /// <returns><c>true</c> if the character is one of the allowed digit character, 
        /// otherwise, <c>false</c>.</returns>
        protected virtual bool IsDigit(char c)
        {
            return (('0' <= c) && (c <= '9'));
        }

        /// <summary>
        /// Determines if the character is a valid letter character.
        /// </summary>
        /// <param name="c">Character to check.</param>
        /// <returns><c>true</c> if the character is one of the allowed Unicode letter character,
        /// otherwise, <c>false</c>.</returns>
        protected virtual bool IsLetter(char c)
        {
            var cat = CharUnicodeInfo.GetUnicodeCategory(c);
            return (cat == UnicodeCategory.UppercaseLetter) || 
                   (cat == UnicodeCategory.LowercaseLetter) || 
                   (cat == UnicodeCategory.TitlecaseLetter) || 
                   (cat == UnicodeCategory.ModifierLetter) || 
                   (cat == UnicodeCategory.OtherLetter) || 
                   (cat == UnicodeCategory.LetterNumber);
        }

        /// <summary>
        /// Determines if the character is a valid identifier part character other than letters.
        /// </summary>
        /// <param name="c">Character to check.</param>
        /// <returns><c>true</c> if the character is one of the allowed identifier part character
        /// excluding letters, otherwise, <c>false</c>.</returns>
        protected virtual bool IsIdentifierMark(char c)
        {
            var cat = CharUnicodeInfo.GetUnicodeCategory(c);
            return (cat == UnicodeCategory.DecimalDigitNumber) ||   // Decimal digits
                   (cat == UnicodeCategory.ConnectorPunctuation) || // Connecting chars
                   (cat == UnicodeCategory.NonSpacingMark) ||       // Combining chars
                   (cat == UnicodeCategory.SpacingCombiningMark) ||
                   (cat == UnicodeCategory.Format);                 // Format chars
        }

        /// <summary>
        /// Determines if the character is a valid regular expression options flag.
        /// </summary>
        /// <param name="c">Character to check.</param>
        /// <returns><c>true</c> if the character is in the allowed regular expression 
        /// options flag format, otherwise, <c>false</c>.</returns>
        protected virtual bool IsRegexOption(char c)
        {
            return (('a' <= c) && (c <= 'z')) || (('A' <= c) && (c <= 'Z'));
        }

        /// <summary>
        /// Reads and returns the next character from stream.
        /// </summary>
        protected char ReadChar()
        {
            Column++;
            return (char)sourceBuffer.Read();
        }
        #endregion

        #region "Private Methods"
        private LexicalToken ProcessOpen(char ch)
        {
            if (ch == '.' && _newline) {
                _state = State.Directive;
                return null;
            }

            if (IsLineBreak(ch)) {
                _state = State.LineBreak;
                return null;
            }

            if (IsWhitespace(ch)) {
                if (_newline)
                    throw new SyntaxException("Unexpected indent");
                else
                    ReadChar(); // Consume additional whitespaces
                return null;
            }

            _newline = false;   // The following are all not whitespaces

            if (ch == '#') {
                _state = State.Comment;
                ReadChar();
                return null;
            }

            if (ch == '$') {
                ReadChar();

                int ahead = sourceBuffer.Peek();
                if (ahead != '/')
                    throw new SyntaxException("Regular expression expected");
                ReadChar();

                _processState = null;
                _state = State.RegularExpression;
                return null;
            }

            _state = State.Token;
            _tokenSubState = TokenState.Open;
            return null;
        }

        private LexicalToken ProcessLineBreak(char ch)
        {
            // This state only gets called whenever a line break is detected by other states
            ReadChar();
            int ahead = sourceBuffer.Peek();
            if ((ch == '\u000d') && (ahead == '\u000a'))
                ReadChar(); // Consume additional line break character

            Line++;
            Column = 1;
            _newline = true;
            _processState = 0;
            _state = State.Whitespace; // Always expect whitespace after line breaks
            return null;
        }

        private LexicalToken ProcessWhitespace(char ch)
        {
            int cur = (int)_processState;

            if ((ch == '\u0009') || (ch == '\u000b') || (ch == '\u000c')) {
                ReadChar();
                _processState = cur + 4;
                return null;
            }

            if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.SpaceSeparator) {
                _processState = cur + 1;
                return null;
            }

            int last = _indentLevels.Count > 0 ? _indentLevels.Peek() : 0;

            if (last > cur) {
                _indentLevels.Pop();
                return new LexicalToken(LexicalTokenType.Dedent);
            }
            else if (last < cur) {
                _indentLevels.Push(cur);
                return new LexicalToken(LexicalTokenType.Indent);
            }

            // Same indent. continue;
            _state = State.Open;
            return null;
        }

        private LexicalToken ProcessComment(char ch)
        {
            // Calling this state will eat up all remaining characters on this line
            if (!IsLineBreak(ch)) {
                _textBuffer.Append(ReadChar());
                return null;
            }

            _state = State.Open;
            return new LexicalToken(LexicalTokenType.Comment, ConsumeBuffer());
        }

        private LexicalToken ProcessToken(char ch)
        {
            // TODO: Split between operators, keywords, and identifiers
            switch (_tokenSubState)
            {
                default: // TokenState.Open
                    if (ch == '"') {
                        _tokenSubState = TokenState.StringLiteralVerbatim;
                        _processState = false;
                        return null;
                    }

                    if (ch == '\'') {
                        _tokenSubState = TokenState.StringLiteral;
                        _processState = false;
                        return null;
                    }

                    if (IsDigit(ch) || ch == '.') { // number sign should be handled as an operator
                        _tokenSubState = TokenState.NumberLiteral;
                        _processState = -1;
                        return null;
                    }

                    if (ch == '@') { // Use @ as an operator precedence override
                        ReadChar();
                        int ahead = sourceBuffer.Peek();
                        if ((ahead < 0) || (ahead != '_' && !IsLetter((char)ahead)))
                            throw new SyntaxException("Keyword or identifier expected after verbatim specifier '@'");
                        
                        _tokenSubState = TokenState.Identifier;
                        _processState = true; // Verbatim state
                        return null;
                    }

                    if ((ch == '_') || IsLetter(ch)) {
                        _tokenSubState = TokenState.Identifier;
                        _processState = false; // Could be keyword
                        return null;
                    }

                    // Special symbols remaining. Check operators.
                    _tokenSubState = TokenState.Operator;
                    return null;

                case TokenState.Identifier:
                    if (IsLetter(ch) || IsIdentifierMark(ch)) {
                        _textBuffer.Append(ReadChar());
                        return null;
                    }

                    if (ch == '~' /* && !strictParsing */ ) { // Identifier wildcard
                        // TODO: Implement strict parsing
                        // POINT: Wildcard at the end? (ex. Con~.Re~Line)
                        _textBuffer.Append(ReadChar());
                        return null;
                    }

                    _state = State.Open;

                    string identifier = ConsumeBuffer();

                    if (!(bool)_processState) {
                        string id_lower = identifier.ToLowerInvariant();

                        // Match strict and contexual keywords
                        foreach (string keyword in strictKeywords)
                            if (String.Equals(keyword, id_lower)) {
                                // TODO: Set strict token type
                                return new LexicalToken(LexicalTokenType.Keyword, id_lower, true);
                            }

                        foreach (string keyword in contextKeywords)
                            if (String.Equals(keyword, id_lower)) {
                                // TODO: Set it as indeterminate keyword
                                return new LexicalToken(LexicalTokenType.Keyword, id_lower, false);
                            }

                        // Match literals
                        if (String.Equals(id_lower, "true")) 
                            return new LexicalToken(LexicalTokenType.BooleanLiteral, id_lower, true);

                        if (String.Equals(id_lower, "false"))
                            return new LexicalToken(LexicalTokenType.BooleanLiteral, id_lower, false);

                        // TODO: Build up a null literal
                        if (String.Equals(id_lower, "null"))
                            return new LexicalToken(LexicalTokenType.Keyword, id_lower, null);
                    }

                    return new LexicalToken(LexicalTokenType.Identifier, identifier);

                case TokenState.NumberLiteral:
                    return ProcessNumberLiteral(ch);

                case TokenState.StringLiteral:
                    return ProcessStringLiteral(ch);

                case TokenState.StringLiteralVerbatim:
                    if (ch != '"') {
                        _textBuffer.Append(ReadChar());
                        return null;
                    }

                    ReadChar(); // Eats the quote
                    if (!((bool)_processState)) {
                        _processState = true;
                        return null;
                    }
                    else
                        if (sourceBuffer.Peek() == '"') {
                            _textBuffer.Append(ReadChar()); // Escape quote
                            return null;
                        }

                    _state = State.Open;
                    return new LexicalToken(LexicalTokenType.StringLiteral, ConsumeBuffer());

                case TokenState.Operator:
                    return ProcessOperator(ch);
            }
        }

        private LexicalToken ProcessNumberLiteral(char ch)
        {
            // Process states: 0 - dec; 10 - real; 16 - hex
            int nanostate = (int)_processState;
            // Int: \d+ | 0x[\da-fA-F]+
            // Real: \d*\.\d+
            if (nanostate == -1) {
                _processState = 0;
                if (ch == '0') {
                    ReadChar();
                    if (sourceBuffer.Peek() == 'x') {
                        ReadChar();
                        _processState = 16;
                        return null;
                    }
                    _textBuffer.Append('0');    // Compensate the zero
                    return null;
                }
                if (ch == '.') {
                    ReadChar();
                    int ahead = sourceBuffer.Peek();
                    if (ahead > 0 && IsDigit((char)ahead)) {
                        _textBuffer.Append("0.");    // Abbreviation form
                        return null;
                    }
                    _state = State.Open;
                    return new LexicalToken(LexicalTokenType.Operator, "."); // Dot operator
                }
            }

            if (IsDigit(ch)) {
                _textBuffer.Append(ReadChar());
                return null;
            }

            if (nanostate == 16) {
                char alt = char.ToLowerInvariant(ch);
                if (('a' <= alt) && (alt <= 'f')) {
                    _textBuffer.Append(ReadChar());
                    return null;
                }
            }

            if ((ch == '.') && (nanostate == 0)) {  // Only int is eligible to convert to real
                ReadChar(); // Eats the period

                int ahead = sourceBuffer.Peek();
                if ((ahead > 0) && (IsDigit((char)ahead)))
                {
                    _processState = 10;
                    _textBuffer.Append('.');
                    return null;
                }

                // If no digits following, then we guessed wrong and need to emit dot operator instead
                _state = State.Open;
                return new LexicalToken(LexicalTokenType.Operator, ".");
            }

            _state = State.Open;
            string digits = ConsumeBuffer();
            if ((int)_processState == 16) { // Perform additional conversion on hex
                if (digits.Length <= 0)
                    throw new SyntaxException("Abruptly ended hexadecimal integer constant");
                else {
                    // TODO: Large hex representation
                    try {
                        digits = Convert.ToInt64(digits, 16).ToString();
                    }
                    catch (OverflowException ex) {
                        throw new SyntaxException("Integral constant is too large on Allegro#", ex);
                    }
                }
            }
            return new LexicalToken(((int)_processState == 10) ? LexicalTokenType.RealLiteral :
                                                                 LexicalTokenType.IntegerLiteral,
                                                                 digits);
        }

        private LexicalToken ProcessStringLiteral(char ch)
        {
            if (ch == '\'') {
                ReadChar(); // Eats the quote
                if ((bool)_processState) {
                    _state = State.Open;
                    return new LexicalToken(LexicalTokenType.StringLiteral, ConsumeBuffer());
                }
                _processState = true;
                return null;
            }

            if (ch == '\\') { // Start of escape sequence
                ReadChar();
                switch (sourceBuffer.Peek())
                {
                    case '\'': _textBuffer.Append('\''); break;
                    case '\"': _textBuffer.Append('\"'); break;
                    case '\\': _textBuffer.Append('\\'); break;
                    case '0': _textBuffer.Append('\0'); break;
                    case 'a': _textBuffer.Append('\a'); break;
                    case 'b': _textBuffer.Append('\b'); break;
                    case 'f': _textBuffer.Append('\f'); break;
                    case 'n': _textBuffer.Append('\n'); break;
                    case 'r': _textBuffer.Append('\r'); break;
                    case 't': _textBuffer.Append('\t'); break;
                    case 'v': _textBuffer.Append('\n'); break;
                    default:
                        throw new SyntaxException("Unrecognized escape sequence");
                }
                ReadChar(); // Eats the escape char
                return null;
            }

            if (IsLineBreak(ch)) {
                // Though technically allowed...
                throw new SyntaxException("Newline in constant");
            }

            _textBuffer.Append(ReadChar());
            return null;
        }

        private LexicalToken ProcessOperator(char ch)
        {
            // TODO: The reason why we use individual cases is that we'll need to replace them with enums
            switch (ch)
            {
                case '+':
                    ReadChar();
                    _state = State.Open;
                    return new LexicalToken(LexicalTokenType.Operator, "+", OperatorType.Add);
                case '-':
                    ReadChar();
                    _state = State.Open;
                    return new LexicalToken(LexicalTokenType.Operator, "-", OperatorType.Subtract);
                case '*':
                    if (_textBuffer.Length > 0) {
                        ReadChar(); ConsumeBuffer();
                        _state = State.Open;
                        return new LexicalToken(LexicalTokenType.Operator, "**", OperatorType.Exponent);
                    }
                    _textBuffer.Append(ReadChar());
                    return null; // If it's a single *, default will handle for us
                case '/':
                    if (_textBuffer.Length > 0) {
                        ReadChar(); ConsumeBuffer();
                        _state = State.Open;
                        return new LexicalToken(LexicalTokenType.Operator, "//", OperatorType.Modulus);
                    }
                    _textBuffer.Append(ReadChar());
                    return null; // If it's a single /, default will handle for us
                case '<':
                    ReadChar();
                    _state = State.Open;
                    switch (sourceBuffer.Peek())
                    {
                        case '<': // Left-shift
                            ReadChar();
                            return new LexicalToken(LexicalTokenType.Operator, "<<", OperatorType.LeftShift);
                        case '=': // Lesser than or equal
                            ReadChar();
                            return new LexicalToken(LexicalTokenType.Operator, "<=", OperatorType.LesserThanOrEqual);
                        default: // Lesser than
                            return new LexicalToken(LexicalTokenType.Operator, "<", OperatorType.LesserThan);
                    }
                case '>':
                    ReadChar();
                    _state = State.Open;
                    switch (sourceBuffer.Peek())
                    {
                        case '>': // Right-shift
                            ReadChar();
                            return new LexicalToken(LexicalTokenType.Operator, ">>", OperatorType.RightShift);
                        case '=': // Greater than or equal
                            ReadChar();
                            return new LexicalToken(LexicalTokenType.Operator, ">=", OperatorType.GreaterThanOrEqual);
                        default: // Greater than
                            return new LexicalToken(LexicalTokenType.Operator, ">", OperatorType.GreaterThan);
                    }
                case '=':
                    ReadChar();
                    _state = State.Open;
                    switch (sourceBuffer.Peek())
                    {
                        case '=': // Equal
                            ReadChar();
                            return new LexicalToken(LexicalTokenType.Operator, "==", OperatorType.Equal);
                        default: // Assign
                            return new LexicalToken(LexicalTokenType.Operator, "=", OperatorType.Assign);
                    }
                case '!':
                    ReadChar();
                    _state = State.Open;
                    if (sourceBuffer.Peek() != '=')
                        return new LexicalToken(LexicalTokenType.Operator, "!", OperatorType.BitwiseNot); // ~ used as wildcard
                    else
                        return new LexicalToken(LexicalTokenType.Operator, "!=", OperatorType.NotEqual);
                case '%':
                    ReadChar();
                    _state = State.Open;
                    return new LexicalToken(LexicalTokenType.Operator, "%", OperatorType.Format);
                case '&':
                    ReadChar();
                    _state = State.Open;
                    return new LexicalToken(LexicalTokenType.Operator, "&", OperatorType.BitwiseAnd);
                case '|':
                    ReadChar();
                    _state = State.Open;
                    return new LexicalToken(LexicalTokenType.Operator, "|", OperatorType.BitwiseOr);
                case '^':
                    ReadChar();
                    _state = State.Open;
                    return new LexicalToken(LexicalTokenType.Operator, "^", OperatorType.BitwiseXor);
                case '.':
                    ReadChar();
                    _state = State.Open;
                    return new LexicalToken(LexicalTokenType.Operator, ".", OperatorType.Dot); // Dot operator
                case ',':
                    ReadChar();
                    _state = State.Open;
                    return new LexicalToken(LexicalTokenType.Operator, ",", OperatorType.Comma); // Comma seperator
                case '(':
                    ReadChar();
                    _state = State.Open;
                    return new LexicalToken(LexicalTokenType.Operator, "(", OperatorType.LeftParenthese);
                case ')':
                    ReadChar();
                    _state = State.Open;
                    return new LexicalToken(LexicalTokenType.Operator, ")", OperatorType.RightParenthese);
                case '[':
                    ReadChar();
                    _state = State.Open;
                    return new LexicalToken(LexicalTokenType.Operator, "[", OperatorType.LeftSquareBracket); // Left Indexer B.
                case ']':
                    ReadChar();
                    _state = State.Open;
                    return new LexicalToken(LexicalTokenType.Operator, "]", OperatorType.RightSquareBracket); // Right Indexer B.
                case '{':
                    ReadChar();
                    _state = State.Open;
                    return new LexicalToken(LexicalTokenType.Operator, "{", OperatorType.LeftCurlyBracket); // Left Entity B.
                case '}':
                    ReadChar();
                    _state = State.Open;
                    return new LexicalToken(LexicalTokenType.Operator, "}", OperatorType.RightCurlyBracket); // Right Entity B.
                case ':':
                    ReadChar();
                    _state = State.Open;
                    return new LexicalToken(LexicalTokenType.Operator, ":", OperatorType.Colon);
                default:
                    if (_textBuffer.Length > 0)
                    {
                        // Pending operator.
                        _state = State.Open;
                        return new LexicalToken(LexicalTokenType.Operator, ConsumeBuffer());
                    }
                    else break;
            }

#if DEBUG
            throw new SyntaxException(String.Format("Unexpected character '{0}' (U+{1:X4})", ch, (int)ch));
#else
            throw new SyntaxException(String.Format("Unexpected character '{0}'", ch));
#endif
        }

        private LexicalToken ProcessDirective(char ch)
        {
            // TODO: Implement preprocessor directive instead of eat up the whole line
            while (!IsLineBreak(ch))
                ReadChar();
            _state = State.Open;
            return null;
        }

        private LexicalToken ProcessRegEx(char ch)
        {
            if (_processState == null) {

                if (ch == '/') {
                    ReadChar();
                    _processState = ConsumeBuffer();
                    return null;
                }
                else
                    if (!IsLineBreak(ch)) {
                        _textBuffer.Append(ReadChar());
                        return null;
                    }

                throw new SyntaxException("Newline in constant");
            }
            else {

                if (IsRegexOption(ch)) {
                    _textBuffer.Append(ReadChar());
                    return null;
                }

                LexicalToken t = new LexicalToken(LexicalTokenType.RegularExpression);
                t.Value = (string)_processState;
                t.Tag = ConsumeBuffer();
                return t;
            }
        }

        private string ConsumeBuffer()
        {
            string s = _textBuffer.ToString();
            _textBuffer = new StringBuilder(16);
            return s;
        }
        #endregion

        #region "Properties"
        /// <summary>
        /// Gets the current state of the <c>LexicalProcessor</c>.
        /// </summary>
        public virtual ReadState ReadState
        {
            get
            {
                switch (_state)
                {
                    case State.Initial: return ReadState.Initial;
                    case State.Error: return ReadState.Error;
                    case State.EndOfFile: return ReadState.EndOfFile;
                    case State.Closed: return ReadState.Closed;
                    default: return ReadState.Interactive;
                }
            }
        }

        /// <summary>
        /// Gets the latest token read from the source.
        /// </summary>
        public virtual LexicalToken Current
        {
            get { return _current; }
            protected set
            {
                lastToken = _current;
                _current = value;
            }
        }

        /// <summary>
        /// Gets the current line number.
        /// </summary>
        public virtual int Line { get; protected set; }

        /// <summary>
        /// Gets the current column number.
        /// </summary>
        public virtual int Column { get; protected set; }
        #endregion

        #region "Fields"
        /// <summary>
        /// Holds the source buffer for reading.
        /// </summary>
        protected TextReader sourceBuffer;

        /// <summary>
        /// Holds the latest token ever read.
        /// </summary>
        protected LexicalToken lastToken;

        private LexicalToken _current;

        private State _state;

        private Stack<int> _indentLevels;

        private bool _newline;

        private StringBuilder _textBuffer;

        private object _processState;

        private TokenState _tokenSubState;
        #endregion

        #region "Static Fields"
        protected static readonly char[] lineTerminators = {'\u000d', '\u000a', '\u0085', '\u2028', '\u2029'};
        protected static readonly string[] strictKeywords = {
                                                                "def", "class", "import", "require", 
                                                                "if", "elif", "else", 
                                                                "for", "while", "until", 
                                                                "try", "catch", "finally", "throw", 
                                                                "continue", "break", "return", 
                                                                "then", 
                                                                "pass", "await", "async", 
                                                            };
        protected static readonly string[] contextKeywords = {
                                                                "from", "in", "get", "set", "as",
                                                             };
        protected static readonly string[] primitiveTypes = { "int", "real", "bool", "string" };
        #endregion

        #region "Enums"
        private enum State
        {
            Initial = 0,
            Open,
            Closed,
            Error,
            EndOfFile,
            Comment,
            LineBreak,
            Whitespace,
            Token,
            Directive,
            RegularExpression,
        }

        private enum TokenState
        {
            Open,
            Identifier,
            NumberLiteral,
            StringLiteral,
            StringLiteralVerbatim,
            Operator,
        }
        #endregion

        #region "IDisposable Implementation"
        private bool disposed = false; 

        /// <summary>
        /// Releases all resources used by the <c>LexicalProcessor</c> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <c>LexicalProcessor</c> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; 
        /// <c>false</c> to release only unmanaged resources.</param>
        protected void Dispose(bool disposing)
        {
            if (disposed) return;
            if (disposing)
            {
                if (sourceBuffer != null)
                    sourceBuffer.Dispose();
            }

            sourceBuffer = null;
            disposed = true;
        }
        #endregion
    }
}
