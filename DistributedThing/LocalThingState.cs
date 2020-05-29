// Copyright (c) 2020 by Rob Jellinghaus.
using System;
using System.Collections.Generic;
using System.Text;

namespace DistributedThing
{
    using DistributedState;

    public class LocalThingState : LocalState
    {
        public int Value { get; set; }

        public override DistributedObject CreateDistributedObject()
        {
            return new DistributedThing(false, new LocalThing(this));
        }
    }
}
