// Copyright (c) 2020 by Rob Jellinghaus.

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
        /// Id of this object (in the originating peer's ID space).
        /// </summary>
        public int Id { get; set; }

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
