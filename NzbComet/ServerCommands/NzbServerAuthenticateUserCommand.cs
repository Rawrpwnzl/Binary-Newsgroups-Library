using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbComet.ServerCommands
{
    internal class NzbServerAuthenticateUserCommand : NzbServerCommand
    {
        public NzbServerAuthenticateUserCommand(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentNullException("username");
            }

            this.Command = string.Format("AUTHINFO USER {0}", username);
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
            if (!string.IsNullOrWhiteSpace(response) && response.StartsWith("3"))
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
