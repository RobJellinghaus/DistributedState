// Copyright (c) 2020 by Rob Jellinghaus.
using System;
using System.Collections.Generic;
using System.Text;

namespace DistributedThing
{
    using DistributedState;

    public class LocalThing : LocalObject
    {
        public LocalThing(LocalThingState initialState)
        {
            // TODO: initialize!
        }

        public override void Delete()
        {
            throw new NotImplementedException();
        }
    }
}
