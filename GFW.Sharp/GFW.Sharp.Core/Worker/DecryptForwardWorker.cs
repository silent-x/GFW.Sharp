using GFW.Sharp.Core.Ciphering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GFW.Sharp.Core.Worker
{
    public class DecryptForwardWorker :TransparentForwardWorker
    {
        private static readonly int BUFFER_SIZE_MAX = 1024 * 768; // 缓冲区可接受的最大值，768K

        private Socket inputSocket = null;

        private Socket outputSocket = null;

        private Encrypt aes = null;

        private SecretKey key = null;




        public DecryptForwardWorker(Socket inputSocket, Socket outputSocket, SecretKey key)
            :base(inputSocket,outputSocket,key)
        {

            this.inputSocket = inputSocket;

            this.outputSocket = outputSocket;

            this.key = key;

            aes = new Encrypt();

        }

        protected override byte[] TransForm(byte[] bytes)
        {
            return aes.decrypt(key, bytes);
        }
    }
}
