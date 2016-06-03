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
            ClientSocket.Blocking = false;
            this.DestinationSocket = DestinationSocket;
            _key = key;
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
                            //var vs = new NetworkStream(ClientSocket);
                            read_num = ClientSocket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                            //new Queue<byte>().en
                        }
                        catch (SocketException sex)
                        {
                            if (sex.SocketErrorCode == SocketError.WouldBlock)
                            {
                                System.Threading.Thread.Sleep(1000);
                                continue;
                            }
                            else
                            {
                                Dispose();
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Dispose();
                            break;
                        }

                        if (read_num != Encrypt.ENCRYPT_SIZE)
                        {
                            Dispose();
                            break;

                        }

                        size_bytes = _aes.decrypt(_key, buffer);

                        if (size_bytes == null)
                        {
                            Dispose();
                            break; // 解密出错，退出

                        }

                        sizes = _aes.getBlockSizes(size_bytes);

                        if (sizes == null || sizes.Length != 2 || sizes[0] > BUFFER_SIZE_MAX)
                        {
                            Dispose();
                            break;

                        }

                        int size_count = sizes[0] + sizes[1];

                        buffer = new byte[size_count];

                        int read_count = 0;

                        while (read_count < size_count)
                        {

                            try
                            {
                                read_num = ClientSocket.Receive(buffer, read_count, size_count - read_count,SocketFlags.None);
                            }
                            catch (SocketException sex)
                            {
                                if (sex.SocketErrorCode == SocketError.WouldBlock)
                                {
                                    System.Threading.Thread.Sleep(1000);
                                    continue;
                                }
                                else
                                {
                                    Dispose();
                                    break;
                                }
                            }
                            catch
                            {
                                Dispose();
                                break;
                            }
                            if (read_num == 0)
                            {
                                Dispose();
                                break;

                            }

                            read_count += read_num;

                        }

                        if (read_count != size_count)
                        {
                            Dispose();
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
                            Dispose();
                            break;

                        }

                        try
                        {
                            DestinationSocket.Send(decrypt_bytes, 0, decrypt_bytes.Length, SocketFlags.None);
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
