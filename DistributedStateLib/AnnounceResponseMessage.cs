// Copyright (c) 2020 by Rob Jellinghaus.
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace DistributedState
{
    /// <summary>
    /// Message sent by Peers that hear announcements which don't mention them.
    /// </summary>
    /// <remarks>
    /// Sent as a direct unconnected message to inform the newly arriving announcer of this peer's endpoint.
    /// Only listener peers send these messages, as only they hear the announcements in the first place.
    /// </remarks>
    public class AnnounceResponseMessage
    {
        /// <summary>
        /// Host-ordered IPV4 address of the responder.
        /// </summary>
        public long ResponderIPV4Address { get; set; }
    }
}
