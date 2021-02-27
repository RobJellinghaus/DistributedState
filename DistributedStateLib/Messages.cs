// Copyright (c) 2020 by Rob Jellinghaus.

using LiteNetLib;
using System;

namespace Distributed.State
{
    public class Messages
    {
        /// <summary>
        /// Given an incoming message (which may be a request from a proxy, or may be an authoritative message
        /// from the owner), look up the referenced object and invoke the message appropriately.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <typeparam name="TObject">The actual type of distributed object.</typeparam>
        /// <typeparam name="TLocalObject">The corresponding type of local object.</typeparam>
        /// <typeparam name="TInterface">The distributed interface implemented by both the distributed and local objects.</typeparam>
        /// <param name="host">The distributed host.</param>
        /// <param name="netPeer">The peer from which this message originated.</param>
        /// <param name="message">The message.</param>
        /// <param name="invokeAction">An action which will invoke the message on either the owner DistributedObject, or on
        /// the proxy's local object.</param>
        public static void HandleReliableMessage<TMessage, TObject, TLocalObject, TInterface>(
            DistributedHost host,
            NetPeer netPeer,
            TMessage message,
            Action<TMessage, TInterface> invokeAction)
            where TMessage : ReliableMessage
            where TLocalObject : ILocalObject, TInterface
            where TObject : DistributedObject<TLocalObject>, TInterface
            where TInterface : IDistributedInterface
        {
            if (message.IsRequest)
            {
                // ignore messages to objects that no longer exist
                DistributedObject target;
                if (host.Owners.TryGetValue(message.Id, out target))
                {
                    invokeAction(message, (TObject)target);
                }
            }
            else
            {
                // this object really ought to exist, but in case it doesn't (maybe disconnection race?),
                // ignore object targets that don't resolve.
                DistributedObject target;
                if (host.ProxiesForPeer(netPeer).TryGetValue(message.Id, out target))
                {
                    // Call straight through to the local object; don't invoke Enqueue on the proxy.
                    // (If we do, it will call back to the owner, and whammo, infinite loop!)
                    invokeAction(message, ((TObject)target).TypedLocalObject);
                }
            }
        }
    }
}
