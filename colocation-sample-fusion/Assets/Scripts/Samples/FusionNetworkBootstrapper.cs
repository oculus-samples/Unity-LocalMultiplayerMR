// Copyright (c) Meta Platforms, Inc. and affiliates.

using com.meta.xr.colocation.fusion;
using com.meta.xr.colocation.fusion.debug;
using Fusion;
using UnityEngine;
using UnityAssert = UnityEngine.Assertions.Assert;

namespace com.meta.xr.colocation.samples.fusion
{
    /// <summary>
    ///     A class that handles setting up and initializing colocation
    /// </summary>
    public class FusionNetworkBootstrapper : NetworkBehaviour
    {
        [SerializeField] private GameObject anchorPrefab;
        [SerializeField] private Nametag nametagPrefab;

        [SerializeField] private FusionNetworkData networkData;
        [SerializeField] private FusionMessenger networkMessenger;

        private string _myOculusName;
        private ulong _myPlayerId;
        private ulong _myOculusId;
        private OVRCameraRig _ovrCameraRig;
        private SharedAnchorManager _sharedAnchorManager;
        private AutomaticColocationLauncher _colocationLauncher;

        private void Awake()
        {
            UnityAssert.IsNotNull(anchorPrefab, $"{nameof(anchorPrefab)} cannot be null.");
            UnityAssert.IsNotNull(nametagPrefab, $"{nameof(nametagPrefab)} cannot be null.");
            UnityAssert.IsNotNull(networkData, $"{nameof(networkData)} cannot be null.");
            UnityAssert.IsNotNull(networkMessenger, $"{nameof(networkMessenger)} cannot be null.");

            _ovrCameraRig = FindObjectOfType<OVRCameraRig>();
        }

        public void SetUpAndStartAutomaticColocation(ulong myOculusId, ulong myPlayerId, string playerDisplayName)
        {
            Logger.Log(
                $"SetUpAndStartAutomaticColocation was called myOculusId {myOculusId}, myPlayerId {myPlayerId}, playerDisplayName {playerDisplayName}",
                LogLevel.Verbose);
            Logger.Log($"{nameof(FusionNetworkBootstrapper)}: Starting colocation.", LogLevel.Verbose);
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
            Logger.Log($"{nameof(FusionNetworkBootstrapper)}: Colocation is Ready!", LogLevel.Info);
            SpawnNametagHostRPC(Runner.LocalPlayer, _myOculusName);
        }

        private void OnColocationFailed(ColocationFailedReason e)
        {
            Logger.Log($"{nameof(FusionNetworkBootstrapper)}: Colocation failed - {e}", LogLevel.Error);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable,
            HostMode = RpcHostMode.SourceIsHostPlayer)]
        private void SpawnNametagHostRPC(PlayerRef owner, string playerName)
        {
            Logger.Log($"{nameof(FusionNetworkBootstrapper)}: Creating nametag for player {owner.PlayerId}: {playerName}", LogLevel.Info);

            Nametag nametag = Runner.Spawn(nametagPrefab, inputAuthority: owner);
            nametag.Name = playerName;
        }
    }
}
