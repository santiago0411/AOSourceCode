using System;
using AO.Core.Logging;

namespace AO.Network.Server
{
    public static class TcpDataHandler
    {
        private static readonly LoggerAdapter log = new(typeof(TcpDataHandler));
        
        public static void HandleData(Client client, byte[] data, Packet receivedData, Action<Client, Packet> serverDataReceivedCallback)
        {
            receivedData.SetBytes(data);

            if (receivedData.UnreadLength() < 4)
            {
                log.Warn("Failed to read packet length, data does not have enough bytes. Packet will not be processed.");
                return;
            }

            // If it's less than 1 reset packet data because all bytes have been read
            int packetLength = receivedData.ReadInt();
            if (packetLength <= 0)
                return;

            // Read as long as receivedData contains another packet that can be handled
            while (packetLength > 0 && packetLength <= receivedData.UnreadLength())
            {
                byte[] packetBytes = receivedData.ReadBytes(packetLength);

                // Handle the packet on the main thread
                ThreadManager.ExecuteOnMainThread((state) =>
                {
                    var (callback, cnt, bytes) = ((Action<Client, Packet>, Client, byte[]))state;
                    var packet = new Packet(bytes);
                    callback(cnt, packet);
                }, (serverDataReceivedCallback, client, packetBytes));

                packetLength = 0;
                
                // If it has more than 4 unread bytes it means it's the start of a packet
                if (receivedData.UnreadLength() >= 4)
                {
                    packetLength = receivedData.ReadInt();
                    if (packetLength <= 0) 
                        return;
                }
            }
        }
    }
}
