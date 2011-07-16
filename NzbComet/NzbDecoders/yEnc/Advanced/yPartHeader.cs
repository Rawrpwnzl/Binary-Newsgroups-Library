using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbComet.NzbDecoders.yEnc.Advanced
{
    internal struct yPartHeader
    {
        public long StartPosition { get; set; }
        public long EndPosition { get; set; }
    }
}
