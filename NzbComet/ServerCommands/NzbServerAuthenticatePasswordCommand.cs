using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbComet.ServerCommands
{
    internal class NzbServerAuthenticatePasswordCommand : NzbServerCommand
    {
        public NzbServerAuthenticatePasswordCommand(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentNullException("password");
            }

            this.Command = string.Format("AUTHINFO PASS {0}", password);
        }

        public override bool InvokesResponse
        {
            get
            {
                return true;
            }
        }

        protected override void CheckResponseCore(string response)
        {
            if (!string.IsNullOrWhiteSpace(response) && response.StartsWith("2"))
            {
                this.Response = NzbServerCommandResponse.Success;
            }
            else
            {
                this.Response = NzbServerCommandResponse.Failure;
            }
        }
    }
}
