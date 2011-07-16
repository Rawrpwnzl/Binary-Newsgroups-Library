using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbComet.ServerCommands
{
    internal class NzbServerJoinGroupCommand : NzbServerCommand
    {
        public NzbServerJoinGroupCommand(string group)
        {
            if (string.IsNullOrWhiteSpace(group))
            {
                throw new ArgumentNullException("group");
            }

            this.Command = string.Format("GROUP {0}", group);
        }
    }
}
