// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;
using Unity.Netcode;

namespace com.meta.xr.colocation.ngo
{
    /// <summary>
    ///     A Netcode for GameObjects wrapper for the Anchor class
    ///     Used to be able to serialize and send the Anchor data over the network
    /// </summary>
    public struct NetcodeGameObjectsAnchor : INetworkSerializeByMemcpy, IEquatable<NetcodeGameObjectsAnchor>
    {
        public Anchor Anchor;

        public NetcodeGameObjectsAnchor(Anchor anchor)
        {
            this.Anchor = anchor;
        }

        public bool Equals(NetcodeGameObjectsAnchor other)
        {
            return Anchor.Equals(other.Anchor);
        }
    }
}
