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
    public abstract class CreateMessage : BaseMessage
    {
        public CreateMessage()
        { }

        public CreateMessage(int id) : base(id)
        { }
    }
}
