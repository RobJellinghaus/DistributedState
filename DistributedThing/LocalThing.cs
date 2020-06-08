// Copyright (c) 2020 by Rob Jellinghaus.
using Distributed.State;
using System.Collections.Generic;

namespace Distributed.Thing
{
    public class LocalThing : LocalObject, IThing
    {
        /// <summary>
        /// Maximum number of 
        /// </summary>
        public static readonly int Capacity = 10;

        private Queue<int> state = new Queue<int>();

        public LocalThing()
        {
        }

        public void Enqueue(int[] values)
        {
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
