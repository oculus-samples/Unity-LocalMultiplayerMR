// Copyright (c) Meta Platforms, Inc. and affiliates.

using Meta.XR.Samples;

namespace com.meta.xr.colocation
{
    [MetaCodeSample("LocalMultiplayerMR.Core-Colocation")]
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
