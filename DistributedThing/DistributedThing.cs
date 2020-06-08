// Copyright (c) 2020 by Rob Jellinghaus.
using Distributed.State;
using LiteNetLib;
using System.Collections.Generic;
using System.Linq;

namespace Distributed.Thing
{
    /// <summary>
    /// Distributed implementation of an IThing.
    /// </summary>
    /// <remarks>
    /// A DistributedObject implementation handles method calls by either routing them to the owner (if this
    /// is a proxy), or by sending updates to all proxies and then updating the local object (if this is the
    /// owner).  The owner's messages directly update the local objects of all proxies.
    /// </remarks>
    public class DistributedThing : DistributedObject<LocalThing>, IThing
    {
        public DistributedThing(DistributedHost peer, LocalThing localThing)
            : base(peer, localThing)
        {
        }

        public DistributedThing(DistributedHost peer, NetPeer owningPeer, int id, LocalThing localThing)
            : base(peer, owningPeer, id, localThing)
        {
        }

        public IEnumerable<int> LocalValues => TypedLocalObject?.LocalValues ?? Enumerable.Empty<int>();

        /// <summary>
        /// Handle an Enqueue message by using default routing.
        /// </summary>
        public void Enqueue(int[] values)
        {
            RouteMessage(
                isRequest => new ThingMessages.Enqueue(Id, isRequest, values),
                () => TypedLocalObject.Enqueue(values));
        }

        protected override void SendCreateMessage(NetPeer netPeer)
        {
            Host.SendReliableMessage(new ThingMessages.Create(Id, LocalValues.ToArray()), netPeer);
        }

        protected override void SendDeleteMessage(NetPeer netPeer, bool isRequest)
        {
            Host.SendReliableMessage(new ThingMessages.Delete(Id, isRequest), netPeer);
        }
    }
}
