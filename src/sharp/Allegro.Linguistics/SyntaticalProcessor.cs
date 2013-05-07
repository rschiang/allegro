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
    	public SyntaticalProcessor()
        {
        }
        #endregion

        #region "Methods"

        /// <summary>
        /// Parse Allegro code elements from specified sequence of lexical tokens.
        /// </summary>
        /// <param name="source">An enumerable sequence of lexical tokens.</param>
        /// <exception cref="ArgumentNullException">Thrown when <c>source</c> is <c>null</c>.</exception>
        /// <returns>An instance of <c>IList</c> containing zero or more code elements.</returns>
        public virtual bool Parse(IEnumerable<LexicalToken> source)
        {
            if (source == null) throw new ArgumentNullException("source");
            sourceBuffer = source;
            // TODO: Implement CodeDOM and parser
        }
        #endregion

        #region "Fields"

        #endregion
    }
}
