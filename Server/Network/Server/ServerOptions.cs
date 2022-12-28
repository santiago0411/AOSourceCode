using System;

namespace AO.Network.Server
{
    public sealed class ServerOptions
    {
        /// <summary>(OPTIONAL) The IP addresses type to listen to incoming connections on.</summary>
        public System.Net.IPAddress IPAddress { get; set; } = System.Net.IPAddress.Any;

        /// <summary>The port to listen on.</summary>
        public ushort Port { get; set; } = 0;

        /// <summary>(OPTIONAL) Maximum number of clients able to connect to the server at the same time. The default value is 100000.</summary>
        public int MaxClients { get; set; } = 100000;

        /// <summary>(OPTIONAL) The size of the socket receive buffer. The default value is 8192 bytes.</summary>
        public int ReceiveDataBufferSize { get; set; } = 8192;

        /// <summary>(OPTIONAL) The size of the socket send buffer. The default value is 8192 bytes.</summary>
        public int SendDataBufferSize { get; set; } = 8192;

        /// <summary>(OPTIONAL) The time in MILLISECONDS after the receive data operation will time out. The default value is 0 which means no timeout.</summary>
        public int ReceiveDataTimeout { get; set; } = 0;

        /// <summary>(OPTIONAL) The time in MILLISECONDS after the send data operation will time out. The default value is 0 which means no timeout.</summary>
        public int SendDataTimeout { get; set; } = 0;

        /// <summary>The protocol(s) to use. The default value is both Tcp and Udp.</summary>
        public ServerProtocol Protocol { get; set; } = ServerProtocol.Both;

        /// <summary>Callback to execute when a new packet is received from a client. Arg1 is the client. Arg2 is the received data.</summary>
        public Action<Client, Packet> DataReceivedCallback { get; set; } = null;

        /// <summary>(OPTIONAL) Callback to execute when a new client connects to the server. Arg1 is the client. Returning false will refuse the connection and disconnect the client.</summary>
        public Func<Client, bool> ClientConnectedCallback { get; set; } = null;

        /// <summary>(OPTIONAL) Callback to execute after a new client has been accepted and a connection has been establish. Arg1 is the accepted client.</summary>
        public Action<Client> ClientConnectionEstablishedCallback { get; set; } = null;

        /// <summary>(OPTIONAL) Callback to execute when a client disconnects or is disconnected from the server. Arg1 is the client. Arg2 is the protocol that got disconnected.</summary>
        public Action<Client, ServerProtocol> ClientDisconnectedCallback { get; set; } = null;

        /// <summary>(OPTIONAL) Callback to execute everytime a client joins but the server has reached it maximum capacity.</summary>
        public Action ServerIsFullCallback { get; set; } = null;
    }
}
