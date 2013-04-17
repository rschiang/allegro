using System;
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
            sourceBuffer = source;
            lastToken = new LexicalToken();
            ReadState = ReadState.Initial;
            Current = new LexicalToken();
            Line = 1;
            Column = 1;
        }
        #endregion

        #region "Methods"
        /// <summary>
        /// Reads the next token from stream.
        /// </summary>
        /// <returns><c>true</c> if the next token was read successfully; otherwise, <c>false</c>.</returns>
        public virtual bool Read()
        {
            StringBuilder buf = new StringBuilder(16);
            State state = State.Open;
            int indent = 0;
            object tag = null;

            while (state != State.Closed)
            {
                int c = sourceBuffer.Peek(), ahead;
                ReadState = ReadState.Interactive;

                switch (state)
                {
                    case State.Open:
                        if (c < 0) {
                            state = State.Closed;
                            continue;
                        }

                        if (c == '#') {
                            state = State.Comment;
                            ReadChar();
                            continue;
                        }

                        if (c == '.') {
                            if (lastToken.Type != LexicalTokenType.LineBreak) {
                                ReadState = ReadState.Error;
                                throw new SyntaxException("Preprocessor directives must appear as " +
                                                          "the first non-whitespace character on a line");
                            }

                            state = State.Directive;
                            continue;
                        }

                        if (c == '$') {
                            ReadChar();
                            ahead = sourceBuffer.Peek();
                            if (ahead != '/') {
                                ReadState = ReadState.Error;
                                throw new SyntaxException("Regular expression expected");
                            }
                            ReadChar();
                            tag = null;
                            state = State.RegularExpression;
                            continue;
                        }

                        if (IsLineBreak((char)c)) {
                            state = State.LineBreak;
                            continue;
                        }

                        if (IsWhitespace((char)c)) {
                            if (lastToken.Type == LexicalTokenType.LineBreak)
                                state = State.Whitespace; // Calculate indent
                            else
                                ReadChar(); // Consume additional whitespace on Open

                            continue;
                        }

                        state = State.Token;
                        break;

                    case State.LineBreak:
                        // This state only gets called whenever a line break is detected by other states
                        sourceBuffer.Read();
                        ahead = sourceBuffer.Peek();
                        if ((c == '\u000d') && (ahead == '\u000a'))
                            sourceBuffer.Read(); // Consume additional line break character

                        Current = new LexicalToken(LexicalTokenType.LineBreak);
                        Line++;
                        Column = 1;
                        return true;

                    case State.Whitespace:
                        if (c < 0) {
                            state = State.Closed;
                            continue;
                        }

                        if ((c == '\u0009') || (c == '\u000b') || (c == '\u000c')) {
                            ReadChar();
                            indent++;
                            continue;
                        }

                        if (CharUnicodeInfo.GetUnicodeCategory((char)c) == UnicodeCategory.SpaceSeparator) {
                            buf.Append(ReadChar());
                            continue;
                        }

                        if ((buf.Length % 4) != 0) {
                            ReadState = ReadState.Error;
                            throw new SyntaxException("Whitespaces except tabs must be a multiple of 4");
                        }

                        indent += (buf.Length / 4);
                        state = State.Open;
                        break;

                    case State.Comment:
                        // Calling this state will eat up all remaining characters on this line
                        if (c >= 0)
                            if (!IsLineBreak((char)c)) {
                                buf.Append(ReadChar());
                                continue;
                            }

                        Current = new LexicalToken(LexicalTokenType.Comment, buf.ToString());
                        return true;

                    case State.Token:
                        // TODO: Split between operators, keywords, and identifiers
                        break;

                    case State.Directive:
                        // TODO: Preprocessor directive
                        break;

                    case State.RegularExpression:
                        if (tag == null) {
                            if (c > 0)
                                if (c == '/') {
                                    ReadChar();
                                    tag = buf.ToString();
                                    buf = new StringBuilder(4);
                                    continue;
                                }
                                else
                                    if (!IsLineBreak((char)c)) {
                                        buf.Append(ReadChar());
                                        continue;
                                    }

                            ReadState = ReadState.Error;
                            throw new SyntaxException("Newline in constant");
                        }
                        else {
                            if (c >= 0) {
                                char ch = (char)c;
                                if (IsRegexOption(ch)) {
                                    buf.Append(ReadChar());
                                    continue;
                                }
                            }

                            LexicalToken t = new LexicalToken(LexicalTokenType.RegularExpression);
                            t.Value = (string)tag;
                            t.Tag = buf.ToString();
                            t.IndentLevel = indent;
                            Current = t;
                            return true;
                        }
                }
            }

            ReadState = ReadState.EndOfFile;
            return false;
        }

        /// <summary>
        /// Closes the underlying stream and changes the <see cref="ReadState"/> to <c>Closed</c>.
        /// </summary>
        public virtual void Close()
        {
            ReadState = ReadState.Closed;
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
        /// Determines if the character is a valid regular expression options flag.
        /// </summary>
        /// <param name="c">Character to check.</param>
        /// <returns><c>true</c> if the character is one of the allowed regular expression 
        /// options flag, otherwise, <c>false</c>.</returns>
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

        #region "Properties"
        /// <summary>
        /// Gets the current state of the <c>LexicalProcessor</c>.
        /// </summary>
        public virtual ReadState ReadState { get; protected set; }

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
        public int Column { get; protected set; }
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
        #endregion

        #region "Static Fields"
        protected static readonly char[] lineTerminators = {'\u000d', '\u000a', '\u0085', '\u2028', '\u2029'};
        #endregion

        #region "Enums"
        private enum State
        {
            Closed = -1,
            Open = 0,
            LineBreak,
            Comment,
            Whitespace,
            Token,
            Directive,
            RegularExpression,
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
