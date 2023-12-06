// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;
using Unity.Collections;
using UnityEngine;

namespace com.meta.xr.colocation.pun2
{
    /// <summary>
    ///     PUN2 wrapper for the Anchor class
    ///     Used to be able to serialize and send the Anchor data over the network
    /// </summary>
    [Serializable]
    public struct PhotonAnchor : IEquatable<PhotonAnchor>
    {
        public bool isAutomaticAnchor;
        public bool isAlignmentAnchor;
        public ulong ownerOculusId;
        public uint colocationGroupId;
        public FixedString64Bytes automaticAnchorUuid;

        public PhotonAnchor(Anchor anchor)
        {
            this.isAutomaticAnchor = anchor.isAutomaticAnchor;
            this.isAlignmentAnchor = anchor.isAlignmentAnchor;
            this.ownerOculusId = anchor.ownerOculusId;
            this.colocationGroupId = anchor.colocationGroupId;
            this.automaticAnchorUuid = anchor.automaticAnchorUuid;
        }

        public bool Equals(PhotonAnchor other)
        {
            return isAutomaticAnchor == other.isAutomaticAnchor &&
                   isAlignmentAnchor == other.isAlignmentAnchor &&
                   ownerOculusId == other.ownerOculusId &&
                   colocationGroupId == other.colocationGroupId &&
                   automaticAnchorUuid == other.automaticAnchorUuid;
        }
    }

    [Serializable]
    public struct PhotonVector3 : IEquatable<PhotonVector3>
    {
        public float x;
        public float y;
        public float z;

        public PhotonVector3(Vector3 pos)
        {
            this.x = pos.x;
            this.y = pos.y;
            this.z = pos.z;
        }

        public bool Equals(PhotonVector3 other)
        {
            return x == other.x && y == other.y && z == other.z;
        }
    }

    [Serializable]
    public struct PhotonQuaternion : IEquatable<PhotonQuaternion>
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public PhotonQuaternion(Quaternion q)
        {
            this.x = q.x;
            this.y = q.y;
            this.z = q.z;
            this.w = q.w;
        }

        public bool Equals(PhotonQuaternion other)
        {
            return x == other.x && y == other.y && z == other.z && w == other.w;
        }
    }
}
