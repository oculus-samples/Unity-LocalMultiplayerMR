// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;
using Unity.Netcode;

namespace com.meta.xr.colocation.ngo
{
    /// <summary>
    ///     A Netcode for GameObjects wrapper for the Player class
    ///     Used to be able to serialize and send the Player data over the network
    /// </summary>
    public struct NetcodeGameObjectsPlayer : INetworkSerializeByMemcpy, IEquatable<NetcodeGameObjectsPlayer>
    {
        public Player Player;

        public NetcodeGameObjectsPlayer(Player player)
        {
            this.Player = player;
        }

        public bool Equals(NetcodeGameObjectsPlayer other)
        {
            return Player.Equals(other.Player);
        }
    }
}
