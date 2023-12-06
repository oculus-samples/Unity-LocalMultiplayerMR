// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;
using Fusion;

namespace com.meta.xr.colocation.fusion
{
    /// <summary>
    ///     A Photon Fusion Wrapper for the Anchor class
    ///     Used to be able to serialize and send the Anchor data over the network
    /// </summary>
    [Serializable]
    public struct FusionAnchor : INetworkStruct, IEquatable<FusionAnchor>
    {
        public NetworkBool isAutomaticAnchor;
        public NetworkBool isAlignmentAnchor;
        public ulong ownerOculusId;
        public uint colocationGroupId;
        public NetworkString<_64> automaticAnchorUuid;

        public FusionAnchor(Anchor anchor)
        {
            isAutomaticAnchor = anchor.isAutomaticAnchor;
            isAlignmentAnchor = anchor.isAlignmentAnchor;
            ownerOculusId = anchor.ownerOculusId;
            colocationGroupId = anchor.colocationGroupId;
            automaticAnchorUuid = anchor.automaticAnchorUuid.ToString();
        }

        public Anchor GetAnchor()
        {
            return new Anchor(isAutomaticAnchor, isAlignmentAnchor, ownerOculusId, colocationGroupId,
                automaticAnchorUuid.ToString());
        }

        public bool Equals(FusionAnchor other)
        {
            return GetAnchor().Equals(other.GetAnchor());
        }
    }
}
