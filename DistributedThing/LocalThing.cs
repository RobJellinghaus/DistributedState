// Copyright (c) 2020 by Rob Jellinghaus.
using System;
using System.Collections.Generic;
using System.Text;

namespace DistributedThing
{
    using DistributedState;
    using System.Runtime.CompilerServices;

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
