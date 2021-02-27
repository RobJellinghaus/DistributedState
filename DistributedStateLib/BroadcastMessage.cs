// Copyright (c) 2020 by Rob Jellinghaus.

namespace Distributed.State
{
    /// <summary>
    /// Base message class containing Id and IsRequest properties.
    /// </summary>
    public abstract class BroadcastMessage : BaseMessage
    {
        /// <summary>
        /// Socket address of the owner of the object doing the broadcasting.
        /// </summary>
        public SerializedSocketAddress AnnouncerSocketAddress { get; set; }


        public BroadcastMessage()
        { }

        public BroadcastMessage(int id, SerializedSocketAddress sourceAddress)
        {
            Id = id;
        }
    }
}
