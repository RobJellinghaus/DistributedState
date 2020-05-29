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
        /// Delete this object.
        /// </summary>
        /// <remarks>
        /// For local objects, this may involve resource cleanup, media shutdown, scene graph removal, etc.
        /// </remarks>
        public abstract void Delete();
    }
}
