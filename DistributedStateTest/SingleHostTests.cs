// Copyright (c) 2020 by Rob Jellinghaus.
using Distributed.Thing;
using LiteNetLib;
using NUnit.Framework;
using System.Linq;

namespace Distributed.State.Test
{
    public class SingleHostTests
    {
        [Test]
        public void ConstructHost()
        {
            var testWorkQueue = new TestWorkQueue();
            using DistributedHost host = new DistributedHost(testWorkQueue, DistributedHost.DefaultListenPort);

            Assert.IsNotNull(host);

            // Should be no work after construction.
            Assert.AreEqual(0, testWorkQueue.Count);

            // Start announcing.
            host.Announce();

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
            testNetManager.NetManager.Start(DistributedHost.DefaultListenPort);

            // the host under test
            using DistributedHost host = new DistributedHost(testWorkQueue, DistributedHost.DefaultListenPort, isListener: false);

            // start announcing
            host.Announce();

            // the list of all pollable objects, to ensure forward progress
            IPollEvents[] pollables = new IPollEvents[] { host, testNetManager };

            // should have received Announce message
            WaitUtils.WaitUntil(pollables, () => testBroadcastListener.ReceivedMessages.Count == 1);
            Assert.IsTrue(testBroadcastListener.ReceivedMessages.TryDequeue(out object announceMessage));
            ValidateAnnounceMessage(announceMessage, host);

            // now execute pending work
            testWorkQueue.PollEvents();

            // should still be one queued item -- the *next* announce message
            Assert.AreEqual(1, testWorkQueue.Count);

            // wait to receive second Announce
            WaitUtils.WaitUntil(pollables, () => testBroadcastListener.ReceivedMessages.Count == 1);
            Assert.IsTrue(testBroadcastListener.ReceivedMessages.TryDequeue(out announceMessage));
            ValidateAnnounceMessage(announceMessage, host);

            static void ValidateAnnounceMessage(object possibleMessage, DistributedHost host)
            {
                AnnounceMessage announceMessage = possibleMessage as AnnounceMessage;
                Assert.IsNotNull(announceMessage);
                Assert.AreEqual(host.SocketAddress, announceMessage.AnnouncerSocketAddress.SocketAddress);
                Assert.AreEqual(0, announceMessage.KnownPeers.Length);
            }
        }


        [Test]
        public void HostListenForAnnounce()
        {
            var testWorkQueue = new TestWorkQueue();

            // the host under test
            using DistributedHost host = new DistributedHost(testWorkQueue, DistributedHost.DefaultListenPort, isListener: true);

            // start announcing
            host.Announce();

            // the list of all pollable objects, to ensure forward progress
            IPollEvents[] pollables = new IPollEvents[] { host };

            // should have received Announce message
            WaitUtils.WaitUntil(pollables, () => host.PeerAnnounceCount == 1);

            // now execute pending work
            testWorkQueue.PollEvents();

            // should still be one queued item -- the *next* announce message
            Assert.AreEqual(1, testWorkQueue.Count);

            // wait to receive second Announce
            WaitUtils.WaitUntil(pollables, () => host.PeerAnnounceCount == 2);
        }

        [Test]
        public void HostCreateObject()
        {
            var testWorkQueue = new TestWorkQueue();

            // the first host under test
            using DistributedHost host = new DistributedHost(testWorkQueue, DistributedHost.DefaultListenPort, isListener: true);

            // create a Distributed.Thing
            var distributedThing = new DistributedThing(host, new LocalThing());

            Assert.AreEqual(1, host.Owners.Count);
            Assert.True(host.Owners.Values.First() == distributedThing);
        }


        [Test]
        public void HostCreateThenDeleteObject()
        {
            var testWorkQueue = new TestWorkQueue();

            // the first host under test
            using DistributedHost host = new DistributedHost(testWorkQueue, DistributedHost.DefaultListenPort, isListener: true);

            // create a Distributed.Thing
            var distributedThing = new DistributedThing(host, new LocalThing());

            distributedThing.Delete();

            Assert.AreEqual(0, host.Owners.Count);
        }
    }
}
