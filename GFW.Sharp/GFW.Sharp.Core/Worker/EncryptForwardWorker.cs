using GFW.Sharp.Core.Ciphering;
using GFW.Sharp.Core.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GFW.Sharp.Core.Worker
{
    public class EncryptForwardWorker : TransparentForwardWorker
    {
        private static readonly int BUFFER_SIZE_MIN = 1024 * 128; // 缓冲区最小值，128K

        private static readonly int BUFFER_SIZE_MAX = 1024 * 512; // 缓冲区最大值，512K

        private static readonly int BUFFER_SIZE_STEP = 1024 * 128; // 缓冲区自动调整的步长值，128K

        private Socket inputSocket = null;

        private Socket outputSocket = null;

        private Encrypt aes = null;

        private SecretKey key = null;

        private byte[] buffer = null;

        public EncryptForwardWorker(Socket inputSocket, Socket outputSocket, SecretKey key)
            :base(inputSocket,outputSocket,key)
        {

            this.inputSocket = inputSocket;

            this.outputSocket = outputSocket;

            this.key = key;

            aes = new Encrypt();

        }
        protected override byte[] TransForm(byte[] bytes)
        {
            return aes.encrypt(key, bytes);
        }
    }
}
