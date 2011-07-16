using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbComet.NzbDecoders
{
    internal abstract class NzbDecoder
    {
        internal abstract byte[] Decode(out string originalFilename);
    }
}
