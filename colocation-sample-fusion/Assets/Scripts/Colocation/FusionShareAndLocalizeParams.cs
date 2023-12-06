// Copyright (c) Meta Platforms, Inc. and affiliates.

using Fusion;

namespace com.meta.xr.colocation.fusion
{
    /// <summary>
    ///     A Photon Fusion wrapper for ShareAndLocalizeParams
    ///     Used to be able to serialize and send the ShareAndLocalizeParams data over the network
    /// </summary>
    public struct FusionShareAndLocalizeParams : INetworkStruct
    {
        public ulong requestingPlayerId;
        public ulong requestingPlayerOculusId;
        public NetworkString<_64> anchorUUID;
        public NetworkBool anchorFlowSucceeded;

        public FusionShareAndLocalizeParams(ShareAndLocalizeParams data)
        {
            requestingPlayerId = data.requestingPlayerId;
            requestingPlayerOculusId = data.requestingPlayerOculusId;
            anchorUUID = data.anchorUUID.ToString();
            anchorFlowSucceeded = data.anchorFlowSucceeded;
        }

        public ShareAndLocalizeParams GetShareAndLocalizeParams()
        {
            return new ShareAndLocalizeParams(
                requestingPlayerId, requestingPlayerOculusId, anchorUUID.ToString(), anchorFlowSucceeded);
        }
    }
}
