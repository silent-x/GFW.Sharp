using GFW.Sharp.Core.Util;
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

        public override void StartForward()
        {
            try
            {
                ClientSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(this.OnClientReceive), ClientSocket);
            }
            catch
            {
                Dispose();
            }
        }
        ///<summary>Called when we have received data from the local client.<br>Incoming data will immediately be forwarded to the remote host.</br></summary>
        ///<param name="ar">The result of the asynchronous operation.</param>
        protected void OnClientReceive(IAsyncResult ar)
        {
            try
            {
                int Ret = ClientSocket.EndReceive(ar);
                if (Ret <= 0)
                {
                    Dispose();
                    return;
                }
                //Logger.WriteSocketToFile(ClientSocket, _buffer, 0, Ret,false);
                int sent = DestinationSocket.Send(_buffer, 0, Ret, SocketFlags.None);
                //Logger.WriteSocketToFile(DestinationSocket, _buffer, 0, Ret,true);
                if (sent > 0)
                {
                    ClientSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(this.OnClientReceive), ClientSocket);
                    return;
                }
            }
            catch
            {
                Dispose();
            }
        }
       
    }
}
