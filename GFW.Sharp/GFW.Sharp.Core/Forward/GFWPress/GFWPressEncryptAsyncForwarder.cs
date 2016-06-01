using GFW.Sharp.Core.Ciphering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GFW.Sharp.Core.Forward.GFWPress
{
    public class GFWPressEncryptAsyncForwarder : Forwarder
    {
        private Encrypt _aes = new Encrypt();
        private SecretKey _key;
        private NetworkStream _clientStream;
        private NetworkStream _destinationStream;
        public GFWPressEncryptAsyncForwarder(Socket ClientSocket, DestroyDelegate Destroyer, Socket DestinationSocket, SecretKey key) : base(ClientSocket, Destroyer)
        {
            //this.MapTo = MapTo;
            this.DestinationSocket = DestinationSocket;
            this._key = key;
            _clientStream = new NetworkStream(this.ClientSocket);
            _destinationStream = new NetworkStream(this.DestinationSocket);
        }
        public override void StartForward()
        {
            try
            {
                _clientStream.BeginRead(_buffer, 0, _buffer.Length, new AsyncCallback(this.OnClientReceive), _clientStream);
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
                int Ret = _clientStream.EndRead(ar);
                if (Ret <= 0)
                {
                    Dispose();
                    return;
                }
                byte[] recv = new byte[Ret];
                System.Array.Copy(_buffer, 0, recv, 0, recv.Length);
                //byte[] encrypt = recv;

                byte[] encrypt = _aes.encryptNet(_key, recv);
                recv = null;

                _destinationStream.Write(encrypt, 0, encrypt.Length);
                _clientStream.BeginRead(_buffer, 0, _buffer.Length, new AsyncCallback(this.OnClientReceive), ClientSocket);
                encrypt = null;
            }
            catch
            {
                Dispose();
            }
        }
    }
}
