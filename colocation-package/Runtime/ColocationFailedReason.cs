// Copyright (c) Meta Platforms, Inc. and affiliates.

namespace com.meta.xr.colocation
{
    /// <summary>
    ///     Enum that lists the reasons for colocation failing
    /// </summary>
    public enum ColocationFailedReason
    {
        AutomaticFailedToCreateAnchor,
        AutomaticFailedToSaveAnchorToCloud,
        AutomaticFailedToShareAnchor,
        AutomaticFailedToLocalizeAnchor
    }
}
