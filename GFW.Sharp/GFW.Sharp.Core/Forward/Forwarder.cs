using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GFW.Sharp.Core.Forward
{
    /// <summary>References the callback method to be called when the <c>Client</c> object disconnects from the local client and the remote server.</summary>
    /// <param name="client">The <c>Client</c> that has closed its connections.</param>
    public delegate void DestroyDelegate(Forwarder client);

    ///<summary>Specifies the basic methods and properties of a <c>Client</c> object. This is an abstract class and must be inherited.</summary>
    ///<remarks>The Client class provides an abstract base class that represents a connection to a local client and a remote server. Descendant classes further specify the protocol that is used between those two connections.</remarks>
    public abstract class Forwarder : IDisposable
    {
        // private variables
        /// <summary>Holds the address of the method to call when this client is ready to be destroyed.</summary>
        private DestroyDelegate Destroyer;
        /// <summary>Holds the value of the ClientSocket property.</summary>
        private Socket m_ClientSocket;
        /// <summary>Holds the value of the DestinationSocket property.</summary>
        private Socket m_DestinationSocket;
        /// <summary>Holds the value of the Buffer property.</summary>
        private byte[] m_Buffer = new byte[64*1024]; //0<->4095 = 4096
        ///<summary>Initializes a new instance of the Client class.</summary>
        ///<param name="ClientSocket">The <see cref ="Socket">Socket</see> connection between this proxy server and the local client.</param>
        ///<param name="Destroyer">The callback method to be called when this Client object disconnects from the local client and the remote server.</param>
        public Forwarder(Socket ClientSocket, DestroyDelegate Destroyer)
        {
            this.ClientSocket = ClientSocket;
            this.Destroyer = Destroyer;
        }
        ///<summary>Initializes a new instance of the Client object.</summary>
        ///<remarks>Both the ClientSocket property and the DestroyDelegate are initialized to null.</remarks>
        public Forwarder()
        {
            this.ClientSocket = null;
            this.Destroyer = null;
        }
        ///<summary>Gets or sets the Socket connection between the proxy server and the local client.</summary>
        ///<value>A Socket instance defining the connection between the proxy server and the local client.</value>
        ///<seealso cref ="DestinationSocket"/>
        internal Socket ClientSocket
        {
            get
            {
                return m_ClientSocket;
            }
            set
            {
                if (m_ClientSocket != null)
                    m_ClientSocket.Close();
                m_ClientSocket = value;
            }
        }
        ///<summary>Gets or sets the Socket connection between the proxy server and the remote host.</summary>
        ///<value>A Socket instance defining the connection between the proxy server and the remote host.</value>
        ///<seealso cref ="ClientSocket"/>
        internal Socket DestinationSocket
        {
            get
            {
                return m_DestinationSocket;
            }
            set
            {
                if (m_DestinationSocket != null)
                    m_DestinationSocket.Close();
                m_DestinationSocket = value;
            }
        }
        ///<summary>Gets the buffer to store all the incoming data from the local client.</summary>
        ///<value>An array of bytes that can be used to store all the incoming data from the local client.</value>
        ///<seealso cref ="RemoteBuffer"/>
        protected byte[] Buffer
        {
            get
            {
                return m_Buffer;
            }
        }
        ///<summary>Disposes of the resources (other than memory) used by the Client.</summary>
        ///<remarks>Closes the connections with the local client and the remote host. Once <c>Dispose</c> has been called, this object should not be used anymore.</remarks>
        ///<seealso cref ="System.IDisposable"/>
        public void Dispose()
        {
            try
            {
                ClientSocket.Shutdown(SocketShutdown.Both);
            }
            catch { }
            try
            {
                DestinationSocket.Shutdown(SocketShutdown.Both);
            }
            catch { }
            //Close the sockets
            if (ClientSocket != null)
                ClientSocket.Close();
            if (DestinationSocket != null)
                DestinationSocket.Close();
            //Clean up
            ClientSocket = null;
            DestinationSocket = null;
            if (Destroyer != null)
                Destroyer(this);
            m_Buffer = null;
        }
        ///<summary>Returns text information about this Client object.</summary>
        ///<returns>A string representing this Client object.</returns>
        public override string ToString()
        {
            try
            {
                return "Incoming connection from " + ((IPEndPoint)DestinationSocket.RemoteEndPoint).Address.ToString();
            }
            catch
            {
                return "Client connection";
            }
        }
        ///<summary>Starts relaying data between the remote host and the local client.</summary>
        ///<remarks>This method should only be called after all protocol specific communication has been finished.</remarks>
        public abstract void StartForward();
        
        
    }
}
