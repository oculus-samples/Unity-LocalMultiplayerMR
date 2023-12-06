// Copyright (c) Meta Platforms, Inc. and affiliates.

using Unity.Netcode;

namespace com.meta.xr.colocation.ngo
{
    /// <summary>
    ///     A Netcode for GameObjects wrapper for ShareAndLocalizeParams
    ///     Used to be able to serialize and send the ShareAndLocalizeParams data over the network
    /// </summary>
    public struct NGOShareAndLocalizeParams : INetworkSerializeByMemcpy
    {
        public ShareAndLocalizeParams Data;

        public override string ToString()
        {
            return Data.ToString();
        }
    }
}
