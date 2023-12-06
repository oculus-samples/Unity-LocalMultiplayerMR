// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;
using Unity.Collections;

namespace com.meta.xr.colocation
{
    /// <summary>
    ///     A network agnostic struct that holds the data that needs to be sent for a person to join another persons colocated group
    /// </summary>
    [Serializable]
    public struct ShareAndLocalizeParams
    {
        public ulong requestingPlayerId;
        public ulong requestingPlayerOculusId;
        public FixedString64Bytes anchorUUID;
        public bool anchorFlowSucceeded;

        public ShareAndLocalizeParams(ulong requestingPlayerId, ulong requestingPlayerOculusId, string anchorUUID)
        {
            this.requestingPlayerId = requestingPlayerId;
            this.requestingPlayerOculusId = requestingPlayerOculusId;
            this.anchorUUID = anchorUUID;
            anchorFlowSucceeded = true;
        }

        public ShareAndLocalizeParams(ulong requestingPlayerId, ulong requestingPlayerOculusId, string anchorUUID,
            bool anchorFlowSucceeded)
        {
            this.requestingPlayerId = requestingPlayerId;
            this.requestingPlayerOculusId = requestingPlayerOculusId;
            this.anchorUUID = anchorUUID;
            this.anchorFlowSucceeded = anchorFlowSucceeded;
        }

        public override string ToString()
        {
            return
                $"{nameof(requestingPlayerId)}: {requestingPlayerId}, {nameof(requestingPlayerOculusId)}: {requestingPlayerOculusId}, {nameof(anchorUUID)}: {anchorUUID}, {nameof(anchorFlowSucceeded)}: {anchorFlowSucceeded}";
        }
    }
}
