using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NzbComet.NzbDecoders.yEnc.Advanced;
using NzbComet.NzbDecoders;

namespace NzbComet.NzbDecoders.yEnc
{
    internal class yDecoder : NzbDecoder
    {
        private NzbPart _encodedPart;
        private string _segmentsFolder;
        private Encoding _fileEncoding;

        public yDecoder(NzbPart encodedPart, string segmentsFolder, Encoding fileEncoding)
        {
            ArgumentChecker.ThrowIfNull("encodedPart", encodedPart);
            ArgumentChecker.ThrowIfNullOrWhitespace("segmentsFolder", segmentsFolder);
            ArgumentChecker.ThrowIfNull("fileEncoding", fileEncoding);

            _encodedPart = encodedPart;
            _segmentsFolder = segmentsFolder;
            _fileEncoding = fileEncoding;
        }

        internal override byte[] Decode(out string originalFilename)
        {
            originalFilename = null;

            using (var decodedBytesStream = new MemoryStream())
            {
                lock (_encodedPart.Segments)
                {
                    var segments = _encodedPart.Segments.Where(seg => seg.Status == NzbSegmentStatus.Downloaded);

                    foreach (NzbSegment currentSegment in segments)
                    {
                        bool currentLineIsPartOfBinaryContent = false;
                        string currentEncodedSegmentLine = null;
                        string segmentPath = null;

                        lock (currentSegment)
                        {
                            segmentPath = Path.Combine(_segmentsFolder, currentSegment.Id);
                        }

                        if (!File.Exists(segmentPath))
                        {
                            continue;
                        }

                        using (var encodedSegmentReader = new StreamReader(segmentPath, _fileEncoding))
                        {

                            while ((currentEncodedSegmentLine = encodedSegmentReader.ReadLine()) != null)
                            {
                                if (currentEncodedSegmentLine.StartsWith("=ybegin "))
                                {
                                    var header = ParseHeader(currentEncodedSegmentLine);

                                    originalFilename = header.Filename;

                                    currentLineIsPartOfBinaryContent = true;

                                    continue;
                                }

                                if (currentLineIsPartOfBinaryContent)
                                {
                                    if (currentEncodedSegmentLine.StartsWith("=ypart "))
                                    {
                                        var header = ParsePartHeader(currentEncodedSegmentLine);

                                        decodedBytesStream.Seek(header.StartPosition - 1L, SeekOrigin.Begin);
                                    }
                                    else if (currentEncodedSegmentLine.StartsWith("=yend "))
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        foreach (var decodedByte in DecodeLine(currentEncodedSegmentLine))
                                        {
                                            decodedBytesStream.WriteByte(decodedByte);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return decodedBytesStream.ToArray();
            }
        }

        private byte[] DecodeLine(string binaryContent)
        {
            List<byte> decodedContent = new List<byte>();
            bool isCurrentCharASpecialChar = false;

            foreach (char currentChar in binaryContent.ToCharArray())
            {

                if ((currentChar == '=') && !isCurrentCharASpecialChar)
                {
                    isCurrentCharASpecialChar = true;
                }
                else
                {
                    byte currentByte = (byte)currentChar;

                    if (isCurrentCharASpecialChar)
                    {
                        currentByte = (byte)(currentByte - 64);
                        isCurrentCharASpecialChar = false;
                    }

                    currentByte = (byte)(currentByte - 42);
                    decodedContent.Add(currentByte);
                }
            }

            return decodedContent.ToArray();
        }

        // Example: =ybegin part=1 total=1 line=128 size=42020 name=Drag.Racing.Simulator-ALiAS.par2
        private yHeader ParseHeader(string rawHeader)
        {
            yHeader parsedHeader = new yHeader();

            parsedHeader.Filename = ParseStringValue(rawHeader, yDecoderConstants.HeaderNameKeyword);
            parsedHeader.SizeInBytes = ParseLongValue(rawHeader, yDecoderConstants.HeaderSizeKeyword);
            parsedHeader.PartNumber = ParseIntValue(rawHeader, yDecoderConstants.HeaderPartNumberKeyword);
            parsedHeader.TotalNumberOfParts = ParseIntValue(rawHeader, yDecoderConstants.HeaderTotalNumberOfPartsKeyword);
            parsedHeader.LineNumber = ParseIntValue(rawHeader, yDecoderConstants.HeaderLineKeyword);

            return parsedHeader;
        }

        // Example: =ypart begin=1 end=100645
        private yPartHeader ParsePartHeader(string rawPartHeader)
        {
            yPartHeader parsedPartHeader = new yPartHeader();

            parsedPartHeader.StartPosition = ParseLongValue(rawPartHeader, yDecoderConstants.PartHeaderStartPositionKeyword);
            parsedPartHeader.EndPosition = ParseLongValue(rawPartHeader, yDecoderConstants.PartHeaderEndPositionKeyword);

            return parsedPartHeader;
        }

        // Example: =yend size=249600 part=1 pcrc32=d0f50c0e
        private yPartFooter ParsePartFooter(string rawPartFooter)
        {
            yPartFooter parsedFootHeader = new yPartFooter();

            parsedFootHeader.PartCRC32 = ParseHexLongValue(rawPartFooter, yDecoderConstants.PartFooterPartCRC32Keyword);
            parsedFootHeader.PartNumber = ParseIntValue(rawPartFooter, yDecoderConstants.PartFooterPartKeyword);
            parsedFootHeader.SizeInBytes = ParseLongValue(rawPartFooter, yDecoderConstants.PartFooterSizeKeyword);

            return parsedFootHeader;
        }

        private long ParseLongValue(string rawHeader, string keyword)
        {
            StringBuilder buffer = new StringBuilder();
            int indexOfKeyword = rawHeader.IndexOf(keyword);

            if (indexOfKeyword == -1)
            {
                return 0L;
            }

            string relevantHeader = rawHeader.Substring(indexOfKeyword + keyword.Length);

            foreach (char currentChar in relevantHeader)
            {
                if (char.IsDigit(currentChar))
                {
                    buffer.Append(currentChar);
                }
                else
                {
                    break;
                }
            }

            if (buffer.Length > 0)
            {
                return Convert.ToInt64(buffer.ToString());
            }
            else
            {
                return 0L;
            }
        }

        private long ParseHexLongValue(string rawHeader, string keyword)
        {
            StringBuilder buffer = new StringBuilder();
            int indexOfKeyword = rawHeader.IndexOf(keyword);

            if (indexOfKeyword == -1)
            {
                return 0L;
            }

            string relevantHeader = rawHeader.Substring(indexOfKeyword + keyword.Length);

            foreach (char currentChar in relevantHeader)
            {
                if (char.IsDigit(currentChar) ||
                    currentChar >= 'a' && currentChar <= 'f' ||
                    currentChar >= 'A' && currentChar <= 'F')
                {
                    buffer.Append(currentChar);
                }
                else
                {
                    break;
                }
            }

            if (buffer.Length > 0)
            {
                return Convert.ToInt64(buffer.ToString(), 16);
            }
            else
            {
                return 0L;
            }
        }

        private int ParseIntValue(string rawHeader, string keyword)
        {
            StringBuilder buffer = new StringBuilder();
            int indexOfKeyword = rawHeader.IndexOf(keyword);

            if (indexOfKeyword == -1)
            {
                return 0;
            }

            string relevantHeader = rawHeader.Substring(indexOfKeyword + keyword.Length);

            foreach (char currentChar in relevantHeader)
            {
                if (char.IsDigit(currentChar))
                {
                    buffer.Append(currentChar);
                }
                else
                {
                    break;
                }
            }

            if (buffer.Length > 0)
            {
                return Convert.ToInt32(buffer.ToString());
            }
            else
            {
                return 0;
            }
        }

        private string ParseStringValue(string rawHeader, string keyword)
        {
            StringBuilder buffer = new StringBuilder();
            int positionOfKeyword = rawHeader.IndexOf(keyword);

            string relevantHeader = rawHeader.Substring(positionOfKeyword + keyword.Length);

            if (positionOfKeyword == -1)
            {
                return "Unknown";
            }

            foreach (char currentChar in relevantHeader)
            {
                if (!char.IsWhiteSpace(currentChar))
                {
                    buffer.Append(currentChar);
                }
                else
                {
                    break;
                }
            }

            if (buffer.Length > 0)
            {
                return buffer.ToString();
            }
            else
            {
                return "Unknown";
            }
        }
    }
}
