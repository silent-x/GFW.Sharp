using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GFW.Sharp.Core.Forward.GFWPress
{
    public class GFWPressForwardListener : ForwarderListener
    {
        public GFWPressForwardListener(IPAddress listenIPAddress, int listenPort, IPAddress forwardIPAddress, int forwardPort)
            : base(listenIPAddress, listenPort, forwardIPAddress, forwardPort)
        { }

        public override Forwarder GetInputToOutputForwarder(Socket inputSocket, Socket outputSocket)
        {
            throw new NotImplementedException();
        }

        public override Forwarder GetOutputToInputForwarder(Socket outputSocket, Socket inputSocket)
        {
            throw new NotImplementedException();
        }
    }
}
