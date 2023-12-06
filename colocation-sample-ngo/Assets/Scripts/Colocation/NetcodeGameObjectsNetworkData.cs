// Copyright (c) Meta Platforms, Inc. and affiliates.

using System.Collections.Generic;
using Unity.Netcode;

namespace com.meta.xr.colocation.ngo
{
    /// <summary>
    ///     A Netcode for GameObjects concrete implementation of INetworkData
    ///     Used to manage a Player and Anchor list that all players that colocated are in
    /// </summary>
    public class NetcodeGameObjectsNetworkData : NetworkBehaviour, INetworkData
    {
        private readonly NetworkVariable<uint> colocationGroupCount = new();

        private NetworkList<NetcodeGameObjectsAnchor> _ngoAnchorList = null;
        private NetworkList<NetcodeGameObjectsPlayer> _ngoPlayerList = null;

        private void Awake()
        {
            _ngoAnchorList = new NetworkList<NetcodeGameObjectsAnchor>();
            _ngoPlayerList = new NetworkList<NetcodeGameObjectsPlayer>();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            _ngoAnchorList?.Dispose();
            _ngoPlayerList?.Dispose();
        }

        public void AddPlayer(Player player)
        {
            var ngoPlayer = new NetcodeGameObjectsPlayer(player);
            AddNGOPlayer(ngoPlayer);
        }

        public void RemovePlayer(Player player)
        {
            var ngoPlayer = new NetcodeGameObjectsPlayer(player);
            RemoveNGOPlayer(ngoPlayer);
        }

        public Player? GetPlayerWithPlayerId(ulong playerId)
        {
            foreach (NetcodeGameObjectsPlayer ngoPlayer in _ngoPlayerList)
            {
                if (ngoPlayer.Player.playerId == playerId)
                {
                    return ngoPlayer.Player;
                }
            }

            return null;
        }

        public Player? GetPlayerWithOculusId(ulong oculusId)
        {
            foreach (var ngoPlayer in _ngoPlayerList)
            {
                if (ngoPlayer.Player.oculusId == oculusId)
                {
                    return ngoPlayer.Player;
                }
            }

            return null;
        }

        public List<Player> GetAllPlayers()
        {
            var allPlayers = new List<Player>();
            foreach (var ngoPlayer in _ngoPlayerList) allPlayers.Add(ngoPlayer.Player);

            return allPlayers;
        }

        public void AddAnchor(Anchor anchor)
        {
            AddNGOAnchor(new NetcodeGameObjectsAnchor(anchor));
        }

        public void RemoveAnchor(Anchor anchor)
        {
            RemoveNGOAnchor(new NetcodeGameObjectsAnchor(anchor));
        }

        public Anchor? GetAnchor(ulong ownerOculusId)
        {
            foreach (var ngoAnchor in _ngoAnchorList)
                if (ngoAnchor.Anchor.ownerOculusId == ownerOculusId)
                {
                    return ngoAnchor.Anchor;
                }

            return null;
        }

        public List<Anchor> GetAllAnchors()
        {
            var anchors = new List<Anchor>();
            foreach (var ngoAnchor in _ngoAnchorList) anchors.Add(ngoAnchor.Anchor);

            return anchors;
        }

        public uint GetColocationGroupCount()
        {
            return colocationGroupCount.Value;
        }

        public void IncrementColocationGroupCount()
        {
            if (NetworkManager.Singleton.IsHost)
            {
                colocationGroupCount.Value = colocationGroupCount.Value + 1;
            }
            else
            {
                IncrementColocationGroupCountServerRpc();
            }
        }

        private void AddNGOPlayer(NetcodeGameObjectsPlayer ngoPlayer)
        {
            if (NetworkManager.Singleton.IsHost)
            {
                _ngoPlayerList.Add(ngoPlayer);
            }
            else
            {
                AddNGOPlayerServerRpc(ngoPlayer);
            }
        }

        private void RemoveNGOPlayer(NetcodeGameObjectsPlayer ngoPlayer)
        {
            if (NetworkManager.Singleton.IsHost)
            {
                _ngoPlayerList.Remove(ngoPlayer);
            }
            else
            {
                RemoveNGOPlayerServerRpc(ngoPlayer);
            }
        }

        private void AddNGOAnchor(NetcodeGameObjectsAnchor ngoAnchor)
        {
            if (NetworkManager.Singleton.IsHost)
            {
                _ngoAnchorList.Add(ngoAnchor);
            }
            else
            {
                AddNGOAnchorServerRpc(ngoAnchor);
            }
        }

        private void RemoveNGOAnchor(NetcodeGameObjectsAnchor ngoAnchor)
        {
            if (NetworkManager.Singleton.IsHost)
            {
                _ngoAnchorList.Remove(ngoAnchor);
            }
            else
            {
                RemoveNGOAnchorServerRpc(ngoAnchor);
            }
        }

        #region ServerRPCs

        [ServerRpc(RequireOwnership = false)]
        private void AddNGOPlayerServerRpc(NetcodeGameObjectsPlayer ngoPlayer)
        {
            AddNGOPlayer(ngoPlayer);
        }

        [ServerRpc(RequireOwnership = false)]
        private void RemoveNGOPlayerServerRpc(NetcodeGameObjectsPlayer ngoPlayer)
        {
            RemoveNGOPlayer(ngoPlayer);
        }

        [ServerRpc(RequireOwnership = false)]
        private void AddNGOAnchorServerRpc(NetcodeGameObjectsAnchor ngoAnchor)
        {
            AddNGOAnchor(ngoAnchor);
        }

        [ServerRpc(RequireOwnership = false)]
        private void RemoveNGOAnchorServerRpc(NetcodeGameObjectsAnchor ngoAnchor)
        {
            RemoveNGOAnchor(ngoAnchor);
        }

        [ServerRpc(RequireOwnership = false)]
        private void IncrementColocationGroupCountServerRpc()
        {
            IncrementColocationGroupCount();
        }

        #endregion
    }
}
