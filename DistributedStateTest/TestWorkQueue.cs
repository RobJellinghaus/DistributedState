// Copyright (c) 2020 by Rob Jellinghaus.
using System;
using System.Collections.Generic;

namespace Holofunk.DistributedState.Test
{
    /// <summary>
    /// Work queue that runs work when polled.
    /// </summary>
    class TestWorkQueue : IWorkQueue
    {
        /// <summary>
        /// The list of actions to run.
        /// </summary>
        private readonly List<Action> actions;

        public TestWorkQueue()
        {
            actions = new List<Action>();
        }

        /// <summary>
        /// The number of queued Actions.
        /// </summary>
        public int Count => actions.Count;

        /// <summary>
        /// Add the action; ignore the requested delay.
        /// </summary>
        /// <remarks>
        /// For testing purposes it is easier to just force the queued work deterministically.
        /// </remarks>
        public void RunLater(Action action, int delayMsec)
        {
            actions.Add(action);
        }

        public void RunQueuedWork()
        {
            // allocate copy so that if any actions queue more work, it gets queued after all current work
            Action[] actionsCopy = actions.ToArray();
            actions.Clear();
            foreach (Action action in actionsCopy)
            {
                action();
            }
        }
    }
}
