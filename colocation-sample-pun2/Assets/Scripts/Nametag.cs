// Copyright (c) Meta Platforms, Inc. and affiliates.

using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;
using UnityEngine.Assertions;
using PhotonPlayer = Photon.Realtime.Player;

namespace com.meta.xr.colocation.pun2.debug
{
    /// <summary>
    ///     A class that handles the behavior of a networked name tag
    /// </summary>
    public class Nametag : MonoBehaviourPun
    {
        [SerializeField] private TextMeshProUGUI textRef;

        [SerializeField] private Canvas canvas;

        [SerializeField] private Image masterClientIcon;

        private void Awake()
        {
            Assert.IsNotNull(canvas, $"{nameof(canvas)} cannot be null;");
            Assert.IsNotNull(masterClientIcon, $"{nameof(masterClientIcon)} cannot be null;");
            Assert.IsNotNull(textRef, $"{nameof(textRef)} cannot be null;");
        }

        private void Start()
        {
            textRef.text = "";
            if (photonView.IsMine)
            {
                canvas.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (photonView.IsMine)
            {
                Camera mainCamera = Camera.main;
                Assert.IsNotNull(mainCamera);
                Transform cameraTransform = mainCamera.transform;

                transform.SetPositionAndRotation(cameraTransform.position + new Vector3(0.0f, 0.15f, 0.0f),
                    cameraTransform.rotation * Quaternion.Euler(0, 180.0f, 0));
            }

            foreach (PhotonPlayer playerRef in PhotonNetwork.PlayerList)
            {
                if (photonView.Owner == playerRef)
                {
                    if (textRef.text == "")
                    {
                        textRef.text = playerRef.NickName;
                    }

                    //Doing this every frame because it can change at any time.
                    if (playerRef.IsMasterClient)
                        masterClientIcon.enabled = true;
                    else
                        masterClientIcon.enabled = false;

                    break;
                }
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (canvas == null)
            {
                canvas = GetComponentInChildren<Canvas>();
            }

            if (masterClientIcon == null)
            {
                masterClientIcon = GetComponentInChildren<Image>();
            }

            if (textRef == null)
            {
                textRef = GetComponentInChildren<TextMeshProUGUI>();
            }
        }
#endif
    }
}
