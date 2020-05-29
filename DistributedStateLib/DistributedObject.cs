// Copyright (c) 2020 by Rob Jellinghaus.
using System;
using System.Collections.Generic;
using System.Text;

namespace DistributedState
{
    public class DistributedObject
    {
        /// <summary>
        /// Is this object the original, owner instance?
        /// </summary>
        /// <remarks>
        /// Owner objects relay commands to proxies, along with updating local state;
        /// proxy objects relay command requests to owners, and update local state only on commands from owners.
        /// </remarks>
        public readonly bool IsOwner;

        protected DistributedObject(bool isOwner)
        {
            IsOwner = isOwner;
        }
    }

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
    /// The DistributedObject derived class also implemnets that same interface.  If the DistributedObject is
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
    public class DistributedObject<TLocalObject> : DistributedObject
        where TLocalObject : LocalObject
    {
        /// <summary>
        /// The local object wrapped by this distributed object (be it owner or proxy).
        /// </summary>
        public readonly TLocalObject LocalObject;

        protected DistributedObject(bool isOwner, TLocalObject localObject) : base(isOwner)
        {
            LocalObject = localObject;
        }
    }
}
