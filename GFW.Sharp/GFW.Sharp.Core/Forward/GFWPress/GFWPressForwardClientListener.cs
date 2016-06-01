using GFW.Sharp.Core.Ciphering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GFW.Sharp.Core.Forward.GFWPress
{
    public class GFWPressForwardClientListener : ForwarderListener
    {
        private SecretKey _key;
        public GFWPressForwardClientListener(IPAddress listenIPAddress, int listenPort, IPAddress forwardIPAddress, int forwardPort, SecretKey key)
            : base(listenIPAddress, listenPort, forwardIPAddress, forwardPort)
        {
            _key = key;
        }

        public override Forwarder GetInputToOutputForwarder(Socket inputSocket, Socket outputSocket)
        {
            return new GFWPressEncryptForwarder(inputSocket, RemoveClient, outputSocket, _key);
        }

        public override Forwarder GetOutputToInputForwarder(Socket outputSocket, Socket inputSocket)
        {
            return new GFWPressDecryptForwarder(outputSocket, RemoveClient, inputSocket, _key);
        }
    }
}
