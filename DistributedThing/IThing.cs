// Copyright (c) 2020 by Rob Jellinghaus.

namespace Distributed.Thing
{
    using Distributed.State;
    using System.Collections.Generic;

    public interface IThing : IDistributedInterface
    {
        /// <summary>
        /// Enqueue these values.
        /// </summary>
        /// <remarks>
        /// Each Thing keeps a bounded queue of values.
        /// 
        /// As with all non-Local IDistributedInterface messages, calling this on a proxy will
        /// result in a request to the owner object.
        /// </remarks>
        void Enqueue(int[] values);

        /// <summary>
        /// Get the local values held by this Thing, in order from first bleated to last.
        /// </summary>
        /// <remarks>
        /// Distributed methods that begin with Local are implemented solely on the local object,
        /// and do not perform remote communication at all.
        /// </remarks>
        IEnumerable<int> LocalValues { get; }
    }
}
