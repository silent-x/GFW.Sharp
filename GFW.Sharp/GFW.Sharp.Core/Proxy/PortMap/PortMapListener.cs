using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GFW.Sharp.Core.Proxy.PortMap
{
    public sealed class PortMapListener : Listener
    {
        ///<summary>Initializes a new instance of the PortMapListener class.</summary>
        ///<param name="Port">The port to listen on.</param>
        ///<param name="MapToIP">The address to forward to.</param>
        ///<remarks>The object will listen on all network addresses on the computer.</remarks>
        ///<exception cref="ArgumentException"><paramref name="Port">Port</paramref> is not positive.</exception>
        ///<exception cref="ArgumentNullException"><paramref name="MapToIP">MapToIP</paramref> is null.</exception>
        public PortMapListener(int Port, IPEndPoint MapToIP) : this(IPAddress.Any, Port, MapToIP) { }
        ///<summary>Initializes a new instance of the PortMapListener class.</summary>
        ///<param name="Port">The port to listen on.</param>
        ///<param name="Address">The network address to listen on.</param>
        ///<param name="MapToIP">The address to forward to.</param>
        ///<remarks>For security reasons, <paramref name="Address">Address</paramref> should not be IPAddress.Any.</remarks>
        ///<exception cref="ArgumentNullException">Address or <paramref name="MapToIP">MapToIP</paramref> is null.</exception>
        ///<exception cref="ArgumentException">Port is not positive.</exception>
        public PortMapListener(IPAddress Address, int Port, IPEndPoint MapToIP) : base(Port, Address)
        {
            MapTo = MapToIP;
        }
        ///<summary>Initializes a new instance of the PortMapListener class.</summary>
        ///<param name="Port">The port to listen on.</param>
        ///<param name="Address">The network address to listen on.</param>
        ///<param name="MapToPort">The port to forward to.</param>
        ///<param name="MapToAddress">The IP address to forward to.</param>
        ///<remarks>For security reasons, Address should not be IPAddress.Any.</remarks>
        ///<exception cref="ArgumentNullException">Address or MapToAddress is null.</exception>
        ///<exception cref="ArgumentException">Port or MapToPort is invalid.</exception>
        public PortMapListener(IPAddress Address, int Port, IPAddress MapToAddress, int MapToPort) : this(Address, Port, new IPEndPoint(MapToAddress, MapToPort)) { }
        ///<summary>Gets or sets the IP EndPoint to map all incoming traffic to.</summary>
        ///<value>An IPEndPoint that holds the IP address and port to use when redirecting incoming traffic.</value>
        ///<exception cref="ArgumentNullException">The specified value is null.</exception>
        ///<returns>An IP EndPoint specifying the host and port to map all incoming traffic to.</returns>
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
        ///<summary>Called when there's an incoming client connection waiting to be accepted.</summary>
        ///<param name="ar">The result of the asynchronous operation.</param>
        public override void OnAccept(IAsyncResult ar)
        {
            try
            {
                Socket NewSocket = ListenSocket.EndAccept(ar);
                if (NewSocket != null)
                {
                    PortMapClient NewClient = new PortMapClient(NewSocket, new DestroyDelegate(this.RemoveClient), MapTo);
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
        ///<summary>Returns a string representation of this object.</summary>
        ///<returns>A string with information about this object.</returns>
        public override string ToString()
        {
            return "PORTMAP service on " + Address.ToString() + ":" + Port.ToString();
        }
        ///<summary>Returns a string that holds all the construction information for this object.</summary>
        ///<value>A string that holds all the construction information for this object.</value>
        public override string ConstructString
        {
            get
            {
                return "host:" + Address.ToString() + ";int:" + Port.ToString() + ";host:" + MapTo.Address.ToString() + ";int:" + MapTo.Port.ToString();
            }
        }
        // private variables
        /// <summary>Holds the value of the MapTo property.</summary>
        private IPEndPoint m_MapTo;
    }
}
