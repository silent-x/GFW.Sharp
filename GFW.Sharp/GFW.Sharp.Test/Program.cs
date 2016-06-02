using GFW.Sharp.Core.Ciphering;
using GFW.Sharp.Core.Forward;
using GFW.Sharp.Core.Forward.GFWPress;
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
            //Encrypt aes = new Encrypt();

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

            //SecretKey key = aes.getPasswordKey("Q1w2e3r$");
            //Listener server = new GFWPressForwardServerListener(IPAddress.Parse("127.0.0.1"), 4567, IPAddress.Parse("192.168.1.200"), 8500, key);
            //server.Start();
            //Listener client = new GFWPressForwardClientListener(IPAddress.Parse("127.0.0.1"), 1234, IPAddress.Parse("127.0.0.1"), 4567, key);
            //client.Start();

            //var ipExt = Listener.GetLocalExternalIP();
            //var ipInt = Listener.GetLocalInternalIP();

            Console.ReadLine();

        }

    }
}
