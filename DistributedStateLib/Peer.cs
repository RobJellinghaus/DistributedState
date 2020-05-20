using LiteNetLib;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Holofunk.DistributedState
{
    /// <summary>
    /// Peer participant in the distributed system.
    /// </summary>
    /// <remarks>
    /// This encapsulates a LiteNetLib NetManager instance, used for both broadcast discovery
    /// and update, and reliable peer-to-peer communication.
    /// </remarks>
    public class Peer : IDisposable
    {
        private class BroadcastListener : INetEventListener
        {
            readonly Peer Peer;
            internal BroadcastListener(Peer peer)
            {
                Peer = peer;
            }
            public void OnConnectionRequest(ConnectionRequest request)
            {
                // this listener receives no connection requests
            }

            public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
            {
                throw new NotImplementedException();
            }

            public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
            {
                // this listener has no peers
            }

            public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
            {
                // this listener only receives broadcasts
            }

            public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
            {
                throw new NotImplementedException();
            }

            public void OnPeerConnected(NetPeer peer)
            {
                // peers don't connect to this listener
            }

            public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
            {
                // peers don't connect to this listener
            }
        }

        private class ReliableListener : INetEventListener
        {
            readonly Peer Peer;
            internal ReliableListener(Peer peer)
            {
                Peer = peer;
            }

            public void OnConnectionRequest(ConnectionRequest request)
            {
                throw new NotImplementedException();
            }

            public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
            {
                throw new NotImplementedException();
            }

            public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
            {
                throw new NotImplementedException();
            }

            public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
            {
                throw new NotImplementedException();
            }

            public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
            {
                // this listener never receives unconnected data
            }

            public void OnPeerConnected(NetPeer peer)
            {
                throw new NotImplementedException();
            }

            public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Random port that happened to be, not only open, but with no other UDP or TCP ports in the 3????? range
        /// on my local Windows laptop.
        /// </summary>
        public static ushort DefaultBroadcastPort = 30303;

        /// <summary>
        /// Random port that happened to be, not only open, but with no other UDP or TCP ports in the 3????? range
        /// on my local Windows laptop.
        /// </summary>
        public static ushort DefaultReliablePort = 30304;

        /// <summary>
        /// The broadcast port for announcing new peers and disseminating information.
        /// </summary>
        /// <remarks>
        /// Maybe shouldn't overload broadcast port; we'll see.
        /// </remarks>
        public readonly ushort BroadcastPort;

        /// <summary>
        /// The reliable port for peer-to-peer mandatory communication.
        /// </summary>
        public readonly ushort ReliablePort;

        /// <summary>
        /// The IPEndPoint, encoded as 32 bits; since we are wifi only and assume IPV4 is locally available. (TBD if true)
        /// </summary>
        public readonly IPAddress IPV4Address;

        /// <summary>
        /// The LiteNetLib instance for handling broadcast traffic.
        /// </summary>
        private NetManager broadcastManager;

        /// <summary>
        /// The LiteNetLib instance for handling reliable traffic.
        /// </summary>
        private NetManager reliableManager;

        public Peer(ushort broadcastPort, ushort reliablePort)
        {
            Contract.Requires(broadcastPort != 0);
            Contract.Requires(reliablePort != 0);

            // determine our IP
            // hat tip https://stackoverflow.com/questions/6803073/get-local-ip-address
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

            IPV4Address = host
                .AddressList
                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

            broadcastManager = new NetManager(new BroadcastListener(this));
            reliableManager = new NetManager(new ReliableListener(this));

            bool broadcastManagerStarted = broadcastManager.Start(BroadcastPort);
            if (!broadcastManagerStarted)
            {
                throw new PeerException("Could not start broadcastManager");
            }

            bool reliableManagerStarted = reliableManager.Start(ReliablePort);
            if (!reliableManagerStarted)
            {
                throw new PeerException("Could not start reliableManager");
            }
        }

        public void Dispose()
        {
            if (broadcastManager.ConnectedPeersCount != 0)
            {
                throw new PeerException("broadcastManager should never have any peers");
            }

            reliableManager.DisconnectAll();
            reliableManager.Flush();
        }
    }
}
