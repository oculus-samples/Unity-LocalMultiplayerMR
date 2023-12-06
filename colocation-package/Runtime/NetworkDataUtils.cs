// Copyright (c) Meta Platforms, Inc. and affiliates.

using System.Collections.Generic;

namespace com.meta.xr.colocation
{
    public static class NetworkDataUtils
    {
        public static ulong? GetOculusIdOfColocatedGroupOwnerFromColocationGroupId(uint colocationGroupId)
        {
            INetworkData data = NetworkAdapter.NetworkData;
            List<Anchor> anchors = data.GetAllAnchors();
            foreach (Anchor anchor in anchors)
            {
                if (colocationGroupId == anchor.colocationGroupId)
                {
                    return anchor.ownerOculusId;
                }
            }

            return null;
        }

        public static List<Player> GetAllPlayersFromColocationGroupId(uint colocationGroupId)
        {
            INetworkData data = NetworkAdapter.NetworkData;
            List<Player> players = data.GetAllPlayers();
            List<Player> colocatedPlayers = new List<Player>();
            foreach (Player player in players)
            {
                if (colocationGroupId == player.colocationGroupId)
                {
                    colocatedPlayers.Add(player);
                }
            }

            return colocatedPlayers;
        }

        public static List<Player> GetAllPlayersColocatedWith(ulong oculusId, bool includeMyself)
        {
            INetworkData data = NetworkAdapter.NetworkData;
            List<Player> players = data.GetAllPlayers();
            Player currentPlayer = data.GetPlayerWithOculusId(oculusId).Value;
            uint colocatedGroupId = currentPlayer.colocationGroupId;
            List<Player> playersColocatedWithMe = new List<Player>();

            foreach (Player player in players)
            {
                if (player.colocationGroupId == colocatedGroupId)
                {
                    playersColocatedWithMe.Add(player);
                    if (!includeMyself && player.oculusId == oculusId)
                    {
                        playersColocatedWithMe.RemoveAt(playersColocatedWithMe.Count - 1);
                    }
                }
            }

            return playersColocatedWithMe;
        }

        public static Player? GetPlayerFromOculusId(ulong oculusId)
        {
            INetworkData data = NetworkAdapter.NetworkData;
            return data.GetPlayerWithOculusId(oculusId);
        }
    }
}
