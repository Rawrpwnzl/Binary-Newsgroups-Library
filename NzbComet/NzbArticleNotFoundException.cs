using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbComet
{
    public class NzbArticleNotFoundException : Exception
    {
        public NzbArticleNotFoundException(string articleId)
            : base()
        {
        }

        public NzbArticleNotFoundException(string articleId, string message)
            : base(message)
        {
        }

        public NzbArticleNotFoundException(string articleId, string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public string ArticleId { get; private set; }
    }
}
