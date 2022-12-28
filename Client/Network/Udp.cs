using System;
using System.Net;
using System.Net.Sockets;
using AOClient.Core.Utils;

namespace AOClient.Network
{
    public class Udp
    {
        private UdpClient socket;
        private IPEndPoint endPoint;

        public Udp()
        {
            endPoint = new IPEndPoint(IPAddress.Parse(Client.Instance.Ip), Client.Instance.Port);
        }

        /// <summary>Attempts to connect to the server via UDP.</summary>
        public void Connect(int localPort, int extra = 0)
        {
            try
            {
                DebugLogger.Debug($"Connecting UDP to port {localPort + extra}.");
                socket = new UdpClient(localPort + extra);

                socket.Connect(endPoint);

                socket.BeginReceive(ReceiveCallback, null);

                using var packet = new Packet();
                SendData(packet);
                DebugLogger.Debug("UDP connection established successfully.");
            }
            catch (SocketException)
            {
                // Happens when the port is still in use and hasn't been properly disconnected
                if (extra == 0)
                    Connect(localPort + UnityEngine.Random.Range(1, 10));
            }
            catch (Exception ex)
            {
                DebugLogger.Error($"Error establishing UDP connection: {ex}");
                Client.Instance.Disconnect();
            }
        }

        /// <summary>Disconnects from the server and cleans up the UDP connection.</summary>
        public void Disconnect()
        {
            if (socket is not null && socket.Client.Connected)
                socket.Client.Shutdown(SocketShutdown.Both);
            
            socket = null;
        }

        /// <summary>Sends data to the client via UDP.</summary>
        /// <param name="packet">The packet to send.</param>
        public void SendData(Packet packet)
        {
            try
            {
                packet.InsertClientId(Client.Instance.MyId);
                socket?.BeginSend(packet.ToArray(), packet.Length(), null, null);
            }
            catch (Exception ex)
            {
                DebugLogger.Error($"Error sending data to server via UDP: {ex}");
                Client.Instance.Disconnect();
            }
        }

        /// <summary>Receives incoming UDP data.</summary>
        private void ReceiveCallback(IAsyncResult result)
        {
            byte[] data;
            
            try
            {
                data = socket.EndReceive(result, ref endPoint);
                socket.BeginReceive(ReceiveCallback, null);

                if (data.Length < 4)
                {
                    Client.Instance.Disconnect();
                    return;
                }
            }
            catch (ObjectDisposedException)
            {
                //Client.Instance.Disconnect();
                //Debug.LogError($"Udp exception: {ex}");
                return;
            }
            
            HandleData(data);
        }

        /// <summary>Prepares received data to be used by the appropriate packet handler methods.</summary>
        /// <param name="data">The received data.</param>
        private static void HandleData(byte[] data)
        {
            using (var packet = new Packet(data))
            {
                var packetLength = packet.ReadInt();
                data = packet.ReadBytes(packetLength);
            }

            ThreadManager.ExecuteOnMainThread(() =>
            {
                using var packet = new Packet(data);
                var packetId = (ServerPackets)packet.ReadShort();
                Client.PacketHandlers[packetId](packet);
            });
        }
    }
}