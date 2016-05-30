using GFW.Sharp.Core.Ciphering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GFW.Sharp.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Encrypt aes = new Encrypt();

            // 文件加密测试
            // aes.testEncryptFile();

            // 测试
            //aes.testSecureRandom();

            //aes.testIsPassword();

            //aes.test();
            byte[] b = new byte[1024];
            b[512] = 97;

            SecretKey key = aes.getKey();
            byte[] encrypted = aes.encryptNet(key, b);
            byte[] decrypted = aes.decrypt(key, encrypted);

            System.Console.ReadLine();
        }
    }
}
