// Copyright (c) Meta Platforms, Inc. and affiliates.

using Meta.XR.Samples;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace com.meta.xr.colocation.ngo.debug
{
    /// <summary>
    ///     A class that handles the behavior of a networked name tag
    /// </summary>
    [MetaCodeSample("LocalMultiplayerMR-NGO")]
    public class Nametag : NetworkBehaviour
    {
        [SerializeField] private TextMeshProUGUI textRef;

        [SerializeField] private Canvas canvas;
        [SerializeField] private Image hostIcon;

        private NetworkVariable<FixedString64Bytes> _name = new("Connecting...");

        public FixedString64Bytes Name
        {
            get => _name.Value;
            set => _name.Value = value;
        }

        private void Awake()
        {
            Assert.IsNotNull(canvas, $"{nameof(canvas)} cannot be null;");
            Assert.IsNotNull(hostIcon, $"{nameof(hostIcon)} cannot be null;");
            Assert.IsNotNull(textRef, $"{nameof(textRef)} cannot be null;");

            textRef.text = Name.Value;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsOwner)
            {
                canvas.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            Transform cameraTransform = Camera.main.transform;

            if (IsOwner)
            {
                UpdatePositionServerRPC(cameraTransform.position);
            }

            // Rotate to face the local player
            canvas.transform.forward = transform.position - cameraTransform.position;

            textRef.text = Name.Value;

            // Doing this every frame because it can change at any time.
            hostIcon.enabled = OwnerClientId == NetworkManager.ServerClientId;
        }

        [ServerRpc(RequireOwnership = true)]
        private void UpdatePositionServerRPC(Vector3 position)
        {
            transform.position = position + new Vector3(0.0f, 0.2f, 0.0f);
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
