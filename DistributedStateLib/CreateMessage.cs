// Copyright (c) 2020 by Rob Jellinghaus.

namespace Distributed.State
{
    /// <summary>
    /// Message sent to create new proxy objects.
    /// </summary>
    /// <remarks>
    /// Derived classes include the initial state of the proxy object they create.
    /// 
    /// Note that derived classes get their own subscriptions; those subscriptions create
    /// proxy objects when CreateMessages are received.
    /// </remarks>
    public abstract class CreateMessage
    {
        /// <summary>
        /// Id of this object (in the originating peer's ID space).
        /// </summary>
        public int Id { get; set; }

        public CreateMessage(int id)
        {
            Id = id;
        }
    }
}
