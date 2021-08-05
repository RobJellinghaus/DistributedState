// Copyright (c) 2020 by Rob Jellinghaus.
using Distributed.Thing;
using LiteNetLib;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Distributed.State.Test
{
    public class SingleHostTests
    {
        [Test]
        public void ConstructHost()
        {
            var testWorkQueue = new WorkQueue();
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
        public void HostListenForAnnounce()
        {
            var testWorkQueue = new WorkQueue();

            // the host under test
            using DistributedHost host = new DistributedHost(testWorkQueue, DistributedHost.DefaultListenPort, isListener: true);
            // reduce announce delay to expedite test
            host.AnnounceDelayMsec = 0;

            // start announcing
            host.Announce();

            // the list of all pollable objects, to ensure forward progress
            IPollEvents[] pollables = new IPollEvents[] { host };

            // should have received Announce message
            WaitUtils.WaitUntil(pollables, () => host.SelfAnnounceCount == 1);

            // now execute pending work
            testWorkQueue.PollEvents();

            // should still be one queued item -- the *next* announce message
            Assert.AreEqual(1, testWorkQueue.Count);

            // wait to receive second Announce
            WaitUtils.WaitUntil(pollables, () => host.SelfAnnounceCount == 2);
        }

        [Test]
        public void HostCreateObject()
        {
            var testWorkQueue = new WorkQueue();

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
            var testWorkQueue = new WorkQueue();

            // the first host under test
            using DistributedHost host = new DistributedHost(testWorkQueue, DistributedHost.DefaultListenPort, isListener: true);

            // create a Distributed.Thing
            var distributedThing = new DistributedThing(host, new LocalThing());

            distributedThing.Delete();

            Assert.AreEqual(0, host.Owners.Count);
        }

        [Test]
        public void HostCreateThenModifyObject()
        {
            var testWorkQueue = new WorkQueue();

            // the first host under test
            using DistributedHost host = new DistributedHost(testWorkQueue, DistributedHost.DefaultListenPort, isListener: true);

            // create a Distributed.Thing
            var distributedThing = new DistributedThing(host, new LocalThing());

            distributedThing.Enqueue(new[] { 1, 2, 3 });
            distributedThing.Enqueue(new[] { 4, 5, 6 });

            List<int> expected = new[] { 1, 2, 3, 4, 5, 6 }.ToList();
            List<int> actual = distributedThing.LocalValues.ToList();
            Assert.IsTrue(Enumerable.SequenceEqual(expected, actual));
        }
    }
}
