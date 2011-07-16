using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbComet.ServerCommands
{
    internal class NzbServerQuitCommand : NzbServerCommand
    {
        public NzbServerQuitCommand()
        {
            this.Command = "QUIT";
        }
    }
}
