using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbComet
{
    public class NzbSegment
    {
        public NzbSegment(long sizeInBytes, string id, List<string> groups)
        {
            ArgumentChecker.ThrowIfBelow("sizeInBytes", "Size in bytes must not be negative.", sizeInBytes, 0L);
            ArgumentChecker.ThrowIfNullOrWhitespace("id", id);
            ArgumentChecker.ThrowIfNull("groups", groups);

            this.SizeInBytes = sizeInBytes;
            this.Id = id;
            this.Groups = groups;
        }

        public long SizeInBytes { get; private set; }
        public List<string> Groups { get; private set; }
        public string Id { get; private set; }
        public NzbSegmentStatus Status { get; set; }
        public NzbPart Parent { get; set; }
    }
}
