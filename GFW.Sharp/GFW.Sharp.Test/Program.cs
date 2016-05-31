using GFW.Sharp.Core.Ciphering;
using GFW.Sharp.Core.Forward.Transparent;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GFW.Sharp.Test
{
    class Program
    {
        static SecretKey key = null;
        static void Main(string[] args)
        {
            Encrypt aes = new Encrypt();

            // 文件加密测试
            // aes.testEncryptFile();

            // 测试
            //aes.testSecureRandom();

            //aes.testIsPassword();

            //aes.test();
            //byte[] original = new byte[1024*1024];
            //var random = new Random();
            //for (int i = 0;i< original.Length;i++)
            //{
            //    original[i] = (byte)random.Next(0, 255);
            //}

            //key = aes.getKey();
            //byte[] encrypted = aes.encryptNet(key, original);
            //byte[] decrypted = DecryptNet(key, encrypted);

            //if(original.Length==decrypted.Length)
            //{
            //    for(int i = 0;i< original.Length;i++)
            //    {
            //        if(original[i]!=decrypted[i])
            //        {
            //            Console.WriteLine(i);
            //        }
            //    }
            //}

            //System.Net.Sockets.TcpListener serverlistener = new System.Net.Sockets.TcpListener(IPAddress.Parse("127.0.0.1"),4567);
            //serverlistener.Start();
            //ListenServer(serverlistener);
            //Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //serverSocket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4567));
            //serverSocket.Listen(5);
            //ListenServer(serverSocket);

            //Socket clientSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            //clientSocket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1234));
            //clientSocket.Listen(100);
            //ListenClient(clientSocket);

            ForwarderListener server = new ForwarderListener(IPAddress.Parse("127.0.0.1"), 4567, IPAddress.Parse("192.168.1.200"), 8500);
            server.Start();
            ForwarderListener client = new ForwarderListener(IPAddress.Parse("127.0.0.1"), 1234, IPAddress.Parse("127.0.0.1"), 4567);
            client.Start();
            //listenerRx.Start();
            Console.ReadLine();

        }
        /*
        public static void ListenServer(Socket listener)
        {
            listener.BeginAccept(new AsyncCallback(ar =>
            {
                ListenServer(listener);
                var clientSocket = listener.EndAccept(ar);
                //clientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                //clientSocket.SendTimeout = 180000;
                //clientSocket.ReceiveTimeout = 180000;

                Socket proxySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                proxySocket.Connect(IPAddress.Parse("192.168.1.200"), 8500);
                //proxySocket.SendTimeout = 180000;
                //proxySocket.ReceiveTimeout = 180000;
                //proxySocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                {
                    TransparentForwardWorker forwardToProxy = new TransparentForwardWorker(clientSocket, proxySocket, key);
                    forwardToProxy.Start();

                    TransparentForwardWorker forwardToClient = new TransparentForwardWorker(proxySocket, clientSocket, key);
                    forwardToClient.Start();
                }
                
            }), null);

        }

        public static void ListenClient(Socket listener)
        {
            listener.BeginAccept(new AsyncCallback(ar =>
            {
                ListenClient(listener);
                var localSocket = listener.EndAccept(ar);
                localSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                localSocket.SendTimeout = 180000;
                localSocket.ReceiveTimeout = 180000;
                Socket serverSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Connect(IPAddress.Parse("127.0.0.1"), 4567);
                serverSocket.SendTimeout = 180000;
                serverSocket.ReceiveTimeout = 180000;
                serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                {
                    DecryptForwardWorker forwardToLocal = new DecryptForwardWorker(serverSocket, localSocket, key);
                    forwardToLocal.Start();

                    EncryptForwardWorker forwardToServer = new EncryptForwardWorker(localSocket, serverSocket, key);
                    forwardToServer.Start();
                }
            }), null);
        }
        */

        public static byte[] DecryptNet(SecretKey key, byte[] bytes)
        {
            try
            {
                Encrypt aes = new Encrypt();
                var inputStream = new MemoryStream(bytes);
                var buffer = new byte[Encrypt.ENCRYPT_SIZE];

                int BUFFER_SIZE_MAX = 1024 * 1024 + Encrypt.IV_SIZE;

                int read_num = inputStream.Read(buffer, 0, buffer.Length);

                if (read_num == -1 || read_num != Encrypt.ENCRYPT_SIZE)
                {

                    return null;

                }

                var size_bytes = aes.decrypt(key, buffer);

                if (size_bytes == null)
                {

                    return null; // 解密出错，退出

                }

                var sizes = aes.getBlockSizes(size_bytes);

                if (sizes == null || sizes.Length != 2 || sizes[0] > BUFFER_SIZE_MAX)
                {

                    return null;

                }

                int size_count = sizes[0] + sizes[1];

                buffer = new byte[size_count];

                int read_count = 0;

                while (read_count < size_count)
                {

                    read_num = inputStream.Read(buffer, read_count, size_count - read_count);

                    if (read_num == -1)
                    {

                        break;

                    }

                    read_count += read_num;

                }

                if (read_count != size_count)
                {

                    return null;

                }
                byte[] decrypt_bytes = null;
                if (sizes[1] > 0)
                { // 如果存在噪音数据

                    byte[] _buffer = new byte[sizes[0]];

                    System.Array.Copy(buffer, 0, _buffer, 0, _buffer.Length);

                    decrypt_bytes = aes.decrypt(key, _buffer);

                }
                else
                {

                    decrypt_bytes = aes.decrypt(key, buffer);

                }

                if (decrypt_bytes == null)
                {

                    return null;

                }

                return decrypt_bytes;
                //outputStream.write(decrypt_bytes);

                //outputStream.flush();

            }

            catch (IOException ex)
            {

            }

            return null;

        }
    }
}
