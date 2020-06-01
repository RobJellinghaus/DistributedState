// Copyright (c) 2020 by Rob Jellinghaus.
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Distributed.State.Test
{
    /// <summary>
    /// Methods to enter polling sleep loops until conditions are met or timeouts happen.
    /// </summary>
    /// <remarks>
    /// Uses only Thread.Sleep and synchronous loops.
    /// </remarks>
    public static class WaitUtils
    {
        public static int SleepMsec = 5;
        public static int IterationsBeforeFailure = 2000;

        /// <summary>
        /// Sleep-retry loop, polling all NetManagers on each iteration, exiting once function becomes true.
        /// </summary>
        public static void WaitUntil(IEnumerable<IPollEvents> pollables, Func<bool> function)
        {
            bool ok = false;
            for (int i = 0; i < IterationsBeforeFailure; i++)
            {
                foreach (IPollEvents pollable in pollables)
                {
                    pollable.PollEvents();
                }
                if (function())
                {
                    ok = true;
                    break;
                }
                Thread.Sleep(SleepMsec);
            }

            if (!ok)
            {
                Assert.Fail("WaitUtils.WaitUntil: reached maximum retries; failing");
            }
        }
    }
}
