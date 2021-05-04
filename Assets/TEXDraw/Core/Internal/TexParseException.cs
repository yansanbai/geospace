using System;

namespace TexDrawLib
{
    public class TexParseException : Exception
    {
        public TexParseException(string message)
            : base(message)
        {
        }

        public TexParseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
