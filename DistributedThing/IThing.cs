// Copyright (c) 2020 by Rob Jellinghaus.

namespace Distributed.Thing
{
    using Distributed.State;

    public interface IThing : IDistributedInterface
    {
        /// <summary>
        /// Why shouldn't Things bleat?
        /// </summary>
        void Bleat();
    }
}
