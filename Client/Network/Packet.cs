using System;
using System.Collections.Generic;
using System.Text;
using AO.Core.Ids;
using UnityEngine;

namespace AOClient.Network
{
    public sealed class Packet : IDisposable
    {
        public ClientPackets PacketId { get; }
        
        private List<byte> buffer;
        private byte[] readableBuffer;
        private int readPos;
        private bool lengthWritten;

        /// <summary>Creates a new empty packet (without an ID).</summary>
        public Packet()
        {
            buffer = new List<byte>(); // Initialize buffer
            readPos = 0; // Set readPos to 0
        }

        /// <summary>Creates a new packet with a given ID. Used for sending.</summary>
        /// <param name="packetId">The packet ID.</param>
        public Packet(ClientPackets packetId)
        {
            buffer = new List<byte>(); // Initialize buffer
            readPos = 0; // Set readPos to 0
            PacketId = packetId;
            
            Write((short)packetId); // Write packet id to the buffer
        }

        /// <summary>Creates a packet from which data can be read. Used for receiving.</summary>
        /// <param name="data">The bytes to add to the packet.</param>
        public Packet(byte[] data)
        {
            buffer = new List<byte>(); // Initialize buffer
            readPos = 0; // Set readPos to 0

            SetBytes(data);
        }

        #region Functions
        /// <summary>Sets the packet's content and prepares it to be read.</summary>
        /// <param name="data">The bytes to add to the packet.</param>
        public void SetBytes(byte[] data)
        {
            buffer.AddRange(data);
            readableBuffer = buffer.ToArray();
        }

        /// <summary>Inserts the length of the packet's content at the start of the buffer.</summary>
        public void WriteLength()
        {
            if (!lengthWritten)
            {
                buffer.InsertRange(0, BitConverter.GetBytes(buffer.Count)); // Insert the byte length of the packet at the very beginning
                lengthWritten = true;
            }
        }

        /// <summary>Inserts the given ClientId at the start of the buffer.</summary>
        /// <param name="value">The ClientId to insert.</param>
        public void InsertClientId(ClientId value)
        {
            buffer.InsertRange(0, BitConverter.GetBytes(value.AsPrimitiveType())); // Insert the ClientId at the start of the buffer
        }

        /// <summary>Gets the packet's content in array form.</summary>
        public byte[] ToArray()
        {
            readableBuffer = buffer.ToArray();
            return readableBuffer;
        }

        /// <summary>Gets the length of the packet's content.</summary>
        public int Length()
        {
            return buffer.Count; // Return the length of buffer
        }

        /// <summary>Gets the length of the unread data contained in the packet.</summary>
        public int UnreadLength()
        {
            return Length() - readPos; // Return the remaining length (unread)
        }

        /// <summary>Resets the packet instance to allow it to be reused.</summary>
        /// <param name="shouldReset">Whether or not to reset the packet.</param>
        public void Reset(bool shouldReset = true)
        {
            if (shouldReset)
            {
                buffer.Clear(); // Clear buffer
                readableBuffer = null;
                readPos = 0; // Reset readPos
            }
            else
            {
                readPos -= 4; // "Unread" the last read int
            }
        }
        #endregion

        #region Write Data
        /// <summary>Adds a byte to the packet.</summary>
        /// <param name="value">The byte to add.</param>
        public void Write(byte value)
        {
            buffer.Add(value);
        }
        /// <summary>Adds a ushort to the packet.</summary>
        /// <param name="value">The short to add.</param>
        public void Write(ushort value)
        {
            buffer.AddRange(BitConverter.GetBytes(value));
        }
        /// <summary>Adds a short to the packet.</summary>
        /// <param name="value">The short to add.</param>
        public void Write(short value)
        {
            buffer.AddRange(BitConverter.GetBytes(value));
        }
        /// <summary>Adds a uint to the packet.</summary>
        /// <param name="value">The int to add.</param>
        public void Write(uint value)
        {
            buffer.AddRange(BitConverter.GetBytes(value));
        }
        /// <summary>Adds an int to the packet.</summary>
        /// <param name="value">The int to add.</param>
        public void Write(int value)
        {
            buffer.AddRange(BitConverter.GetBytes(value));
        }
        /// <summary>Adds a long to the packet.</summary>
        /// <param name="value">The long to add.</param>
        public void Write(long value)
        {
            buffer.AddRange(BitConverter.GetBytes(value));
        }
        /// <summary>Adds a float to the packet.</summary>
        /// <param name="value">The float to add.</param>
        public void Write(float value)
        {
            buffer.AddRange(BitConverter.GetBytes(value));
        }
        /// <summary>Adds a bool to the packet.</summary>
        /// <param name="value">The bool to add.</param>
        public void Write(bool value)
        {
            buffer.AddRange(BitConverter.GetBytes(value));
        }
        /// <summary>Adds a string to the packet.</summary>
        /// <param name="value">The string to add.</param>
        public void Write(string value)
        {
            byte[] stringBytes = Encoding.UTF8.GetBytes(value);
            Write((ushort)stringBytes.Length); // Add the length of the string to the packet
            buffer.AddRange(stringBytes); // Add the string itself
        }
        /// <summary>Adds a Vector2 to the packet.</summary>
        /// <param name="value">The Vector2 to add.</param>
        public void Write(Vector2 value)
        {
            Write(value.x);
            Write(value.y);
        }
        #endregion

        #region Read Data
        /// <summary>Reads a byte from the packet.</summary>
        /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
        public byte ReadByte(bool moveReadPos = true)
        {
            byte value = readableBuffer[readPos]; // Get the byte at readPos' position
            if (moveReadPos)
            {
                // If moveReadPos is true
                readPos += 1; // Increase readPos by 1
            }
            return value; // Return the byte
        }

        /// <summary>Reads an array of bytes from the packet.</summary>
        /// <param name="length">The length of the byte array.</param>
        /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
        public byte[] ReadBytes(int length, bool moveReadPos = true)
        {
            byte[] value = buffer.GetRange(readPos, length).ToArray(); // Get the bytes at readPos' position with a range of length
            if (moveReadPos)
            {
                // If moveReadPos is true
                readPos += length; // Increase readPos by length
            }
            return value; // Return the bytes
        }

        /// <summary>Reads a ushort from the packet.</summary>
        /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
        public ushort ReadUShort(bool moveReadPos = true)
        {
            return (ushort)ReadShort(moveReadPos);
        }

        /// <summary>Reads a short from the packet.</summary>
        /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
        public short ReadShort(bool moveReadPos = true)
        {
            short value = BitConverter.ToInt16(readableBuffer, readPos); // Convert the bytes to a short
            if (moveReadPos)
            {
                // If moveReadPos is true and there are unread bytes
                readPos += 2; // Increase readPos by 2
            }
            return value; // Return the short
        }

        /// <summary>Reads a uint from the packet.</summary>
        /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
        public uint ReadUInt(bool moveReadPos = true)
        {
            return (uint)ReadInt(moveReadPos);
        }

        /// <summary>Reads an int from the packet.</summary>
        /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
        public int ReadInt(bool moveReadPos = true)
        {
            int value = BitConverter.ToInt32(readableBuffer, readPos); // Convert the bytes to an int
            if (moveReadPos)
            {
                // If moveReadPos is true
                readPos += 4; // Increase readPos by 4
            }
            return value; // Return the int
        }

        /// <summary>Reads a long from the packet.</summary>
        /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
        public long ReadLong(bool moveReadPos = true)
        {
            long value = BitConverter.ToInt64(readableBuffer, readPos); // Convert the bytes to a long
            if (moveReadPos)
            {
                // If moveReadPos is true
                readPos += 8; // Increase readPos by 8
            }
            return value; // Return the long
        }

        /// <summary>Reads a float from the packet.</summary>
        /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
        public float ReadFloat(bool moveReadPos = true)
        {
            float value = BitConverter.ToSingle(readableBuffer, readPos); // Convert the bytes to a float
            if (moveReadPos)
            {
                // If moveReadPos is true
                readPos += 4; // Increase readPos by 4
            }
            return value; // Return the float
        }

        /// <summary>Reads a bool from the packet.</summary>
        /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
        public bool ReadBool(bool moveReadPos = true)
        {
            bool value = BitConverter.ToBoolean(readableBuffer, readPos); // Convert the bytes to a bool
            if (moveReadPos)
            {
                // If moveReadPos is true
                readPos += 1; // Increase readPos by 1
            }
            return value; // Return the bool
        }

        /// <summary>Reads a string from the packet.</summary>
        /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
        public string ReadString(bool moveReadPos = true)
        {
            ushort length = ReadUShort(); // Get the length of the string
            string value = Encoding.UTF8.GetString(readableBuffer, readPos, length); // Convert the bytes to a string
            if (moveReadPos && value.Length > 0)
            {
                // If moveReadPos is true string is not empty
                readPos += length; // Increase readPos by the length of the string
            }
            return value; // Return the string
        }

        /// <summary>Reads a Vector2 from the packet.</summary>
        /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
        public Vector2 ReadVector2(bool moveReadPos = true)
        {
            return new Vector2(ReadFloat(moveReadPos), ReadFloat(moveReadPos));
        }
        
                /// <summary>Reads a ClientId from the packet.</summary>
        /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
        public ClientId ReadClientId(bool moveReadPos = true)
        {
            var id = ClientId.FromBytes(readableBuffer, readPos);
            if (moveReadPos)
                readPos += ClientId.Size();
            return id;
        }
        
        /// <summary>Reads an ItemId from the packet.</summary>
        /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
        public ItemId ReadItemId(bool moveReadPos = true)
        {
            var id = ItemId.FromBytes(readableBuffer, readPos);
            if (moveReadPos)
                readPos += ItemId.Size();
            return id;
        }
        
        /// <summary>Reads a SpellId from the packet.</summary>
        /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
        public SpellId ReadSpellId(bool moveReadPos = true)
        {
            var id = SpellId.FromBytes(readableBuffer, readPos);
            if (moveReadPos)
                readPos += SpellId.Size();
            return id;
        }
        
        /// <summary>Reads an NpcId from the packet.</summary>
        /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
        public NpcId ReadNpcId(bool moveReadPos = true)
        {
            var id = NpcId.FromBytes(readableBuffer, readPos);
            if (moveReadPos)
                readPos += NpcId.Size();
            return id;
        }
        
        /// <summary>Reads a QuestId from the packet.</summary>
        /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
        public QuestId ReadQuestId(bool moveReadPos = true)
        {
            var id = QuestId.FromBytes(readableBuffer, readPos);
            if (moveReadPos)
                readPos += QuestId.Size();
            return id;
        }
        
        /// <summary>Reads a CharacterId from the packet.</summary>
        /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
        public CharacterId ReadCharacterId(bool moveReadPos = true)
        {
            var id = CharacterId.FromBytes(readableBuffer, readPos);
            if (moveReadPos)
                readPos += CharacterId.Size();
            return id;
        }
        #endregion

        private bool disposed;

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    buffer = null;
                    readableBuffer = null;
                    readPos = 0;
                }

                disposed = true;
            }
        }

        ~Packet()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}