using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GFW.Sharp.Core.Forward.GFWPress
{
    public class GFWPressEncryptForwarder : Forwarder
    {
        public GFWPressEncryptForwarder(Socket ClientSocket, DestroyDelegate Destroyer, Socket DestinationSocket) : base(ClientSocket, Destroyer)
        {
            //this.MapTo = MapTo;
            this.DestinationSocket = DestinationSocket;
        }
        public override void StartForward()
        {
            throw new NotImplementedException();
        }
    }
}
