// Copyright (c) Meta Platforms, Inc. and affiliates.

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using com.meta.xr.colocation.pun2;
using com.meta.xr.colocation.pun2.debug;
using Oculus.Platform;
using Oculus.Platform.Models;
using Photon.Pun;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;

namespace com.meta.xr.colocation.samples.pun2
{
    /// <summary>
    ///     A class that handles setting up and initializing colocation
    /// </summary>
    public class PUNNetworkBootstrapper : MonoBehaviour
    {
        [SerializeField] private GameObject AnchorPrefab;
        [SerializeField] private Nametag nametagPrefab;

        private string _myOculusName;
        private ulong _myPlayerId;
        private ulong _myOculusId;
        private OVRCameraRig _ovrCameraRig;
        private PhotonIDDictionary _idDictionary;

        public void Init()
        {
            _ovrCameraRig = FindObjectOfType<OVRCameraRig>();
            _myPlayerId = (ulong)SystemInfo.deviceUniqueIdentifier.GetHashCode();
            Core.Initialize();
            Users.GetLoggedInUser().OnComplete(GetLoggedInUserCallback);
        }

        private void GetLoggedInUserCallback(Message message)
        {
            if (message.IsError)
            {
                Logger.Log(
                    $"{nameof(PUNNetworkBootstrapper)}: Failed to receive a message from {nameof(GetLoggedInUserCallback)}",
                    LogLevel.Error);
                return;
            }

            Logger.Log(
                $"{nameof(PUNNetworkBootstrapper)}: Success in receiving a message from {nameof(GetLoggedInUserCallback)}",
                LogLevel.Verbose);
            bool isLoggedInUserMessage = message.Type == Message.MessageType.User_GetLoggedInUser;
            if (!isLoggedInUserMessage)
            {
                return;
            }

            User user = message.GetUser();
            _myOculusId = user.ID;

            if (_myOculusId == 0)
            {
                Logger.Log($"{nameof(PUNNetworkBootstrapper)}: Failed to get oculus id: _oculusId = {_myOculusId}",
                    LogLevel.Error);
                return;
            }

            _myOculusName = string.IsNullOrEmpty(user.DisplayName) ? user.OculusID : user.DisplayName;
            Logger.Log(
                $"{nameof(PUNNetworkBootstrapper)}: Got a valid oculus id = {_myOculusId} name = {_myOculusName}",
                LogLevel.Verbose);

            // We add a delay so that we can find the right Network Objects when they spawn
            Invoke($"{nameof(SetUpAndStartAutomaticColocation)}", 3);
        }

        public void SetUpAndStartAutomaticColocation()
        {
            INetworkData networkData = SetUpNetworkData();

            _idDictionary = SetUpIDDictionary();
            AddIDToDictionary(_myOculusId, PhotonNetwork.LocalPlayer.ActorNumber);

            INetworkMessenger networkMessenger = SetUpNetworkMessenger(_idDictionary);
            SharedAnchorManager sharedAnchorManager = new SharedAnchorManager();
            sharedAnchorManager.AnchorPrefab = AnchorPrefab;
            AutomaticColocationLauncher acl = new AutomaticColocationLauncher();

            NetworkAdapter.SetConfig(networkData, networkMessenger);

            AutomaticColocationLauncher colocationLauncher = new AutomaticColocationLauncher();
            colocationLauncher.Init(
                NetworkAdapter.NetworkData,
                NetworkAdapter.NetworkMessenger,
                sharedAnchorManager,
                _ovrCameraRig.gameObject,
                _myPlayerId,
                _myOculusId
            );

            colocationLauncher.ColocationReady += OnColocationReady;
            colocationLauncher.ColocationFailed += OnColocationFailed;
            colocationLauncher.ColocateAutomatically();
        }

        private void OnColocationReady()
        {
            Logger.Log($"{nameof(PUNNetworkBootstrapper)}:Colocation is Ready!", LogLevel.Info);
            SpawnNametag(_myOculusName);
        }

        private void OnColocationFailed(ColocationFailedReason e)
        {
            Logger.Log($"{nameof(PUNNetworkBootstrapper)}: Colocation failed - {e}", LogLevel.Info);
        }

        private void SpawnNametag(string playerName)
        {
            PhotonPlayer localPlayer = PhotonNetwork.LocalPlayer;
            localPlayer.NickName = playerName;

            Logger.Log($"Creating nametag for player {localPlayer.ActorNumber} {localPlayer.NickName}", LogLevel.Info);

            GameObject nametag = PhotonNetwork.Instantiate("Nametag", Vector3.zero, Quaternion.identity);

            if (!nametag.TryGetComponent(out PhotonView nametagView))
            {
                Logger.Log("Nametag prefab doesn't have a PhotonView.", LogLevel.Error);
                return;
            }

            nametagView.TransferOwnership(localPlayer);
        }

        private byte[] ConvertFromUlongToByteArray(ulong num)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream();
            binaryFormatter.Serialize(memoryStream, num);
            return memoryStream.ToArray();
        }

        private ulong ConvertFromByteArrayToUlong(byte[] byteArray)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream();
            memoryStream.Write(byteArray, 0, byteArray.Length);
            memoryStream.Position = 0;

            ulong num = (ulong)binaryFormatter.Deserialize(memoryStream);
            return num;
        }

        private PhotonNetworkData SetUpNetworkData()
        {
            PhotonNetworkData photonNetworkData = new PhotonNetworkData();
            photonNetworkData.Init();
            return photonNetworkData;
        }

        private PhotonMessenger SetUpNetworkMessenger(PhotonIDDictionary idDictionary)
        {
            PhotonMessenger photonMessenger = new PhotonMessenger();
            photonMessenger.Init(idDictionary);
            return photonMessenger;
        }

        private PhotonIDDictionary SetUpIDDictionary()
        {
            PhotonIDDictionary idDictionary = new PhotonIDDictionary();
            idDictionary.Init();
            idDictionary.AddOrSet(_myPlayerId, PhotonNetwork.LocalPlayer.ActorNumber);
            return idDictionary;
        }

        private void AddIDToDictionary(ulong oculusId, int photonId)
        {
            _idDictionary.AddOrSet(oculusId, photonId);
        }
    }
}
