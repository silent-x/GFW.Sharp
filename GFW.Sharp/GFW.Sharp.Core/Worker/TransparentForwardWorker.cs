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
    public class TransparentForwardWorker : BaseWorker
    {
        private static readonly int BUFFER_SIZE_MAX = 1024 * 768; // 缓冲区可接受的最大值，768K

        private Socket inputSocket = null;

        private Socket outputSocket = null;

        private SecretKey key = null;

        public TransparentForwardWorker(Socket inputSocket, Socket outputSocket, SecretKey key)
        {
            this.inputSocket = inputSocket;
            this.outputSocket = outputSocket;
            Interval = 200;
        }

        protected virtual byte[] TransForm(byte[] bytes)
        {
            return bytes;
        }

        protected override void PerformWork()
        {
            if (inputSocket.Connected && outputSocket.Connected)
            {
                using (NetworkStream inputStream = new NetworkStream(inputSocket))
                using (NetworkStream outputStream = new NetworkStream(outputSocket))
                {
                    byte[] buffer = new byte[BUFFER_SIZE_MAX];
                    byte[] read_bytes = null;
                    int read_num = 0;
                    do
                    {
                        read_num = inputStream.Read(buffer, 0, buffer.Length);
                        if (read_num > 0)
                        {
                            Logger.Write(string.Format("read {0} bytes from server", read_num));
                        }
                        read_bytes = new byte[read_num];

                        System.Array.Copy(buffer, 0, read_bytes, 0, read_num);

                        byte[] transformedBytes = TransForm(read_bytes);

                        outputStream.Write(read_bytes, 0, read_bytes.Length);

                        outputStream.Flush();

                    } while (read_num > 0);

                }
            }
            else
            {
                inputSocket.Close();
                outputSocket.Close();
                Stop();
            }
        }
    }
}
