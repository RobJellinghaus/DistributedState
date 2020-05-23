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
        public void ListenForAnnounce()
        {
            var testWorkQueue = new TestWorkQueue();

            var testBroadcastListener = new TestBroadcastNetEventListener();
            var testNetManager = new NetManager(testBroadcastListener);
            testNetManager.Start(Peer.DefaultBroadcastPort);

            using Peer peer = new Peer(testWorkQueue, Peer.DefaultBroadcastPort, Peer.DefaultReliablePort);

            // should have received Announce message
            WaitUtils.WaitUntil(new[] { testNetManager }, () => testBroadcastListener.ReceivedMessages.Count == 1);
            Assert.IsTrue(testBroadcastListener.ReceivedMessages.TryDequeue(out object announceMessage));
            ValidateAnnounceMessage(announceMessage, peer);

            // now execute pending work
            testWorkQueue.RunQueuedWork();

            // should still be one queued item -- the *next* announce message
            Assert.AreEqual(1, testWorkQueue.Count);

            // wait to receive second Announce
            WaitUtils.WaitUntil(new[] { testNetManager }, () => testBroadcastListener.ReceivedMessages.Count == 1);
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
    }
}