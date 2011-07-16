using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbComet.NzbDecoders
{
    public class NzbDecoderJob
    {
        public NzbDecoderJob(string rawSegmentsDirectory, string destinationDirectory, NzbPart part, Encoding fileEncoding, Action<NzbPart> callWhenDone)
        {
            ArgumentChecker.ThrowIfNullOrWhitespace("rawSegmentsDirectory", rawSegmentsDirectory);
            ArgumentChecker.ThrowIfNullOrWhitespace("destinationDirectory", destinationDirectory);
            ArgumentChecker.ThrowIfNull("part", part);
            ArgumentChecker.ThrowIfNull("fileEncoding", fileEncoding);
            ArgumentChecker.ThrowIfNull("callWhenDone", callWhenDone);

            this.RawSegmentsDirectory = rawSegmentsDirectory;
            this.DestinationDirectory = destinationDirectory;
            this.Part = part;
            this.FileEncoding = fileEncoding;
            this.CallWhenDone = callWhenDone;
        }

        public string RawSegmentsDirectory { get; private set; }
        public string DestinationDirectory { get; private set; }
        public NzbPart Part { get; private set; }
        public Encoding FileEncoding { get; private set; }
        public Action<NzbPart> CallWhenDone { get; private set; }
    }
}
