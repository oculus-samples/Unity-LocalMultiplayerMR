// Copyright (c) Meta Platforms, Inc. and affiliates.

using System.Net;
using System.Threading.Tasks;
using Oculus.Platform;
using Oculus.Platform.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace com.meta.xr.colocation.samples.ngo
{
    /// <summary>
    ///     A class that initializes the multiplayer sample and handles player connection
    /// </summary>
    public class NGOSampleFlowBootstrapper : MonoBehaviour
    {
        [SerializeField] private NGONetworkBootstrapper ngoNetworkBootstrapperPrefab;

        private LocalNetworkDiscovery _localNetworkDiscovery;
        private NGONetworkBootstrapper _ngoNetworkBootstrapper;

        private bool _foundRoomToJoin;
        private ulong _myPlayerId;
        private ulong _myOculusId;
        private string _myOculusName;

        private async void Start()
        {
            _myPlayerId = (ulong)SystemInfo.deviceUniqueIdentifier.GetHashCode();
            if (await TryGetOculusUser())
            {
                Logger.Log(
                    $"{nameof(NGOSampleFlowBootstrapper)}: Local PlayerID: {_myPlayerId}, OculusID: {_myOculusId}, DisplayName: {_myOculusName}",
                    LogLevel.Info);
                DiscoverOrHostSession();
            }
        }

        private void OnDestroy()
        {
            if (_localNetworkDiscovery != null)
            {
                _localNetworkDiscovery.Dispose();
                _localNetworkDiscovery = null;
            }
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

            Logger.Log($"{nameof(NGOSampleFlowBootstrapper)}: Listening for local sessions.", LogLevel.Verbose);
            var (ip, data) = await _localNetworkDiscovery.ListenForConnection();

            _foundRoomToJoin = ip != null;

            if (_foundRoomToJoin)
            {
                JoinSession(ip, data);
            }
            else
            {
                HostSession();
            }
        }

        private void JoinSession(IPEndPoint ipEndPoint, byte[] data)
        {
            Logger.Log($"{nameof(NGOSampleFlowBootstrapper)}: Session was discovered.", LogLevel.Verbose);

            _foundRoomToJoin = true;

            var ipAddress = ipEndPoint.Address.ToString();
            NetworkManager.Singleton.GetComponent<UnityTransport>()
                .SetConnectionData(ipAddress, (ushort)9877u, "0.0.0.0");
            Logger.Log($"Set connection data on client to ipAddress: {ipAddress}", LogLevel.Verbose);

            Logger.Log($"{nameof(NGOSampleFlowBootstrapper)}: Starting client.", LogLevel.Verbose);
            NetworkManager.Singleton.OnClientStarted += OnClientStarted;
            NetworkManager.Singleton.StartClient();
        }

        private void HostSession()
        {
            Logger.Log($"{nameof(NGOSampleFlowBootstrapper)}: Hosting session.", LogLevel.Verbose);

            NetworkManager.Singleton.GetComponent<UnityTransport>()
                .SetConnectionData("0.0.0.0", (ushort)9877u, "0.0.0.0");

            Logger.Log($"{nameof(NGOSampleFlowBootstrapper)}: Starting host.", LogLevel.Verbose);
            NetworkManager.Singleton.OnServerStarted += OnServerStarted;
            NetworkManager.Singleton.StartHost();
        }

        private async void OnClientStarted()
        {
            Logger.Log($"{nameof(NGOSampleFlowBootstrapper)}: Client started.", LogLevel.Verbose);

            NetworkManager.Singleton.OnClientStarted -= OnClientStarted;

            while (_ngoNetworkBootstrapper == null)
            {
                _ngoNetworkBootstrapper = FindObjectOfType<NGONetworkBootstrapper>();
                await Task.Yield();
            }

            _ngoNetworkBootstrapper.SetUpAndStartAutomaticColocation(_myOculusId, _myPlayerId, _myOculusName);
        }

        private void OnServerStarted()
        {
            Logger.Log($"{nameof(NGOSampleFlowBootstrapper)}: Server started.", LogLevel.Verbose);

            NetworkManager.Singleton.OnServerStarted -= OnServerStarted;

            _ngoNetworkBootstrapper = Instantiate(ngoNetworkBootstrapperPrefab, Vector3.zero, Quaternion.identity);
            _ngoNetworkBootstrapper.GetComponent<NetworkObject>().Spawn();

            _ngoNetworkBootstrapper.SetUpAndStartAutomaticColocation(_myOculusId, _myPlayerId, _myOculusName);

            Logger.Log($"{nameof(NGOSampleFlowBootstrapper)}: Broadcasting session info.", LogLevel.Verbose);
            _localNetworkDiscovery.StartBroadcasting(new byte[] { });
        }

        private async Task<bool> TryGetOculusUser()
        {
            bool isComplete = false;

            Core.Initialize();
            Users.GetLoggedInUser().OnComplete(
                message =>
                {
                    isComplete = true;

                    if (message.IsError)
                    {
                        Logger.Log($"{nameof(NGOSampleFlowBootstrapper)}: Could not get Oculus ID.", LogLevel.Error);
                        return;
                    }

                    User user = message.GetUser();
                    _myOculusId = user.ID;
                    _myOculusName = user.DisplayName.Length > 0 ? user.DisplayName : user.OculusID;
                });

            while (!isComplete)
            {
                await Task.Yield();
            }

            if (_myOculusId == 0)
            {
                Logger.Log($"{nameof(NGOSampleFlowBootstrapper)}: Did not get a valid Oculus ID.", LogLevel.Error);
                return false;
            }

            return true;
        }
    }
}
