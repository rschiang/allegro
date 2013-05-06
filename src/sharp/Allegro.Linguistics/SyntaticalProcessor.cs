using System;
using System.Collections.Generic;
using System.Text;

namespace Allegro
{
    /// <summary>
    /// Provides functionality to translate lexical tokens from Allegro source.
    /// </summary>
    public class SyntaticalProcessor
    {
        #region "Constructor"
        /// <summary>
        /// Creates a new instance of <c>LexicalProcessor</c>.
        /// </summary>
        /// <param name="source">An enumerable sequence of lexical tokens.</param>
        /// <exception cref="ArgumentNullException">Thrown when <c>source</c> is <c>null</c>.</exception>
    	public SyntaticalProcessor(IEnumerable<LexicalToken> source)
        {
            if (source == null) throw new ArgumentNullException("source");
            sourceBuffer = source;
        }
        #endregion

        #region "Methods"

        /// <summary>
        /// Parse the next syntatical element.
        /// </summary>
        /// <returns><c>true</c> if the next token was read successfully; otherwise, <c>false</c>.</returns>
        public virtual bool Read()
        {
            // TODO
        }
        #endregion

        #region "Properties"
        
        #endregion

        #region "Fields"
        protected IEnumerable<LexicalToken> sourceBuffer;
        private IList<LexicalToken> queue;
        #endregion
    }
}
