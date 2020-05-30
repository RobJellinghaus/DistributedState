// Copyright (c) 2020 by Rob Jellinghaus.
using LiteNetLib;
using NUnit.Framework;

namespace DistributedState.Test
{
    public class DoublePeerTests
    {
        [Test]
        public void PeerConnectToPeer()
        {
            var testWorkQueue = new TestWorkQueue();

            // the first peer under test
            using DistributedPeer peer = new DistributedPeer(testWorkQueue, DistributedPeer.DefaultListenPort, isListener: true);

            // construct second peer
            using DistributedPeer peer2 = new DistributedPeer(testWorkQueue, DistributedPeer.DefaultListenPort, isListener: false);

            // peer could start announcing also, but peer2 isn't listening so it wouldn't be detectable
            peer2.Announce();

            // the list of all pollable objects, to ensure forward progress
            IPollEvents[] pollables = new IPollEvents[] { peer, peer2 };

            // should generate announce response and then connection
            WaitUtils.WaitUntil(pollables, () => peer.PeerCount == 1 && peer2.PeerCount == 1);

            // Should be one announce received by peer, and one announce response received by peer2
            Assert.AreEqual(1, peer.PeerAnnounceCount);
            Assert.AreEqual(0, peer.PeerAnnounceResponseCount);
            Assert.AreEqual(0, peer2.PeerAnnounceCount);
            Assert.AreEqual(1, peer2.PeerAnnounceResponseCount);
        }
    }
}
