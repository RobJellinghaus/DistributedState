﻿// Copyright (c) 2020 by Rob Jellinghaus.

namespace Distributed.State
{
    /// <summary>
    /// Base class for local object implementations that handle the local behavior of a distributed object.
    /// </summary>
    /// <remarks>
    /// Both owner and proxy objects contain an instance of the appropriate type of local object; this ensures the same
    /// behavior regardless of owner/proxy topology.
    /// </remarks>
    public interface ILocalObject : IDistributedInterface
    {
        /// <summary>
        /// The distributed object that contains this local object.
        /// </summary>
        DistributedObject DistributedObject { get; }

        /// <summary>
        /// ID of this local object; same as its containing distributed object's ID.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Connect this local object to its distributed object; may only be called once.
        /// </summary>
        /// <param name="distributedObject"></param>
        void Initialize(DistributedObject distributedObject);
    }
}
