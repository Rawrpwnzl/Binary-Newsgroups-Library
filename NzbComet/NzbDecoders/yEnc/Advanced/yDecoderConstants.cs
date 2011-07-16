using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbComet.NzbDecoders.yEnc.Advanced
{
    internal static class yDecoderConstants
    {
        // Header constants.
        public static readonly string HeaderSizeKeyword = "size=";
        public static readonly string HeaderNameKeyword = "name=";
        public static readonly string HeaderPartNumberKeyword = "part=";
        public static readonly string HeaderTotalNumberOfPartsKeyword = "total=";
        public static readonly string HeaderLineKeyword = "line=";


        // Part Header constants.
        public static readonly string PartHeaderStartPositionKeyword = "begin=";
        public static readonly string PartHeaderEndPositionKeyword = "end=";


        // Part Footer constants.
        public static readonly string PartFooterSizeKeyword = "size=";
        public static readonly string PartFooterPartKeyword = "part=";
        public static readonly string PartFooterPartCRC32Keyword = "pcrc32=";
    }
}
