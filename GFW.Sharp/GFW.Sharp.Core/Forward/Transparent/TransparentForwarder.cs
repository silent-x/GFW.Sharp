using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GFW.Sharp.Core.Forward.Transparent
{
    public class TransparentForwarder : Forwarder
    {
        //private IPEndPoint m_MapTo;
        public TransparentForwarder(Socket ClientSocket, DestroyDelegate Destroyer, Socket DestinationSocket) : base(ClientSocket, Destroyer)
        {
            //this.MapTo = MapTo;
            this.DestinationSocket = DestinationSocket;
        }

        private void OnConnected(IAsyncResult ar)
        {
            try
            {
                DestinationSocket.EndConnect(ar);
                StartForward();
            }
            catch
            {
                Dispose();
            }
        }
    }
}
