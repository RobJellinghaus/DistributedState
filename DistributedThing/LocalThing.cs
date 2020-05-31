// Copyright (c) 2020 by Rob Jellinghaus.

namespace DistributedThing
{
    using DistributedState;

    public class LocalThing : LocalObject
    {
        private LocalThingState localState;

        public LocalThing(LocalThingState initialState)
        {
            localState = initialState;
        }

        public override LocalState LocalState => localState;

        public override void Delete()
        {
            // do nothing... accept the void
        }
    }
}
