﻿namespace VSS.TRex.Types
{
    /// <summary>
    /// The set of generic response codes various parts of the system emit...
    /// </summary>
    public enum ServerRequestResult
    {
        UnknownError = 0x00000000,
        NoError = 0x00000001,
        ClientNotRegistered = 0x00000002,
        CellNotFound = 0x00000003,
        UnknownCellPassSelectionMethod = 0x00000004,
        NoFilteredCellValue = 0x00000005,
        FailedToProcessFile = 0x00000006,
        TimedOutWaitingForProcessing = 0x00000007,
        SubGridNotFound = 0x00000008,
        NoSelectedSiteModel = 0x00000009,
        ServerUnavailable = 0x0000000A,
        NoOutstandingEvents = 0x0000000B,
        NameAlreadyExists = 0x0000000C,
        EventNotFound = 0x0000000D,
        MaxNoOfMachinesReached = 0x0000000E,
        DataAdminDeleteOpActive = 0x0000000F,
        DataAdminArchiveOpActive = 0x00000010,
        ServerBusyNoDBWriteLockAcquired = 0x00000011,
        ServerNotReady = 0x00000012,
        FailedToReadSubgridSegment = 0x00000013,
        OperationAbortedByCaller = 0x00000014,
        ProductionDataRequiresUpgrade = 0x00000015,
        NoConnectionToServer = 0x00000016,
        FailedToConvertClientWGSCoords = 0x00000017,
        UnableToPrepareFilterForUse = 0x00000018,
        ServiceStopped = 0x00000019,
        FailedToRequestSubgridExistenceMap = 0x0000001A,
        FailedToComputeDesignBoundary = 0x0000001B,
        FailedToComputeDesignFilterBoundary = 0x0000001C,
        NoResponseDataInResponse = 0x0000001D,
        FailedToConvertServerNEECoords = 0x0000001E,
        FailedToBuildLiftsForCell = 0x0000001F,
        FailedToLock = 0x00000020,
        Cancelled = 0x00000021,
        MissingInputParameters = 0x00000022,
        FilterInitialisationFailure = 0x00000023,
        InvokeError_rpcirFailed = 0x00000024,
        InvokeError_rpcirEncodeFailure = 0x00000025,
        InvokeError_rpcirDecodeFailure = 0x00000026,
        InvokeError_rpcirNotConnected = 0x00000027,
        FailedToComputeDesignFilterPatch = 0x00000028,
        DataModelDoesNotHaveValidPlanExtents = 0x00000029,
        DataModelHasInvalidZeroCellSize = 0x0000002A,
        ProfileGenerationFailure = 0x0000002B,
        DataModelDoesNotHaveValidPlanExtentsNoData = 0x0000002C,
        FailedToComputeDesignElevationPatch = 0x0000002D
    }
}
