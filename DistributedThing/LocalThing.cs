// Copyright (c) 2020 by Rob Jellinghaus.
using Distributed.State;
using System;
using System.Collections.Generic;

namespace Distributed.Thing
{
    /// <summary>
    /// A trivial local object implementation that just keeps a bounded queue of integers.
    /// </summary>
    public class LocalThing : LocalObject, IThing
    {
        /// <summary>
        /// Maximum number of values tracked.
        /// </summary>
        public static readonly int Capacity = 10;

        private Queue<int> state = new Queue<int>();

        public LocalThing(int[] values = null)
        {
            Enqueue(values ?? new int[] { });
        }

        public void Enqueue(int[] values)
        {
            Contract.Requires(values != null);
            Contract.Requires(values.Length <= Capacity);

            while (values.Length + state.Count > Capacity)
            {
                state.Dequeue();
            }
            foreach (int value in values)
            {
                state.Enqueue(value);
            }
        }

        public IEnumerable<int> LocalValues => state;

        public override void Delete()
        {
            // do nothing... accept the void
        }
    }
}
