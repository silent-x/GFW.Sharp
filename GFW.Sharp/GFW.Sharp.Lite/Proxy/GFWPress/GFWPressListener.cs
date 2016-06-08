using GFW.Sharp.Core.Ciphering;
using Org.Mentalis.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace GFW.Sharp.Lite.Proxy.GFWPress
{
    public class GFWPressListener : Listener
    {
        private IPEndPoint m_MapTo;
        private SecretKey _key;
        private GFWPressWorkMode _mode;

        public override string ConstructString
        {
            get
            {
                return string.Empty;
            }
        }

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
        public GFWPressListener(IPAddress Address, int Port, IPAddress MapToAddress, int MapToPort, SecretKey key, GFWPressWorkMode mode) : this(Address, Port, new IPEndPoint(MapToAddress, MapToPort))
        {
            _key = key;
            _mode = mode;
        }
        private GFWPressListener(IPAddress Address, int Port, IPEndPoint MapToIP) : base(Port, Address) {
            MapTo = MapToIP;
        }

        public override void OnAccept(IAsyncResult ar)
        {
            try
            {
                Socket NewSocket = ListenSocket.EndAccept(ar);
                if (NewSocket != null)
                {
                    Client NewClient = null;
                    if (_mode == GFWPressWorkMode.Client)
                    {
                        NewClient = new GFWPressClient(NewSocket, new DestroyDelegate(this.RemoveClient), MapTo, _key);
                    }
                    else if(_mode == GFWPressWorkMode.Server)
                    {
                        NewClient = new GFWPressServer(NewSocket, new DestroyDelegate(this.RemoveClient), MapTo, _key);
                    }
                    AddClient(NewClient);
                    NewClient.StartHandshake();
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
            return string.Empty;
        }
    }
}
