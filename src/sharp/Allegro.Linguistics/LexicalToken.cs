using System;
using System.Collections.Generic;
using System.Text;

namespace Allegro
{
    /// <summary>
    /// Represents a read token from <see cref="LexicalProcessor"/>.
    /// </summary>
    public class LexicalToken
    {
        #region "Constructor"
        /// <summary>
        /// Creates an empty instance of <c>LexicalToken</c>.
        /// </summary>
        public LexicalToken()
        {
            Type = LexicalTokenType.None;
            Value = null;
            Tag = null;
        }

        /// <summary>
        /// Creates an instance of <c>LexicalToken</c> with specified type.
        /// </summary>
        /// <param name="type">Type of token.</param>
        public LexicalToken(LexicalTokenType type)
        {
            Type = type;
            Value = null;
            Tag = null;
        }

        /// <summary>
        /// Creates an instance of <c>LexicalToken</c> with specified parameters.
        /// </summary>
        /// <param name="type">Type of token.</param>
        /// <param name="value">Associated value of this token.</param>
        public LexicalToken(LexicalTokenType type, string value)
        {
            Type = type;
            Value = value;
        }

        /// <summary>
        /// Creates an instance of <c>LexicalToken</c> with specified parameters.
        /// </summary>
        /// <param name="type">Type of token.</param>
        /// <param name="value">Associated value of this token.</param>
        /// <param name="tag">Additional data related to this token.</param>
        public LexicalToken(LexicalTokenType type, string value, object tag)
        {
            Type = type;
            Value = value;
            Tag = tag;
        }
        #endregion 

        #region "Properties"
        /// <summary>
        /// Gets or sets the token type.
        /// </summary>
        public virtual LexicalTokenType Type { get; set; }

        /// <summary>
        /// Gets or sets the associated token value.
        /// </summary>
        public virtual string Value { get; set; }

        /// <summary>
        /// Gets or sets additional data for this token.
        /// </summary>
        public virtual object Tag { get; set; }

        /// <summary>
        /// Gets the indent level of this token.
        /// </summary>
        /// <remarks>A valid value can only be expected when the instance represents a token.
        /// That is, on <c>LexicalTokenType.Comment</c>, <c>LexicalTokenType.Directive</c>, 
        /// <c>LexicalTokenType.LineBreak</c>, <c>LexicalTokenType.Whitespace</c>, 
        /// the correctness of this value is implement-dependent.</remarks>
        public virtual int IndentLevel { get; set; }
        #endregion
    }
}
