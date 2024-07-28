// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using ExitGames.Client.Photon;
using Photon.Pun;
using PhotonRealtime = Photon.Realtime;
using RaiseEventOptions = Photon.Realtime.RaiseEventOptions;

namespace com.meta.xr.colocation.pun2
{
    /// <summary>
    ///     A PUN2 concrete implementation of INetworkMessenger
    ///     Used to send the RaiseEvent calls needed for a player to join another player's colocated space
    /// </summary>
    public class PhotonMessenger : INetworkMessenger, IDisposable
    {
        public event Action<ShareAndLocalizeParams> AnchorShareRequestReceived;
        public event Action<ShareAndLocalizeParams> AnchorShareRequestCompleted;

        public struct PhotonEventCodes
        {
            public byte AnchorShareRequest;
            public byte AnchorShareCompleted;
        }

        public static readonly PhotonEventCodes DEFAULT_PHOTON_EVENT_CODES = new PhotonEventCodes()
        { AnchorShareRequest = 4, AnchorShareCompleted = 7 };

        private PhotonIDDictionary _idDictionary;
        private PhotonEventCodes _photonEventCodes;

        public void Init(PhotonIDDictionary idDictionary)
        {
            Init(idDictionary, DEFAULT_PHOTON_EVENT_CODES);
        }

        public void Init(PhotonIDDictionary idDictionary, PhotonEventCodes photonEventCodes)
        {
            _idDictionary = idDictionary;
            _photonEventCodes = photonEventCodes;
            PhotonNetwork.NetworkingClient.EventReceived += ProcessEvent;
        }

        public void Dispose()
        {
            PhotonNetwork.NetworkingClient.EventReceived -= ProcessEvent;
        }

        public void SendAnchorShareRequest(ulong targetPlayerId, ShareAndLocalizeParams shareAndLocalizeParams)
        {
            byte[] messageData = ConvertFromShareAndLocalizeParamsToByteArray(shareAndLocalizeParams);
            SendMessageToPlayer(_photonEventCodes.AnchorShareRequest, targetPlayerId, messageData);
        }

        public void SendAnchorShareCompleted(ulong targetPlayerId, ShareAndLocalizeParams shareAndLocalizeParams)
        {
            byte[] messageData = ConvertFromShareAndLocalizeParamsToByteArray(shareAndLocalizeParams);
            SendMessageToPlayer(_photonEventCodes.AnchorShareCompleted, targetPlayerId, messageData);
        }

        private void SendMessageToPlayer(byte eventCode, ulong playerId, byte[] messageData)
        {
            object networkId = _idDictionary.GetNetworkIdFromPlayerId(playerId);
            if (networkId == null)
            {
                Logger.Log($"{nameof(PhotonMessenger)}: NetworkId is null from playerId: {playerId}", LogLevel.Error);
            }

            int photonId = (int)networkId;
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { TargetActors = new int[] { photonId } };
            PhotonNetwork.RaiseEvent(eventCode, messageData, raiseEventOptions, SendOptions.SendReliable);
        }

        private byte[] ConvertFromShareAndLocalizeParamsToByteArray(ShareAndLocalizeParams shareAndLocalizeParams)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream();
            binaryFormatter.Serialize(memoryStream, shareAndLocalizeParams);
            return memoryStream.ToArray();
        }

        private ShareAndLocalizeParams ConvertFromByteArrayToShareAndLocalizeParams(byte[] byteArray)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream();
            memoryStream.Write(byteArray, 0, byteArray.Length);
            memoryStream.Position = 0;

            ShareAndLocalizeParams shareAndLocalizeParams =
                (ShareAndLocalizeParams)binaryFormatter.Deserialize(memoryStream);
            return shareAndLocalizeParams;
        }

        private void ProcessEvent(EventData eventData)
        {
            byte eventCode = eventData.Code;

            if (eventCode == _photonEventCodes.AnchorShareRequest)
            {
                if (eventData.CustomData is not byte[] data)
                {
                    Logger.Log($"{nameof(PhotonMessenger)}: AnchorShareRequest event received with invalid data.",
                        LogLevel.Error);
                    return;
                }

                AnchorShareRequestReceived?.Invoke(ConvertFromByteArrayToShareAndLocalizeParams(data));
            }
            else if (eventCode == _photonEventCodes.AnchorShareCompleted)
            {
                if (eventData.CustomData is not byte[] data)
                {
                    Logger.Log($"{nameof(PhotonMessenger)}: AnchorShareCompleted event received with invalid data.",
                        LogLevel.Error);
                    return;
                }

                AnchorShareRequestCompleted?.Invoke(ConvertFromByteArrayToShareAndLocalizeParams(data));
            }
        }
    }
}
