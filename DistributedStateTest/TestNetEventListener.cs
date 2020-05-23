// Copyright (c) 2020 by Rob Jellinghaus.
using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Holofunk.DistributedState.Test
{
    class TestBroadcastNetEventListener : INetEventListener
    {
        /// <summary>
        /// Messages received.
        /// </summary>
        private readonly ConcurrentQueue<object> receivedMessages;

        /// <summary>
        /// Net packet processor for deserializing.
        /// </summary>
        private readonly NetPacketProcessor netPacketProcessor;

        private readonly NetDataReader netDataReader;

        public TestBroadcastNetEventListener()
        {
            receivedMessages = new ConcurrentQueue<object>();
            netPacketProcessor = new NetPacketProcessor();
            netPacketProcessor.Subscribe(message => receivedMessages.Enqueue(message), () => new AnnounceMessage());
        }

        public ConcurrentQueue<object> ReceivedMessages => receivedMessages;

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
            netPacketProcessor.ReadAllPackets(reader);
        }

        public void OnPeerConnected(NetPeer peer)
        {
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
        }
    }
}
