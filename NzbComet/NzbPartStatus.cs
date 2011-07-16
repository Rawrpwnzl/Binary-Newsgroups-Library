using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbComet
{
    public enum NzbPartStatus
    {
        Unknown,
        Downloading,
        Downloaded,
        QueuedDecoding,
        Decoding,
        Decoded,
    }
}
