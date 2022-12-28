using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using AO.Core.Logging;

namespace AO.Network.Server
{
    public sealed class Tcp
    {
        private static readonly LoggerAdapter log = new(typeof(Tcp));

        public TcpClient Socket { get; private set; }
        public IPEndPoint RemoteIPEndPoint { get; private set; }

        private NetworkStream stream;
        private Packet receivedData;
        private byte[] receiveBuffer;

        private readonly Client client;
        private readonly ServerOptions options;

        public Tcp(Client client, ServerOptions options)
        {
            this.client = client;
            this.options = options;
        }

        public void Connect(TcpClient socket)
        {
            try
            {
                log.Debug("Connecting TCP client...");

                Socket = socket;
                Socket.ReceiveBufferSize = options.ReceiveDataBufferSize;
                Socket.SendBufferSize = options.SendDataBufferSize;
                Socket.ReceiveTimeout = options.ReceiveDataTimeout;
                Socket.SendTimeout = options.SendDataTimeout;

                stream = Socket.GetStream();

                receivedData = new Packet();
                receiveBuffer = new byte[options.ReceiveDataBufferSize];

                RemoteIPEndPoint = (IPEndPoint)Socket.Client.RemoteEndPoint;

                stream.BeginRead(receiveBuffer, 0, options.ReceiveDataBufferSize, ReceiveCallback, null);

                log.Debug("Successfully connected client through TCP.");
            }
            catch (Exception ex)
            {
                log.Error("There was an error trying to establish a TCP connection to the client with id: {0}. {1}", client.Id, ex.Message);
                client.Disconnect();
            }
        }

        public void Disconnect()
        {
            Socket?.Client.Shutdown(SocketShutdown.Both);
            Socket = null;

            receivedData?.Dispose();
            receivedData = null;

            receiveBuffer = null;

            log.Info("TCP client socket has been disconnected and closed.");
        }

        public void SendData(Packet packet)
        {
            try
            {
                packet.WriteLength();
                stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
            }
            catch (Exception ex)
            {
                log.Error("There was an error trying to send TCP data to the client with id: {0}. The client will be fully disconnected. {1}", client.Id, ex.Message);
                client.Disconnect();
            }
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                int byteLength = stream.EndRead(result);
                
                if (byteLength <= 0)
                {
                    log.Warn("The received TCP data contains no bytes. Disconnecting client...");
                    client.Disconnect();
                    return;
                }

                byte[] data = new byte[byteLength];
                Array.Copy(receiveBuffer, data, byteLength);

                LogPacketData(data);
                TcpDataHandler.HandleData(client, data, receivedData, options.DataReceivedCallback);
                receivedData.Reset();
                stream.BeginRead(receiveBuffer, 0, options.ReceiveDataBufferSize, ReceiveCallback, null);
            }
            catch (System.IO.IOException)
            {
                log.Info("The socket has been closed by the client.");
                client.Disconnect();
            }
            catch (Exception ex)
            {
                log.Error("There was an error trying to receive TCP data from the client with id: {0}. The client will be fully disconnected. {1}", client.Id, ex.Message);
                client.Disconnect();
            }
        }
        
        [Conditional("AO_LOG_PACKETS")]
        [Conditional("AO_LOG_NETWORKING")]
        public static void LogPacketId(ClientPackets packetId)
        {
            log.Debug("Received TCP packet: {0}.", packetId);
        }
        
        [Conditional("AO_LOG_PACKETS")]
        [Conditional("AO_LOG_NETWORKING")]
        public static void LogPacketId(ServerPackets packetId)
        {
            log.Debug("Sending TCP packet: {0}.", packetId);
        }

        [Conditional("AO_LOG_NETWORKING")]
        private static void LogPacketData(byte[] data)
        {
            log.Debug($"Received raw data is [{BitConverter.ToString(data).Replace("-", "")}].");
        }
    }
}
