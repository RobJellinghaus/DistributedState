// Copyright (c) 2020 by Rob Jellinghaus.

namespace DistributedThing
{
    using DistributedState;

    public interface IThing : IDistributedInterface
    {
        /// <summary>
        /// Why shouldn't Things bleat?
        /// </summary>
        void Bleat();
    }
}
