// Copyright (c) 2020 by Rob Jellinghaus.

namespace Distributed.State
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
        /// The distributed object that contains this local object.
        /// </summary>
        public DistributedObject DistributedObject { get; private set; }

        /// <summary>
        /// ID of this local object; same as its containing distributed object's ID.
        /// </summary>
        public int Id => DistributedObject?.Id ?? 0;

        public LocalObject()
        {
        }

        /// <summary>
        /// Connect this local object to its distributed object; may only be called once.
        /// </summary>
        /// <param name="distributedObject"></param>
        internal void Initialize(DistributedObject distributedObject)
        {
            Contract.Requires(DistributedObject == null);
            Contract.Requires(distributedObject != null);
            
            DistributedObject = distributedObject;
        }

        /// <summary>
        /// Delete this object.
        /// </summary>
        /// <remarks>
        /// For local objects, this may involve resource cleanup, media shutdown, scene graph removal, etc.
        /// </remarks>
        public abstract void Delete();
    }
}
