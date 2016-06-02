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
                if (Ret <  Encrypt.ENCRYPT_SIZE)
                {
                    //System.Threading.Thread.Sleep(10);
                    Dispose();
                    return;
                }
                byte[] recv = new byte[Ret];
                System.Array.Copy(_buffer, 0, recv, 0, recv.Length);
                
                _sendingQueue.AddRange(recv);
                recv = null;


                byte[] head = new byte[Encrypt.ENCRYPT_SIZE];
                int read = _sendingQueue.PopRange(head, head.Length);

                byte[] size_bytes = _aes.decrypt(_key, head);
                if (size_bytes == null)
                {

                    Dispose();
                    return;
                }
                int[] sizes = _aes.getBlockSizes(size_bytes);
                if (sizes == null || sizes.Length != 2 || sizes[0] > 1024 * 768)
                {
                    Dispose();
                    return;

                }

                int size_count = sizes[0] + sizes[1];

                byte[] buffer = new byte[size_count];

                int read_count = _sendingQueue.PopRange(buffer, buffer.Length);
                int read_num = 0;
                while (read_count < size_count)
                {

                    read_num = _clientStream.Read(buffer, read_count, size_count - read_count);

                    if (read_num == 0)
                    {

                        break;

                    }

                    read_count += read_num;

                }

                if (read_count != size_count)
                {
                    Dispose();
                    return;
                }

                byte[] decrypt_bytes = null;
                if (sizes[1] > 0)
                { // 如果存在噪音数据

                    byte[] realdataBuffer = new byte[sizes[0]];

                    System.Array.Copy(buffer, 0, realdataBuffer, 0, realdataBuffer.Length);

                    decrypt_bytes = _aes.decrypt(_key, realdataBuffer);

                }
                else
                {

                    decrypt_bytes = _aes.decrypt(_key, buffer);

                }

                if(decrypt_bytes==null)
                {
                    Dispose();
                    return;
                }

                _destinationStream.Write(decrypt_bytes, 0, decrypt_bytes.Length);

                _clientStream.BeginRead(_buffer, 0, _buffer.Length, new AsyncCallback(this.OnClientReceive), ClientSocket);
                decrypt_bytes = null;
            }
            catch
            {
                Dispose();
            }
        }
    }
}
