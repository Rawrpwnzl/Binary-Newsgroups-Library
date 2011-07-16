using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.IO;

namespace NzbComet
{
    public class NzbDownload
    {
        public NzbDownload(string nzbFilename)
        {
            ArgumentChecker.ThrowIfNullOrWhitespace("nzbFilename", nzbFilename);

            this.NzbFilename = nzbFilename;
            this.DownloadDirectory = Path.GetFileNameWithoutExtension(nzbFilename);
            this.Status = NzbDownloadStatus.Unknown;
            this.SizeInBytes = 0L;

            this.Parts = new List<NzbPart>();
        }

        public string NzbFilename { get; private set; }
        public string DownloadDirectory { get; set; }
        public List<NzbPart> Parts { get; private set; }
        public NzbDownloadStatus Status { get; set; }
        public long SizeInBytes { get; private set; }

        internal void Add(NzbPart newPart)
        {
            lock (this.Parts)
            {
                lock (newPart)
                {
                    this.Parts.Add(newPart);
                    this.SizeInBytes += newPart.SizeInBytes;
                }
            }
        }
    }

 

}
