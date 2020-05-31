using LiteNetLib.Utils;
using System.Net;
using System.Net.Sockets;

namespace DistributedState
{
    /// <summary>
    /// Wrapper struct which enables serializing a SocketAddress in a LiteNetLib message.
    /// </summary>
    /// <remarks>
    /// Might as well bite the bullet and handle the full port/IPV4/IPV6 enchilada.
    /// 
    /// Wanted to try making this a struct, but serializing it did not function well.
    /// </remarks>
    public struct SerializedSocketAddress
    {
        public SocketAddress SocketAddress { get; set; }

        public SerializedSocketAddress(SocketAddress socketAddress)
        {
            SocketAddress = socketAddress;
        }

        public static void RegisterWith(NetPacketProcessor packetProcessor)
        {
            packetProcessor.RegisterNestedType(Serialize, Deserialize);
        }

        public static SerializedSocketAddress Deserialize(NetDataReader reader)
        {
            int socketAddressSize = reader.GetByte();
            // first two bytes of serialized socket is the address family, which we need to make a SocketAddress
            AddressFamily socketFamily = (AddressFamily)reader.GetShort();
            SocketAddress socketAddress = new SocketAddress(socketFamily, socketAddressSize);
            for (int i = 0; i < socketAddressSize - 2; i++)
            {
                socketAddress[i + 2] = reader.GetByte();
            }
            return new SerializedSocketAddress(socketAddress);
        }

        public static void Serialize(NetDataWriter writer, SerializedSocketAddress socketAddress)
        {
            // first write the size
            writer.Put((byte)socketAddress.SocketAddress.Size);
            // then the bytes
            for (int i = 0; i < socketAddress.SocketAddress.Size; i++)
            {
                writer.Put(socketAddress.SocketAddress[i]);
            }
        }
    }
}
