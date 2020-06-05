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

        public static void Register(DistributedPeer.ProxyCapability proxyCapability)
        {
            proxyCapability.Peer.SubscribeReusable((Create createMessage, NetPeer netPeer) =>
            {
                var newProxy = new DistributedThing(
                    id: createMessage.Id,
                    isOwner: false,
                    localThing: new LocalThing(createMessage.Id));

                proxyCapability.AddProxy(netPeer, newProxy);
            });
        }
    }
}
