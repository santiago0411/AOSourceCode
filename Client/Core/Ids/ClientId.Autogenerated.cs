// THIS CODE WAS AUTO-GENERATED BY 'IdStructGenerator' TO COMPLETE PARTIAL STRUCT 'ClientId'
// DO *NOT* MODIFY MANUALLY!!

using System;

namespace AO.Core.Ids
{
	public readonly partial struct ClientId : IEquatable<ClientId>, IFormattable
    {
        public static readonly ClientId Empty = new(0);

        private readonly Int32 id;

        private ClientId(Int32 id)
        {
            this.id = id;
        }

        public static ClientId FromBytes(byte[] bytes, int startIndex)
        {
            return BitConverter.ToInt32(bytes, startIndex);
        }

        public static implicit operator ClientId(Int32 id)
        {
            return new ClientId(id);
        }

        public static bool operator ==(ClientId lhs, ClientId rhs)
        {
            return lhs.id == rhs.id;
        }

        public static bool operator !=(ClientId lhs, ClientId rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object obj)
        {
            return obj is ClientId other && other.id == id;
        }

        public static int Size()
        {
            return sizeof(Int32);
        }

        public bool Equals(ClientId other)
        {
            return other.id == id;
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public override string ToString()
        {
            return id.ToString();
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return id.ToString(format, formatProvider);
        }

        public Int32 AsPrimitiveType()
        {
            return id;
        }

        public static ClientId Parse(string value)
        {
            return Int32.Parse(value);
        }

        public static bool TryParse(string value, out ClientId result)
        {
            bool parsed = Int32.TryParse(value, out var outVal);
            result = outVal;
            return parsed;
        }
	}
}