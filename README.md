# Multiplayer Samples

This Repo contains samples that demonstrates how to enable Local Multiplayer in Mixed Reailty using Unity

If you are using Oculus Core SDK v65+ and (Netcode for GameObjects or Photon Fusion 2) the majority of this code has been ported over to [Multiplayer Building Blocks](https://developer.oculus.com/documentation/unity/bb-multiplayer-blocks/) which provides a simpler version of this repo that demonstrates how to enable Mixed Reality Local Multiplayer

## Sample Description

The sample contains 3 different Unity Projects. Where each project contains one of the following Networking Layers

- Netcode for GameObjects
- Photon Fusion 1
- Photon Unity Networking 2

In each Unity Project, the Multiplayer Sample contains the following
- **Matchmaking** - Finding Players to play with
- **Player Connection**  - Players joining the same game to play in
- **Colocation** - Players can see the same object in the same position

## Repo Breakdown

[colocation-package](https://github.com/oculus-samples/Unity-LocalMultiplayerMR/tree/main/colocation-package) - Contains network agnostic interfaces that can be used to implement colocation

[colocation-sample-fusion](https://github.com/oculus-samples/Unity-LocalMultiplayerMR/tree/main/colocation-sample-fusion) - Contains a Local Multiplayer Sample using Photon Fusion 1 as the networking layer

 - [Getting Started](https://github.com/oculus-samples/Unity-LocalMultiplayerMR/blob/main/colocation-sample-fusion/README.md)

[colocation-sample-ngo](https://github.com/oculus-samples/Unity-LocalMultiplayerMR/tree/main/colocation-sample-ngo) - Contains a Local Multiplayer Sample using Netcode for GameObjcts as the networking layer

 - [Getting Started](https://github.com/oculus-samples/Unity-LocalMultiplayerMR/blob/main/colocation-sample-ngo/README.md)
 - [Multiplayer Building Blocks Getting Started](https://developer.oculus.com/documentation/unity/bb-multiplayer-blocks/)

[colocation-sample-pun2](https://github.com/oculus-samples/Unity-LocalMultiplayerMR/tree/main/colocation-sample-pun2) - Contains a Local Multiplayer Sample using Photon Unity Networking 2 as the networking layer

 - [Getting Started](https://github.com/oculus-samples/Unity-LocalMultiplayerMR/blob/main/colocation-sample-pun2/README.md)

## License

This codebase is available as both a sample and a template for mixed reality projects. The [Oculus License](./LICENSE.txt) applies to the SDK and supporting material. The MIT License applies to only certain, clearly marked documents. If an individual file does not indicate which license it is subject to, then the Oculus License applies.

See the [CONTRIBUTING](./CONTRIBUTING.md) file for how to help out.
