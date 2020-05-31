// Copyright (c) 2020 by Rob Jellinghaus.
using DistributedState;
using System;

namespace DistributedThing
{
    public class DistributedThing : DistributedObject<LocalThing>, IThing
    {
        public DistributedThing(bool isOwner, LocalThing localThing) : base(isOwner, localThing)
        {
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
