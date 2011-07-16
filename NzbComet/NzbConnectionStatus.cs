using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbComet
{
    public enum NzbConnectionStatus
    {
        Disconnecting,
        Disconnected,
        Connecting,
        Authenticating,
        AuthenticationFailure,
        Connected,
        Discarded
    }
}
