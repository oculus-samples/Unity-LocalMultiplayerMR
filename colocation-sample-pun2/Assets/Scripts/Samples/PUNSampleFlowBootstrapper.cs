// Copyright (c) Meta Platforms, Inc. and affiliates.

using System.Collections.Generic;
using System.Text;
using Meta.XR.Samples;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace com.meta.xr.colocation.samples.pun2
{
    /// <summary>
    ///     A class that initializes the multiplayer sample and handles player connection
    /// </summary>
    [MetaCodeSample("LocalMultiplayerMR-PUN2")]
    public class PUNSampleFlowBootstrapper : MonoBehaviour, IConnectionCallbacks, IMatchmakingCallbacks
    {
        [SerializeField] private GameObject PUNNetworkBootstrapperPrefab;
        private LocalNetworkDiscovery _localNetworkDiscovery;
        private string _roomNameToCreate;
        private string _roomNameToJoin;
        private bool _foundRoomToJoin;

        void Start()
        {
            Init();
        }

        private void Init()
        {
            Logger.Log($"{nameof(PUNSampleFlowBootstrapper)}: Init was called", LogLevel.Verbose);
            DiscoverOrHostSession();
            _roomNameToCreate = "TestRoomName";
        }

        private async void DiscoverOrHostSession()
        {
            if (_localNetworkDiscovery != null)
            {
                _localNetworkDiscovery.Dispose();
                _localNetworkDiscovery = null;
            }

            _foundRoomToJoin = false;
            _localNetworkDiscovery = new LocalNetworkDiscovery();

            Logger.Log($"{nameof(PUNSampleFlowBootstrapper)}: Listening for local sessions.", LogLevel.Verbose);
            var (ip, data) = await _localNetworkDiscovery.ListenForConnection();

            _foundRoomToJoin = ip != null;
            if (_foundRoomToJoin)
            {
                _roomNameToJoin = Encoding.UTF8.GetString(data);
            }

            ConnectToThePhoton();
        }

        private void ConnectToThePhoton()
        {
            PhotonNetwork.AddCallbackTarget(this);
            PhotonNetwork.ConnectUsingSettings();
        }

        public void OnConnectedToMaster()
        {
            if (_foundRoomToJoin)
            {
                PhotonNetwork.JoinRoom(_roomNameToJoin);
            }
            else
            {
                PhotonNetwork.CreateRoom(_roomNameToCreate);
                byte[] dataToSendOverNetwork = Encoding.UTF8.GetBytes(_roomNameToCreate);
                _localNetworkDiscovery.StartBroadcasting(dataToSendOverNetwork);
            }
        }

        public void OnJoinedRoom()
        {
            InitPUNNetworkBootstrapper();
        }

        private void InitPUNNetworkBootstrapper()
        {
            GameObject punNetworkBootstrapperGameObject = Instantiate(PUNNetworkBootstrapperPrefab);
            PUNNetworkBootstrapper punNetworkBootstrapper =
                punNetworkBootstrapperGameObject.GetComponent<PUNNetworkBootstrapper>();

            punNetworkBootstrapper.Init();
        }

        public void OnDisconnected(DisconnectCause cause)
        {
            Logger.Log($"{nameof(PUNSampleFlowBootstrapper)}: OnDisconnected was called", LogLevel.Verbose);
        }

        #region IConnectionCallbacks Empty Implementations

        public void OnConnected()
        {
        }

        public void OnRegionListReceived(RegionHandler regionHandler)
        {
        }

        public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
        {
        }

        public void OnCustomAuthenticationFailed(string debugMessage)
        {
        }

        #endregion

        #region IMatchmakingCallbacks Empty Implementations

        public void OnFriendListUpdate(List<FriendInfo> friendList)
        {
        }

        public void OnCreatedRoom()
        {
        }

        public void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug.LogError(
                $"PUNSampleFlowBootstrapper: OnCreateRoomFailed: return code: {returnCode}. Message: {message}");
        }

        public void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.LogError(
                $"PUNSampleFlowBootstrapper: OnJoinRoomFailed: return code: {returnCode}. Message: {message}");
        }

        public void OnJoinRandomFailed(short returnCode, string message)
        {
        }

        public void OnLeftRoom()
        {
        }

        #endregion
    }
}
