using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;


namespace GFW.Sharp.Core.Util
{
    public class Logger
    {
        private static object _lock = new object();
        public static void ThreadWrite(string message)
        {
            lock (_lock)
            {
                Console.WriteLine(string.Format("{0}:Thd-{2}\t{1}", DateTime.Now.TimeOfDay, message, System.Threading.Thread.CurrentThread.ManagedThreadId));
            }
        }

        public static void Write(string message)
        {
            Console.WriteLine(string.Format("{0}:\t{1}", DateTime.Now.TimeOfDay, message));
        }

        public static void WriteSocketToFile(Socket socket,byte[] buffer,int index, int count, bool send)
        {
            string strRemoteEndPoint = socket.RemoteEndPoint.ToString().Replace(':', '-');
            string strLocalEndPoint = socket.LocalEndPoint.ToString().Replace(':', '-');
            string fileName = string.Empty;
            if (send)
            {
                fileName = string.Format(@"D:\abc\send-{0}_{1}.txt", strLocalEndPoint, strRemoteEndPoint);
            }
            else
            {
                fileName = string.Format(@"D:\abc\recv-{0}_{1}.txt", strRemoteEndPoint, strLocalEndPoint);
            }
            StringBuilder sb = new StringBuilder();

            byte[] data = new byte[count];
            System.Array.Copy(buffer, 0, data, 0, count);

            File.AppendAllText(fileName, BitConverter.ToString(data).Replace("-", string.Empty));
        }
    }
}
