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
    public class TransparentForwardWorker 
    {
        private static readonly int BUFFER_SIZE_MAX = 1024 * 768; // 缓冲区可接受的最大值，768K

        private Socket inputSocket = null;

        private Socket outputSocket = null;

        private SecretKey key = null;

        public TransparentForwardWorker(Socket inputSocket, Socket outputSocket, SecretKey key)
        {
            this.inputSocket = inputSocket;
            this.outputSocket = outputSocket;
            
        }

        protected virtual byte[] TransForm(byte[] bytes)
        {
            return bytes;
        }

        public void Start()
        {
            byte[] buffer = new byte[BUFFER_SIZE_MAX];

            try
            {
                inputSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(OnInputReceived), buffer);
            }
            catch (Exception ex)
            {
                inputSocket.Shutdown(SocketShutdown.Both);
            }



        }

        private void OnInputReceived(IAsyncResult ar)
        {
            
            try
            {
                var buffer = (byte[])ar.AsyncState;
                int ret = inputSocket.EndReceive(ar);
                if (ret > 0)
                {
                    outputSocket.BeginSend(buffer, 0, ret, SocketFlags.None, new AsyncCallback(this.OnOutputSent), buffer);
                }
                else
                {
                    System.Threading.Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                //Dispose();
            }
            Start();
        }

        private void OnOutputSent(IAsyncResult ar)
        {
            try
            {
                int ret = outputSocket.EndSend(ar);
                
            }
            catch (Exception ex)
            {

            }
            //Dispose();
        }
    }
}
