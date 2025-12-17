// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Meta.XR.Samples;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace com.meta.xr.colocation.pun2
{
    /// <summary>
    ///     Class that handles the mapping between playerId and networkId
    /// </summary>
    [MetaCodeSample("LocalMultiplayerMR-PUN2")]
    public class PhotonIDDictionary : IInRoomCallbacks
    {
        private List<ulong> _playerIds;
        private List<int> _networkIds;

        public void Init()
        {
            PhotonNetwork.AddCallbackTarget(this);
            InitializePhotonIDDictionary();
        }

        private void InitializePhotonIDDictionary()
        {
            object data =
                PhotonNetwork.CurrentRoom.CustomProperties[PhotonCustomProperties.PhotonIDDictionary.ToString()];
            if (data == null)
            {
                Logger.Log($"{nameof(PhotonIDDictionary)}: Create PhotonDictionary", LogLevel.Verbose);
                _playerIds = new List<ulong>();
                _networkIds = new List<int>();
                PhotonDictionary photonDictionary = new PhotonDictionary(_playerIds, _networkIds);
                byte[] dataToSend = ConvertFromPhotonDictionaryToByteArray(photonDictionary);
                Hashtable hashtable = new Hashtable();
                hashtable.Add(PhotonCustomProperties.PhotonIDDictionary.ToString(), dataToSend);
                Logger.Log($"Set custom prop: {PhotonCustomProperties.PhotonIDDictionary.ToString()}",
                    LogLevel.Verbose);
                PhotonNetwork.CurrentRoom.SetCustomProperties(hashtable);
                Logger.Log("Finished setting custom prop", LogLevel.Verbose);
            }
            else
            {
                Logger.Log("PhotonIDDictionary: Getting Rooms Custom Properties", LogLevel.Verbose);
                byte[] byteArray = (byte[])data;
                PhotonDictionary photonDictionary = ConvertFromByteArrayToPhotonDictionary(byteArray);
                _playerIds = photonDictionary.playerIds;
                _networkIds = photonDictionary.networkIds;
            }
        }

        private byte[] ConvertFromPhotonDictionaryToByteArray(PhotonDictionary photonDictionary)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream();
            binaryFormatter.Serialize(memoryStream, photonDictionary);
            return memoryStream.ToArray();
        }

        private PhotonDictionary ConvertFromByteArrayToPhotonDictionary(byte[] byteArray)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream();
            memoryStream.Write(byteArray, 0, byteArray.Length);
            memoryStream.Position = 0;

            PhotonDictionary photonDictionary = (PhotonDictionary)binaryFormatter.Deserialize(memoryStream);
            return photonDictionary;
        }

        public void AddOrSet(ulong playerId, int photonId)
        {
            bool playerIdAlreadyExists = false;
            for (int i = 0; i < _playerIds.Count; i++)
            {
                if (playerId == _playerIds[i])
                {
                    _networkIds[i] = photonId;
                    playerIdAlreadyExists = true;
                    break;
                }
            }

            if (!playerIdAlreadyExists)
            {
                _playerIds.Add(playerId);
                _networkIds.Add(photonId);
            }

            UpdateRoomProperties();
        }

        public void Remove(ulong playerId)
        {
            for (int i = 0; i < _playerIds.Count; i++)
            {
                if (playerId == _playerIds[i])
                {
                    _playerIds.RemoveAt(i);
                    _networkIds.RemoveAt(i);
                    return;
                }
            }

            Debug.LogError($"PhotonIDDictionary could not find playerId: {playerId} to remove");
        }

        private void UpdateRoomProperties()
        {
            Hashtable hashtable = new Hashtable();
            PhotonDictionary photonDictionary = new PhotonDictionary(_playerIds, _networkIds);
            byte[] dataToSend = ConvertFromPhotonDictionaryToByteArray(photonDictionary);
            hashtable.Add(PhotonCustomProperties.PhotonIDDictionary.ToString(), dataToSend);
            PhotonNetwork.CurrentRoom.SetCustomProperties(hashtable);
        }

        public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            if (!propertiesThatChanged.ContainsKey(PhotonCustomProperties.PhotonIDDictionary.ToString()))
            {
                return;
            }

            object data =
                PhotonNetwork.CurrentRoom.CustomProperties[PhotonCustomProperties.PhotonIDDictionary.ToString()];
            byte[] byteArrayData = (byte[])data;

            PhotonDictionary photonDictionary = ConvertFromByteArrayToPhotonDictionary(byteArrayData);
            _playerIds = photonDictionary.playerIds;
            _networkIds = photonDictionary.networkIds;
        }

        #region IInRoomCallbacks Empty Implementations

        public void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
        {
        }

        public void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
        {
        }

        public void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, Hashtable changedProps)
        {
        }

        public void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
        {
        }

        #endregion

        #region IIDDictionary Implementations

        public object GetNetworkIdFromPlayerId(ulong playerId)
        {
            for (int i = 0; i < _playerIds.Count; i++)
            {
                if (playerId == _playerIds[i])
                {
                    return _networkIds[i];
                }
            }

            return null;
        }

        public ulong? GetPlayerIdFromNetworkId(object networkId)
        {
            int intNetworkId = (int)networkId;
            for (int i = 0; i < _networkIds.Count; i++)
            {
                if (intNetworkId == _networkIds[i])
                {
                    return _playerIds[i];
                }
            }

            return null;
        }

        public bool ContainsNetworkId(object networkId)
        {
            var intNetworkId = (int)networkId;
            return _networkIds.Contains(intNetworkId);
        }

        public bool ContainsPlayerId(ulong playerId)
        {
            return _playerIds.Contains(playerId);
        }

        #endregion
    }

    [Serializable]
    public struct PhotonDictionary
    {
        public List<ulong> playerIds;
        public List<int> networkIds;

        public PhotonDictionary(List<ulong> playerIds, List<int> networkIds)
        {
            this.playerIds = playerIds;
            this.networkIds = networkIds;
        }
    }
}
