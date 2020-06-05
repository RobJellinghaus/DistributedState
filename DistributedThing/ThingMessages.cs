// Copyright (c) 2020 by Rob Jellinghaus.
using Distributed.State;
using LiteNetLib;
using System;
using System.Collections.Generic;
using System.Text;

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
    public static class ThingMessages
    {
        public class Create : Distributed.State.CreateMessage
        {
            public Create() : base(0)
            { }

            public Create(int id) : base(id)
            { }
        }

        public class Delete : Distributed.State.DeleteMessage
        {
            public Delete() : base(0, false)
            { }

            public Delete(int id, bool isRequest) : base(id, isRequest)
            { }
        }

        public static void Register(DistributedHost.ProxyCapability proxyCapability)
        {
            proxyCapability.SubscribeReusable((Create createMessage, NetPeer netPeer) =>
            {
                var newProxy = new DistributedThing(
                    proxyCapability.Host,
                    netPeer,
                    createMessage.Id,
                    localThing: new LocalThing());

                proxyCapability.AddProxy(netPeer, newProxy);
            });

            proxyCapability.SubscribeReusable((Delete deleteMessage, NetPeer netPeer) =>
            {
                proxyCapability.OnDelete(netPeer, deleteMessage.Id, deleteMessage.IsRequest);
            });
        }
    }
}
