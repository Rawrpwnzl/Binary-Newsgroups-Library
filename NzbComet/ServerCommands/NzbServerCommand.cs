using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbComet.ServerCommands
{
    internal abstract class NzbServerCommand
    {
        protected NzbServerCommand()
        {
        }

        public string Command { get; protected set; }

        public virtual bool InvokesResponse
        {
            get
            {
                return false;
            }
        }

        public NzbServerCommandResponse Response { get; protected set; }

        public void CheckResponse(string response)
        {
            this.CheckResponseCore(response);
        }

        protected virtual void CheckResponseCore(string response)
        {
        }
    }
}
