using GFW.Sharp.Core.Ciphering;
using GFW.Sharp.Core.Util;
using Org.Mentalis.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace GFW.Sharp.Lite.Proxy.GFWPress
{
    public class GFWPressClient : Client
    {
        private ConcurrentList<byte> _sendingQueue;
        private IPEndPoint m_MapTo;

        private IPEndPoint MapTo
        {
            get
            {
                return m_MapTo;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();
                m_MapTo = value;
            }
        }

        private SecretKey _key;

        public GFWPressClient(Socket ClientSocket, DestroyDelegate Destroyer, IPEndPoint MapTo, SecretKey key) : base(ClientSocket, Destroyer)
        {
            this.MapTo = MapTo;
            _key = key;
            _sendingQueue = new ConcurrentList<byte>();
        }

        public override void StartHandshake()
        {
            try
            {
                DestinationSocket = new Socket(MapTo.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                DestinationSocket.BeginConnect(MapTo, new AsyncCallback(this.OnConnected), DestinationSocket);
            }
            catch
            {
                Dispose();
            }
        }

        private void OnConnected(IAsyncResult ar)
        {
            try
            {
                DestinationSocket.EndConnect(ar);
                StartRelay();
            }
            catch
            {
                Dispose();
            }
        }

        protected override void OnClientReceive(IAsyncResult ar)
        {
            try
            {
                if (((Socket)ar.AsyncState).Connected == false)
                {
                    Dispose();
                    return;
                }
                int Ret = ClientSocket.EndReceive(ar);
                if (Ret <= 0)
                {
                    Dispose();
                    return;
                }
                byte[] receivedBytes = new byte[Ret];
                System.Array.Copy(Buffer, 0, receivedBytes, 0, Ret);
                Encrypt aes = new Encrypt();
                byte[] sendingBytes = aes.encryptNet(_key, receivedBytes);
                DestinationSocket.BeginSend(sendingBytes, 0, sendingBytes.Length, SocketFlags.None, new AsyncCallback(this.OnRemoteSent), DestinationSocket);
            }
            catch
            {
                Dispose();
            }
        }

        protected override void OnRemoteReceive(IAsyncResult ar)
        {
            try
            {
                if (((Socket)ar.AsyncState).Connected == false)
                {
                    Dispose();
                    return;
                }
                int Ret = DestinationSocket.EndReceive(ar);
                if (Ret <= Encrypt.ENCRYPT_SIZE)
                {
                    Dispose();
                    return;
                }

                byte[] receivedBytes = new byte[Ret];
                System.Array.Copy(RemoteBuffer, 0, receivedBytes, 0, receivedBytes.Length);

                _sendingQueue.AddRange(receivedBytes);
                receivedBytes = null;
                byte[] head = new byte[Encrypt.ENCRYPT_SIZE];
                int read = _sendingQueue.PopRange(head, head.Length);
                Encrypt aes = new Encrypt();
                byte[] size_bytes = aes.decrypt(_key, head);
                if (size_bytes == null)
                {

                    Dispose();
                    return;
                }
                
                int[] sizes = aes.getBlockSizes(size_bytes);
                if (sizes == null || sizes.Length != 2)
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

                    read_num = DestinationSocket.Receive(buffer, read_count, size_count - read_count, SocketFlags.None);

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

                    decrypt_bytes = aes.decrypt(_key, realdataBuffer);

                }
                else
                {

                    decrypt_bytes = aes.decrypt(_key, buffer);

                }
                //Logger.ThreadWrite("got bytes:\t" + decrypt_bytes.Length);
                if (decrypt_bytes == null)
                {
                    Dispose();
                    return;
                }


                ClientSocket.BeginSend(decrypt_bytes, 0, decrypt_bytes.Length, SocketFlags.None, new AsyncCallback(this.OnClientSent), ClientSocket);
            }
            catch
            {
                Dispose();
            }
        }
        
    }
}
