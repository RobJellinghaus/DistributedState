// Copyright (c) 2020 by Rob Jellinghaus.
using LiteNetLib;

namespace DistributedState.Test
{
    /// <summary>
    /// Test class wrapping a NetManager.
    /// </summary>
    class TestNetManager : IPollEvents
    {
        public readonly NetManager NetManager;

        public TestNetManager(NetManager netManager)
        {
            NetManager = netManager;
        }

        public void PollEvents()
        {
            NetManager.PollEvents();
        }
    }
}
