// Copyright (c) 2020 by Rob Jellinghaus.
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Holofunk.DistributedState
{
    /// <summary>
    /// Message sent by Peers that hear announcements which don't mention them.
    /// </summary>
    /// <remarks>
    /// Sent as a direct unconnected message to inform the newly arriving announcer of the peer's endpoint.
    /// </remarks>
    public class AnnounceResponseMessage : INetSerializable
    {
        /// <summary>
        /// Host-ordered IPV4 address of the responder.
        /// </summary>
        public long ResponderIPV4Address { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            ResponderIPV4Address = reader.GetLong();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(ResponderIPV4Address);
        }
    }
}
