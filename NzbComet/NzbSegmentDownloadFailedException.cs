using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbComet
{
    public class NzbSegmentDownloadFailedException : Exception
    {
        public NzbSegmentDownloadFailedException(NzbSegment segment)
            : base()
        {
        }

        public NzbSegmentDownloadFailedException(NzbSegment segment, string message)
            : base(message)
        {
        }

        public NzbSegmentDownloadFailedException(NzbSegment segment, string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public NzbSegment Segment { get; private set; }
    }
}
