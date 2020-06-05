// Copyright (c) 2020 by Rob Jellinghaus.

using LiteNetLib;

namespace Distributed.State
{
    /// <summary>
    /// Base class for distributed objects.
    /// </summary>
    /// <remarks>
    /// The DistributedState framework divides the functionality of a distributed object into two parts:
    /// 
    /// 1) A DistributedObject derived class, which handles message routing from/to proxy objects, and which
    ///    also relays messages to:
    /// 2) A LocalObject derived class, which implements the actual local behavior of the distributed object.
    /// 
    /// The LocalObject derived class implements an interface (derived from IDistributedObject) which presents
    /// all the methods that can be invoked on those objects.
    /// 
    /// The DistributedObject derived class also implements that same interface.  If the DistributedObject is
    /// an owner, it will relay calls as commands to its proxies (as well as to its local object); if the
    /// DistributedObject is a proxy, it will relay the message as a command request to the owner.
    /// If the DistributedObject is a proxy and receives a command from the owner, it relays it to the proxy's
    /// local object.
    /// 
    /// The net result is:
    /// 1) All method invocations on the owner are relayed reliably and in sequence to all proxies.
    /// 2) The owner and all proxies update local state in response to that reliable command sequence, keeping
    ///    all proxies synchronized.
    /// 3) Proxies whose methods are invoked do not update any local state, but only relay command requests to
    ///    the owner; the owner is always authoritative about state.
    /// </remarks>
    public abstract class DistributedObject : IDistributedInterface
    {
        /// <summary>
        /// The peer that contains this object.
        /// </summary>
        public DistributedHost Host { get; internal set; }

        /// <summary>
        /// The NetPeer which owns this proxy, if this is a proxy.
        /// </summary>
        public NetPeer OwningPeer { get; private set; }

        /// <summary>
        /// Is this object the original, owner instance?
        /// </summary>
        /// <remarks>
        /// Owner objects relay commands to proxies, along with updating local state;
        /// proxy objects relay command requests to owners, and update local state only on commands from owners.
        /// </remarks>
        public readonly bool IsOwner;

        /// <summary>
        /// The id of this object; unique within its owning DistributedPeer.
        /// </summary>
        public readonly int Id;

        /// <summary>
        /// The local object which implements the local behavior of the distributed object.
        /// </summary>
        public readonly LocalObject LocalObject;

        /// <summary>
        /// Create an owner DistributedObject.
        /// </summary>
        protected DistributedObject(DistributedHost host, LocalObject localObject)
        {
            Contract.Requires(host != null);
            Contract.Requires(localObject != null);

            Host = host;
            Id = host.NextOwnerId();
            IsOwner = true;
            LocalObject = localObject;
            // and connect the local object to us
            LocalObject.Initialize(this);
        }

        /// <summary>
        /// Create a proxy DistributedObject.
        /// </summary>
        protected DistributedObject(DistributedHost host, NetPeer netPeer, int id, LocalObject localObject)
        {
            Contract.Requires(host != null);
            Contract.Requires(netPeer != null);
            Contract.Requires(id > 0);
            Contract.Requires(localObject != null);

            Host = host;
            OwningPeer = netPeer;
            Id = id;
            IsOwner = false;
            LocalObject = localObject;
            LocalObject.Initialize(this);
        }

        /// <summary>
        /// Detach this object from its Host; this occurs when the owner or proxy is deleted (or the proxy gets disconnected
        /// from the host).
        /// </summary>
        internal void Detach()
        {
            Contract.Requires(Host != null);

            Host = null;
        }

        /// <summary>
        /// Delete this DistributedObject.
        /// </summary>
        /// <remarks>
        /// If called on the owner object, this will delete it (and all its proxies).  If called on a
        /// proxy object, this will send a deletion request to the owner.
        /// </remarks>
        public void Delete()
        {
            Contract.Requires(Host != null);

            Host.Delete(this, SendDeleteMessage);
            
            // once we are deleted we lose our connection to our peer
            Host = null;
        }

        internal void SendDeleteMessageInternal(NetPeer netPeer, bool isRequest)
        {
            SendDeleteMessage(netPeer, isRequest);
        }

        /// <summary>
        /// Construct the appropriate kind of DeleteMessage for this type of object.
        /// </summary>
        /// <returns></returns>
        protected abstract void SendDeleteMessage(NetPeer netPeer, bool isRequest);
    }

    /// <summary>
    /// More strongly typed base class, for convenience of derived classes.
    /// </summary>
    public abstract class DistributedObject<TLocalObject> : DistributedObject
        where TLocalObject : LocalObject
    {
        /// <summary>
        /// The local object wrapped by this distributed object (be it owner or proxy).
        /// </summary>
        public readonly TLocalObject TypedLocalObject;

        protected DistributedObject(DistributedHost peer, TLocalObject localObject)
            : base(peer, localObject)
        {
            TypedLocalObject = localObject;
        }

        protected DistributedObject(DistributedHost peer, NetPeer owningPeer, int id, TLocalObject localObject)
            : base(peer, owningPeer, id, localObject)
        {
            TypedLocalObject = localObject;
        }
    }
}
