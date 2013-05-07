using System;

namespace Allegro
{
    /// <summary>
    /// Specifies the type of operator token.
    /// </summary>
    public enum OperatorType
    {
        /// <summary>
        /// No operator intended to be represented.
        /// </summary>
        None = 0,

        /// <summary>
        /// Add operator. (+)
        /// </summary>
        Add,

        /// <summary>
        /// Subtract operator. (-)
        /// </summary>
        Subtract,

        /// <summary>
        /// Multiply operator. (*)
        /// </summary>
        Multiply,

        /// <summary>
        /// Divide operator. (/)
        /// </summary>
        Divide,

        /// <summary>
        /// Exponent operator. (**)
        /// </summary>
        Exponent,

        /// <summary>
        /// Modulus operator. (//)
        /// </summary>
        Modulus,

        /// <summary>
        /// Left-shift operator. (<<)
        /// </summary>
        LeftShift,

        /// <summary>
        /// Right-shift operator. (>>)
        /// </summary>
        RightShift,

        /// <summary>
        /// Bitwise AND operator. (&)
        /// </summary>
        BitwiseAnd,

        /// <summary>
        /// Bitwise OR operator. (|)
        /// </summary>
        BitwiseOr,

        /// <summary>
        /// Bitwise NOT operator. (~)
        /// </summary>
        BitwiseNot,

        /// <summary>
        /// Bitwise XOR operator. (^)
        /// </summary>
        BitwiseXor,

        /// <summary>
        /// Lesser than operator. (<)
        /// </summary>
        LesserThan,

        /// <summary>
        /// Greater than operator. (>)
        /// </summary>
        GreaterThan,

        /// <summary>
        /// Lesser than or equal operator. (<=)
        /// </summary>
        LesserThanOrEqual,

        /// <summary>
        /// Greater than or equal operator. (>=)
        /// </summary>
        GreaterThanOrEqual,

        /// <summary>
        /// Equal operator. (==)
        /// </summary>
        Equal,

        /// <summary>
        /// Not equal operator. (!=)
        /// </summary>
        NotEqual,

        /// <summary>
        /// Assign operator. (=)
        /// </summary>
        Assign,

        /// <summary>
        /// Format operator. (%)
        /// </summary>
        Format,

        /// <summary>
        /// Dot operator. (.)
        /// </summary>
        Dot,

        /// <summary>
        /// Comma. (,)
        /// </summary>
        Comma,

        /// <summary>
        /// Left parenthese.
        /// </summary>
        LeftParenthese,

        /// <summary>
        /// Right parenthese.
        /// </summary>
        RightParenthese,

        /// <summary>
        /// Left square bracket.
        /// </summary>
        LeftSquareBracket,

        /// <summary>
        /// Right square bracket.
        /// </summary>
        RightSquareBracket,

        /// <summary>
        /// Left curly bracket.
        /// </summary>
        LeftCurlyBracket,

        /// <summary>
        /// Right curly bracket.
        /// </summary>
        RightCurlyBracket,

        /// <summary>
        /// Reserved for future use.
        /// </summary>
        Future = 1024
    }
}