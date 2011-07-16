using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbComet.ServerCommands
{
    internal class NzbServerGetBodyCommand : NzbServerCommand
    {
        public NzbServerGetBodyCommand(string articleId)
        {
            if (string.IsNullOrWhiteSpace(articleId))
            {
                throw new ArgumentNullException("articleId");
            }

            this.Command = string.Format("BODY <{0}>", articleId);
        }
    }
}
