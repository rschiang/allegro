using System;

namespace Allegro
{
    /// <summary>
    /// Specifies the state of the <see cref="LexicalProcessor"/>.
    /// </summary>
    public enum ReadState
    {
        /// <summary>
        /// The <c>Read</c> method has not been called.
        /// </summary>
        Initial = 0,

        /// <summary>
        /// The <c>Read</c> method has been called, and methods on the reader are available.
        /// </summary>
        Interactive,

        /// <summary>
        /// The reader encountered an error while reading.
        /// </summary>
        Error,

        /// <summary>
        /// The reader reached the end of stream successfully.
        /// </summary>
        EndOfFile,

        /// <summary>
        /// The <c>Closed</c> method has been called.
        /// </summary>
        Closed,
    }
}