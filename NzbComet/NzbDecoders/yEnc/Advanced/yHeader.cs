using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbComet.NzbDecoders.yEnc.Advanced
{
    internal struct yHeader
    {
        public string Filename { get; set; }
        public int PartNumber { get; set; }
        public int TotalNumberOfParts { get; set; }
        public int LineNumber { get; set; }
        public long SizeInBytes { get; set; }
    }
}
