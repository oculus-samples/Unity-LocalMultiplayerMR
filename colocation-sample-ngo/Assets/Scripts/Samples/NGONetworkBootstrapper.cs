// Copyright (c) Meta Platforms, Inc. and affiliates.

using com.meta.xr.colocation.ngo;
using com.meta.xr.colocation.ngo.debug;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

namespace com.meta.xr.colocation.samples.ngo
{
    /// <summary>
    ///     A class that handles setting up and initializing colocation
    /// </summary>
    public class NGONetworkBootstrapper : NetworkBehaviour
    {
        [SerializeField] private GameObject anchorPrefab;
        [SerializeField] private Nametag nametagPrefab;

        [SerializeField] private NetcodeGameObjectsNetworkData networkData;
        [SerializeField] private NetcodeGameObjectsMessenger networkMessenger;

        private OVRCameraRig _ovrCameraRig;

        private SharedAnchorManager _sharedAnchorManager;
        private AutomaticColocationLauncher _colocationLauncher;

        private string _myOculusName;

        private void Awake()
        {
            Assert.IsNotNull(anchorPrefab, $"{nameof(anchorPrefab)} cannot be null.");
            Assert.IsNotNull(nametagPrefab, $"{nameof(nametagPrefab)} cannot be null.");
            Assert.IsNotNull(networkData, $"{nameof(networkData)} cannot be null.");
            Assert.IsNotNull(networkMessenger, $"{nameof(networkMessenger)} cannot be null.");

            _ovrCameraRig = FindObjectOfType<OVRCameraRig>();
        }

        public void SetUpAndStartAutomaticColocation(ulong myOculusId, ulong myPlayerId, string playerDisplayName)
        {
            if (!NetworkObject.IsSpawned)
            {
                Logger.Log(
                    $"{nameof(NGONetworkBootstrapper)}: {nameof(SetUpAndStartAutomaticColocation)} called before NetworkObject has spawned.",
                    LogLevel.Error);
                return;
            }

            Logger.Log($"{nameof(NGONetworkBootstrapper)}: Starting colocation.", LogLevel.Info);

            _myOculusName = playerDisplayName;

            networkMessenger.RegisterLocalPlayer(myPlayerId);

            _sharedAnchorManager = new SharedAnchorManager();
            _sharedAnchorManager.AnchorPrefab = anchorPrefab;

            NetworkAdapter.SetConfig(networkData, networkMessenger);

            _colocationLauncher = new AutomaticColocationLauncher();
            _colocationLauncher.Init(
                NetworkAdapter.NetworkData,
                NetworkAdapter.NetworkMessenger,
                _sharedAnchorManager,
                _ovrCameraRig.gameObject,
                myPlayerId,
                myOculusId
            );

            _colocationLauncher.ColocationReady += OnColocationReady;
            _colocationLauncher.ColocationFailed += OnColocationFailed;
            _colocationLauncher.ColocateAutomatically();
        }

        private void OnColocationReady()
        {
            Logger.Log($"{nameof(NGONetworkBootstrapper)}: Colocation is Ready!", LogLevel.Info);
            SpawnNametagServerRPC(NetworkManager.Singleton.LocalClientId, _myOculusName);
        }

        private void OnColocationFailed(ColocationFailedReason e)
        {
            Logger.Log($"{nameof(NGONetworkBootstrapper)}: Colocation failed - {e}", LogLevel.Error);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SpawnNametagServerRPC(ulong ownerId, FixedString64Bytes name)
        {
            Logger.Log($"Creating nametag for player {ownerId}: {name}", LogLevel.Info);
            Nametag nametag = Instantiate(nametagPrefab);
            nametag.NetworkObject.SpawnWithOwnership(ownerId);
            nametag.Name = name;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (networkMessenger == null)
            {
                networkMessenger = GetComponent<NetcodeGameObjectsMessenger>();
            }

            if (networkData == null)
            {
                networkData = GetComponent<NetcodeGameObjectsNetworkData>();
            }
        }
#endif
    }
}
