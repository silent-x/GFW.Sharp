using GFW.Sharp.Core.Ciphering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GFW.Sharp.Core.Forward.GFWPress
{
    public class GFWPressDecryptForwarder : Forwarder
    {
        private SecretKey _key;
        private Encrypt _aes = new Encrypt();
        public GFWPressDecryptForwarder(Socket ClientSocket, DestroyDelegate Destroyer, Socket DestinationSocket, SecretKey key) : base(ClientSocket, Destroyer)
        {
            //this.MapTo = MapTo;
            this.DestinationSocket = DestinationSocket;
            _key = key;
        }
        public override void StartForward()
        {
            try
            {
                ClientSocket.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, new AsyncCallback(this.OnClientReceive), ClientSocket);
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
                System.Array.Copy(Buffer, 0, recv, 0, recv.Length);
                byte[] decrypted = recv;
                //System.Array.Reverse(recv);
                DestinationSocket.BeginSend(decrypted, 0, decrypted.Length, SocketFlags.None, new AsyncCallback(this.OnRemoteSent), DestinationSocket);
            }
            catch
            {
                Dispose();
            }
        }
        ///<summary>Called when we have sent data to the remote host.<br>When all the data has been sent, we will start receiving again from the local client.</br></summary>
        ///<param name="ar">The result of the asynchronous operation.</param>
        protected void OnRemoteSent(IAsyncResult ar)
        {
            try
            {
                int Ret = DestinationSocket.EndSend(ar);
                if (Ret > 0)
                {
                    ClientSocket.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, new AsyncCallback(this.OnClientReceive), ClientSocket);
                    return;
                }
            }
            catch { }
            Dispose();
        }
    }
}
