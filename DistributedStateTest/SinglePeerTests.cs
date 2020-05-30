// Copyright (c) 2020 by Rob Jellinghaus.
using LiteNetLib;
using NUnit.Framework;

namespace DistributedState.Test
{
    public class SinglePeerTests
    {
        [Test]
        public void ConstructPeer()
        {
            var testWorkQueue = new TestWorkQueue();
            using Peer peer = new Peer(testWorkQueue, Peer.DefaultListenPort);

            Assert.IsNotNull(peer);

            // Should be no work after construction.
            Assert.AreEqual(0, testWorkQueue.Count);

            // Start announcing.
            peer.Announce();

            // should have sent one Announce message, and queued the action to send the next
            Assert.AreEqual(1, testWorkQueue.Count);
        }

        [Test]
        public void TestListenForAnnounce()
        {
            var testWorkQueue = new TestWorkQueue();

            var testBroadcastListener = new TestBroadcastNetEventListener();
            var testNetManager = new TestNetManager(new NetManager(testBroadcastListener));
            testNetManager.NetManager.BroadcastReceiveEnabled = true;
            testNetManager.NetManager.Start(Peer.DefaultListenPort);

            // the peer under test
            using Peer peer = new Peer(testWorkQueue, Peer.DefaultListenPort, isListener: false);

            // start announcing
            peer.Announce();

            // the list of all pollable objects, to ensure forward progress
            IPollEvents[] pollables = new IPollEvents[] { peer, testNetManager };

            // should have received Announce message
            WaitUtils.WaitUntil(pollables, () => testBroadcastListener.ReceivedMessages.Count == 1);
            Assert.IsTrue(testBroadcastListener.ReceivedMessages.TryDequeue(out object announceMessage));
            ValidateAnnounceMessage(announceMessage, peer);

            // now execute pending work
            testWorkQueue.PollEvents();

            // should still be one queued item -- the *next* announce message
            Assert.AreEqual(1, testWorkQueue.Count);

            // wait to receive second Announce
            WaitUtils.WaitUntil(pollables, () => testBroadcastListener.ReceivedMessages.Count == 1);
            Assert.IsTrue(testBroadcastListener.ReceivedMessages.TryDequeue(out announceMessage));
            ValidateAnnounceMessage(announceMessage, peer);

            static void ValidateAnnounceMessage(object possibleMessage, Peer peer)
            {
                AnnounceMessage announceMessage = possibleMessage as AnnounceMessage;
                Assert.IsNotNull(announceMessage);
                Assert.AreEqual(peer.SocketAddress, announceMessage.AnnouncerSocketAddress.SocketAddress);
                Assert.AreEqual(0, announceMessage.KnownPeers.Length);
            }
        }


        [Test]
        public void PeerListenForAnnounce()
        {
            var testWorkQueue = new TestWorkQueue();

            // the peer under test
            using Peer peer = new Peer(testWorkQueue, Peer.DefaultListenPort, isListener: true);

            // start announcing
            peer.Announce();

            // the list of all pollable objects, to ensure forward progress
            IPollEvents[] pollables = new IPollEvents[] { peer };

            // should have received Announce message
            WaitUtils.WaitUntil(pollables, () => peer.PeerAnnounceCount == 1);

            // now execute pending work
            testWorkQueue.PollEvents();

            // should still be one queued item -- the *next* announce message
            Assert.AreEqual(1, testWorkQueue.Count);

            // wait to receive second Announce
            WaitUtils.WaitUntil(pollables, () => peer.PeerAnnounceCount == 2);
        }

        [Test]
        public void PeerConnectToPeer()
        {
            var testWorkQueue = new TestWorkQueue();

            // the first peer under test
            using Peer peer = new Peer(testWorkQueue, Peer.DefaultListenPort, isListener: true);

            // construct second peer
            using Peer peer2 = new Peer(testWorkQueue, Peer.DefaultListenPort, isListener: false);

            // peer could start announcing also, but peer2 isn't listening so it wouldn't be detectable
            peer2.Announce();

            // the list of all pollable objects, to ensure forward progress
            IPollEvents[] pollables = new IPollEvents[] { peer, peer2 };

            // should have received Announce message
            WaitUtils.WaitUntil(pollables, () => peer.PeerAnnounceCount == 1);

            // now execute pending work
            testWorkQueue.PollEvents();

            // should still be one queued item -- the *next* announce message
            Assert.AreEqual(1, testWorkQueue.Count);

            // wait to receive second Announce
            WaitUtils.WaitUntil(pollables, () => peer.PeerAnnounceCount == 2);
        }
    }
}
