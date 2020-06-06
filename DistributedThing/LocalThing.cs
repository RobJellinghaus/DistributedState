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

        public override void Delete()
        {
            // do nothing... accept the void
        }
    }
}
