// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;
using Unity.Collections;

namespace com.meta.xr.colocation
{
    /// <summary>
    ///     A network agnostic class that holds the Anchor data that is shared between all players
    /// </summary>
    [Serializable]
    public struct Anchor : IEquatable<Anchor>
    {
        public bool isAutomaticAnchor;
        public bool isAlignmentAnchor;
        public ulong ownerOculusId;
        public uint colocationGroupId;
        public FixedString64Bytes automaticAnchorUuid;

        public Anchor(
            bool isAutomaticAnchor,
            bool isAlignmentAnchor,
            ulong ownerOculusId,
            uint colocationGroupId,
            FixedString64Bytes? automaticAnchorUuid = null
        )
        {
            this.isAutomaticAnchor = isAutomaticAnchor;
            this.isAlignmentAnchor = isAlignmentAnchor;
            this.ownerOculusId = ownerOculusId;
            this.colocationGroupId = colocationGroupId;

            this.automaticAnchorUuid = automaticAnchorUuid ?? "";
        }

        public bool Equals(Anchor other)
        {
            return isAutomaticAnchor == other.isAutomaticAnchor
                   && isAlignmentAnchor == other.isAlignmentAnchor
                   && ownerOculusId == other.ownerOculusId
                   && colocationGroupId == other.colocationGroupId
                   && automaticAnchorUuid == other.automaticAnchorUuid;
        }
    }
}
