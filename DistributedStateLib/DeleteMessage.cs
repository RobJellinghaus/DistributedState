﻿// Copyright (c) 2020 by Rob Jellinghaus.

namespace Distributed.State
{
    /// <summary>
    /// Message sent to delete existing proxy objects (or request deletion of an owner object).
    /// </summary>
    public abstract class DeleteMessage : ReliableMessage
    {
        public DeleteMessage()
        { }

        public DeleteMessage(DistributedId id, bool isRequest) : base(id, isRequest)
        {
        }

        public override void Invoke(IDistributedInterface target)
        {
            target.OnDelete();
        }
    }
}
