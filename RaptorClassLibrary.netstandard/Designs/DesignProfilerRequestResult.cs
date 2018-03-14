﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.Velociraptor.DesignProfiling
{
    public enum DesignProfilerRequestResult
    {
        OK,
        UnknownError,
        CouldNotConnectToServer,
        FailedToConvertClientWGSCoords,
        FailedToLoadDesignFile,
        ProfileGenerationFailure,
        FailedToResultFromResponseVerb,
        UnsupportedDesignType,
        FailedToSaveIntermediaryResult,
        FailedToLoadIntermediaryResult,
        NoElevationsInRequestedPatch,
        ServiceStopped,
        DesignDoesNotSupportSubgridOverlayIndex,
        FailedToSaveSubgridOverlayIndexToStream,
        AlignmentContainsNoElements,
        AlignmentContainsNoStationing,
        AlignmentContainsInvalidStationing,
        InvalidStationValues,
        NoSelectedSiteModel,
        FailedToComputeAlignmentVertices,
        FailedToAddItemToCache,
        FailedToUpdateCache,
        FailedGetDataModelSpatialExtents,
        NoAlignmentsFound,
        InvalidResponseCode
    }
}
