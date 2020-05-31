// Copyright (c) 2020 by Rob Jellinghaus.

namespace DistributedState
{
    /// <summary>
    /// Base class for the local state of a distributed object.
    /// </summary>
    /// <remarks>
    /// The appropriate derived type of LocalState is passed in the payload of a Create message.
    /// </remarks>
    public abstract class LocalState
    {
        /// <summary>
        /// Create a proxy distributed object of the correct type for this state.
        /// </summary>
        /// <returns></returns>
        public abstract DistributedObject CreateDistributedObject();
    }
}
