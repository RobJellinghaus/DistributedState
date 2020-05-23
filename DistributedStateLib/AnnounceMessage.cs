// Copyright (c) 2020 by Rob Jellinghaus.
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Holofunk.DistributedState
{
    /// <summary>
    /// Message sent by Peers that are just entering the system.
    /// </summary>
    /// <remarks>
    /// Peers with no connections will periodically re-announce.
    /// </remarks>
    public class AnnounceMessage : INetSerializable
    {
        /// <summary>
        /// Host-ordered IPV4 address of the announcer.
        /// </summary>
        public uint AnnouncerIPV4Address;

        /// <summary>
        /// The announcer intends to host audio for the whole group.
        /// </summary>
        /// <remarks>
        /// This is the case if the announcer, for example, is a PC hosting all the sound hardware for the space.
        /// </remarks>
        public bool AnnouncerIsHostingAudio;

        /// <summary>
        /// The host-ordered IPV4 addresses of peers already known to this Peer.
        /// </summary>
        public uint[] KnownPeers;

        public void Deserialize(NetDataReader reader)
        {
            AnnouncerIPV4Address = reader.GetUInt();
            AnnouncerIsHostingAudio = reader.GetBool();
            KnownPeers = reader.GetUIntArray();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(AnnouncerIPV4Address);
            writer.Put(AnnouncerIsHostingAudio);
            writer.PutArray(KnownPeers);
        }
    }
}
