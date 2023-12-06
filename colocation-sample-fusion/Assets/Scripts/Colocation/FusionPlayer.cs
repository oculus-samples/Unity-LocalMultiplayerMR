// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;
using Fusion;

namespace com.meta.xr.colocation.fusion
{
    /// <summary>
    ///     A Photon Fusion wrapper for the Player class
    ///     Used to be able to serialize and send the Player data over the network
    /// </summary>
    [Serializable]
    public struct FusionPlayer : INetworkStruct, IEquatable<FusionPlayer>
    {
        public ulong playerId;
        public ulong oculusId;
        public uint colocationGroupId;

        public FusionPlayer(Player player)
        {
            playerId = player.playerId;
            oculusId = player.oculusId;
            colocationGroupId = player.colocationGroupId;
        }

        public Player GetPlayer()
        {
            return new Player(playerId, oculusId, colocationGroupId);
        }

        public bool Equals(FusionPlayer other)
        {
            return GetPlayer().Equals(other.GetPlayer());
        }
    }
}
