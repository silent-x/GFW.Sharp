using GFW.Sharp.Core.Ciphering;
using GFW.Sharp.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;


namespace GFW.Sharp.Core.Forward.GFWPress
{
    public class GFWPressDecryptAsyncForwarder : Forwarder
    {
        private Encrypt _aes = new Encrypt();
        private SecretKey _key;

        private ConcurrentList<byte> _sendingQueue;
        public GFWPressDecryptAsyncForwarder(Socket ClientSocket, DestroyDelegate Destroyer, Socket DestinationSocket, SecretKey key) : base(ClientSocket, Destroyer)
        {
            this.DestinationSocket = DestinationSocket;
            this._key = key;
            _sendingQueue = new ConcurrentList<byte>();
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
                if (sizes == null || sizes.Length != 2 )
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

                    read_num = ClientSocket.Receive(buffer, read_count, size_count - read_count, SocketFlags.None);

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
                //Logger.ThreadWrite("got bytes:\t" + decrypt_bytes.Length);
                if (decrypt_bytes==null)
                {
                    Dispose();
                    return;
                }

                //using (var ns = new NetworkStream(DestinationSocket))
                //{
                //    ns.Write(decrypt_bytes, 0, decrypt_bytes.Length);
                //    ns.Flush();
                //}
                int sentCount = 0;
                while (sentCount<decrypt_bytes.Length)
                {
                    var toSend = Math.Min(decrypt_bytes.Length - sentCount, 1024);
                    sentCount += DestinationSocket.Send(decrypt_bytes, sentCount, decrypt_bytes.Length - sentCount, SocketFlags.None);
                }
                
                ClientSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(this.OnClientReceive), ClientSocket);
                decrypt_bytes = null;
            }
            catch
            {
                Dispose();
            }
        }
    }
}
