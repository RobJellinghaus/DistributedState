// Copyright (c) 2020 by Rob Jellinghaus.

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
