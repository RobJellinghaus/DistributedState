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

        public BaseMessage()
        { }

        public BaseMessage(int id)
        {
            Id = id;
        }
    }
}
