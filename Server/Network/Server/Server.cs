using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using AO.Core.Ids;
using AO.Core.Logging;

namespace AO.Network.Server
{
    /// <summary>The protocols the server can use.</summary>
    public enum ServerProtocol { Tcp, Udp, Both }

    public sealed class Server
    {
        private static readonly LoggerAdapter log = new(typeof(Server));
        
        private TcpListener tcpListener;
        private UdpClient udpListener;
        private Thread internalThread;

        private readonly ServerOptions options;
        private readonly Dictionary<ClientId, Client> clients = new();
        private readonly ServerClientsIdAssigner idAssigner;

        public Server(ServerOptions options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options), "ServerOptions was null.");

            if (options.IPAddress is null)
                throw new ArgumentNullException(nameof(options.IPAddress),"IPAddress was null.");

            if (options.DataReceivedCallback is null)
                throw new ArgumentNullException(nameof(options.DataReceivedCallback),"DataReceivedCallback was null.");

            if (options.ReceiveDataBufferSize <= 0)
                throw new Exception("The ReceiveDataBufferSize value cannot be smaller than 1.");

            if (options.SendDataBufferSize <= 0)
                throw new Exception("The SendDataBufferSize value cannot be smaller than 1.");

            if (options.ReceiveDataTimeout < 0)
                throw new Exception("The ReceiveDataTimeout value cannot be smaller than 0.");

            if (options.SendDataTimeout < 0)
                throw new Exception("The SendDataTimeout value cannot be smaller than 0.");

            if ((int)options.Protocol > 2)
                throw new Exception("Protocol does not contain a valid option.");

            this.options = options;
            idAssigner = new ServerClientsIdAssigner(this.options.MaxClients);
        }

        private static string ProtocolToString(ServerProtocol protocol)
        {
            return protocol switch
            {
                ServerProtocol.Tcp => "Tcp",
                ServerProtocol.Udp => "Udp",
                ServerProtocol.Both => "Tcp & Udp",
                _ => ""
            };
        }
        
        /// <summary>Launches the main thread and prompts the server to start listening for connections using the selected protocols.</summary>
        public void Listen()
        {
            internalThread = new Thread(() =>
            {
                log.Info("Starting server using protocol: '{0}' on port: {1}.",ProtocolToString(options.Protocol) , options.Port);

                if (options.Protocol != ServerProtocol.Udp)
                {
                    log.Info("Starting TCP listener...");
                    tcpListener = new TcpListener(options.IPAddress, options.Port);
                    tcpListener.Start();
                    tcpListener.BeginAcceptTcpClient(TcpConnectCallback, null);
                }

                if (options.Protocol != ServerProtocol.Tcp)
                {
                    log.Info("Starting UDP listener...");
                    udpListener = new UdpClient(options.Port);
                    udpListener.BeginReceive(UdpReceiveCallback, null);
                }
            });
            internalThread.Start();
        }

        /// <summary>Stops both the TCP and UDP listeners.</summary>
        public void Stop()
        {
            log.Info("Disconnecting all clients.");

            var clientsList = new List<Client>(clients.Values);
            
            foreach (var client in clientsList)
            {
                client.Disconnect(true);
                idAssigner.FreeId(client.Id);
            }

            clients.Clear();

            log.Info("Stopping TCP and UDP listeners...");
            tcpListener?.Stop();
            udpListener?.Close();
            log.Info("TCP and UDP listeners have been stopped.");
            
            log.Debug("Joining internal thread...");
            internalThread.Join();
        }

        /// <summary>Sends a packet via TCP.</summary>
        /// <param name="toClient">The client to which the packet should be sent.</param>
        /// <param name="packet">The packet to send.</param>
        public void SendPacketTcp(ClientId toClient, Packet packet)
        {
            if (options.Protocol == ServerProtocol.Udp)
                throw new Exception("Cannot send TCP data the server protocol is set to UDP only.");

            if (clients.TryGetValue(toClient, out Client client))
            {
                if (!client.IsConnectedTcp())
                {
                    log.Error("Cannot send TCP data to client: {0} because it doesn't have an active TCP connection.", toClient);
                    return;
                }

                client.Tcp.SendData(packet);
            }
        }

        /// <summary>Sends a packet via TCP to all the connected clients.</summary>
        /// <param name="packet">The packet to send.</param>
        public void SendPacketTcpToAll(Packet packet)
        {
            if (options.Protocol == ServerProtocol.Udp)
                throw new Exception("Cannot send TCP data the server protocol is set to UDP only.");

            foreach (var client in clients.Values)
            {
                if (client.IsConnectedTcp())
                    client.Tcp.SendData(packet);
            }
        }

        /// <summary>Sends a packet via TCP to all the connected clients except a specific one.</summary>
        /// <param name="packet">The packet to send.</param>
        /// <param name="exceptClient">The client to skip sending the data.</param>
        public void SendPacketTcpToAll(Packet packet, ClientId exceptClient)
        {
            if (options.Protocol == ServerProtocol.Udp)
                throw new Exception("Cannot send TCP data the server protocol is set to UDP only.");

            foreach (var client in clients.Values)
            {
                if (client.IsConnectedTcp() && client.Id != exceptClient)
                    client.Tcp.SendData(packet);
            }
        }

        /// <summary>Sends a packet via UDP.</summary>
        /// <param name="toClient">The client to which the packet should be sent.</param>
        /// <param name="packet">The packet to send.</param>
        public void SendPacketUdp(ClientId toClient, Packet packet)
        {
            if (options.Protocol == ServerProtocol.Tcp)
                throw new Exception("Cannot send UDP data the server protocol is set to TCP only.");

            if (clients.TryGetValue(toClient, out Client client))
                client.Udp.SendData(packet);
        }

        /// <summary>Sends a packet via UDP to all the connected clients.</summary>
        /// <param name="packet">The packet to send.</param>
        public void SendPacketUdpToAll(Packet packet)
        {
            if (options.Protocol == ServerProtocol.Tcp)
                throw new Exception("Cannot send UDP data the server protocol is set to TCP only.");

            foreach (var client in clients.Values)
                client.Udp.SendData(packet);
        }

        /// <summary>Sends a packet via TCP to all the connected clients except a specific one.</summary>
        /// <param name="packet">The packet to send.</param>
        /// <param name="exceptClient">The client to skip sending the data.</param>
        public void SendPacketUdpToAll(Packet packet, ClientId exceptClient)
        {
            if (options.Protocol == ServerProtocol.Tcp)
                throw new Exception("Cannot send UDP data the server protocol is set to TCP only.");

            foreach (var client in clients.Values)
            {
                if (client.Id != exceptClient)
                    client.Udp.SendData(packet);
            }
        }

        private void RemoveClient(ClientId id)
        {
            clients.Remove(id);
            idAssigner.FreeId(id);
        }

        private void TcpConnectCallback(IAsyncResult result)
        {
            log.Debug("New TCP connection received.");

            TcpClient tcpClient;

            try
            {
                tcpClient = tcpListener.EndAcceptTcpClient(result);
                tcpListener.BeginAcceptTcpClient(TcpConnectCallback, null);
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            if (!idAssigner.FindAvailableId(out ClientId id))
            {
                log.Warn("Could not find an available id to create a client. Server is full!");
                options.ServerIsFullCallback?.Invoke();
                tcpClient.Close();
                tcpClient.Dispose();
                return;
            }

            var client = new Client(id, options, udpListener, RemoveClient);

            if (ApproveNewConnection(client))
            {
                FinishProcessingAcceptedTcpClient(client, tcpClient);
                return;
            }

            tcpClient.Close();
            tcpClient.Dispose();
            log.Info("The new client has been refused by the ClientConnectCallback and the connection has been closed.");
        }

        private bool ApproveNewConnection(Client client)
        {
            bool? accepted = options.ClientConnectedCallback?.Invoke(client);
            return accepted ?? true;
        }

        private void FinishProcessingAcceptedTcpClient(Client client, TcpClient tcpClient)
        {
            client.Tcp.Connect(tcpClient);
            clients.Add(client.Id, client);
            options.ClientConnectionEstablishedCallback?.Invoke(client);
        }

        private void UdpReceiveCallback(IAsyncResult result)
        {
            IPEndPoint clientEndPoint;
            byte[] data;

            try
            {
                clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                data = udpListener.EndReceive(result, ref clientEndPoint);
                udpListener.BeginReceive(UdpReceiveCallback, null);
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            if (data.Length < 4)
            {
                log.Warn("Failed to read client id, data does not have enough bytes. Packet will not be processed.");
                return;
            }

            using var packet = new Packet(data);
            int clientId = packet.ReadInt();

            if (!clients.TryGetValue(clientId, out Client client))
            {
                log.Warn("{0} sent an id: '{1}' that doesn't match any client.", clientEndPoint.ToString(), clientId);
                return;
            }
            
            if (!client.IsConnectedTcp())
            {
                log.Warn("{0} is trying to establish a UDP connection impersonating client with id: {1} but that client doesn't have an active TCP connection.", clientEndPoint.ToString(), clientId);
                return;
            }

            if (client.Udp.EndPoint is null)
            {
                if (!client.Tcp.RemoteIPEndPoint.Address.Equals(clientEndPoint.Address))
                {
                    log.Warn("{0} is trying to establish a UDP connection impersonating client: {1} with IP: {2}.", clientEndPoint.ToString(), client.Id, client.Tcp.RemoteIPEndPoint.Address.ToString());
                    return;
                }

                client.Udp.Connect(clientEndPoint);
                return;
            }

            if (client.Udp.EndPoint.Equals(clientEndPoint))
            {
                client.Udp.HandleData(packet);
                return;
            }

            log.Warn("{0} tried to impersonate another client by sending a false id: {1}.", clientEndPoint.ToString(), clientId);
        }

        private class ServerClientsIdAssigner
        {
            private readonly ClientId[] locks;

            public ServerClientsIdAssigner(int maxClients)
            {
                locks = new ClientId[maxClients];
            }
            
            public bool FindAvailableId(out ClientId id)
            {
                for (int i = 1; i <= locks.Length; i++)
                {
                    ref ClientId lck = ref locks[i - 1];
                    
                    if (lck == ClientId.Empty)
                    {
                        lck = 1;
                        id = i;
                        return true;
                    }
                }
                
                id = default;
                return false;
            }

            public void FreeId(ClientId id)
            {
                locks[id.AsPrimitiveType() - 1] = ClientId.Empty;
            }
        }
    }
}
