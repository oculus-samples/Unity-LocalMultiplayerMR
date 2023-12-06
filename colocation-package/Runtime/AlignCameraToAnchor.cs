// Copyright (c) Meta Platforms, Inc. and affiliates.

using UnityEngine;

namespace com.meta.xr.colocation
{
    /// <summary>
    ///     Class that handles aligning the camera to a given anchor
    /// </summary>
    [DefaultExecutionOrder(10)]
    public class AlignCameraToAnchor : MonoBehaviour
    {
        public OVRSpatialAnchor CameraAlignmentAnchor { get; set; }

        private void Update()
        {
            Align(CameraAlignmentAnchor);
        }

        public void RealignToAnchor()
        {
            Align(CameraAlignmentAnchor);
        }

        private void Align(OVRSpatialAnchor anchor)
        {
            // Align the scene by transforming the camera.
            // The inverse anchor pose is used to move the camera so that the scene appears as if it was parented to the anchor.

            // Get the anchor's raw tracking space pose to align the camera.
            // Note that the anchor's world space pose is dependent on the camera position, in order to maintain consistent world-locked rendering.
            OVRPose trackingSpacePose;
            if (!TryGetPose(anchor, out trackingSpacePose))
            {
                this.enabled = false;
                return;
            }

            // Position the anchor in tracking space
            Transform anchorTransform = anchor.transform;
            anchorTransform.SetPositionAndRotation(trackingSpacePose.position, trackingSpacePose.orientation);

            // Transform the camera to the inverse of the anchor pose to align the scene
            transform.position = anchorTransform.InverseTransformPoint(Vector3.zero);
            transform.eulerAngles = new Vector3(0, -anchorTransform.eulerAngles.y, 0);

            // Update the world space position of the anchor so it renders in a consistent world-locked position.
            OVRPose worldSpacePose = trackingSpacePose.ToWorldSpacePose(Camera.main);
            anchorTransform.SetPositionAndRotation(worldSpacePose.position, worldSpacePose.orientation);
        }

        private static bool TryGetPose(OVRSpatialAnchor anchor, out OVRPose pose)
        {
            if (anchor == null)
            {
                Logger.Log($"{nameof(AlignCameraToAnchor)}: Unable to get anchor pose, anchor is null.",
                    LogLevel.Error);
                pose = OVRPose.identity;
                return false;
            }

            if (!OVRPlugin.TryLocateSpace(anchor.Space, OVRPlugin.GetTrackingOriginType(), out var posef))
            {
                Logger.Log($"{nameof(AlignCameraToAnchor)}: Unable to get anchor pose for anchor {anchor.Space}.",
                    LogLevel.Error);
                pose = OVRPose.identity;
                return false;
            }

            pose = posef.ToOVRPose();
            return true;
        }
    }
}
