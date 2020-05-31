// Copyright (c) 2020 by Rob Jellinghaus.

using LiteNetLib;
using System;

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
        /// ID of this local object; same as its containing distributed object's ID.
        /// </summary>
        /// <remarks>
        /// TBD whether it would be better to have the local object just point to the distributed object.
        /// </remarks>
        public int Id { get; }

        public LocalObject(int id)
        {
            Id = id;
        }

        /// <summary>
        /// Get an action that will send the right CreateMessage to create a proxy for this object.
        /// </summary>
        /// <remarks>
        /// The LiteNetLib serialization library does not support polymorphism except for toplevel packets
        /// being sent (e.g. the only dynamic type mapping is in the NetPacketProcessor which maps packets
        /// to subscription callbacks).  So we can't make a generic CreateMessage with polymorphic payload.
        /// Instead, when it's time to create a proxy, we get an Action which will send the right CreateMessage
        /// to create the right proxy.
        /// 
        /// In practice this is only called on the local object held by an owning object, since only owning
        /// objects need to create proxies.
        /// </remarks>
        public abstract void SendProxyCreateMessage(DistributedPeer distributedPeer, NetPeer targetPeer);

        /// <summary>
        /// Delete this object.
        /// </summary>
        /// <remarks>
        /// For local objects, this may involve resource cleanup, media shutdown, scene graph removal, etc.
        /// </remarks>
        public abstract void Delete();
    }
}
