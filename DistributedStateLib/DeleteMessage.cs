// Copyright (c) 2020 by Rob Jellinghaus.

namespace Distributed.State
{
    /// <summary>
    /// Message sent to delete existing proxy objects (or request deletion of an owner object).
    /// </summary>
    public abstract class DeleteMessage
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

        public DeleteMessage()
        { }

        public DeleteMessage(int id, bool isRequest)
        {
            Id = id;
            IsRequest = isRequest;
        }
    }
}
