// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Meta.XR.Samples;
using UnityEngine;

namespace com.meta.xr.colocation.samples.fusion
{
    /// <summary>
    ///     A class that handles matchmaking
    /// </summary>
    [MetaCodeSample("LocalMultiplayerMR-Fusion")]
    public class LocalNetworkDiscovery : IDisposable
    {
        private readonly int _port;
        private bool _isBroadcasting;
        private UdpClient _udpClient;
        private TaskCompletionSource<(IPEndPoint, byte[])> _onListenTcs;

        public LocalNetworkDiscovery(int port = 9876)
        {
            _port = port;
        }

        public void Dispose()
        {
            StopListening();
            StopBroadcasting();
        }

        public async Task<(IPEndPoint, byte[])> ListenForConnection()
        {
            _onListenTcs = new();
            Logger.Log($"{nameof(LocalNetworkDiscovery)}: Listening for local sessions on port {_port}.",
                LogLevel.Info);

            _udpClient = new UdpClient(_port);
            _udpClient.BeginReceive(Receive, _udpClient);
            var resultTask = await Task.WhenAny(_onListenTcs.Task, CreateHostSessionTask());
            return resultTask.Result;
        }

        private async Task<(IPEndPoint, byte[])> CreateHostSessionTask()
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
            return (null, null);
        }

        private void Receive(IAsyncResult asyncResult)
        {
            UdpClient udpClient = (UdpClient)(asyncResult.AsyncState);
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Broadcast, _port);
            byte[] data = Array.Empty<byte>();

            try
            {
                data = udpClient.EndReceive(asyncResult, ref ipEndPoint);
            }
            catch (ObjectDisposedException)
            {
                Debug.Log($"{nameof(LocalNetworkDiscovery)}: Stopped listening for local sessions.");
                return;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            _onListenTcs.TrySetResult((ipEndPoint, data));
            StopListening();
        }

        private void StopListening()
        {
            _udpClient?.Dispose();
            _udpClient = null;
        }

        public async void StartBroadcasting(byte[] sessionInformation, float broadcastInterval = 0.5f)
        {
            Logger.Log($"{nameof(LocalNetworkDiscovery)}: Broadcasting session on port {_port}.", LogLevel.Verbose);

            _isBroadcasting = true;

            try
            {
                var ipEndPoint = new IPEndPoint(IPAddress.Broadcast, _port);
                using (var senderSocket = new UdpClient())
                {
                    senderSocket.EnableBroadcast = true;

                    while (_isBroadcasting)
                    {
                        senderSocket.Send(sessionInformation, sessionInformation.Length, ipEndPoint);
                        await Task.Delay(TimeSpan.FromSeconds(broadcastInterval));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                _isBroadcasting = false;
            }

            Logger.Log($"{nameof(LocalNetworkDiscovery)}: Stopped broadcasting.", LogLevel.Verbose);
        }

        private void StopBroadcasting()
        {
            _isBroadcasting = false;
        }
    }
}
