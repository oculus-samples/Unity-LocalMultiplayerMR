// Copyright (c) Meta Platforms, Inc. and affiliates.

using System.Net;
using System.Threading.Tasks;
using com.meta.xr.colocation.fusion;
using Fusion;
using Meta.XR.Samples;
using Oculus.Platform;
using Oculus.Platform.Models;
using UnityEngine;

namespace com.meta.xr.colocation.samples.fusion
{
    /// <summary>
    ///     A class that initializes the multiplayer sample and handles player connection
    /// </summary>
    [MetaCodeSample("LocalMultiplayerMR-Fusion")]
    public class FusionSampleFlowBootstrapper : MonoBehaviour
    {
        [SerializeField] private FusionNetworkSpawner fusionNetworkSpawner;
        private const string TEST_ROOM_NAME = "LAMBO_RABBIT_ROOM";

        private LocalNetworkDiscovery _localNetworkDiscovery;
        private FusionNetworkBootstrapper _fusionNetworkBootstrapper;

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
                    $"{nameof(FusionSampleFlowBootstrapper)}: Local PlayerID: {_myPlayerId}, OculusID: {_myOculusId}, DisplayName: {_myOculusName}",
                    LogLevel.Verbose);
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

            Logger.Log($"{nameof(FusionSampleFlowBootstrapper)}: Listening for local sessions.", LogLevel.Verbose);
            var (ip, data) = await _localNetworkDiscovery.ListenForConnection();

            _foundRoomToJoin = ip != null;

            if (_foundRoomToJoin)
            {
                // We didn't find a session in time, so host one.
                JoinSession(ip, data);
            }
            else
            {
                HostSession();
            }
        }

        private async void JoinSession(IPEndPoint ipEndPoint, byte[] data)
        {
            Logger.Log($"{nameof(FusionSampleFlowBootstrapper)}: Session was discovered.", LogLevel.Verbose);

            _foundRoomToJoin = true;
            await fusionNetworkSpawner.StartGame(GameMode.Client, TEST_ROOM_NAME, _myOculusId, _myPlayerId,
                _myOculusName, OnStartGameCompleted);
        }

        private void OnStartGameCompleted(GameMode gameMode, StartGameResult result)
        {
            if (!result.Ok)
            {
                Logger.Log($"{nameof(FusionSampleFlowBootstrapper)}: OnStartGameCompleted: result was not ok", LogLevel.Error);
            }

            Logger.Log($"{nameof(FusionSampleFlowBootstrapper)}: OnStartGameCompleted was called", LogLevel.Verbose);

            switch (gameMode)
            {
                case GameMode.Host:
                    OnHostStarted();
                    break;
                case GameMode.Client:
                    OnClientStarted();
                    break;
                default:
                    Logger.Log(
                        $"{nameof(FusionSampleFlowBootstrapper)}: Unable to start game because GameMode: {gameMode} was chosen",
                        LogLevel.Error);
                    break;
            }
        }


        private async void HostSession()
        {
            Logger.Log($"{nameof(FusionSampleFlowBootstrapper)}: Hosting session.", LogLevel.Verbose);

            Logger.Log("Calling fusionNetowrkSpawner StartGame function with forget", LogLevel.Verbose);
            await fusionNetworkSpawner.StartGame(GameMode.Host, TEST_ROOM_NAME, _myOculusId, _myPlayerId, _myOculusName,
                OnStartGameCompleted);
            Logger.Log("Exiting fusionNetowrkSpawner StartGame function", LogLevel.Verbose);
        }

        private void OnClientStarted()
        {
            Logger.Log($"{nameof(FusionSampleFlowBootstrapper)}: OnClientStarted called", LogLevel.Verbose);
        }

        private void OnHostStarted()
        {
            Logger.Log($"{nameof(FusionSampleFlowBootstrapper)}: OnHostStarted called", LogLevel.Verbose);
            Logger.Log($"{nameof(FusionSampleFlowBootstrapper)}: Broadcasting session info.", LogLevel.Verbose);
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
                        Logger.Log(
                            $"{nameof(FusionSampleFlowBootstrapper)}: Could not get Oculus ID.\n{message.GetError().Message}",
                            LogLevel.Error);
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
                Logger.Log($"{nameof(FusionSampleFlowBootstrapper)}: Did not get a valid Oculus ID.", LogLevel.Error);
                return false;
            }

            return true;
        }
    }
}
