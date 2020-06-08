// Copyright (c) 2020 by Rob Jellinghaus.

namespace Distributed.State
{
    /// <summary>
    /// Base message class containing Id and IsRequest properties.
    /// </summary>
    public abstract class BaseMessage
    {
        /// <summary>
        /// Id of this object (in the originating peer's ID space).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Is this a request to delete (from a proxy)?
        /// </summary>
        /// <remarks>
        /// If not, this is an authoritative deletion from the owner to a proxy.
        /// </remarks>
        public bool IsRequest { get; set; }

        public BaseMessage()
        { }

        public BaseMessage(int id, bool isRequest)
        {
            Id = id;
            IsRequest = isRequest;
        }
    }
}
