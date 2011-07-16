using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbComet
{
    public class NzbArticleDownloadFailedException : Exception
    {
        public NzbArticleDownloadFailedException(string articleId)
            : base()
        {
        }

        public NzbArticleDownloadFailedException(string articleId, string message)
            : base(message)
        {
        }

        public NzbArticleDownloadFailedException(string articleId, string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public string ArticleId { get; private set; }
    }
}
