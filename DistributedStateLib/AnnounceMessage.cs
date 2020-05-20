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
        /// IPV4 address of the announcer.
        /// </summary>
        public int AnnouncerIPV4Address;

        /// <summary>
        /// The announcer intends to host audio for the whole group.
        /// </summary>
        /// <remarks>
        /// This is the case if the announcer, for example, is a PC hosting all the sound hardware for the space.
        /// </remarks>
        public bool AnnouncerIsHostingAudio;

        /// <summary>
        /// The IP addresses of peers already known to this Peer.
        /// </summary>
        public int[] KnownPeers;

        public void Deserialize(NetDataReader reader)
        {
            AnnouncerIPV4Address = reader.GetInt();
            AnnouncerIsHostingAudio = reader.GetBool();
            KnownPeers = reader.GetIntArray();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(AnnouncerIPV4Address);
            writer.Put(AnnouncerIsHostingAudio);
            writer.PutArray(KnownPeers);
        }
    }
}
