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
            while (sourceBuffer.Peek() >= 0)
            {
                char c = (char)sourceBuffer.Read();
                ReadState = ReadState.Interactive;
                
                // Line-break
                if (Array.IndexOf(lineTerminators, c) >= 0)
                {
                    if ((c == '\u000d') && (sourceBuffer.Peek() == '\u000a'))
                        sourceBuffer.Read(); // Consume additional line break character

                    Current = new LexicalToken(LexicalTokenType.LineBreak);
                    return true;
                }

                // Comment
                if (c == '#')
                {
                    int ch;
                    while ((ch = sourceBuffer.Peek()) >= 0)
                        if (Array.IndexOf(lineTerminators, (char)ch) >= 0)
                            break;
                        else
                            buf.Append((char)sourceBuffer.Read());

                    Current = new LexicalToken(LexicalTokenType.Comment, buf.ToString());
                    return true;
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
        protected virtual bool IsWhitespace(char c)
        {
            if ((c == '\u0009') || (c == '\u000b') || (c == '\u000c'))
                return true;
            return (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.SpaceSeparator);
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
        #endregion

        #region "Static Fields"
            protected static readonly char[] lineTerminators = {'\u000d', '\u000a', '\u0085', '\u2028', '\u2029'};
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
