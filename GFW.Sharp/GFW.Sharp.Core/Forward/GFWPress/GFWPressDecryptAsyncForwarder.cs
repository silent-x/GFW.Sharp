using GFW.Sharp.Core.Ciphering;
using GFW.Sharp.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GFW.Sharp.Core.Forward.GFWPress
{
    public class GFWPressDecryptAsyncForwarder : Forwarder
    {
        private Encrypt _aes = new Encrypt();
        private SecretKey _key;
        private NetworkStream _clientStream;
        private NetworkStream _destinationStream;

        private ConcurrentList<byte> _sendingQueue;
        public GFWPressDecryptAsyncForwarder(Socket ClientSocket, DestroyDelegate Destroyer, Socket DestinationSocket, SecretKey key) : base(ClientSocket, Destroyer)
        {
            //this.MapTo = MapTo;
            this.DestinationSocket = DestinationSocket;
            this._key = key;
            _clientStream = new NetworkStream(this.ClientSocket);
            _destinationStream = new NetworkStream(this.DestinationSocket);
            _sendingQueue = new ConcurrentList<byte>();
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
                    System.Threading.Thread.Sleep(10);
                    Dispose();
                    return;
                }
                byte[] recv = new byte[Ret];
                System.Array.Copy(_buffer, 0, recv, 0, recv.Length);
                byte[] decrypted = recv;
                _sendingQueue.AddRange(recv);
                recv = null;
                //_aes.encryptNet(_key, recv);
                if(_sendingQueue.Count>30)
                {
                    byte[] buffer = new byte[30];
                    _sendingQueue.PopRange(buffer, 30);
                    var size_bytes = _aes.decrypt(_key, buffer);
                }

                _destinationStream.Write(decrypted, 0, decrypted.Length);

                _clientStream.BeginRead(_buffer, 0, _buffer.Length, new AsyncCallback(this.OnClientReceive), ClientSocket);
                decrypted = null;
            }
            catch
            {
                Dispose();
            }
        }
    }
}
