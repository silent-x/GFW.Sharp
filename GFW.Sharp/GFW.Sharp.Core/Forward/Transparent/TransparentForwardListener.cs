using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;


namespace GFW.Sharp.Core.Forward.Transparent
{
    public class TransparentForwardListener : ForwarderListener
    {
        public TransparentForwardListener(IPAddress listenIPAddress, int listenPort, IPAddress forwardIPAddress, int forwardPort)
            :base(listenIPAddress,listenPort,forwardIPAddress,forwardPort)
        { }
        public override Forwarder GetInputToOutputForwarder(Socket inputSocket, Socket outputSocket)
        {
            return new TransparentForwarder(inputSocket, new DestroyDelegate(this.RemoveClient), outputSocket);
        }

        public override Forwarder GetOutputToInputForwarder(Socket outputSocket, Socket inputSocket)
        {
            return new TransparentForwarder(outputSocket, new DestroyDelegate(this.RemoveClient), inputSocket);
        }
    }
}
