using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Holofunk.DistributedState
{
    public static class Contract
    {
        public static void Requires(bool condition, string message = null)
        {
            if (!condition)
            {
                Debug.Assert(condition, message);
            }
        }
    }
}
