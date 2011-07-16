using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbComet
{
    internal class NzbAuthenticationFailedException : Exception
    {
        public NzbAuthenticationFailedException()
            : base()
        {
        }

        public NzbAuthenticationFailedException(string message)
            : base(message)
        {
        }

        public NzbAuthenticationFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
