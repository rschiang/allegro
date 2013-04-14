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
        /// Line break.
        /// </summary>
        LineBreak,

        /// <summary>
        /// Whitespace.
        /// </summary>
        Whitespace,

        /// <summary>
        /// Code comment.
        /// </summary>
        Comment,

        /// <summary>
        /// Preprocessor directives.
        /// </summary>
        Directive,

        /// <summary>
        /// An identifier.
        /// </summary>
        Identifier,

        /// <summary>
        /// A recognized keyword of Allegro without escaping.
        /// </summary>
        Keyword,

        /// <summary>
        /// Integer literal.
        /// </summary>
        IntegerLiteral,

        /// <summary>
        /// Real number literal.
        /// </summary>
        RealLiteral,

        /// <summary>
        /// String literal.
        /// </summary>
        StringLiteral,

        /// <summary>
        /// A recognized operator of Allgero.
        /// </summary>
        Operator,

        /// <summary>
        /// Regular expression literal of Allegro.
        /// </summary>
        RegularExpression,
    }
}