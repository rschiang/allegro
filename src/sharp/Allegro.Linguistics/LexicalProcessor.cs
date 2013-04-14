using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

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
        }
        #endregion

        #region "Methods"
        /// <summary>
        /// Reads the next token from stream.
        /// </summary>
        /// <returns><c>true</c> if the next token was read successfully; otherwise, <c>false</c>.</returns>
        public virtual bool Read()
        {
            // TODO: Implement LexicalProcessor.Read()
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

        #region "Properties"
        public virtual ReadState ReadState { get; protected set; }

        public virtual LexicalToken Current { get; protected set; }
        #endregion

        #region "Fields"
        /// <summary>
        /// Holds the source buffer for reading.
        /// </summary>
        protected TextReader sourceBuffer;
        #endregion

        #region "IDisposable Implementation"
        private bool disposed = false; 

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

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
