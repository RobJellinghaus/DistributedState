// Copyright (c) 2020 by Rob Jellinghaus.
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace DistributedState
{
    /// <summary>
    /// Message sent to create new proxy objects.
    /// </summary>
    /// <remarks>
    /// Includes the initial state of the proxy object.
    /// </remarks>
    public class CreateMessage
    {
        /// <summary>
        /// The initial state of the proxy object.
        /// </summary>
        /// <remarks>
        /// Note that the LocalState derived class will implement methods for actually
        /// instantiating the correct type of distributed proxy object.
        /// </remarks>
        public LocalState InitialState { get; set; }
    }
}
