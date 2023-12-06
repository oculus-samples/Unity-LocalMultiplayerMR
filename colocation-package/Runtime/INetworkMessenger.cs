// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;

namespace com.meta.xr.colocation
{
    /// <summary>
    ///   An interface used to send RPC calls used for a player to join someone else's colocated space
    /// </summary>
    public interface INetworkMessenger
    {
        event Action<ShareAndLocalizeParams> AnchorShareRequestReceived;
        event Action<ShareAndLocalizeParams> AnchorShareRequestCompleted;

        public void SendAnchorShareRequest(ulong targetPlayerId, ShareAndLocalizeParams shareAndLocalizeParams);
        public void SendAnchorShareCompleted(ulong targetPlayerId, ShareAndLocalizeParams shareAndLocalizeParams);
    }
}
