using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbComet
{
    public enum NzbSegmentStatus
    {
        Unknown,
        QueuedDownload,
        Downloading,
        Downloaded,
        DownloadFailed,
        Discarded
    }
}
