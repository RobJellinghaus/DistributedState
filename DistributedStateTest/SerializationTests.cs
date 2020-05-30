// Copyright (c) 2020 by Rob Jellinghaus.
using LiteNetLib;
using LiteNetLib.Utils;
using NUnit.Framework;
using System.Net;

namespace DistributedState.Test
{
    public class SerializationTests
    {
        class Packet
        {
            public SerializedSocketAddress SerializedSocketAddress { get; set; }
        }

        [Test]
        public void TestSocketAddress()
        {
            IPEndPoint endPoint = new IPEndPoint(new IPAddress(0), 1);
            SocketAddress socketAddress = endPoint.Serialize();
            var serializedSocketAddress = new SerializedSocketAddress(socketAddress);
            var packet = new Packet { SerializedSocketAddress = serializedSocketAddress };

            var writer = new NetDataWriter();
            NetPacketProcessor processor = new NetPacketProcessor();
            SerializedSocketAddress.RegisterWith(processor);

            processor.Write(writer, packet);

            var reader = new NetDataReader(writer.CopyData());
            Packet readPacket = null;
            processor.Subscribe<Packet>(packet => readPacket = packet, () => new Packet());

            processor.ReadAllPackets(reader);

            Assert.IsNotNull(readPacket);
            Assert.AreEqual(serializedSocketAddress.SocketAddress, readPacket.SerializedSocketAddress.SocketAddress);
        }
    }
}
