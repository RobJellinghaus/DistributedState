// Copyright (c) 2020 by Rob Jellinghaus.
using LiteNetLib;
using LiteNetLib.Utils;
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
    public class Peer : IPollEvents, IDisposable
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
                Peer.netPacketProcessor.ReadAllPackets(reader, remoteEndPoint);
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
        public static ushort DefaultBroadcastPort = 9050;

        /// <summary>
        /// Random port that happened to be, not only open, but with no other UDP or TCP ports in the 3????? range
        /// on my local Windows laptop.
        /// </summary>
        public static ushort DefaultReliablePort = 30304;

        /// <summary>
        /// Delay between announce messages.
        /// </summary>
        public static int AnnounceDelayMsec = 100;

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
        /// The IPEndPoint as an IPV4 address, in host order; 
        /// since we are wifi only and assume IPV4 is locally available. (TBD if true)
        /// </summary>
        public readonly long IPV4Address;

        /// <summary>
        /// The LiteNetLib instance for handling broadcast traffic; has no peers.
        /// </summary>
        private NetManager broadcastManager;

        /// <summary>
        /// The LiteNetLib instance for handling reliable traffic; has one NetPeer per other participant.
        /// </summary>
        private NetManager reliableManager;

        /// <summary>
        /// Reusable NetDataWriter instance; reset before use.
        /// </summary>
        private NetDataWriter netDataWriter;

        /// <summary>
        /// Packet processor that handles type mapping on the wire.
        /// </summary>
        private NetPacketProcessor netPacketProcessor;

        /// <summary>
        /// The IWorkQueue used for scheduling future work.
        /// </summary>
        private readonly IWorkQueue workQueue;

        /// <summary>
        /// How many peer announcements has this peer received?
        /// </summary>
        /// <remarks>
        /// Only for testing.
        /// </remarks>
        public int PeerAnnouncementCount { get; private set; }

        public Peer(
            IWorkQueue workQueue,
            ushort broadcastPort,
            ushort reliablePort,
            bool listenForPeerAnnouncements = true)
        {
            Contract.Requires(broadcastPort != 0);
            Contract.Requires(reliablePort != 0);

            BroadcastPort = broadcastPort;
            ReliablePort = reliablePort;
            this.workQueue = workQueue;

            // determine our IP
            // hat tip https://stackoverflow.com/questions/6803073/get-local-ip-address
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

            IPAddress ipv4Address = host
                .AddressList
                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
#pragma warning disable CS0618 // Type or member is obsolete
            long ipv4AddressAddress = ipv4Address.Address;
#pragma warning restore CS0618 // Type or member is obsolete
            IPV4Address = IPAddress.NetworkToHostOrder(ipv4AddressAddress);

            broadcastManager = new NetManager(new BroadcastListener(this))
            {
                BroadcastReceiveEnabled = true,
                UnconnectedMessagesEnabled = true
            };

            reliableManager = new NetManager(new ReliableListener(this));

            netPacketProcessor = new NetPacketProcessor();
            netPacketProcessor.SubscribeReusable<AnnounceMessage, IPEndPoint>(OnAnnouncementReceived);

            netDataWriter = new NetDataWriter();

            bool broadcastManagerStarted;
            if (listenForPeerAnnouncements)
            {
                broadcastManagerStarted = broadcastManager.Start(BroadcastPort);
            }
            else
            {
                broadcastManagerStarted = broadcastManager.Start();
            }

            if (!broadcastManagerStarted)
            {
                throw new PeerException("Could not start broadcastManager");
            }

            bool reliableManagerStarted = reliableManager.Start(ReliablePort);
            if (!reliableManagerStarted)
            {
                throw new PeerException("Could not start reliableManager");
            }

            // send a dang announce message
            Announce();
        }

        public int PeerCount => reliableManager.ConnectedPeersCount;

        /// <summary>
        /// Send an announcement message, and schedule the next such message.
        /// </summary>
        private void Announce()
        {
            AnnounceMessage message = new AnnounceMessage
            {
                AnnouncerIPV4Address = IPV4Address,
                AnnouncerIsHostingAudio = false,
                KnownPeers = new long[0]
            };

            SendBroadcastMessage(message);

            // schedule next announcement
            workQueue.RunLater(Announce, AnnounceDelayMsec);
        }

        private void SendBroadcastMessage<T>(T message)
            where T : class, new()
        {
            netDataWriter.Reset();
            netPacketProcessor.Write(netDataWriter, message);
            broadcastManager.SendBroadcast(netDataWriter, BroadcastPort);
        }

        /// <summary>
        /// An announcement has been received via broadcast; react accordingly.
        /// </summary>
        /// <param name="message"></param>
        private void OnAnnouncementReceived(AnnounceMessage message, IPEndPoint userData)
        {
            // heed only ipv4 for now... TBD what to do about this
            if (userData.AddressFamily == AddressFamily.InterNetwork)
            {
                PeerAnnouncementCount++;

                if (message.AnnouncerIPV4Address == IPV4Address)
                {
                    // we sent this, ignore it
                }
            }
        }

        /// <summary>
        /// Poll all network events pending.
        /// </summary>
        /// <remarks>
        /// Called from game thread; calls to this must not be concurrent with work that has
        /// been queued on the work queue.
        /// </remarks>
        public void PollEvents()
        {
            broadcastManager.PollEvents();
            reliableManager.PollEvents();
        }

        public void Dispose()
        {
            if (broadcastManager.ConnectedPeersCount != 0)
            {
                throw new PeerException("broadcastManager should never have any peers");
            }

            broadcastManager.Stop();
            reliableManager.Stop(true);
        }
    }
}
