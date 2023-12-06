// Copyright (c) Meta Platforms, Inc. and affiliates.

using System.Collections.Generic;
using Unity.Collections;

namespace com.meta.xr.colocation
{
    /// <summary>
    ///     An interface that is used to keep track of all players and anchors and can be accessed by any player
    /// </summary>
    public interface INetworkData
    {
        public void AddPlayer(Player player);
        public void RemovePlayer(Player player);
        public Player? GetPlayerWithPlayerId(ulong playerId);
        public Player? GetPlayerWithOculusId(ulong oculusId);
        public List<Player> GetAllPlayers();

        public void AddAnchor(Anchor anchor);
        public void RemoveAnchor(Anchor anchor);

        public Anchor? GetAnchor(ulong ownerOculusId);
        public List<Anchor> GetAllAnchors();

        public uint GetColocationGroupCount();

        public void IncrementColocationGroupCount();
    }
}
