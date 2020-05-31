// Copyright (c) 2020 by Rob Jellinghaus.
using Distributed.State;
using System;

namespace Distributed.Thing
{
    public class DistributedThing : DistributedObject<LocalThing>, IThing
    {
        public DistributedThing(int id, bool isOwner, LocalThing localThing)
            : base(id, isOwner, localThing)
        {
            // ensure consistent IDs
            Contract.Requires(id == localThing.Id);

            // TODO: create proxies if owner
        }

        public void Bleat()
        {
            throw new NotImplementedException();
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }
    }
}
