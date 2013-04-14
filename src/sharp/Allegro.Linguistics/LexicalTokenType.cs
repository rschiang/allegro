using System;

namespace Allegro
{
    /// <summary>
    /// Specifies the type of token.
    /// </summary>
    public enum LexicalTokenType
    {
        /// <summary>
        /// The token does not represent a valid data.
        /// </summary>
        None = 0,

        /// <summary>
        /// An identifier.
        /// </summary>
        Identifier,

        /// <summary>
        /// A recognized keyword of Allegro without escaping.
        /// </summary>
        Keyword,

        /// <summary>
        /// A recognized operator of Allgero.
        /// </summary>
        Operator,

        /// <summary>
        /// String literal.
        /// </summary>
        Literal,

        /// <summary>
        /// Code comment.
        /// </summary>
        Comment,

        /// <summary>
        /// Preprocessor directives.
        /// </summary>
        Directive,

        /// <summary>
        /// Regular expression literal of Allegro.
        /// </summary>
        RegularExpression,
    }
}