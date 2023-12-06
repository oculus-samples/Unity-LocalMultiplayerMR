// Copyright (c) Meta Platforms, Inc. and affiliates.

using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using PhotonRealtime = Photon.Realtime;

namespace com.meta.xr.colocation.pun2
{
    /// <summary>
    ///     A PUN2 concrete implementation of INetworkData
    ///     Used to manage a Player and Anchor list that all players that colocated are in
    /// </summary>
    public class PhotonNetworkData : INetworkData, IInRoomCallbacks
    {
        private List<Player> _photonPlayerList;
        private List<PhotonAnchor> _photonAnchorList;
        private uint _colocationGroupCount;

        public void Init()
        {
            if (PhotonNetwork.CurrentRoom == null)
            {
                Logger.Log(
                    $"{nameof(PhotonNetworkData)}: Current Room is not ready make sure the player has joined a photon room",
                    LogLevel.Error);
            }

            InitPhotonPlayerList();
            InitPhotonAnchorList();
        }

        private void InitPhotonPlayerList()
        {
            Hashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;
            if (properties == null)
            {
                Logger.Log(
                    $"{nameof(PhotonNetworkData)}: PhotonNetwork.CurrentRoom.CustomProperties is null is the photon room set up properly?",
                    LogLevel.Error);
                return;
            }

            if (!properties.ContainsKey(PhotonCustomProperties.Player.ToString()))
            {
                _photonPlayerList = new List<Player>();
                byte[] dataToSend = ConvertFromPlayerListToByteArray(_photonPlayerList);
                Hashtable hashtable = new Hashtable();
                hashtable.Add(PhotonCustomProperties.Player.ToString(), dataToSend);
                PhotonNetwork.CurrentRoom.SetCustomProperties(hashtable);
            }
            else
            {
                object data = properties[PhotonCustomProperties.Player.ToString()];
                byte[] byteArray = (byte[])data;

                _photonPlayerList = ConvertFromByteArrayToPlayerList(byteArray);
            }
        }

        private void InitPhotonAnchorList()
        {
            Hashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;
            if (properties == null)
            {
                Logger.Log(
                    $"{nameof(PhotonNetworkData)}: InitPhotonAnchorList: PhotonNetwork.CurrentRoom.CustomProperties is null is the photon room set up properly?",
                    LogLevel.Error);
                return;
            }

            if (!properties.ContainsKey(PhotonCustomProperties.Anchor.ToString()))
            {
                _photonAnchorList = new List<PhotonAnchor>();
                byte[] dataToSend = ConvertFromPhotonAnchorListToByteArray(_photonAnchorList);
                Hashtable hashtable = new Hashtable();
                hashtable.Add(PhotonCustomProperties.Anchor.ToString(), dataToSend);
                PhotonNetwork.CurrentRoom.SetCustomProperties(hashtable);
            }
            else
            {
                object data = properties[PhotonCustomProperties.Anchor.ToString()];
                byte[] byteArray = (byte[])data;

                _photonAnchorList = ConvertFromByteArrayToPhotonAnchorList(byteArray);
            }
        }

        public void AddPlayer(Player player)
        {
            _photonPlayerList.Add(player);
            byte[] dataToSend = ConvertFromPlayerListToByteArray(_photonPlayerList);
            Hashtable properties = new Hashtable();
            properties.Add(PhotonCustomProperties.Player.ToString(), dataToSend);
            PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
        }

        public void RemovePlayer(Player player)
        {
            _photonPlayerList.Remove(player);
            byte[] dataToSend = ConvertFromPlayerListToByteArray(_photonPlayerList);
            Hashtable properties = new Hashtable();
            properties.Add(PhotonCustomProperties.Player.ToString(), dataToSend);
            PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
        }

        public Player? GetPlayerWithPlayerId(ulong playerId)
        {
            foreach (Player player in _photonPlayerList)
            {
                if (player.playerId == playerId)
                {
                    return player;
                }
            }

            return null;
        }

        public Player? GetPlayerWithOculusId(ulong oculusId)
        {
            foreach (Player player in _photonPlayerList)
            {
                if (player.oculusId == oculusId)
                {
                    return player;
                }
            }

            return null;
        }

        public List<Player> GetAllPlayers()
        {
            return _photonPlayerList;
        }

        public void AddAnchor(Anchor anchor)
        {
            PhotonAnchor photonAnchor = new PhotonAnchor(anchor);
            _photonAnchorList.Add(photonAnchor);
            byte[] dataToSend = ConvertFromPhotonAnchorListToByteArray(_photonAnchorList);
            Hashtable properties = new Hashtable();
            properties.Add(PhotonCustomProperties.Anchor.ToString(), dataToSend);
            PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
        }

        public void RemoveAnchor(Anchor anchor)
        {
            PhotonAnchor photonAnchor = new PhotonAnchor(anchor);
            _photonAnchorList.Remove(photonAnchor);
            byte[] dataToSend = ConvertFromPhotonAnchorListToByteArray(_photonAnchorList);
            Hashtable properties = new Hashtable();
            properties.Add(PhotonCustomProperties.Anchor.ToString(), dataToSend);
            PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
        }

        public Anchor? GetAnchor(ulong ownerOculusId)
        {
            foreach (var photonAnchor in _photonAnchorList)
            {
                if (photonAnchor.ownerOculusId == ownerOculusId)
                {
                    return CreateAnchorFromPhotonAnchor(photonAnchor);
                }
            }

            return null;
        }

        private Anchor CreateAnchorFromPhotonAnchor(PhotonAnchor photonAnchor)
        {
            Anchor anchor = new Anchor(
                photonAnchor.isAutomaticAnchor,
                photonAnchor.isAlignmentAnchor,
                photonAnchor.ownerOculusId,
                photonAnchor.colocationGroupId,
                photonAnchor.automaticAnchorUuid
            );

            return anchor;
        }

        public List<Anchor> GetAllAnchors()
        {
            List<Anchor> anchors = new List<Anchor>();
            foreach (PhotonAnchor photonAnchor in _photonAnchorList)
            {
                Anchor anchor = CreateAnchorFromPhotonAnchor(photonAnchor);
                anchors.Add(anchor);
            }

            return anchors;
        }

        public uint GetColocationGroupCount()
        {
            return _colocationGroupCount;
        }

        public void IncrementColocationGroupCount()
        {
            _colocationGroupCount++;
            byte[] dataToSend = ConvertFromUintToByteArray(_colocationGroupCount);
            Hashtable properties = new Hashtable();
            properties.Add(PhotonCustomProperties.GroupCount.ToString(), dataToSend);
            PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
        }

        public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            if (propertiesThatChanged.ContainsKey(PhotonCustomProperties.Player))
            {
                object data = propertiesThatChanged[PhotonCustomProperties.Player.ToString()];
                byte[] byteArray = (byte[])data;
                List<Player> photonPlayerList = ConvertFromByteArrayToPlayerList(byteArray);
                SavePlayerData(photonPlayerList);
            }

            if (propertiesThatChanged.ContainsKey(PhotonCustomProperties.Anchor))
            {
                object data = propertiesThatChanged[PhotonCustomProperties.Anchor.ToString()];
                byte[] byteArray = (byte[])data;
                List<PhotonAnchor> photonAnchorList = ConvertFromByteArrayToPhotonAnchorList(byteArray);
                SaveAnchorData(photonAnchorList);
            }

            if (propertiesThatChanged.ContainsKey(PhotonCustomProperties.GroupCount))
            {
                object data = propertiesThatChanged[PhotonCustomProperties.GroupCount.ToString()];
                byte[] byteArray = (byte[])data;
                uint groupCount = ConvertFromByteArrayToUint(byteArray);
                SaveColocationGroupCountData(groupCount);
            }
        }

        private void SavePlayerData(List<Player> photonPlayerList)
        {
            _photonPlayerList = photonPlayerList;
        }

        private void SaveAnchorData(List<PhotonAnchor> photonAnchorList)
        {
            _photonAnchorList = photonAnchorList;
        }

        private void SaveColocationGroupCountData(uint colocationGroupCount)
        {
            _colocationGroupCount = colocationGroupCount;
        }

        #region IInRoomCallbacksEmptyImplementation

        public void OnPlayerEnteredRoom(PhotonRealtime.Player newPlayer)
        {
            // Intentionally Empty
        }

        public void OnPlayerLeftRoom(PhotonRealtime.Player otherPlayer)
        {
            // Intentionally Empty
        }

        public void OnPlayerPropertiesUpdate(PhotonRealtime.Player targetPlayer, Hashtable changedProps)
        {
            // Intentionally Empty
        }

        public void OnMasterClientSwitched(PhotonRealtime.Player newMasterClient)
        {
            // Intentionally Empty
        }

        #endregion

        #region Converting Helper Functions

        private byte[] ConvertFromPlayerListToByteArray(List<Player> players)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream();
            binaryFormatter.Serialize(memoryStream, players);
            return memoryStream.ToArray();
        }

        private List<Player> ConvertFromByteArrayToPlayerList(byte[] byteArray)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream();
            memoryStream.Write(byteArray, 0, byteArray.Length);
            memoryStream.Position = 0;

            List<Player> players = binaryFormatter.Deserialize(memoryStream) as List<Player>;
            return players;
        }

        private byte[] ConvertFromPhotonAnchorListToByteArray(List<PhotonAnchor> anchors)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream();
            binaryFormatter.Serialize(memoryStream, anchors);
            return memoryStream.ToArray();
        }

        private List<PhotonAnchor> ConvertFromByteArrayToPhotonAnchorList(byte[] byteArray)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream();
            memoryStream.Write(byteArray, 0, byteArray.Length);
            memoryStream.Position = 0;

            List<PhotonAnchor> photonAnchors = binaryFormatter.Deserialize(memoryStream) as List<PhotonAnchor>;
            return photonAnchors;
        }

        private byte[] ConvertFromUintToByteArray(uint num)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream();
            binaryFormatter.Serialize(memoryStream, num);
            return memoryStream.ToArray();
        }

        private uint ConvertFromByteArrayToUint(byte[] byteArray)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream();
            memoryStream.Write(byteArray, 0, byteArray.Length);
            memoryStream.Position = 0;

            uint num = (uint)binaryFormatter.Deserialize(memoryStream);
            return num;
        }

        #endregion
    }
}
