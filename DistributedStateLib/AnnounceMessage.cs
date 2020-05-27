﻿// Copyright (c) 2020 by Rob Jellinghaus.
using LiteNetLib.Utils;

namespace Holofunk.DistributedState
{
    /// <summary>
    /// Message sent by Peers that are just entering the system.
    /// </summary>
    /// <remarks>
    /// Peers with no connections will periodically re-announce.
    /// </remarks>
    public class AnnounceMessage
    {
        /// <summary>
        /// Host-ordered IPV4 address of the announcer.
        /// </summary>
        public long AnnouncerIPV4Address { get; set; }

        /// <summary>
        /// The announcer intends to host audio for the whole group.
        /// </summary>
        /// <remarks>
        /// This is the case if the announcer, for example, is a PC hosting all the sound hardware for the space.
        /// </remarks>
        public bool AnnouncerIsHostingAudio { get; set; }

        /// <summary>
        /// The host-ordered IPV4 addresses of peers already known to this Peer.
        /// </summary>
        public long[] KnownPeers { get; set; }
    }
}
