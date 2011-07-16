using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbComet.NzbDecoders.yEnc.Advanced
{
    internal struct yPartFooter
    {
        public long SizeInBytes { get; set; }
        public int PartNumber { get; set; }
        public long PartCRC32 { get; set; }
    }
}
