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
        private NetworkStream _clientStream;
        private NetworkStream _destinationStream;
        public GFWPressDecryptForwarder(Socket ClientSocket, DestroyDelegate Destroyer, Socket DestinationSocket, SecretKey key) : base(ClientSocket, Destroyer)
        {
            //this.MapTo = MapTo;
            this.DestinationSocket = DestinationSocket;
            _key = key;
            _clientStream = new NetworkStream(this.ClientSocket);
            _destinationStream = new NetworkStream(this.DestinationSocket);
        }
        public override void StartForward()
        {
            new Task(new Action(() => {
                byte[] buffer = null;

                byte[] size_bytes = null;

                int[] sizes = null;

                byte[] decrypt_bytes = null;
                const int BUFFER_SIZE_MAX = 1024 * 768;
                try
                {

                    while (true)
                    {

                        buffer = new byte[Encrypt.ENCRYPT_SIZE];

                        int read_num = -1;
                        try
                        {
                            read_num = _clientStream.Read(buffer,0,buffer.Length);
                            //new Queue<byte>().en
                        }
                        catch
                        {
                            Dispose();
                            break;
                        }
                        

                        if (read_num == -1 || read_num != Encrypt.ENCRYPT_SIZE)
                        {

                            break;

                        }

                        size_bytes = _aes.decrypt(_key, buffer);

                        if (size_bytes == null)
                        {

                            break; // 解密出错，退出

                        }

                        sizes = _aes.getBlockSizes(size_bytes);

                        if (sizes == null || sizes.Length != 2 || sizes[0] > BUFFER_SIZE_MAX)
                        {

                            break;

                        }

                        int size_count = sizes[0] + sizes[1];

                        buffer = new byte[size_count];

                        int read_count = 0;

                        while (read_count < size_count)
                        {

                            try
                            {
                                read_num = _clientStream.Read(buffer, read_count, size_count - read_count);
                            }
                            catch
                            {
                                Dispose();
                                break;
                            }
                            if (read_num == -1)
                            {

                                break;

                            }

                            read_count += read_num;

                        }

                        if (read_count != size_count)
                        {

                            break;

                        }

                        if (sizes[1] > 0)
                        { // 如果存在噪音数据

                            byte[] _buffer = new byte[sizes[0]];

                            System.Array.Copy(buffer, 0, _buffer, 0, _buffer.Length);

                            decrypt_bytes = _aes.decrypt(_key, _buffer);

                        }
                        else
                        {

                            decrypt_bytes = _aes.decrypt(_key, buffer);

                        }

                        if (decrypt_bytes == null)
                        {

                            break;

                        }

                        try
                        {
                            _destinationStream.Write(decrypt_bytes,0,decrypt_bytes.Length);
                            _destinationStream.Flush();
                        }
                        catch
                        {
                            Dispose();
                        }
                        

                    }

                }
                catch (Exception ex)
                {
                    Dispose();
                }

                buffer = null;

                size_bytes = null;

                sizes = null;

                decrypt_bytes = null;
                

            })).Start();
        }
        
    }
}
