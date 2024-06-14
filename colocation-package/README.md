# Colocation Package

To enable Local Multiplayer the following is needed

- **Matchmaking** - Finding Players to play with
- **Player Connection**  - Players joining the same game to play in
- **Colocation** - Players can see the same object in the same position

Colocation Package provides network agnostic interfaces to enable colocation in Mixed Reality using Unity. Colocation Package does this by implementing the core logic of Shared Spatial Anchors.

If you are using Oculus Core SDK v65+ the majority of this code has been ported over to [Multiplayer Building Blocks](https://developer.oculus.com/documentation/unity/bb-multiplayer-blocks/) which provides a simpler version of using the Colocation Package

## How to enable Colocation with a custom Network Layer

1. Import the Network Layer that will be used. (e.g Netcode for GameObjects or Photon)
    1. For colocation to work, the custom network layer must support state synchronization and RPC calls
2. Implement the interfaces `INetworkData` and `INetworkMessenger`
    1. Examples of how to implement `INetworkData` can be found in `NetcodeGameObjectsNetworkData`, `FusionNetworkData`, and `PhotonNetworkData`
    2. Examples of how to implement `INetworkMessenger` can be found in `NetcodeGameObjectsMessenger` , `FusionMessenger`, and `PhotonMessenger`
3. Depending on the Network Layer being used, they may have specific ways to store the data or send the data via an RPC call
    1. Examples of how to write a wrapper to store and send data can be found
        1. Netcode for GameObjects - `NGOShareAndLocalizeParams` `NetcodeGameObjectsAnchor` `NetcodeGameObjectsPlayer`
        2. Photon Fusion 1 - `FusionShareAndLocalizeParam` `FusionAnchor` `FusionPlayer`
        3. Photon Unity Networking 2 - `PhotonAnchor` `PhotonCustomProperties` `PhotonIDDictionary`
4. Calling Colocation
   1. Instaniate an `AutomaticColocationLauncher`
   2. Call the `Init` function in `AutomaticColocationLauncher`. The `Init` functions requires the following parameters
        1. A concrete implementation of `INetworkData` which step 2 refers to
        2. A concrete implementation of `INetworkMessenger` which step 2 refers to
        3. `SharedAnchorManager` which can be instaniated without any dependencies
        4. The `GameObject` of `OVRCameraRig`
        5. PlayerId which is an arbitrary unique id
        6. OculusId
             1. As an example of how to get OculusId refer to `TryGetOculusUser` in `NGOSampleFlowBootstrapper`, `FusionSampleFlowBootstrapper`
             2. As an different example of how to get OculusId refer to `Init()` in `PUNNetworkBootstrapper`
        7. Examples of Calling Colocation can be found in `SetUpAndStartAutomaticColocation` in `NGONetworkBootstrapper`, `FusionNetworkBootstrapper`, and `PUNNetworkBootstrapper`
