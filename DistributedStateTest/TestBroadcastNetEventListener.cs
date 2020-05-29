// Copyright (c) 2020 by Rob Jellinghaus.
using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace DistributedState.Test
{
    class TestBroadcastNetEventListener : INetEventListener
    {
        /// <summary>
        /// Net packet processor for deserializing.
        /// </summary>
        private readonly NetPacketProcessor netPacketProcessor;

        public TestBroadcastNetEventListener()
        {
            ReceivedMessages = new ConcurrentQueue<object>();
            netPacketProcessor = new NetPacketProcessor();
            netPacketProcessor.Subscribe(message => ReceivedMessages.Enqueue(message), () => new AnnounceMessage());
        }

        public ConcurrentQueue<object> ReceivedMessages { get; }

        public void OnConnectionRequest(ConnectionRequest request)
        {
        }

        public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            // ipv4 only for the moment... maybe someday do ipv6 switch, and then later duplicate suppression
            if (remoteEndPoint.AddressFamily == AddressFamily.InterNetwork)
            {
                netPacketProcessor.ReadAllPackets(reader);
            }
        }

        public void OnPeerConnected(NetPeer peer)
        {
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
        }
    }
}
