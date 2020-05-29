// Copyright (c) 2020 by Rob Jellinghaus.
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace DistributedState
{
    /// <summary>
    /// Base type for objects that represent a serialized method invocation.
    /// </summary>
    /// <remarks>
    /// Note that all commands are asynchronous and void-returning; all state is kept locally,
    /// not passed as RPC return values.  (There are no return values from Commands.)
    /// 
    /// Commands are sent as the payload of CommandMessages (which are sent from
    /// owners to proxies, and are authoritative, reliable, and sequenced).  They are also sent
    /// as the payload of CommandRequestMessages (which are sent from proxies to owners, and
    /// are subject to races).
    /// </remarks>
    public abstract class Command
    {
        /// <summary>
        /// Execute this command on the associated target.
        /// </summary>
        /// <remarks>
        /// This should be implmented via C# 9 code generation, eventually. For now, hand-coded.
        /// </remarks>
        public abstract void Invoke<TDistributedInterface>(TDistributedInterface target);
    }
}
