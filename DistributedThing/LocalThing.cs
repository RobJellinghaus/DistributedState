// Copyright (c) 2020 by Rob Jellinghaus.
using Distributed.State;
using LiteNetLib;
using System;

namespace Distributed.Thing
{
    public class LocalThing : LocalObject
    {
        public LocalThing()
        {
        }

        public override void SendCreateMessage(DistributedHost distributedPeer, NetPeer netPeer)
        {
            distributedPeer.SendReliableMessage(new ThingMessages.Create(Id), netPeer);
        }

        public override void Delete()
        {
            // do nothing... accept the void
        }
    }
}
