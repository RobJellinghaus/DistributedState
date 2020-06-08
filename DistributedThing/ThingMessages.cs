// Copyright (c) 2020 by Rob Jellinghaus.
using Distributed.State;
using LiteNetLib;
using System;

namespace Distributed.Thing
{
    /// <summary>
    /// Messages pertaining to Things.
    /// </summary>
    /// <remarks>
    /// Since LiteNetLib only supports polymorphism at the level of packets, effectively every type of
    /// message (create, delete, and all type-specific commands) needs to exist for every type of object.
    /// This class embeds all these derived message types for DistributedThings.
    /// </remarks>
    public class ThingMessages : Messages
    {
        public class Create : CreateMessage
        {
            public int[] Values { get; set; }

            public Create() : base()
            { }

            public Create(int id, int[] values) : base(id)
            {
                Values = values;
            }
        }

        public class Delete : DeleteMessage
        {
            public Delete() : base(0, false)
            { }

            public Delete(int id, bool isRequest) : base(id, isRequest)
            { }
        }

        public class Enqueue : BaseMessage
        {
            public int[] Values { get; set; }

            public Enqueue() : base(0, false)
            { }

            public Enqueue(int id, bool isRequest, int[] values) : base(id, isRequest)
            {
                Values = values;
            }
        }

        public static void Register(DistributedHost.ProxyCapability proxyCapability)
        {
            proxyCapability.SubscribeReusable((Create createMessage, NetPeer netPeer) =>
                proxyCapability.AddProxy(
                    netPeer,
                    new DistributedThing(
                        proxyCapability.Host,
                        netPeer,
                        createMessage.Id,
                        localThing: new LocalThing())));

            proxyCapability.SubscribeReusable((Delete deleteMessage, NetPeer netPeer) =>
                proxyCapability.OnDelete(netPeer, deleteMessage.Id, deleteMessage.IsRequest));

            proxyCapability.SubscribeReusable((Enqueue enqueueMessage, NetPeer netPeer) =>
                HandleMessage<Enqueue, DistributedThing, LocalThing, IThing>(
                    proxyCapability.Host, netPeer, enqueueMessage, (message, thing) => thing.Enqueue(message.Values)));
        }
    }
}
