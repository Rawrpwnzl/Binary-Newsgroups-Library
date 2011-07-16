using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbComet
{
    public class NzbPart
    {
        public NzbPart()
        {
            this.Segments = new List<NzbSegment>();
            this.Status = NzbPartStatus.Unknown;
        }

        public List<NzbSegment> Segments { get; private set; }
        public NzbPartStatus Status { get; set; }
        public NzbDownload Parent { get; set; }
        public long SizeInBytes { get; private set; }

        internal void Add(NzbSegment newSegment)
        {
            lock (this.Segments)
            {
                lock (newSegment)
                {
                    this.Segments.Add(newSegment);
                    this.SizeInBytes += newSegment.SizeInBytes;
                }
            }
        }
    }
}
