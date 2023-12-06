// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace com.meta.xr.colocation
{
    public class SharedAnchorManager
    {
        /// <summary>
        ///   Handles interacting with the OVRSpatialAnchor API to create, save, localize, and share a OVRSpatialAnchor
        /// </summary>
        private readonly List<OVRSpatialAnchor> _localAnchors = new();

        private readonly List<OVRSpatialAnchor> _sharedAnchors = new();

        private readonly HashSet<OVRSpaceUser> _userShareList = new();

        private const int SaveAnchorWaitTimeThreshold = 10000;
        private bool _saveAnchorSaveToCloudIsSuccessful;

        private const int ShareAnchorWaitTimeThreshold = 10000;
        private bool _shareAnchorIsSuccessful;

        private const int RetrieveAnchorWaitTimeThreshold = 10000;
        private bool _retrieveAnchorIsSuccessful;

        private List<Task> _localizationTasks;
        private List<TaskCompletionSource<bool>> _localizationTcsList;

        public GameObject AnchorPrefab { get; set; }
        public IReadOnlyList<OVRSpatialAnchor> LocalAnchors => _localAnchors;

        private List<OVRSpatialAnchor> _createdAnchors;

        public async Task<OVRSpatialAnchor> CreateAnchor(Vector3 position, Quaternion orientation)
        {
            Logger.Log($"{nameof(SharedAnchorManager)}: Attempt to InstantiateAnchor", LogLevel.Verbose);
            var anchor = InstantiateAnchor();
            if (anchor == null)
            {
                Logger.Log($"{nameof(SharedAnchorManager)}: Anchor is null", LogLevel.Error);
            }
            else if (anchor.transform == null)
            {
                Logger.Log("CreateAnchor: anchor.transform is null", LogLevel.Error);
            }

            while (!anchor.Created)
            {
                await Task.Delay(100);
            }

            if (!anchor || !anchor.Created)
            {
                Logger.Log($"{nameof(SharedAnchorManager)}: Anchor creation failed.",
                    LogLevel.SharedSpatialAnchorsError);
                return null;
            }

            Logger.Log($"{nameof(SharedAnchorManager)}: Created anchor with id {anchor.Uuid}", LogLevel.Info);

            _localAnchors.Add(anchor);
            return anchor;
        }

        public async Task<bool> SaveLocalAnchorsToCloud()
        {
            TaskCompletionSource<bool> utcs = new();
            _saveAnchorSaveToCloudIsSuccessful = false;
            CheckIfSavingAnchorsServiceHung();
            Logger.Log(
                $"{nameof(SharedAnchorManager)}: Saving anchors: {string.Join(", ", _localAnchors.Select(el => el.Uuid))}",
                LogLevel.Verbose
            );

            OVRSpatialAnchor.Save(
                _localAnchors,
                new OVRSpatialAnchor.SaveOptions { Storage = OVRSpace.StorageLocation.Cloud },
                (_, result) =>
                {
                    utcs.TrySetResult(result == OVRSpatialAnchor.OperationResult.Success);
                    _saveAnchorSaveToCloudIsSuccessful = true;
                }
            );

            return await utcs.Task;
        }

        private async void CheckIfSavingAnchorsServiceHung()
        {
            await Task.Delay(SaveAnchorWaitTimeThreshold);
            if (!_saveAnchorSaveToCloudIsSuccessful)
            {
                Logger.Log(
                    $"SharedAnchorManager: It has been {SaveAnchorWaitTimeThreshold}ms since attempting to save to the cloud. Anchors service may have failed",
                    LogLevel.Warning);
            }
        }

        public async Task<IReadOnlyList<OVRSpatialAnchor>> RetrieveAnchors(Guid[] anchorIds)
        {
            Assert.IsTrue(anchorIds.Length <= OVRPlugin.SpaceFilterInfoIdsMaxSize,
                "SpaceFilterInfoIdsMaxSize exceeded.");

            TaskCompletionSource<IReadOnlyList<OVRSpatialAnchor>> utcs = new();
            _retrieveAnchorIsSuccessful = false;
            CheckIfRetrievingAnchorServiceHung();
            Logger.Log($"{nameof(SharedAnchorManager)}: Querying anchors: {string.Join(", ", anchorIds)}",
                LogLevel.Verbose);

            OVRSpatialAnchor.LoadUnboundAnchors(
                new OVRSpatialAnchor.LoadOptions
                {
                    StorageLocation = OVRSpace.StorageLocation.Cloud,
                    Timeout = 0,
                    Uuids = anchorIds
                },
                async unboundAnchors =>
                {
                    _retrieveAnchorIsSuccessful = true;
                    if (unboundAnchors == null)
                    {
                        Logger.Log(
                            $"{nameof(SharedAnchorManager)}: Failed to query anchors - {nameof(OVRSpatialAnchor.LoadUnboundAnchors)} returned null.",
                            LogLevel.SharedSpatialAnchorsError
                        );
                        utcs.TrySetResult(null);
                        return;
                    }

                    if (unboundAnchors.Length != anchorIds.Length)
                    {
                        Logger.Log(
                            $"{nameof(SharedAnchorManager)}: {anchorIds.Length - unboundAnchors.Length}/{anchorIds.Length} anchors failed to relocalize.",
                            LogLevel.SharedSpatialAnchorsError
                        );
                    }

                    _createdAnchors = new List<OVRSpatialAnchor>();
                    _localizationTasks = new List<Task>();
                    _localizationTcsList = new List<TaskCompletionSource<bool>>();

                    for (int i = 0; i < unboundAnchors.Length; i++)
                    {
                        TaskCompletionSource<bool> localizationTcs = new TaskCompletionSource<bool>();
                        _localizationTcsList.Add(localizationTcs);
                        _localizationTasks.Add(localizationTcs.Task);
                    }

                    // Bind anchors
                    for (int i = 0; i < unboundAnchors.Length; i++)
                    {
                        var unboundAnchor = unboundAnchors[i];
                        LocalizeUnboundedAnchor(unboundAnchor, i);
                    }

                    // Wait for anchors to be created
                    await Task.WhenAll(_localizationTasks);

                    foreach (var anchor in _createdAnchors)
                    {
                        while (anchor.PendingCreation)
                        {
                            await Task.Yield();
                        }
                    }

                    utcs.TrySetResult(_createdAnchors);
                }
            );

            return await utcs.Task;
        }

        private void LocalizeUnboundedAnchor(OVRSpatialAnchor.UnboundAnchor unboundAnchor, int index)
        {
            unboundAnchor.Localize((unboundedAnchor, success) =>
            {
                if (!success)
                {
                    Logger.Log($"{nameof(SharedAnchorManager)}: {unboundedAnchor} Localization failed!",
                        LogLevel.Error);
                    _localizationTcsList[index].TrySetResult(false);
                    return;
                }

                var anchor = InstantiateAnchor();
                unboundedAnchor.BindTo(anchor);
                _sharedAnchors.Add(anchor);
                Logger.Log($"{nameof(SharedAnchorManager)}: Localization Succeeded", LogLevel.Verbose);
                _createdAnchors.Add(anchor);
                _localizationTcsList[index].TrySetResult(true);
            });
        }

        private async void CheckIfRetrievingAnchorServiceHung()
        {
            await Task.Delay(RetrieveAnchorWaitTimeThreshold);
            if (!_retrieveAnchorIsSuccessful)
            {
                Logger.Log(
                    $"{nameof(SharedAnchorManager)}: It has been {RetrieveAnchorWaitTimeThreshold}ms since attempting to retrieve anchor(s). Anchors service may have failed",
                    LogLevel.Warning);
            }
        }

        public async Task<bool> ShareAnchorsWithUser(ulong userId)
        {
            _userShareList.Add(new OVRSpaceUser(userId));
            _shareAnchorIsSuccessful = false;
            CheckIfSharingAnchorServiceHung();
            if (_localAnchors.Count == 0)
            {
                Logger.Log($"{nameof(SharedAnchorManager)}: No anchors to share.", LogLevel.Warning);
                return true;
            }

            OVRSpaceUser[] users = _userShareList.ToArray();

            Logger.Log($"{nameof(SharedAnchorManager)}: Sharing {_localAnchors.Count} anchors with users: {userId}",
                LogLevel.Verbose);

            TaskCompletionSource<bool> utcs = new();
            OVRSpatialAnchor.Share(
                _localAnchors,
                users,
                (_, result) =>
                {
                    Logger.Log($"{nameof(SharedAnchorManager)}: result of sharing the anchor is {result}",
                        LogLevel.Verbose);
                    utcs.TrySetResult(result == OVRSpatialAnchor.OperationResult.Success);
                    _shareAnchorIsSuccessful = true;
                }
            );

            return await utcs.Task;
        }

        private async void CheckIfSharingAnchorServiceHung()
        {
            await Task.Delay(ShareAnchorWaitTimeThreshold);
            if (!_shareAnchorIsSuccessful)
            {
                Logger.Log(
                    $"{nameof(SharedAnchorManager)}: It has been {ShareAnchorWaitTimeThreshold}ms since attempting to share anchor(s). Anchors service may have failed",
                    LogLevel.Warning);
            }
        }

        public void StopSharingAnchorsWithUser(ulong userId)
        {
            _userShareList.RemoveWhere(el => el.Id == userId);
        }

        private OVRSpatialAnchor InstantiateAnchor()
        {
            GameObject anchorGo;
            if (AnchorPrefab != null)
            {
                anchorGo = Object.Instantiate(AnchorPrefab);
            }
            else
            {
                anchorGo = new GameObject();
                anchorGo.AddComponent<OVRSpatialAnchor>();
            }

            anchorGo.name = $"_{anchorGo.name}";
            Object.DontDestroyOnLoad(anchorGo.transform.root);

            var anchor = anchorGo.GetComponent<OVRSpatialAnchor>();
            Assert.IsNotNull(anchor, $"{nameof(AnchorPrefab)} must have an OVRSpatialAnchor component attached to it.");
            return anchor;
        }
    }
}
