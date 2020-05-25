// Copyright (c) 2020 by Rob Jellinghaus.
using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
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
    /// 
    /// The Peer's first responsibility is discovering other Peers. 
    /// </remarks>
    public class Peer : IPollEvents, IDisposable
    {
        private static string RequestKey = "";

        /// <summary>
        /// Listener handles unconnected messages (broadcasts), as well as connected
        /// messages from peers.
        /// </summary>
        private class Listener : INetEventListener
        {
            readonly Peer Peer;
            internal Listener(Peer peer)
            {
                Peer = peer;
            }
            public void OnConnectionRequest(ConnectionRequest request)
            {
                request.AcceptIfKey(RequestKey);
            }

            public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
            {
                // TBD what to do here
            }

            public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
            {
                // TBD whether anything should be done here
            }

            public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
            {
                Peer.netPacketProcessor.ReadAllPackets(reader, peer);
            }

            public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
            {
                Peer.netPacketProcessor.ReadAllPackets(reader, remoteEndPoint);
            }

            public void OnPeerConnected(NetPeer peer)
            {
                // TODO: send the peer all locally known objects
            }

            public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
            {
                // TODO: clean up all proxies owned by that peer
            }
        }

        /// <summary>
        /// Random port that happened to be, not only open, but with no other UDP or TCP ports in the 3????? range
        /// on my local Windows laptop.
        /// </summary>
        public static ushort DefaultListenPort = 9050;

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
        public readonly ushort ListenPort;

        /// <summary>
        /// The IPEndPoint as an IPV4 address, in host order; 
        /// since we are wifi only and assume IPV4 is locally available. (TBD if true)
        /// </summary>
        public readonly long IPV4Address;

        /// <summary>
        /// The LiteNetLib instance for handling broadcast traffic; has no peers.
        /// </summary>
        private NetManager netManager;

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
        public int PeerAnnounceCount { get; private set; }

        /// <summary>
        /// How many peer announcement responses has this peer received?
        /// </summary>
        /// <remarks>
        /// Only for testing.
        /// </remarks>
        public int PeerAnnounceResponseCount { get; private set; }

        /// <summary>
        /// Known IPV4 addresses of other peers.
        /// </summary>
        /// <remarks>
        /// This set is populated when Announce messages are received.
        /// </remarks>
        private HashSet<long> knownPeerIPV4Addresses = new HashSet<long>();

        public Peer(
            IWorkQueue workQueue,
            ushort listenPort,
            bool isListener = true)
        {
            Contract.Requires(listenPort != 0);

            ListenPort = listenPort;
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

            netManager = new NetManager(new Listener(this))
            {
                BroadcastReceiveEnabled = true,
                UnconnectedMessagesEnabled = true
            };

            netPacketProcessor = new NetPacketProcessor();
            netPacketProcessor.SubscribeReusable<AnnounceMessage, IPEndPoint>(OnAnnounceReceived);
            netPacketProcessor.SubscribeReusable<AnnounceResponseMessage, IPEndPoint>(OnAnnounceResponseReceived);

            netDataWriter = new NetDataWriter();

            bool managerStarted;
            if (isListener)
            {
                managerStarted = netManager.Start(ListenPort);
            }
            else
            {
                managerStarted = netManager.Start();
            }

            if (!managerStarted)
            {
                throw new PeerException("Could not start netManager");
            }
        }

        public int PeerCount => netManager.ConnectedPeersCount;

        /// <summary>
        /// Send an announcement message, and schedule the next such message.
        /// </summary>
        /// <remarks>
        /// After constructing a Peer, generally one calls Announce() just once
        /// to start the perpetual cycle of announcements that each Peer makes.
        /// </remarks>
        public void Announce()
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
            netManager.SendBroadcast(netDataWriter, ListenPort);
        }

        private void SendUnconnectedMessage<T>(T message, IPEndPoint endpoint)
            where T : class, new()
        {
            netDataWriter.Reset();
            netPacketProcessor.Write(netDataWriter, message);
            netManager.SendUnconnectedMessage(netDataWriter, endpoint);
        }

        /// <summary>
        /// An announcement has been received via broadcast; react accordingly.
        /// </summary>
        /// <param name="message"></param>
        private void OnAnnounceReceived(AnnounceMessage message, IPEndPoint endpoint)
        {
            // heed only ipv4 for now... TBD what to do about this
            if (endpoint.AddressFamily == AddressFamily.InterNetwork)
            {
                PeerAnnounceCount++;

                // do we know this peer already?
                // (could happen in race scenario)
                if (netManager.ConnectedPeerList.Any(peer => peer.EndPoint.Equals(endpoint)))
                {
                    return;
                }

                // did this peer know us already? (typical scenario given re-announcements)
                if (message.KnownPeers.Contains(IPV4Address))
                {
                    return;
                }

                // send announce response
                AnnounceResponseMessage response = new AnnounceResponseMessage { ResponderIPV4Address = IPV4Address };
                SendUnconnectedMessage(response, endpoint);
            }
        }

        /// <summary>
        /// An announcement has been received via broadcast; react accordingly.
        /// </summary>
        private void OnAnnounceResponseReceived(AnnounceResponseMessage message, IPEndPoint endpoint)
        {
            // heed only ipv4 for now... TBD what to do about this
            if (endpoint.AddressFamily == AddressFamily.InterNetwork)
            {
                PeerAnnounceResponseCount++;

                // we shouldn't know this peer yet, let's check.
                if (netManager.ConnectedPeerList.Any(peer => peer.EndPoint.Equals(endpoint)))
                {
                    // surprise, we do! must have been a rave.
                    return;
                }

                // So, connect away.
                netManager.Connect(endpoint, RequestKey);
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
            netManager.PollEvents();
        }

        public void Dispose()
        {
            netManager.Stop(true);
        }
    }
}
