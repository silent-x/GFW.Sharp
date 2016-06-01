using GFW.Sharp.Core.Ciphering;
using GFW.Sharp.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GFW.Sharp.Core.Forward.GFWPress
{
    public class GFWPressEncryptForwarder : Forwarder
    {
        private Encrypt _aes = new Encrypt();
        private SecretKey _key;
        private NetworkStream _clientStream;
        private NetworkStream _destinationStream;
        public GFWPressEncryptForwarder(Socket ClientSocket, DestroyDelegate Destroyer, Socket DestinationSocket, SecretKey key) : base(ClientSocket, Destroyer)
        {
            //this.MapTo = MapTo;
            this.DestinationSocket = DestinationSocket;
            this._key = key;
            _clientStream = new NetworkStream(this.ClientSocket);
            _destinationStream = new NetworkStream(this.DestinationSocket);
        }
        public override void StartForward()
        {
            new Task(new Action(() =>
            {
                const int BUFFER_SIZE_MIN = 1024 * 128; // 缓冲区最小值，128K


                const int BUFFER_SIZE_MAX = 1024 * 512; // 缓冲区最大值，512K

                const int BUFFER_SIZE_STEP = 1024 * 128; // 缓冲区自动调整的步长值，128K
                byte[] buffer = new byte[BUFFER_SIZE_MIN];

                byte[] read_bytes = null;

                byte[] encrypt_bytes = null;

                try
                {

                    while (true)
                    {
                        int read_num = -1;

                        try
                        {
                            Logger.Write(this.GetType().Name + " reading bytes");
                            read_num = _clientStream.Read(buffer,0,buffer.Length);
                            Logger.Write(this.GetType().Name + " " + read_num + " bytes read");
                        }
                        catch
                        {
                            Dispose();
                            break;
                        }
                        if (read_num <= 0)
                        {
                            Dispose();
                            break;

                        }

                        read_bytes = new byte[read_num];

                        System.Array.Copy(buffer, 0, read_bytes, 0, read_num);

                        encrypt_bytes = _aes.encryptNet(_key, read_bytes);

                        if (encrypt_bytes == null)
                        {

                            break; // 加密出错，退出

                        }

                        try
                        {
                            Logger.Write(this.GetType().Name + " sending bytes: " + encrypt_bytes.Length);
                            _destinationStream.Write(encrypt_bytes,0,encrypt_bytes.Length);
                            Logger.Write(this.GetType().Name + " sending completed");

                        }
                        catch
                        {
                            Dispose();
                            break;
                        }
                        //outputStream.flush();

                        if (read_num == buffer.Length && read_num < BUFFER_SIZE_MAX)
                        { // 自动调整缓冲区大小

                            buffer = new byte[read_num + BUFFER_SIZE_STEP];

                            // log(this.getName() + " 缓冲区大小自动调整为：" + buffer.length);

                        }
                        else if (read_num < (buffer.Length - BUFFER_SIZE_STEP) && (buffer.Length - BUFFER_SIZE_STEP) >= BUFFER_SIZE_MIN)
                        {

                            buffer = new byte[buffer.Length - BUFFER_SIZE_STEP];

                            // log(this.getName() + " 缓冲区大小自动调整为：" + +buffer.length);

                        }

                    }

                }
                catch (Exception ex)
                {
                    Dispose();
                }
                finally
                {
                    buffer = null;

                    read_bytes = null;

                    encrypt_bytes = null;
                }



            })).Start();
        }

    }
}
