// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;
using Meta.XR.Samples;
using UnityEngine;

namespace com.meta.xr.colocation.samples.utils
{
    /// <summary>
    ///     Class that handles the visual appearance of an Anchor
    /// </summary>
    [MetaCodeSample("LocalMultiplayerMR.Core-Colocation")]
    public class AnchorDebugVisual : MonoBehaviour
    {
        private static bool _debugVisualsVisible = true;
        private static event Action _debugVisibilityChanged;

        public static bool DebugVisualsVisible
        {
            get => _debugVisualsVisible;
            set
            {
                if (value == _debugVisualsVisible)
                {
                    return;
                }

                _debugVisualsVisible = value;
                _debugVisibilityChanged.Invoke();
            }
        }

        private void Awake()
        {
            _debugVisibilityChanged += OnDebugVisibilityChanged;
            OnDebugVisibilityChanged();
        }

        private void OnDebugVisibilityChanged()
        {
            gameObject.SetActive(_debugVisualsVisible);
        }
    }
}
