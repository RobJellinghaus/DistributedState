// Copyright (c) 2020 by Rob Jellinghaus.

namespace Distributed.State
{
    /// <summary>
    /// Message sent by owner objects to proxies.
    /// </summary>
    /// <remarks>
    /// Sent as a reliable sequenced message from owning peer to proxy peer; owner sends
    /// one CommandMessage per proxy.
    /// 
    /// Note that deleting an object is done by sending a CommandMessage containing a DeleteCommand.
    /// </remarks>
    public abstract class CommandMessage
    {
        /// <summary>
        /// The object ID.
        /// </summary>
        /// <remarks>
        /// Note that the endpoints involved in the message transmission are enough to determine
        /// which peer this ID came from.
        /// </remarks>
        public int Id { get; set; }

        /// <summary>
        /// Is this message a command request?
        /// </summary>
        /// <remarks>
        /// Since a peer will in general have a mix of owned objects (which receive command requests
        /// from proxies) and proxy objects (which receive commands from owners), we use this same
        /// CommandMessage type for both purposes.  The IsRequest field distinguishes authoritative
        /// commands from racy requests.</remarks>
        public bool IsRequest { get; set; }

        /// <summary>
        /// Invoke this CommandMessage, given this Peer it's invoking within, and this object that exists locally.
        /// </summary>
        public abstract void Invoke(DistributedHost distributedPeer, DistributedObject distributedObject);
    }
}
