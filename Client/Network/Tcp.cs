using System;
using System.Diagnostics;
using System.Net.Sockets;
using AOClient.Core;
using AOClient.Core.Utils;
using AOClient.UI;

namespace AOClient.Network
{
    public class Tcp
    {
        public TcpClient Socket { get; private set; }
        public bool IsConnected => Socket is { Connected: true };

        private bool connecting;
        private NetworkStream stream;
        private Packet receivedData;
        private byte[] receiveBuffer;
        
        private const int DATA_BUFFER_SIZE = 4096;

        public void Connect(Action onConnected)
        {
            if (connecting)
                return;

            connecting = true;
            
            Socket = new TcpClient
            {
                ReceiveBufferSize = DATA_BUFFER_SIZE,
                SendBufferSize = DATA_BUFFER_SIZE,
            };

            receiveBuffer = new byte[DATA_BUFFER_SIZE];

            try
            {
                DebugLogger.Debug($"Connecting TCP to {Client.Instance.Ip}:{Client.Instance.Port}.");
                Socket.BeginConnect(Client.Instance.Ip, Client.Instance.Port, ConnectCallback, onConnected);
            }
            catch (Exception ex)
            {
                ThreadManager.ExecuteOnMainThread(() => UIManager.LoginRegister.ShowPopupWindow(Constants.COULD_NOT_CONNECT_TO_SERVER));
                DebugLogger.Error($"Error opening a connection to the server: {ex.Message}");
            }
        }
        
        /// <summary>Disconnects from the server and cleans up the TCP connection.</summary>
        public void Disconnect()
        {
            Socket?.Client.Shutdown(SocketShutdown.Both);
            Socket = null;
            
            receivedData?.Dispose();
            receivedData = null;
            
            receiveBuffer = null;
            
            DebugLogger.Debug("Tcp socket has been disconnected and closed.");
        }

        /// <summary>Sends data to the client via TCP.</summary>
        /// <param name="packet">The packet to send.</param>
        public void SendData(Packet packet)
        {
            try
            {
                LogPacketId(packet.PacketId);
                stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
            }
            catch (Exception ex)
            {
                DebugLogger.Error($"Error sending data to server via TCP: {ex}");
                Client.Instance.Disconnect();
            }
        }

        private void ConnectCallback(IAsyncResult result)
        {
            try
            {
                connecting = false;
                Socket.EndConnect(result);

                stream = Socket.GetStream();
                receivedData = new Packet();

                stream.BeginRead(receiveBuffer, 0, DATA_BUFFER_SIZE, ReceiveCallback, null);
                DebugLogger.Debug("TCP connection established successfully.");

                var onConnected = result.AsyncState as Action;
                onConnected?.Invoke();
            }
            catch (Exception ex)
            {
                ThreadManager.ExecuteOnMainThread(() => UIManager.LoginRegister.ShowPopupWindow(Constants.COULD_NOT_CONNECT_TO_SERVER));
                DebugLogger.Error($"Error establishing a connection to the server: {ex.Message}");
            }
        }

        /// <summary>Reads incoming data from the stream.</summary>
        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                int byteLength = stream.EndRead(result);
                if (byteLength <= 0)
                {
                    Client.Instance.Disconnect();
                    return;
                }

                byte[] data = new byte[byteLength];
                Array.Copy(receiveBuffer, data, byteLength);

                // Whether or not the received data packet gets reset depends on the value returned by HandleData
                // Handles packets splits and avoids data loss in case the data was sent in multiple packets
                receivedData.Reset(HandleData(data));
                stream.BeginRead(receiveBuffer, 0, DATA_BUFFER_SIZE, ReceiveCallback, null);
            }
            catch (System.IO.IOException)
            {
                DebugLogger.Error("The socket has been closed by the server.");
                Client.Instance.Disconnect();
            }
            catch (Exception ex)
            {
                DebugLogger.Error($"Error receiving TCP data: {ex}");
                Client.Instance.Disconnect();
            }
        }

        /// <summary>Prepares received data to be used by the appropriate packet handler methods.</summary>
        /// <param name="data">The received data.</param>
        private bool HandleData(byte[] data)
        {
            LogPacketData(data);
            receivedData.SetBytes(data);
            
            if (receivedData.UnreadLength() < 4)
            {
                DebugLogger.Warn("Failed to read packet length, data does not have enough bytes. Packet will not be processed.");
                return true;
            }
            
            // If it's less than 1 reset packet data because all bytes have been read
            int packetLength = receivedData.ReadInt();
            if (packetLength <= 0) return true;

            // Read as long as receivedData contains another packet that can be handled
            while (packetLength > 0 && packetLength <= receivedData.UnreadLength())
            {
                byte[] packetBytes = receivedData.ReadBytes(packetLength);
                // Handle the packet on the main thread
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using var packet = new Packet(packetBytes);
                    var packetId = (ServerPackets)packet.ReadShort();
                    LogPacketId(packetId);
                    Client.PacketHandlers[packetId](packet);
                });

                packetLength = 0;
                
                // If it has more than 4 unread bytes it means it's the start of a packet
                if (receivedData.UnreadLength() >= 4)
                {
                    packetLength = receivedData.ReadInt();
                    if (packetLength <= 0) return true;
                }
            }

            return true;
        }
        
        [Conditional("AO_LOG_PACKETS")]
        [Conditional("AO_LOG_NETWORKING")]
        private static void LogPacketId(ClientPackets packetId)
        {
            DebugLogger.Debug($"Received TCP packet: {packetId}.");
        }
        
        [Conditional("AO_LOG_PACKETS")]
        [Conditional("AO_LOG_NETWORKING")]
        private static void LogPacketId(ServerPackets packetId)
        {
            DebugLogger.Debug($"Sending TCP packet: {packetId}.");
        }

        [Conditional("AO_LOG_NETWORKING")]
        private static void LogPacketData(byte[] data)
        {
            DebugLogger.Debug($"Received raw data is [{BitConverter.ToString(data).Replace("-", "")}].");
        }
    }
}