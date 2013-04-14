using System;
using System.Collections.Generic;
using System.Text;

namespace Allegro
{
    /// <summary>
    /// Represents an error that occurs when parsing Allegro code.
    /// </summary>
    [Serializable]
    public class ParserException : Exception
    {
        public ParserException() { }
        public ParserException(string message) : base(message) { }
        public ParserException(string message, Exception inner) : base(message, inner) { }
        protected ParserException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
