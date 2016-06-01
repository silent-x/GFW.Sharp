using GFW.Sharp.Core.Ciphering;
using GFW.Sharp.Core.Forward;
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


            Listener server = new TransparentForwardListener(IPAddress.Parse("192.168.1.107"), 4567, IPAddress.Parse("192.168.1.200"), 8500);
            server.Start();
            Listener client = new TransparentForwardListener(IPAddress.Parse("192.168.1.107"), 1234, IPAddress.Parse("192.168.1.107"), 4567);
            client.Start();

            Console.ReadLine();

        }

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
