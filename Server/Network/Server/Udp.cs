using System;
using System.Net;
using System.Net.Sockets;
using AO.Core.Logging;

namespace AO.Network.Server
{
    public sealed class Udp
    {
        private static readonly LoggerAdapter log = new(typeof(Udp));

        public IPEndPoint EndPoint { get; private set; }

        private readonly Client client;
        private readonly ServerOptions options;
        private readonly UdpClient udpListener;

        public Udp(Client client, ServerOptions options, UdpClient udpListener)
        {
            this.client = client;
            this.options = options;
            this.udpListener = udpListener;
        }

        public void Connect(IPEndPoint clientEndpoint)
        {
            EndPoint = clientEndpoint;
        }

        public void Disconnect()
        {
            EndPoint = null;
            log.Info("UDP EndPoint disconnected.");
        }

        public void SendData(Packet packet)
        {
            try
            {
                packet.WriteLength();

                if (EndPoint is not null)
                    udpListener.BeginSend(packet.ToArray(), packet.Length(), EndPoint, null, null);
            }
            catch (Exception ex)
            {
                log.Error("There was an error trying to send UDP data to the client with id: {0}. {1}", client.Id, ex.Message);
            }
        }

        public void HandleData(Packet receivedData)
        {
            int packetLength = receivedData.ReadInt();
            if (packetLength <= 0) return;

            if (packetLength > 500)
            {
                log.Warn($"Client {client.Id} ({client.Tcp.RemoteIPEndPoint}): sent a udp packet with more than 500 bytes.");
                return;
            }

            byte[] packetBytes = receivedData.ReadBytes(packetLength);
            
            ThreadManager.ExecuteOnMainThread((state) =>
            {
                var (bytes, udp) = ((byte[], Udp))state;
                using var packet = new Packet(bytes);
                udp.options.DataReceivedCallback(udp.client, packet);
            }, (packetBytes, this));
        }
    }
}
