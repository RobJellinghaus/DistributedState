using NUnit.Framework;

namespace Holofunk.DistributedState.Tests
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
            using Peer peer = new Peer(Peer.DefaultBroadcastPort, Peer.DefaultReliablePort);

            Assert.IsNotNull(peer);
        }
    }
}