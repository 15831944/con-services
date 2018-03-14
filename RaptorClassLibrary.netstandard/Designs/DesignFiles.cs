﻿using System.Collections.Generic;
using System.Diagnostics;
using VSS.VisionLink.Raptor.Designs;

namespace VSS.Velociraptor.DesignProfiling
{
    /*
  TDesignFiles = class(TObject)
    private
      FDesigns : TObjectList;
      FDesignCacheSizeInKB : Integer;
      FCurrentCacheSizeInKB : Integer;
      FDesignUnlockedEvent : TSimpleEvent;
      function EnsureSufficientSpaceToLoadDesign(SpaceRequiredInKB: Integer): Boolean;
      function SpaceAvailableInKB: Integer;
      function RemoveDesignFromCache(Design :TDesignBase; DeleteTTMFile : Boolean) : Boolean;
      procedure AddDesignToCache(const Design: TDesignBase);
      function ImportFileFromTCC(const DesignDescriptor : TVLPDDesignDescriptor; const DataModelID : Int64) : Boolean;

      procedure DeleteLocallyCachedFile(const FileToDelete: TFileName);

      function GetDesignInCache(const DataModelID     :Int64;
                                const DesignFileName  :String;
                                out   Design          :TDesignBase) :Boolean;
    protected
      Function Locate(const AFileName : TFileName;
                      const ADataModelID : Int64) : TDesignBase;

    public
      constructor Create(ADesignCacheSizeInKB : Integer);
      Destructor Destroy; Override;

      Function Lock(const DesignDescriptor : TVLPDDesignDescriptor;
                    const DataModelID : Int64; const ACellSize: Double; out LoadResult: TDesignLoadResult) : TDesignBase;
      Function UnLock(ADesign : TDesignBase) : Boolean;

      Function AnyLocks(out LockCount : integer) : Boolean;

      function GetCombinedSubgridIndexStream(const Surfaces: TICGroundSurfaceDetailsList;
                                             const DataModelID : Int64; const ACellSize: Double;
                                             out MS: TMemoryStream): Boolean;

      procedure UpdateDesignCache(const DataModelID     :Int64;
                                  const DesignFileName  :String;
                                  const DeleteTTMFile   :Boolean);
  end;
  */
    public class DesignFiles
    {
        /// <summary>
        /// A static instance of the designs currently in use
        /// </summary>
        public static DesignFiles Designs = new DesignFiles();

        private Dictionary<DesignDescriptor, DesignBase> designs = new Dictionary<DesignDescriptor, DesignBase>();

        public bool RemoveDesignFromCache(DesignDescriptor designDescriptor, DesignBase design, bool deleteFile)
        {
            if (deleteFile)
            {
                Debug.Assert(false, "Deletefile not implemented");
                return false;
            }

            if (designs.TryGetValue(designDescriptor, out DesignBase Design))
            {
                return designs.Remove(designDescriptor);
            }
            else
            {
                return false;
            }
        }

        public void AddDesignToCache(DesignDescriptor designDescriptor, DesignBase design)
        {
            lock (designs)
            {
                if (designs.TryGetValue(designDescriptor, out DesignBase Design))
                {
                    // The design is already there...
                    Debug.Assert(false, $"Error adding design {designDescriptor} to designs, already present.");
                    return;
                }

                designs.Add(designDescriptor, design);
            }
        }

        /// <summary>
        /// Acquire a lock and referance to the design referenced by the given design descriptor
        /// </summary>
        /// <param name="designDescriptor"></param>
        /// <param name="DataModelID"></param>
        /// <param name="ACellSize"></param>
        /// <param name="LoadResult"></param>
        /// <returns></returns>
        public DesignBase Lock(DesignDescriptor designDescriptor,
                               long DataModelID, double ACellSize, out DesignLoadResult LoadResult)
        {
            DesignBase design;

            // Very simple lock function...
            lock (designs)
            {
                designs.TryGetValue(designDescriptor, out design);
            }

            if (design == null)
            {
                // Load the design into the cache (in this case jsut TTM files)
                design = new TTMDesign(ACellSize);
                design.LoadFromFile(designDescriptor.FullPath);

                AddDesignToCache(designDescriptor, design);
            }

            LoadResult = design == null ? DesignLoadResult.UnknownFailure : DesignLoadResult.Success;
            return design;
        }

        /// <summary>
        /// Release a lock to the design referenced by the given design descriptor
        /// </summary>
        /// <param name="designDescriptor"></param>
        /// <param name="design"></param>
        /// <returns></returns>
        public bool UnLock(DesignDescriptor designDescriptor, DesignBase design)
        {
            lock (Designs)
            {
                // Very simple unlock function...
                return true;
            }
        }

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public DesignFiles()
        {

        }
    }
}
