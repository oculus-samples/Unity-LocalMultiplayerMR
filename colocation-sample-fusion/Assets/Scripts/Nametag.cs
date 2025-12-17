// Copyright (c) Meta Platforms, Inc. and affiliates.

using UnityEngine;
using UnityEngine.UI;
using Fusion;
using Meta.XR.Samples;
using TMPro;
using UnityAssert = UnityEngine.Assertions.Assert;

namespace com.meta.xr.colocation.fusion.debug
{
    /// <summary>
    ///     A class that handles the behavior of a networked name tag
    /// </summary>
    [MetaCodeSample("LocalMultiplayerMR-Fusion")]
    public class Nametag : NetworkBehaviour
    {
        [SerializeField] private TextMeshProUGUI textRef;
        [SerializeField] private Canvas canvas;
        [SerializeField] private Image hostIcon;

        [Networked(OnChanged = nameof(OnNameChanged))]
        public NetworkString<_64> Name { get; set; }

        [Networked(OnChanged = nameof(OnIsHostChanged))]
        public bool IsHost { get; set; }

        private void Awake()
        {
            UnityAssert.IsNotNull(canvas, $"{nameof(canvas)} cannot be null;");
            UnityAssert.IsNotNull(hostIcon, $"{nameof(hostIcon)} cannot be null;");
            UnityAssert.IsNotNull(textRef, $"{nameof(textRef)} cannot be null;");
        }

        public override void Spawned()
        {
            base.Spawned();

            if (Object.HasInputAuthority)
            {
                canvas.gameObject.SetActive(false);
            }

            hostIcon.enabled = IsHost;
        }

        private void Update()
        {
            if (!Object.HasInputAuthority)
            {
                // Rotate to face the local player
                Transform cameraTransform = Camera.main.transform;
                canvas.transform.forward = transform.position - cameraTransform.position;
            }
        }

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();

            if (Object.HasInputAuthority)
            {
                Transform cameraTransform = Camera.main.transform;
                UpdatePositionHostRPC(cameraTransform.position);
            }
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, Channel = RpcChannel.Unreliable,
            HostMode = RpcHostMode.SourceIsHostPlayer)]
        private void UpdatePositionHostRPC(Vector3 position)
        {
            transform.position = position + new Vector3(0.0f, 0.2f, 0.0f);

            // Doing this every tick because it can change at any time.
            IsHost = Object.InputAuthority.PlayerId == Runner.Simulation.MaxConnections;
        }

        public static void OnNameChanged(Changed<Nametag> changed)
        {
            var bvr = changed.Behaviour;
            bvr.textRef.text = bvr.Name.Value;
        }

        public static void OnIsHostChanged(Changed<Nametag> changed)
        {
            var bvr = changed.Behaviour;
            bvr.hostIcon.enabled = bvr.IsHost;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (canvas == null)
            {
                canvas = GetComponentInChildren<Canvas>();
            }

            if (hostIcon == null)
            {
                hostIcon = GetComponentInChildren<Image>();
            }

            if (textRef == null)
            {
                textRef = GetComponentInChildren<TextMeshProUGUI>();
            }
        }
#endif
    }
}
