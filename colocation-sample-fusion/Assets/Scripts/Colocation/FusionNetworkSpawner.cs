// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using com.meta.xr.colocation.samples.fusion;
using Fusion;
using Fusion.Sockets;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace com.meta.xr.colocation.fusion
{
    /// <summary>
    ///     A class to handle starting a game
    /// </summary>
    public class FusionNetworkSpawner : MonoBehaviour, INetworkRunnerCallbacks
    {
        [SerializeField] private GameObject _fusionNetworkBootstrapperPrefab;
        private NetworkRunner _runner;
        private FusionNetworkBootstrapper _fusionNetworkBootstrapperClient;

        public async Task<bool> StartGame(GameMode gameMode, string roomName, ulong myOculusId, ulong myPlayerId,
            string playerDisplayName, Action<GameMode, StartGameResult> onComplete = null)
        {
            Logger.Log($"{nameof(FusionNetworkSpawner)}: StartGame in FusionNetworkSpawner called", LogLevel.Verbose);
            _runner = gameObject.GetComponent<NetworkRunner>();
            if (_runner == null)
            {
                Logger.Log($"{nameof(FusionNetworkSpawner)}: _runner is null", LogLevel.Error);
                return false;
            }

            Logger.Log($"{nameof(FusionNetworkSpawner)}: runner is not null", LogLevel.Verbose);
            _runner.ProvideInput = true;

            Logger.Log($"{nameof(FusionNetworkSpawner)}: Provide input is set to true", LogLevel.Verbose);
            StartGameArgs startGameArgs = new StartGameArgs()
            {
                GameMode = gameMode,
                SessionName = roomName,
                Scene = SceneManager.GetActiveScene().buildIndex,
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
            };

            Logger.Log($"{nameof(FusionNetworkSpawner)}: StartGameArgs are populated", LogLevel.Verbose);
            _runner.AddCallbacks(this);
            Logger.Log($"{nameof(FusionNetworkSpawner)}: Added runner callbacks", LogLevel.Verbose);

            StartGameResult result = null;
            Logger.Log($"{nameof(FusionNetworkSpawner)}: Starting the game as GameMode {gameMode.ToString()}",
                LogLevel.Verbose);
            result = await _runner.StartGame(startGameArgs);

            if (!result.Ok)
            {
                Logger.Log($"{nameof(FusionNetworkSpawner)}: StartGame failed. Result: {result}", LogLevel.Error);
                onComplete?.Invoke(gameMode, result);
                return false;
            }

            if (gameMode == GameMode.Host)
            {
                NetworkObject networkBootstrapperNetworkObject = _runner.Spawn(_fusionNetworkBootstrapperPrefab);
                Logger.Log($"{nameof(FusionNetworkSpawner)}: Spawned Network Object", LogLevel.Verbose);
                FusionNetworkBootstrapper networkBootstrapper =
                    networkBootstrapperNetworkObject.GameObject().GetComponent<FusionNetworkBootstrapper>();
                Logger.Log($"{nameof(FusionNetworkSpawner)}: Spawned FusionNetworkBootstrapper", LogLevel.Verbose);
                networkBootstrapper.SetUpAndStartAutomaticColocation(myOculusId, myPlayerId, playerDisplayName);
            }
            else if (gameMode == GameMode.Client)
            {
                while (_fusionNetworkBootstrapperClient == null)
                {
                    _fusionNetworkBootstrapperClient = FindObjectOfType<FusionNetworkBootstrapper>();
                    await Task.Yield();
                }

                _fusionNetworkBootstrapperClient.SetUpAndStartAutomaticColocation(myOculusId, myPlayerId,
                    playerDisplayName);
            }

            onComplete?.Invoke(gameMode, result);
            return true;
        }

        #region INetworkRunnerCallbacks

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            Logger.Log("OnPlayerJoined worked!", LogLevel.Verbose);
            Logger.Log($"NetworkId is {runner.LocalPlayer.PlayerId}", LogLevel.Verbose);
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
        }

        public void OnConnectedToServer(NetworkRunner runner)
        {
        }

        public void OnDisconnectedFromServer(NetworkRunner runner)
        {
        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request,
            byte[] token)
        {
        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
            Logger.Log($"OnConnectFailed was called {reason}", LogLevel.Error);
        }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {
        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
        {
        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {
        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {
        }

        #endregion // INetworkRunnerCallbacks
    }
}
