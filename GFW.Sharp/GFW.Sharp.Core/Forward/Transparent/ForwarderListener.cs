using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GFW.Sharp.Core.Forward.Transparent
{
    public class ForwarderListener : Listener
    {
        public ForwarderListener(int Port, IPEndPoint MapToIP) : this(IPAddress.Any, Port, MapToIP) { }
        public ForwarderListener(IPAddress Address, int Port, IPEndPoint MapToIP) : base(Port, Address)
        {
            MapTo = MapToIP;
        }
        public ForwarderListener(IPAddress Address, int Port, IPAddress MapToAddress, int MapToPort) : this(Address, Port, new IPEndPoint(MapToAddress, MapToPort)) { }
        private IPEndPoint m_MapTo;
        private IPEndPoint MapTo
        {
            get
            {
                return m_MapTo;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();
                m_MapTo = value;
            }
        }
        public override string ConstructString
        {
            get
            {
                return "host:" + Address.ToString() + ";int:" + Port.ToString() + ";host:" + MapTo.Address.ToString() + ";int:" + MapTo.Port.ToString();
            }
        }

        public override void OnAccept(IAsyncResult ar)
        {
            try
            {
                Socket inputSocket = ListenSocket.EndAccept(ar);
                if (inputSocket != null)
                {
                    Socket outputSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    outputSocket.Connect(MapTo);
                    Forwarder forwarderTx = new TransparentForwarder(inputSocket, new DestroyDelegate(this.RemoveClient), outputSocket);
                    Forwarder forwarderRx = new TransparentForwarder(outputSocket, new DestroyDelegate(this.RemoveClient), inputSocket);
                    AddClient(forwarderTx);
                    AddClient(forwarderRx);
                    forwarderTx.StartForward();
                    forwarderRx.StartForward();
                }
            }
            catch { }
            try
            {
                //Restart Listening
                ListenSocket.BeginAccept(new AsyncCallback(this.OnAccept), ListenSocket);
            }
            catch
            {
                Dispose();
            }
        }

        public override string ToString()
        {
            return "PORTMAP service on " + Address.ToString() + ":" + Port.ToString();
        }
    }
}
