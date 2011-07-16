using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbComet.ServerCommands
{
    internal class NzbServerSwitchToReaderModeCommand : NzbServerCommand
    {
        public NzbServerSwitchToReaderModeCommand()
        {
            this.Command = "MODE READER";
        }
    }
}
