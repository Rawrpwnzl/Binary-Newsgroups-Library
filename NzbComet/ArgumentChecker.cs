using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbComet
{
    internal static class ArgumentChecker
    {
        internal static void ThrowIfNullOrWhitespace(string paramName, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException(paramName);
            }
        }

        internal static void ThrowIfOutsideBounds(string paramName, string message,int value, int lowerLimit, int upperLimit)
        {
            if (value < lowerLimit || value > upperLimit)
            {
                throw new ArgumentOutOfRangeException(paramName, value, message);
            }
        }

        internal static void ThrowIfBelow(string paramName, string message, long value, long lowerLimit)
        {
            if (value < lowerLimit)
            {
                throw new ArgumentOutOfRangeException(paramName, value, message);
            }
        }

        internal static void ThrowIfBelow(string paramName, string message, int value, int lowerLimit)
        {
            if (value < lowerLimit)
            {
                throw new ArgumentOutOfRangeException(paramName, value, message);
            }
        }

        internal static void ThrowIfNull(string paramName, object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }
    }
}
