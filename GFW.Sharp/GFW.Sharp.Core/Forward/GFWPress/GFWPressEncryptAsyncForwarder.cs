using GFW.Sharp.Core.Ciphering;
using GFW.Sharp.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;


namespace GFW.Sharp.Core.Forward.GFWPress
{
    public class GFWPressEncryptAsyncForwarder : Forwarder
    {
        private Encrypt _aes = new Encrypt();
        private SecretKey _key;

        public GFWPressEncryptAsyncForwarder(Socket ClientSocket, DestroyDelegate Destroyer, Socket DestinationSocket, SecretKey key) : base(ClientSocket, Destroyer)
        {
            //this.MapTo = MapTo;
            this.DestinationSocket = DestinationSocket;
            this._key = key;
            this.ClientSocket.ReceiveBufferSize = 16 * 1024;
            this.ClientSocket.SendBufferSize = 16 * 1024;
            this.DestinationSocket.ReceiveBufferSize = 16 * 1024;
            this.DestinationSocket.SendBufferSize = 16 * 1024;
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
                byte[] recv = new byte[Ret];
                System.Array.Copy(_buffer, 0, recv, 0, recv.Length);
                //byte[] encrypt = recv;
                //Logger.ThreadWrite("sending bytes:\t" + recv.Length);
                byte[] encrypt = _aes.encryptNet(_key, recv);
                recv = null;
                //using (var ns = new NetworkStream(DestinationSocket))
                //{
                //    ns.Write(encrypt, 0, encrypt.Length);
                //    ns.Flush();
                //}
                int sentCount = 0;
                while (sentCount < encrypt.Length)
                {
                    var toSend = Math.Min(encrypt.Length - sentCount, 1024);
                    sentCount += DestinationSocket.Send(encrypt, sentCount, toSend, SocketFlags.None);
                }
                ClientSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(this.OnClientReceive), ClientSocket);
                encrypt = null;
            }
            catch
            {
                Dispose();
            }
        }
    }
}
