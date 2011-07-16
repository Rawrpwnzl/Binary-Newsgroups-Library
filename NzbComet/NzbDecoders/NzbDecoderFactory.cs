using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NzbComet.NzbDecoders.yEnc;

namespace NzbComet.NzbDecoders
{
    internal class NzbDecoderFactory
    {
        public NzbDecoderFactory()
        {
        }

        public NzbDecoder CreateDecoder(NzbPart encodedPart, string segmentsFolder, Encoding fileEncoding)
        {
            lock (encodedPart.Segments)
            {
                var segments = encodedPart.Segments;
                foreach (var currentSegment in segments)
                {
                    string filepath = null;
                    lock (currentSegment)
                    {
                        filepath = Path.Combine(segmentsFolder, currentSegment.Id);
                    }

                    using (var reader = new StreamReader(filepath, Encoding.GetEncoding("iso-8859-1")))
                    {
                        string currentLine = null;

                        while ((currentLine = reader.ReadLine()) != null)
                        {
                            if (currentLine.StartsWith("=ybegin "))
                            {
                                return new yDecoder(encodedPart, segmentsFolder, fileEncoding);
                            }
                        }

                    }
                }
            }

            return null;
        }
    }
}
