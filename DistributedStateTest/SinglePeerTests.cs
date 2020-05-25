// Copyright (c) 2020 by Rob Jellinghaus.
using LiteNetLib;
using NUnit.Framework;

namespace Holofunk.DistributedState.Test
{
    public class SinglePeerTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void ConstructPeer()
        {
            var testWorkQueue = new TestWorkQueue();
            using Peer peer = new Peer(testWorkQueue, Peer.DefaultBroadcastPort, Peer.DefaultReliablePort);

            Assert.IsNotNull(peer);

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
            testNetManager.NetManager.Start(Peer.DefaultBroadcastPort);

            // the peer under test
            using Peer peer = new Peer(
                testWorkQueue,
                Peer.DefaultBroadcastPort,
                Peer.DefaultReliablePort,
                listenForPeerAnnouncements: false);

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
                Assert.AreEqual(peer.IPV4Address, announceMessage.AnnouncerIPV4Address);
                Assert.AreEqual(0, announceMessage.KnownPeers.Length);
            }
        }


        [Test]
        public void PeerListenForAnnounce()
        {
            var testWorkQueue = new TestWorkQueue();

            // the peer under test
            using Peer peer = new Peer(
                testWorkQueue,
                Peer.DefaultBroadcastPort,
                Peer.DefaultReliablePort,
                listenForPeerAnnouncements: true);

            // the list of all pollable objects, to ensure forward progress
            IPollEvents[] pollables = new IPollEvents[] { peer };

            // should have received Announce message
            WaitUtils.WaitUntil(pollables, () => peer.PeerAnnouncementCount == 1);

            // now execute pending work
            testWorkQueue.PollEvents();

            // should still be one queued item -- the *next* announce message
            Assert.AreEqual(1, testWorkQueue.Count);

            // wait to receive second Announce
            WaitUtils.WaitUntil(pollables, () => peer.PeerAnnouncementCount == 2);
        }
    }
}
