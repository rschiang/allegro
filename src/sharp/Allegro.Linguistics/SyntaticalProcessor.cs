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
        }
        #endregion

        #region "Methods"
        
        #endregion
    }
}
