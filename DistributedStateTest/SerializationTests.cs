// Copyright (c) 2020 by Rob Jellinghaus.
using Distributed.Thing;
using LiteNetLib.Utils;
using NUnit.Framework;
using System.Net;

namespace Distributed.State.Test
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

        [Test]
        public void TestThingMessage()
        {
            // Test whether the serialization framework supports property inheritance.
            // ThingMessage.Create derives from CreateMessage which has an Id property.
            var createThingMessage = new ThingMessages.Create(1, new int[] { });

            var writer = new NetDataWriter();
            NetPacketProcessor processor = new NetPacketProcessor();
            processor.RegisterNestedType<DistributedId>();

            processor.Write(writer, createThingMessage);

            var reader = new NetDataReader(writer.CopyData());
            ThingMessages.Create readMessage = null;
            processor.Subscribe(
                createMessage => readMessage = createMessage,
                () => new ThingMessages.Create());

            processor.ReadAllPackets(reader);

            Assert.IsNotNull(readMessage);
            Assert.AreEqual(new DistributedId(1), readMessage.Id);
        }
    }
}
