﻿// Copyright (c) 2020 by Rob Jellinghaus.
using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Distributed.State
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
    public class DistributedPeer : IPollEvents, IDisposable
    {
        private static string RequestKey = "";

        #region Listener inner class

        /// <summary>
        /// Listener handles unconnected messages (broadcasts), as well as connected
        /// messages from peers.
        /// </summary>
        private class Listener : INetEventListener
        {
            readonly DistributedPeer Peer;
            internal Listener(DistributedPeer peer)
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

            public void OnNetworkLatencyUpdate(NetPeer netPeer, int latency)
            {
                // TBD whether anything should be done here
            }

            public void OnNetworkReceive(NetPeer netPeer, NetPacketReader reader, DeliveryMethod deliveryMethod)
            {
                Peer.netPacketProcessor.ReadAllPackets(reader, netPeer);
            }

            public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
            {
                Peer.netPacketProcessor.ReadAllPackets(reader, remoteEndPoint);
            }

            public void OnPeerConnected(NetPeer netPeer)
            {
                Peer.proxies.Add(netPeer, new Dictionary<int, DistributedObject>());

                Peer.SendProxiesToPeer(netPeer);
            }

            /// <summary>
            /// Handle a disconnected peer by deleting all that peer's proxies' local objects, and dropping
            /// all the proxies.
            /// </summary>
            public void OnPeerDisconnected(NetPeer netPeer, DisconnectInfo disconnectInfo)
            {
                if (Peer.proxies.TryGetValue(netPeer, out Dictionary<int, DistributedObject> peerObjects))
                {
                    // delete them all
                    foreach (DistributedObject proxy in peerObjects.Values)
                    {
                        // Delete the local object only; calling Delete() on the proxy itself would result
                        // in a delete request to the owning peer, which just became inaccessible!
                        proxy.LocalObject.Delete();
                    }

                    // and drop the whole collection of proxies
                    Peer.proxies.Remove(netPeer);
                }
            }
        }

        #endregion

        #region Fields and properties

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
        /// The address of this Peer.
        /// </summary>
        public readonly SocketAddress SocketAddress;

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
        /// Map from owner object ID to owner object instance.
        /// </summary>
        /// <remarks>
        /// Note that these IDs are unique only within this Peer; each Peer defines its own ID space
        /// for its owned objects.
        /// </remarks>
        private Dictionary<int, DistributedObject> owners = new Dictionary<int, DistributedObject>();

        /// <summary>
        /// The next id to assign to a new owning object.
        /// </summary>
        private int nextOwnerId;

        /// <summary>
        /// Map from NetPeer to proxy ID to proxy instance.
        /// </summary>
        /// <remarks>
        /// Note that each ID is unique only within that peer's collection; each peer defines its
        /// own proxies' ID space.
        /// </remarks>
        private Dictionary<NetPeer, Dictionary<int, DistributedObject>> proxies
            = new Dictionary<NetPeer, Dictionary<int, DistributedObject>>();

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

        public int PeerCount => netManager.ConnectedPeersCount;

        public Dictionary<int, DistributedObject> Owners => owners;

        public IEnumerable<NetPeer> NetPeers => netManager.ConnectedPeerList;

        public Dictionary<int, DistributedObject> ProxiesForPeer(NetPeer peer)
        {
            Contract.Requires(netManager.ConnectedPeerList.Contains(peer));
            Contract.Requires(proxies.ContainsKey(peer));

            return proxies[peer];
        }

        #endregion

        #region Construction and disposal

        public DistributedPeer(
            IWorkQueue workQueue,
            ushort listenPort,
            bool isListener = true,
            int disconnectTimeout = -1)
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
            SocketAddress = new IPEndPoint(ipv4Address, listenPort).Serialize();

            netManager = new NetManager(new Listener(this))
            {
                BroadcastReceiveEnabled = true,
                UnconnectedMessagesEnabled = true
            };

            if (disconnectTimeout != -1)
            {
                netManager.DisconnectTimeout = disconnectTimeout;
            }

            netPacketProcessor = new NetPacketProcessor();
            SerializedSocketAddress.RegisterWith(netPacketProcessor);
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

        /// <summary>
        /// Dispose of this Peer, reclaiming network resources.
        /// </summary>
        public void Dispose()
        {
            netManager.Stop(true);
        }

        #endregion

        #region Managing DistributedObjects


        /// <summary>
        /// Get the next owner ID for this DistributedPeer.
        /// </summary>
        /// <remarks>
        /// This allows external code to create its own owner DistributedObjects, giving them fresh
        /// IDs at construction.
        /// </remarks>
        public int NextOwnerId()
        {
            return ++nextOwnerId;
        }

        /// <summary>
        /// This is a new owner DistributedObject entering the system on this peer.
        /// </summary>
        public void AddOwner(DistributedObject distributedObject)
        {
            owners.Add(distributedObject.Id, distributedObject);
            
            // and tell all the peers
            foreach (NetPeer netPeer in netManager.ConnectedPeerList)
            {
                SendProxiesToPeer(netPeer);
            }
        }

        /// <summary>
        /// This is a new proxy being created on this peer, owned by netPeer.
        /// </summary>
        public void AddProxy(NetPeer netPeer, DistributedObject proxy)
        {
            Contract.Requires(!proxy.IsOwner);

            proxies[netPeer].Add(proxy.Id, proxy);
        }

        /// <summary>
        /// Add a subscription for this message type.
        /// </summary>
        /// <remarks>
        /// Since the only polymorphism supported by LiteNetLib is for packets (e.g. messages),
        /// adding new DistributedObject implementations requires adding new messages, which requires
        /// subscribing to those new messages.
        /// </remarks>
        public void SubscribeReusable<TMessage, TUserData>(Action<TMessage, TUserData> action)
            where TMessage : class, new()
        {
            netPacketProcessor.SubscribeReusable(action);
        }

        #endregion

        #region Sending

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
                AnnouncerSocketAddress = new SerializedSocketAddress(SocketAddress),
                AnnouncerIsHostingAudio = false,
                KnownPeers = netManager
                    .ConnectedPeerList
                    .Select(peer => new SerializedSocketAddress(peer.EndPoint.Serialize()))
                    .ToArray()
            };

            SendBroadcastMessage(message);

            // schedule next announcement
            workQueue.RunLater(Announce, AnnounceDelayMsec);
        }

        /// <summary>
        /// Send this message as a broadcast.
        /// </summary>
        private void SendBroadcastMessage<T>(T message)
            where T : class, new()
        {
            netDataWriter.Reset();
            netPacketProcessor.Write(netDataWriter, message);
            netManager.SendBroadcast(netDataWriter, ListenPort);
        }

        /// <summary>
        /// Send this message directly, as an unconnected message.
        /// </summary>
        private void SendUnconnectedMessage<T>(T message, IPEndPoint endpoint)
            where T : class, new()
        {
            netDataWriter.Reset();
            netPacketProcessor.Write(netDataWriter, message);
            netManager.SendUnconnectedMessage(netDataWriter, endpoint);
        }

        /// <summary>
        /// Send this message directly, as a reliable (sequenced) message.
        /// </summary>
        public void SendReliableMessage<T>(T message, NetPeer netPeer)
            where T : class, new()
        {
            netDataWriter.Reset();
            netPacketProcessor.Write(netDataWriter, message);
            netPeer.Send(netDataWriter, DeliveryMethod.ReliableOrdered);
        }

        /// <summary>
        /// Send create messages for all our owned objects to this peer.
        /// </summary>
        private void SendProxiesToPeer(NetPeer netPeer)
        {
            foreach (KeyValuePair<int, DistributedObject> entry in owners)
            {
                entry.Value.LocalObject.SendProxyCreateMessage(this, netPeer);
            }
        }

        #endregion

        #region Receiving

        /// <summary>
        /// An announcement has been received (via broadcast); react accordingly.
        /// </summary>
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
                if (message.KnownPeers.Select(ssa => ssa.SocketAddress).Contains(SocketAddress))
                {
                    return;
                }

                // send announce response
                AnnounceResponseMessage response = new AnnounceResponseMessage { };
                SendUnconnectedMessage(response, endpoint);
            }
        }

        /// <summary>
        /// An announcement has been received (via unconnected messaeg); react accordingly.
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
                    // surprise, we do! must have been a race.
                    return;
                }

                // So, connect away.
                netManager.Connect(endpoint, RequestKey);
            }
        }

        public void OnCreateReceived(NetPeer creatingPeer, DistributedObject newProxy)
        {
            // make sure it's actually a proxy
            Contract.Requires(!newProxy.IsOwner);

            proxies[creatingPeer].Add(newProxy.Id, newProxy);
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

        #endregion
    }
}