// Copyright (c) 2020 by Rob Jellinghaus.
using Distributed.Thing;
using NUnit.Framework;
using System.Linq;

namespace Distributed.State.Test
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

        [Test]
        public void PeerCreateAfterConnection()
        {
            var testWorkQueue = new TestWorkQueue();

            // the first peer under test
            using DistributedPeer peer = new DistributedPeer(
                testWorkQueue,
                DistributedPeer.DefaultListenPort,
                isListener: true,
                disconnectTimeout: 10000000); // we want to be able to debug

            // construct second peer
            using DistributedPeer peer2 = new DistributedPeer(
                testWorkQueue,
                DistributedPeer.DefaultListenPort,
                isListener: false,
                disconnectTimeout: 10000000);

            // make sure the peers know what to do with ThingMessages
            ThingMessages.Register(peer);
            ThingMessages.Register(peer2);

            // peer could start announcing also, but peer2 isn't listening so it wouldn't be detectable
            peer2.Announce();

            // the list of all pollable objects, to ensure forward progress
            IPollEvents[] pollables = new IPollEvents[] { peer, peer2 };

            // should generate announce response and then connection
            WaitUtils.WaitUntil(pollables, () => peer.PeerCount == 1 && peer2.PeerCount == 1);

            // create object
            var distributedThing = new DistributedThing(
                1,
                isOwner: true,
                localThing: new LocalThing(1));

            peer.AddOwner(distributedThing);

            // wait until the proxy for the new object makes it to the other peer
            WaitUtils.WaitUntil(pollables, () => peer2.ProxiesForPeer(peer2.NetPeers.First()).Count == 1);

            DistributedObject peer2Proxy = peer2.ProxiesForPeer(peer2.NetPeers.First()).Values.First();
            Assert.AreEqual(1, peer2Proxy.Id);
            Assert.False(peer2Proxy.IsOwner);

            // now create an owner object on the other peer
            var distributedThing2 = new DistributedThing(
                2,
                isOwner: true,
                localThing: new LocalThing(2));

            peer2.AddOwner(distributedThing2);

            // wait until the proxy for the new object makes it to the first peer
            WaitUtils.WaitUntil(pollables, () => peer.ProxiesForPeer(peer.NetPeers.First()).Count == 1);

            DistributedObject peerProxy = peer.ProxiesForPeer(peer.NetPeers.First()).Values.First();
            Assert.AreEqual(2, peerProxy.Id);
            Assert.False(peerProxy.IsOwner);
        }
    }
}
