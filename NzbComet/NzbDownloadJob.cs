using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbComet
{
    public class NzbDownloadJob
    {
        public NzbDownloadJob(string downloadDirectory, string temporaryDirectory, NzbSegment segment, Action<NzbDownloadJob> callWhenDone)
        {
            ArgumentChecker.ThrowIfNullOrWhitespace("downloadDirectory", downloadDirectory);
            ArgumentChecker.ThrowIfNullOrWhitespace("temporaryDirectory", temporaryDirectory);
            ArgumentChecker.ThrowIfNull("segment", segment);
            ArgumentChecker.ThrowIfNull("callWhenDone", callWhenDone);

            this.DownloadDirectory = downloadDirectory;
            this.TemporaryDirectory = temporaryDirectory;
            this.Segment = segment;
            this.CallWhenDone = callWhenDone;
        }

        public string DownloadDirectory { get; private set; }
        public string TemporaryDirectory { get; private set; }
        public NzbSegment Segment { get; private set; }
        public Action<NzbDownloadJob> CallWhenDone { get; private set; }
    }
}
