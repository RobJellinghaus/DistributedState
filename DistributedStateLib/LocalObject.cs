// Copyright (c) 2020 by Rob Jellinghaus.

namespace DistributedState
{
    /// <summary>
    /// Base class for local object implementations that handle the local behavior of a distributed object.
    /// </summary>
    /// <remarks>
    /// Both owner and proxy objects contain an instance of the appropriate type of local object; this ensures the same
    /// behavior regardless of owner/proxy topology.
    /// </remarks>
    public abstract class LocalObject : IDistributedInterface
    {
        /// <summary>
        /// Get the local state for this local object.
        /// </summary>
        /// <remarks>
        /// This is called when a new peer connects, and we need to create proxies on the new peer for the
        /// state of an owner object. The owner's LocalObject's LocalState is passed to instantiate the new
        /// proxies.
        /// </remarks>
        public abstract LocalState LocalState { get; }

        /// <summary>
        /// Delete this object.
        /// </summary>
        /// <remarks>
        /// For local objects, this may involve resource cleanup, media shutdown, scene graph removal, etc.
        /// </remarks>
        public abstract void Delete();
    }
}
