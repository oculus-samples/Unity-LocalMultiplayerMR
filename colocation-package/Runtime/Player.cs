// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;

namespace com.meta.xr.colocation
{
    /// <summary>
    ///     A network agnostic class that holds the Player data that is shared between all players
    /// </summary>
    [Serializable]
    public struct Player : IEquatable<Player>
    {
        public ulong playerId;
        public ulong oculusId;
        public uint colocationGroupId;

        public Player(ulong playerId, ulong oculusId, uint colocationGroupId)
        {
            this.playerId = playerId;
            this.oculusId = oculusId;
            this.colocationGroupId = colocationGroupId;
        }

        public bool Equals(Player other)
        {
            return playerId == other.playerId && oculusId == other.oculusId &&
                   colocationGroupId == other.colocationGroupId;
        }
    }
}
