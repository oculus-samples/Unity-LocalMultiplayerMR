# Multiplayer Sample -  Netcode for GameObjects

This Sample demonstrates how to enable Local Multiplayer in Mixed Reality using Unity and Netcode for GameObjects

If you are using Oculus Core SDK v65+, the majority of this code has been ported over to [Multiplayer Building Blocks](https://developer.oculus.com/documentation/unity/bb-multiplayer-blocks/) which provides a simpler version of this sample that demonstrates how to enable Mixed Reality Local Multiplayer

More specifically the sample demonstrates the following
- **Matchmaking** - Finding Players to play with
- **Player Connection**  - Players joining the same game to play in
- **Colocation** - Players can see the same object in the same position

## Requirements

- Unity Version: 2022.3.15f1
- Mac or Windows

## Getting Started

<b>Opening the project </b><br>
1. Install [Unity Version 2022.3.15f1](https://unity.com/releases/editor/whats-new/2022.3.15)

<b> Building the project</b><br>
In order to colocate with Shared Spatial Anchors we are required to have entitlements. The following steps outline how get the entitlements

1. If you don't already have a keystore to build an APK, [create a new keystore](https://docs.unity3d.com/Manual/android-keystore-create.html) by following Unity's documentation

2. Set up your app id<br>
   a.  Go to the [Oculus Developer Website](https://developer.oculus.com/manage/)<br>
   b. In the "My Apps" Section, click on Create New App<br>
   c. Once you have created a new app, it will come with an App Id<br>
   d. Go to the App. Your URL should look like the following `https://developer.oculus.com/manage/applications/{APP ID}/`<br>
   e. In Assets/Resources/OculusPlatformSettings.asset, add your App ID to the `Oculus Rift` and `Meta Quest/2/Pro`<br>


3. Get Entitlements<br>
   a. Build an APK of the sample project<br>
   b. Upload the APK to the same app that was created in the <b>Building the project</b> section<br>
   c. Download the newly created app on to your headset that was done in the <b>Building the project</b> Section<br>
   d. If everything was successful, an anchor can be seen in the same spot in multiple headsets and nametags can be seen on top of the users head<br>


## Scene

`NGOSampleFlowScene.unity` is the Unity Scene that shows how to set up local multiplayer

## Core Components

<b>NGOSampleFlowBootstrapper</b> - Class that handles matchmaking and player connection

<b>NGONetworkBootsrapper</b> -   Class that handles colocation


## How the Multiplayer Sample Works

Enabling Local Multiplayer involves 3 major steps
1. Matchmaking
2. Player Connection
3. Colocation

Below we will demonstrate each players perspective. Assume that Player 1 must finish the entire set up before Player 2 joins

<b>Player 1's Perspective</b>

When Player 1 boots up, they enter the matchmaking phase and find out there aren't any games going on. Player 1 then enters the player connection phase and hosts a game. After sucessfully hosting a game, Player 1 enters the colocation phase where they create and save a Shared Spatial Anchor. Then Player 1 aligns to the Shared Spatial Anchor.

<b>Player 2's Perspective</b>

When Player 2 boots up, they enter the matchmaking phase and find out Player 1 is already in a game. Player 2 then enters the player connection phase and joins Player 1's game. After sucessfully joining the game, Player 2 is now in the colocation phase where Player 2 tries to find any Shared Spatial Anchors that exist and asks Player 1 to share it. Player 1 will share the Anchor with Player 2 and tell Player 2 the Shared Spatial Anchor is now being shared. Player 2 will try to find/localize the Shared Spatial Anchor and align to it.



## FAQ

<b>I got an error that had the tag [SharedSpatialAnchorsError] what does this mean?</b>

This means that the Shared Spatial Anchors Service may be currently down. Please report the issue


<b>In the logs my oculus id is 0 what does that mean?</b>

Your app appears not to be entitled. Make sure the **Building the project** section has been completed. It is working correctly when in your headset, your app is shown in any section except for the Unknown Sources.

<b>The networking layer I want to use for my Unity Multiplayer Mixed Reality App isn't supported what do I do?</b>

The networking layer will most likely have its own APIs for handling Matchmaking and for Player Connection. As for Colocation, the Colocation Package provides interfaces like `INetworkData` and `INetworkMessenger` that need to be implemented in order to enable colocation regardless of the networking layer used. Refer to how we implement the Colocation Package in the Samples.

## License

The majority of this sample is licensed under [MIT LICENSE](./LICENSE.txt), however files from [Text Mesh Pro](http://www.unity3d.com/legal/licenses/Unity_Companion_License), are licensed under their respective licensing terms.
