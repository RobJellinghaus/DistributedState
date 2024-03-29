// Copyright (c) 2020 by Rob Jellinghaus.
using Distributed.Thing;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using System.Collections.Generic;
using System.Linq;

namespace Distributed.State.Test
{
    public class DoubleHostTests
    {
        [Test]
        public void HostConnectToHost()
        {
            var testWorkQueue = new WorkQueue();

            // the first host under test
            using DistributedHost host = new DistributedHost(testWorkQueue, DistributedHost.DefaultListenPort, isListener: true);

            // construct second host
            using DistributedHost host2 = new DistributedHost(testWorkQueue, DistributedHost.DefaultListenPort, isListener: false);

            // host could start announcing also, but host2 isn't listening so it wouldn't be detectable
            host2.Announce();

            // should generate announce response and then connection
            WaitUtils.WaitUntil(new[] { host, host2 }, () => host.PeerCount == 1 && host2.PeerCount == 1);

            // Should be one announce received by host, and one announce response received by host2
            Assert.AreEqual(1, host.PeerAnnounceCount);
            Assert.AreEqual(0, host.PeerAnnounceResponseCount);
            Assert.AreEqual(0, host2.PeerAnnounceCount);
            Assert.AreEqual(1, host2.PeerAnnounceResponseCount);
        }

        private static DistributedHost CreateHost(WorkQueue testWorkQueue, bool isListener)
        {
            var host = new DistributedHost(
                testWorkQueue,
                DistributedHost.DefaultListenPort,
                isListener: isListener,
                disconnectTimeout: 10000000);
            host.RegisterWith(ThingMessages.Register);
            return host;
        }

        private static IReadOnlyDictionary<DistributedId, IDistributedObject> ProxiesForFirstPeer(DistributedHost host)
        {
            return host.ProxiesForPeer(new SerializedSocketAddress(host.NetPeers.First()));
        }

        private static LocalThing FirstProxyLocalThing(DistributedHost host)
        {
            return (LocalThing)ProxiesForFirstPeer(host).First().Value.LocalObject;
        }

        [Test]
        public void HostCreateAfterConnection()
        {
            var testWorkQueue = new WorkQueue();

            // the first host under test
            using DistributedHost host = CreateHost(testWorkQueue, true);

            // construct second host
            using DistributedHost host2 = CreateHost(testWorkQueue, false);

            // host could start announcing also, but host2 isn't listening so it wouldn't be detectable
            host2.Announce();

            // should generate announce response and then connection
            WaitUtils.WaitUntil(new[] { host, host2 }, () => host.PeerCount == 1 && host2.PeerCount == 1);

            // create object
            var distributedThing = new DistributedThing(host, new LocalThing());

            // wait until the proxy for the new object makes it to the other host
            WaitUtils.WaitUntil(new[] { host, host2 }, () => ProxiesForFirstPeer(host2).Count == 1);

            IDistributedObject host2Proxy = ProxiesForFirstPeer(host2).Values.First();
            Assert.AreEqual(new DistributedId(1), host2Proxy.Id);
            Assert.False(host2Proxy.IsOwner);

            // now create an owner object on the other host
            var distributedThing2 = new DistributedThing(host2, new LocalThing());

            // wait until the proxy for the new object makes it to the first host
            WaitUtils.WaitUntil(new[] { host, host2 }, () => ProxiesForFirstPeer(host).Count == 1);

            IDistributedObject hostProxy = ProxiesForFirstPeer(host).Values.First();
            Assert.AreEqual(new DistributedId(1), hostProxy.Id);
            Assert.False(hostProxy.IsOwner);
        }

        [Test]
        public void HostCreateBeforeConnection()
        {
            var testWorkQueue = new WorkQueue();

            // the first host under test
            using DistributedHost host = CreateHost(testWorkQueue, true);

            // create object
            var distributedThing = new DistributedThing(host, new LocalThing());

            // construct second host
            using DistributedHost host2 = CreateHost(testWorkQueue, false);

            // consume one owner ID so second object has ID 2 instead of (matching first object) ID 1
            host2.NextOwnerId();

            // now create an owner object on the other host
            var distributedThing2 = new DistributedThing(host2, new LocalThing());

            // host could start announcing also, but host2 isn't listening so it wouldn't be detectable
            host2.Announce();

            // wait until the proxy for the new object makes it to the other host
            WaitUtils.WaitUntil(new[] { host, host2 }, () =>
                host2.NetPeers.Count() == 1
                && ProxiesForFirstPeer(host2).Count == 1
                && host.NetPeers.Count() == 1
                && host.ProxiesForPeer(new SerializedSocketAddress(host.NetPeers.First())).Count == 1);

            IDistributedObject host2Proxy = ProxiesForFirstPeer(host2).Values.First();
            Assert.AreEqual(new DistributedId(1), host2Proxy.Id);
            Assert.False(host2Proxy.IsOwner);

            IDistributedObject hostProxy = ProxiesForFirstPeer(host).Values.First();
            Assert.AreEqual(new DistributedId(2), hostProxy.Id);
            Assert.False(hostProxy.IsOwner);
        }

        [Test]
        public void HostCreateThenDisconnect()
        {
            var testWorkQueue = new WorkQueue();

            // the first host under test
            using DistributedHost host = CreateHost(testWorkQueue, true);

            // create object
            var distributedThing = new DistributedThing(host, new LocalThing());

            // construct second host
            using (DistributedHost host2 = CreateHost(testWorkQueue, false))
            {
                // consume one owner ID so second object has ID 2 instead of (matching first object) ID 1
                host2.NextOwnerId();

                // now create an owner object on the other host
                var distributedThing2 = new DistributedThing(host2, new LocalThing());

                // host could start announcing also, but host2 isn't listening so it wouldn't be detectable
                host2.Announce();

                // wait until the proxy for the new object makes it to the other host
                WaitUtils.WaitUntil(new[] { host, host2 }, () =>
                    host2.NetPeers.Count() == 1
                    && ProxiesForFirstPeer(host2).Count == 1
                    && host.NetPeers.Count() == 1
                    && ProxiesForFirstPeer(host).Count == 1);

                IDistributedObject host2Proxy = ProxiesForFirstPeer(host2).Values.First();
                Assert.AreEqual(new DistributedId(1), host2Proxy.Id);
                Assert.False(host2Proxy.IsOwner);

                IDistributedObject hostProxy = ProxiesForFirstPeer(host).Values.First();
                Assert.AreEqual(new DistributedId(2), hostProxy.Id);
                Assert.False(hostProxy.IsOwner);
            }

            // now after things settle down there should be no proxy
            WaitUtils.WaitUntil(new[] { host }, () => host.PeerCount == 0 && host.ProxyPeerCount == 0);
        }

        [Test]
        public void HostCreateAfterConnectionThenDelete()
        {
            var testWorkQueue = new WorkQueue();

            // the first host under test
            using DistributedHost host = CreateHost(testWorkQueue, true);

            // construct second host
            using DistributedHost host2 = CreateHost(testWorkQueue, false);

            // host could start announcing also, but host2 isn't listening so it wouldn't be detectable
            host2.Announce();

            // create object
            var distributedThing = new DistributedThing(host, new LocalThing());

            // wait until the proxy for the new object makes it to the other host
            WaitUtils.WaitUntil(
                new[] { host, host2 },
                () => host2.NetPeers.Count() == 1 && ProxiesForFirstPeer(host2).Count == 1);

            // now delete the object
            distributedThing.Delete();

            // wait until the delete messages flow around
            WaitUtils.WaitUntil(
                new[] { host, host2 },
                () => ProxiesForFirstPeer(host).Count == 0);
        }

        [Test]
        public void HostCreateAndDeleteAfterConnection()
        {
            var testWorkQueue = new WorkQueue();

            // the first host under test
            using DistributedHost host = CreateHost(testWorkQueue, true);

            // construct second host
            using DistributedHost host2 = CreateHost(testWorkQueue, false);

            // host could start announcing also, but host2 isn't listening so it wouldn't be detectable
            host2.Announce();

            // create object
            var distributedThing = new DistributedThing(host, new LocalThing());

            // wait until the proxy for the new object makes it to the other host
            WaitUtils.WaitUntil(
                new[] { host, host2 },
                () => host2.NetPeers.Count() == 1 && ProxiesForFirstPeer(host2).Count == 1);

            // now delete the object
            distributedThing.Delete();

            // and create a new object
            var distributedThing2 = new DistributedThing(host, new LocalThing());

            // wait until the messages flow around, and make sure there is only one proxy with id 2
            WaitUtils.WaitUntil(
                new[] { host, host2 },
                () => ProxiesForFirstPeer(host2).Count == 1
                    && ProxiesForFirstPeer(host2).ContainsKey(new DistributedId(2)));
        }

        [Test]
        public void HostCreateAndDeleteProxyAfterConnection()
        {
            var testWorkQueue = new WorkQueue();

            // the first host under test
            using DistributedHost host = CreateHost(testWorkQueue, true);

            // construct second host
            using DistributedHost host2 = CreateHost(testWorkQueue, false);

            // host could start announcing also, but host2 isn't listening so it wouldn't be detectable
            host2.Announce();

            // create object
            var distributedThing = new DistributedThing(host, new LocalThing());

            // wait until the proxy for the new object makes it to the other host
            WaitUtils.WaitUntil(
                new[] { host, host2 },
                () => host2.NetPeers.Count() == 1 && ProxiesForFirstPeer(host2)?.Count == 1);

            // now delete the proxy
            ProxiesForFirstPeer(host2)[1].Delete();

            // wait until the messages flow around, and make sure there are now no proxies and no owners
            WaitUtils.WaitUntil(
                new[] { host, host2 },
                () => ProxiesForFirstPeer(host2).Count == 0 && host.Owners.Count == 0);
        }

        [Test]
        public void HostCreateWithState()
        {
            var testWorkQueue = new WorkQueue();

            // the first host under test
            using DistributedHost host = CreateHost(testWorkQueue, true);

            // construct second host
            using DistributedHost host2 = CreateHost(testWorkQueue, false);

            // host could start announcing also, but host2 isn't listening so it wouldn't be detectable
            host2.Announce();

            // should generate announce response and then connection
            WaitUtils.WaitUntil(new[] { host, host2 }, () => host.PeerCount == 1 && host2.PeerCount == 1);

            // create object
            var distributedThing = new DistributedThing(host, new LocalThing(new[] { 1, 2 }));

            // wait until the proxy for the new object makes it to the other host
            WaitUtils.WaitUntil(new[] { host, host2 }, () => ProxiesForFirstPeer(host2).Count == 1);

            LocalThing host2LocalThing = FirstProxyLocalThing(host2);

            Assert.IsTrue(Enumerable.SequenceEqual(new[] { 1, 2 }, host2LocalThing.LocalValues.ToList()));

            // now create an owner object on the other host
            var distributedThing2 = new DistributedThing(host2, new LocalThing(new[] { 3, 4 }));

            // wait until the proxy for the new object makes it to the first host
            WaitUtils.WaitUntil(new[] { host, host2 }, () => ProxiesForFirstPeer(host).Count == 1);

            IDistributedObject hostProxy = ProxiesForFirstPeer(host).Values.First();
            LocalThing hostLocalThing = (LocalThing)hostProxy.LocalObject;

            Assert.IsTrue(Enumerable.SequenceEqual(new[] { 3, 4 }, hostLocalThing.LocalValues.ToList()));
        }

        [Test]
        public void HostCreateThenUpdate()
        {
            var testWorkQueue = new WorkQueue();

            // the first host under test
            using DistributedHost host = CreateHost(testWorkQueue, true);

            // construct second host
            using DistributedHost host2 = CreateHost(testWorkQueue, false);

            // host could start announcing also, but host2 isn't listening so it wouldn't be detectable
            host2.Announce();

            // should generate announce response and then connection
            WaitUtils.WaitUntil(new[] { host, host2 }, () => host.PeerCount == 1 && host2.PeerCount == 1);

            // create object
            var distributedThing = new DistributedThing(host, new LocalThing());

            // wait until the proxy for the new object makes it to the other host
            WaitUtils.WaitUntil(new[] { host, host2 }, () => ProxiesForFirstPeer(host2).Count == 1);

            // change the state of the owner
            distributedThing.Enqueue(new[] { 1, 2 });

            // verify the proxy gets updated
            WaitUtils.WaitUntil(
                new[] { host, host2 },
                () => FirstProxyLocalThing(host2).LocalValues.Count() == 2);

            Assert.IsTrue(Enumerable.SequenceEqual(new[] { 1, 2 }, FirstProxyLocalThing(host2).LocalValues.ToList()));
        }

        [Test]
        public void HostCreateThenProxyUpdate()
        {
            var testWorkQueue = new WorkQueue();

            // the first host under test
            using DistributedHost host = CreateHost(testWorkQueue, true);

            // construct second host
            using DistributedHost host2 = CreateHost(testWorkQueue, false);

            // host could start announcing also, but host2 isn't listening so it wouldn't be detectable
            host2.Announce();

            // should generate announce response and then connection
            WaitUtils.WaitUntil(new[] { host, host2 }, () => host.PeerCount == 1 && host2.PeerCount == 1);

            // create object
            var distributedThing = new DistributedThing(host, new LocalThing());

            // wait until the proxy for the new object makes it to the other host
            WaitUtils.WaitUntil(new[] { host, host2 }, () => ProxiesForFirstPeer(host2).Count == 1);

            // this time update the state of the *proxy* -- or more precisely, send a request to the proxy
            // to update the owner's state (which will, in turn, update the proxy's state)
            DistributedThing host2DistributedThing = (DistributedThing)ProxiesForFirstPeer(host2).First().Value;
            host2DistributedThing.Enqueue(new[] { 1, 2 });

            // make sure we didn't actually update the local thing yet; only messages from the owner can do that
            Assert.AreEqual(0, host2DistributedThing.TypedLocalObject.LocalValues.Count());

            // wait until owner gets the proxy's memo
            WaitUtils.WaitUntil(
                new[] { host, host2 },
                () => distributedThing.TypedLocalObject.LocalValues.Count() == 2);

            // and now wait until proxy gets the owner's memo
            WaitUtils.WaitUntil(
                new[] { host, host2 },
                () => host2DistributedThing.TypedLocalObject.LocalValues.Count() == 2);

            Assert.IsTrue(Enumerable.SequenceEqual(new[] { 1, 2 }, distributedThing.TypedLocalObject.LocalValues.ToList()));
            Assert.IsTrue(Enumerable.SequenceEqual(new[] { 1, 2 }, host2DistributedThing.TypedLocalObject.LocalValues.ToList()));
        }


        [Test]
        public void HostCreateThenProxyBroadcast()
        {
            var testWorkQueue = new WorkQueue();

            // the first host under test
            using DistributedHost host = CreateHost(testWorkQueue, true);

            // construct second host
            using DistributedHost host2 = CreateHost(testWorkQueue, false);

            // host could start announcing also, but host2 isn't listening so it wouldn't be detectable
            host2.Announce();

            // should generate announce response and then connection
            WaitUtils.WaitUntil(new[] { host, host2 }, () => host.PeerCount == 1 && host2.PeerCount == 1);

            // create object
            var distributedThing = new DistributedThing(host, new LocalThing());

            // wait until the proxy for the new object makes it to the other host
            WaitUtils.WaitUntil(new[] { host, host2 }, () => ProxiesForFirstPeer(host2).Count == 1);

            // this time ping the proxy
            DistributedThing host2DistributedThing = (DistributedThing)ProxiesForFirstPeer(host2).First().Value;
            host2DistributedThing.Ping("hello".ToCharArray());

            // make sure we DID update the local thing; unreliable messages take effect immediately locally
            Assert.AreEqual("hello", new string(host2DistributedThing.TypedLocalObject.LastMessage));

            // wait until owner gets the broadcast
            WaitUtils.WaitUntil(
                new[] { host, host2 },
                () => distributedThing.TypedLocalObject.LastMessage != null);

            Assert.AreEqual("hello", new string(distributedThing.TypedLocalObject.LastMessage));
        }
    }
}
