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

            while (state != State.Closed)
            {
                int c = sourceBuffer.Peek();
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
                            sourceBuffer.Read();
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

                        if (Array.IndexOf(lineTerminators, (char)c) >= 0) {
                            state = State.Whitespace;
                            continue;
                        }

                        if (IsWhitespace((char)c)) {
                            if (lastToken.Type == LexicalTokenType.LineBreak)
                                state = State.Whitespace; // Calculate indent
                            else
                                sourceBuffer.Read(); // Consume additional whitespace on Open

                            continue;
                        }

                        state = State.Token;
                        break;

                    case State.LineBreak:
                        // This state only gets called whenever a line break is detected by other states
                        sourceBuffer.Read();
                        int ahead = sourceBuffer.Peek();
                        if ((c == '\u000d') && (ahead == '\u000a'))
                            sourceBuffer.Read(); // Consume additional line break character

                        NextToken(new LexicalToken(LexicalTokenType.LineBreak));
                        return true;

                    case State.Whitespace:
                        if (c < 0) {
                            state = State.Closed;
                            continue;
                        }

                        if ((c == '\u0009') || (c == '\u000b') || (c == '\u000c')) {
                            sourceBuffer.Read();
                            indent++;
                            continue;
                        }

                        if (CharUnicodeInfo.GetUnicodeCategory((char)c) == UnicodeCategory.SpaceSeparator) {
                            sourceBuffer.Read();
                            buf.Append((char)c);
                            continue;
                        }

                        if ((buf.Length % 4) != 0)
                            throw new SyntaxException("Whitespaces except tabs must be a multiple of 4");

                        indent += (buf.Length / 4);
                        state = State.Open;
                        break;

                    case State.Comment:
                        // Calling this state will eat up all remaining characters on this line
                        if ((c >= 0) && (Array.IndexOf(lineTerminators, (char)c) < 0)) {
                            buf.Append((char)c);
                            continue;
                        }
                        else {
                            state = (c < 0) ? State.Closed : State.LineBreak;
                            NextToken(new LexicalToken(LexicalTokenType.Comment, buf.ToString()));
                            return true;
                        }

                    case State.Token:
                        break;

                    case State.Directive:
                        break;
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
        /// Advances to the next token.
        /// </summary>
        /// <param name="t">New token read from <c>Read</c> method.</param>
        protected virtual void NextToken(LexicalToken t)
        {
            lastToken = Current;
            Current = t;
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
        public virtual LexicalToken Current { get; protected set; }
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
