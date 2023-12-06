// Copyright (c) Meta Platforms, Inc. and affiliates.

namespace com.meta.xr.colocation
{
    public static class NetworkAdapter
    {
        /// <summary>
        ///   Provides a global reference to access INetworkData and INetworkMessenger
        /// </summary>
        public static INetworkData NetworkData { get; private set; }

        public static INetworkMessenger NetworkMessenger { get; private set; }

        public static void SetConfig(INetworkData networkData, INetworkMessenger networkMessenger)
        {
            NetworkData = networkData;
            NetworkMessenger = networkMessenger;
        }
    }
}
