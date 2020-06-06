// Copyright (c) 2020 by Rob Jellinghaus.
using Distributed.State;
using LiteNetLib;
using System;

namespace Distributed.Thing
{
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

        public void Bleat()
        {
            throw new NotImplementedException();
        }

        protected override void SendCreateMessage(NetPeer netPeer)
        {
            Host.SendReliableMessage(new ThingMessages.Create(Id), netPeer);
        }

        protected override void SendDeleteMessage(NetPeer netPeer, bool isRequest)
        {
            Host.SendReliableMessage(new ThingMessages.Delete(Id, isRequest), netPeer);
        }
    }
}
