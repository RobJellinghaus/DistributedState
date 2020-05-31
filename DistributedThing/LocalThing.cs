// Copyright (c) 2020 by Rob Jellinghaus.
using Distributed.State;
using LiteNetLib;
using System;

namespace Distributed.Thing
{
    public class LocalThing : LocalObject
    {
        public LocalThing(int id) : base(id)
        {
        }

        public override void SendProxyCreateMessage(DistributedPeer distributedPeer, NetPeer netPeer)
        {
            distributedPeer.SendReliableMessage(new ThingMessages.Create(Id), netPeer);
        }

        public override void Delete()
        {
            // do nothing... accept the void
        }
    }
}
